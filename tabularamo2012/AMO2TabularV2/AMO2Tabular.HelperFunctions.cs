/*=====================================================================
  
    File:      AMO2Tabular.HelperFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               utility or helper functions.
               Here you will find utilities to parse an MDX statement,
               to read a file structure, to create the dataset schema
               of a database, etc.
 
               AMO to Tabular (AMO2Tabular) is sample code to show and 
               explain how to use AMO to handle Tabular model objects. 
               The sample can be seen as a sample library of functions
               with the necessary code to execute each particular 
               action or operation over a logical tabular object. 

    Authors:   JuanPablo Jofre (jpjofre@microsoft.com)
    Date:	   11-Apr-2012
  
    Change history:

    @TODO: 
  
-----------------------------------------------------------------------
  
    This file is part of the Microsoft SQL Server Code Samples.
    Copyright (C) Microsoft Corporation.  All rights reserved.
  
  This source code is intended only as a supplement to Microsoft
  Development Tools and/or on-line documentation.  See these other
  materials for detailed information regarding Microsoft code samples.
  
  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF 
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
  PARTICULAR PURPOSE.
  
======================================================================*/
using System;
using System.Data;
using System.Data.OleDb;
using System.Security.Principal;
using System.Linq;
using System.Text.RegularExpressions;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;



namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        //  ***********************************************************
        //
        //  Constant Strings 
        //
        //  ***********************************************************

        public const string MdxScriptHeader = "----------------------------------------------------------\r\n" +
                                              "-- PowerPivot measures command (do not modify manually) --\r\n" +
                                              "----------------------------------------------------------\r\n" +
                                              "\r\n" +
                                              "\r\n";
        public const string createMeasureTemplate = "CREATE MEASURE '{0}'[{1}]={2};";
        // -------------------------------------------------------------------------
        //
        // The following KPI templates apply to 2012 RTM version
        //
        // -------------------------------------------------------------------------
        public const string createGoalMemberTemplate = "CREATE MEMBER CURRENTCUBE.Measures.[_{1} Goal] AS '{2}', ASSOCIATED_MEASURE_GROUP = '{0}';";
        public const string createStatusMemberTemplate = "CREATE MEMBER CURRENTCUBE.Measures.[_{1} Status] AS '{2}', ASSOCIATED_MEASURE_GROUP = '{0}';";
        public const string createTrendMemberTemplate = "CREATE MEMBER CURRENTCUBE.Measures.[_{1} Trend] AS '0', ASSOCIATED_MEASURE_GROUP = '{0}';";
        public const string createKpiTemplate = "CREATE KPI CURRENTCUBE.[{1}] AS Measures.[{1}]"
                                                           + ", ASSOCIATED_MEASURE_GROUP = '{0}'"
                                                           + ", GOAL = Measures.[_{1} Goal]"
                                                           + ", STATUS = Measures.[_{1} Status]"
                                                           + ", TREND = Measures.[_{1} Trend]"
                                                           + ", STATUS_GRAPHIC = '{2}'"
                                                           + ", TREND_GRAPHIC = '{2}';";
        // -------------------------------------------------------------------------
        //
        // The following KPI templates apply to 2012 SP1 and beyond versions
        //
        // -------------------------------------------------------------------------
        public const string createGoalMeasureTemplate = "CREATE MEASURE '{0}'[_{1} Goal] = {2};";
        public const string createStatusMeasureTemplate = "CREATE MEASURE '{0}'[_{1} Status] = {2};";
        public const string createKpiTemplate2 = "CREATE KPI CURRENTCUBE.[{1}] AS Measures.[{1}]"
                                                           + ", ASSOCIATED_MEASURE_GROUP = '{0}'"
                                                           + ", GOAL = Measures.[_{1} Goal]"
                                                           + ", STATUS = Measures.[_{1} Status]"
                                                           + ", STATUS_GRAPHIC = '{2}';";
        public const string goalMeasureNameTemplate = "Measures.[_{0} Goal]";
        public const string statusMeasureNameTemplate = "Measures.[_{0} Status]";
        public const string trendMeasureNameTemplate = "Measures.[_{0} Trend]";
        public const string kpiNameTemplate = "KPIs.[{0}]";

        //  ***********************************************************
        //
        //  Common RegEx expressions
        //
        //  ***********************************************************

        public const string MeasurePattern = @"\b(?<createMeasure>create\s+measure)\s+(?<tableName>(\w+|'(\w|\s+\w)+?'))\[(?<measureName>(\w+|(\w|\s+\w)+?))\]\s*=(?<commandTypeExpression>(((?<doubleQuote>" + "\"" + @")(?:\\\k<doubleQuote>|.)*?\k<doubleQuote>)|&lt;|&gt;|&quot;|&amp;|.|\n)*?);";

        public const string MemberPattern = @"\b(?<createMember>create\s+member)\s+(?<memberFullName>(?<cubeName>(currentcube|\w+|(\[.*?\]))\.){0,1}(?<dimensionName>(\w+|\[.*?\])\.)(?<memberName>(\w+|\[.*?\])))\s+AS\s+(?<memberExpression>(?<singleQuote>')(?:\\\k<singleQuote>|.)*?\k<singleQuote>)\s*(?<propertyPairs>(?<propertyPair>,\s*(?<propertyName>\w+)\s*=\s*(?<propertyValue>(\w+|(?<singleQuote>')(?:\\\k<singleQuote>|.)*?\k<singleQuote>(\[.*?\]){0,1}))\s*)+?);";

        public const string KpiPattern = @"\b(?<createKpi>create\s+kpi)\s+(?<kpiFullName>(?<cubeName>(currentcube|\w+|(\[.*?\]))\.){0,1}(?<dimensionName>(\w+|\[.*?\])\.){0,1}(?<kpiName>(\w+|\[.*?\])))\s+AS\s+(?<kpiExpression>((?<singleQuote>')(?:\\\k<singleQuote>|.)*?\k<singleQuote>|(\w+|\[.*?\])\.\[.*?\]))\s*(?<propertyPairs>(?<propertyPair>,\s*(?<propertyName>\w+)\s*=\s*(?<propertyValue>(\w+|(?<singleQuote>')(?:\\\k<singleQuote>|.)*?\k<singleQuote>(\[.*?\]){0,1}|(\w+|\[.*?\])\.\[.*?\]))\s*)+?)\s*;";

        public const string ntLoginPattern = @"\A(\w+||(\w+|\.)\\\w+)\z";

        // ************************************************************
        //
        // Group and property string names
        //
        // ************************************************************

        public const string HierarchyStringName = "hierarchyName";
        public const string DefiningLevelColumnName = "definingLevelColumnName";
        public const string LevelName = "levelName";
        public const string LevelInfo = "levelInfo";

        public const string TabularDatabaseStringName = "tabularDatabase";
        public const string TableStringName = "tableName";
        public const string ColumnStringName = "columnName";
        public const string MeasureStringName = "measureName";
        public const string PerspectiveStringName = "perspectiveName";
        public const string DatasourceColumStringName = "datasourceColumnName";
        public const string MdxScriptStringName = "MdxScript";
        public const string DaxExpressionStringName = "daxExpression";
        public const string DaxFilterExpressionStringName = "daxFilterExpression";
        public const string RoleStringName = "roleName";


        //  ***********************************************************
        //
        //  Extension Methods
        //
        //  ***********************************************************

        public static bool IsNullOrEmptyOrWhitespace(this string s)
        {
            //return string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);
            //This change retains the functionality and doesn't requiere the assembly to be compiled with .Net 4.0
            return string.IsNullOrEmpty(s) || (s.Trim().Length == 0);

        }


        //  ***********************************************************
        //
        //  Utility Functions
        //
        //  ***********************************************************

        /// <summary>
        ///     Replicates the database schema into a DataSet object from an oleDb connection string
        ///     No data is loaded to the returning dataset
        ///     For each of the supported providers, there is a specific method that obtains the 
        ///     database schema for the DataSet
        /// </summary>
        /// <param name="oleDbConnectionString">oleDb connection string</param>
        /// <returns>Dataset object with database schema; no data is loaded to the dataset.</returns>
        private static DataSet GetDatabaseSchema(string oleDbConnectionString)
        {
            // Note:    As of SQL Server 2012 Analysis Services, the only supported OleDb providers were:
            //     
            //          -   "MICROSOFT.JET.OLEDB.4.0"                   <-- Microsoft OLE DB Provider for Microsoft Jet 4.0
            //          -   "SQLOLEDB"                                  <-- Microsoft SQL OLE DB Provider for SQL Server
            //          -   "SNAC"                                      <-- Microsoft SQL Native Client OLE DB Provider
            //          -   "SQLNCLI"                                   <-- SQL Server Native Client
            //          -   "MSDAORA"                                   <-- Microsoft OLE DB Provider for Oracle
            //          -   "DB2OLEDB"                                  <-- Microsoft OLE DB Provider for DB2
            //          -   "TDOLEDB"                                   <-- OLE DB Provider for Teradata
            //          -   "IFXOLEDBC"                                 <-- IBM Informix OLE DB Provider
            //          -   "SYBASE ASE OLE DB PROVIDER"                <-- OLE DB Provider for Sybase Adaptive Server Enterprise (ASE)
            //          -   "ASAPROV"                                   <-- OLE DB Provider for Sybase Adaptive Server Anywhere (ASA)
            //          -   "SYBASE OLEDB PROVIDER"                     <-- OLE DB Provider for Sybase in version 12
            //          -   "ASEOLEDB"                                  <-- OLE DB Provider for Sybase in version 15
            //          -   "Microsoft SQL Server MPP OLE DB Provider"  <-- OLE DB Provider for SQL Server MPP OLE DB Provider

            OleDbConnectionStringBuilder cnxString = new OleDbConnectionStringBuilder(oleDbConnectionString);

            using (DataSet dsv = new DataSet())
            {
                if (cnxString.Provider.Contains("SQLOLEDB"))
                    return GetDatabaseSchemaSQLOLEDB(oleDbConnectionString);


                #region Remaining providers have not been implemented yet
                if (cnxString.Provider.Contains("MICROSOFT.JET.OLEDB.4.0"))
                    return GetDatabaseSchemaJETOLEDB(oleDbConnectionString);

                if (cnxString.Provider.Contains("SNAC"))
                    return GetDatabaseSchemaSNAC(oleDbConnectionString);

                if (cnxString.Provider.Contains("SQLNCLI"))
                    return GetDatabaseSchemaSQLNCLI(oleDbConnectionString);

                if (cnxString.Provider.Contains("MSDAORA"))
                    return GetDatabaseSchemaMSDAORA(oleDbConnectionString);

                if (cnxString.Provider.Contains("DB2OLEDB"))
                    return GetDatabaseSchemaDB2OLEDB(oleDbConnectionString);

                if (cnxString.Provider.Contains("TDOLEDB"))
                    return GetDatabaseSchemaTDOLEDB(oleDbConnectionString);

                if (cnxString.Provider.Contains("IFXOLEDBC"))
                    return GetDatabaseSchemaIFXOLEDBC(oleDbConnectionString);

                if (cnxString.Provider.Contains("SYBASE ASE OLE DB PROVIDER"))
                    return GetDatabaseSchemaSYBASEASE(oleDbConnectionString);

                if (cnxString.Provider.Contains("ASAPROV"))
                    return GetDatabaseSchemaASAPROV(oleDbConnectionString);

                if (cnxString.Provider.Contains("SYBASE OLEDB PROVIDER"))
                    return GetDatabaseSchemaSYBASE(oleDbConnectionString);

                if (cnxString.Provider.Contains("ASEOLEDB"))
                    return GetDatabaseSchemaASEOLEDB(oleDbConnectionString);

                if (cnxString.Provider.Contains("Microsoft SQL Server MPP OLE DB Provider"))
                    return GetDatabaseSchemaMPPOLEDB(oleDbConnectionString);

                throw new NotSupportedException(Resources.OleDbProviderNotSupportedException);
                #endregion
            }
        }

        /// <summary>
        /// Finds if a Column is defined as unique in a table
        /// </summary>
        /// <param name="dataSet">The dataset object that contains the table and column</param>
        /// <param name="tableName">The table that has the calculatedColumn</param>
        /// <param name="columnName">The column to verify uniqueness</param>
        /// <returns></returns>
        private static bool IsUnique(DataSet dataSet, string tableName, string columnName)
        {
            foreach (Constraint c in dataSet.Tables[tableName].Constraints)
            {
                if (c is UniqueConstraint)
                {
                    if ((((UniqueConstraint)c).Columns.Length == 1)
                        && (0 == string.Compare(((UniqueConstraint)c).Columns[0].ColumnName, columnName, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Maps a .Net data type to supported Tabular data types
        /// If the .Net type is not supported it is mapped to DataType.Unsupported
        /// </summary>
        /// <param name="type">A System.Type type</param>
        /// <returns>A mapping value from the DataType enumeration</returns>
        private static DataType mapToSupportedTabularDataTypes(Type type)
        {
            //  Supported Tabular Data Types are:
            //  -   Whole Number    <-- Int16, Int32, Int64, UInt16, UInt32, UInt64, Byte, SByte
            //  -   Decimal         <-- Single, Double, Decimal
            //  -   YesNo           <-- Boolean
            //  -   Date            <-- Date
            //  -   String          <-- String, Guid
            //  -   Image           <-- Byte[]

            //  Note: Because there is no currency data type in .Net there is no default data type to match to Currency here

            //  Note: MS SQL UniqueIdentifier type maps to .Net Guid type; not having a corresponding data type in tabular models
            //        the AS engine maps a Guid to a String in tabular models

            //  Note: DataType.Unsupported value is returned if there is no correspondence between type Type and supported Tabular data types

            //  Note?: Is this function a better approach to do data type convertion than using AMO.OleDbTypeConverter.GetRestrictedOleDbType(dataColumn.DataType)


            switch (Type.GetTypeCode(type))
            {
                //  case WholeNumber
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return DataType.WholeNumber;

                //  case Decimal
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return DataType.Decimal;

                //  case YesNo
                case TypeCode.Boolean:
                    return DataType.YesNo;

                //  case Date
                case TypeCode.DateTime:
                    return DataType.Date;

                //  case Text
                case TypeCode.String:
                    return DataType.Text;


                //  case Image
                //  case Guid --> String
                //  else Unsupported
                case TypeCode.Object:


                    if (type == typeof(byte[]))
                        return DataType.Image;

                    if (type == typeof(Guid))
                        return DataType.Text;

                    return DataType.Unsupported;

                default:
                    return DataType.Unsupported;
            }

        }

        /// <summary>
        /// Returns the Security Identifier (SID) of a MS Windows user, group or other security principal, from the login user name.
        /// </summary>
        /// <param name="ntLogin">A string with the user name to get the SID from. NtLogin must be in the form of: [username] | .\[username] | [domain]\[username]</param>
        /// <returns>A string with the Security Identifier (SID) of a MS Windows user, group or other security principal</returns>
        static private string getSid(string ntLogin)
        {
            //  Validate ntLogin
            if (!Regex.IsMatch(ntLogin, ntLoginPattern, RegexOptions.CultureInvariant))
                throw new ArgumentException(Resources.InvalidUserNameArgumentException);
            //[JPJofre, 2012-10-04] commenting out this section to simplify SID acquisition
            //  Also, allows the assembly to be compiled with an earlier version of .Net framework
                //  Set principal context, based on ntLogin string
                //  ==> principal context is DOMAIN if login string pattern --> <domain>\<username>
                //  ==> principal context is MACHINE if login string pattern --> .\<username> | <username>
                //ContextType contextType = ContextType.Machine;
                //if (ntLogin.StartsWith(@".\",true,System.Globalization.CultureInfo.InvariantCulture))
                //{
                //    ntLogin = ntLogin.Replace(@".\", string.Empty);
                //}
                //else if (ntLogin.Contains('\\'))
                //{
                //    contextType = ContextType.Domain;
                //}

                //PrincipalContext principalContext = new PrincipalContext(contextType);

                ////  Get user information 
                //UserPrincipal user = UserPrincipal.FindByIdentity(principalContext, ntLogin);

            //[JPJofre, 2012-10-04]
            //Per Darren Gosbell suggestion, we can simplify the acquisition of the SID
            //The following code is verbatim from a mail he sent to me on Oct 4th., 2012

            NTAccount f = new NTAccount(ntLogin);
            SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));

            //  Return SID string
            return s.ToString();
        }


    }
}
