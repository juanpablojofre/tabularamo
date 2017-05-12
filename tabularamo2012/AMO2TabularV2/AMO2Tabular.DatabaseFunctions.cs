/*=====================================================================
  
    File:      AMO2Tabular.DatabaseFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Databases in a tabular model
 
               AMO to Tabular (AMO2Tabular) is sample code to show and 
               explain how to use AMO to handle Tabular model objects. 
               The sample can be seen as a sample library of functions
               with the necessary code to execute each particular 
               action or operation over a logical tabular object. 

    Authors:   JuanPablo Jofre (jpjofre@microsoft.com)
    Date:	   04-Apr-2012
  
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
using System.Globalization;
using System.Linq;
using System.Text;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;

[assembly: CLSCompliant(true)]
namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        /// <summary>
        /// Creates a tabular database, under the given server, with the provided
        /// name.
        /// If optional parameter 'datasourceOledbConnectionString' is provided 
        /// the connection object (DataSource object in AMO) and the AMO DSV object
        /// will be created.
        /// The DSV object will  be populated with the entire schema of the data
        /// source database.
        /// </summary>
        /// <param name="server">A reference to an AMO server object</param>
        /// <param name="databaseName">A string with the name of the new database</param>
        /// <param name="datasourceOledbConnectionString">(optional) A well formed OleDb connection string to the relational data source</param>
        /// <param name="connectionName">(optional) A string with the name on the connection object</param>
        /// <param name="lcid">(optional) An integer value with the language locale to assign to the database. A value of zero (default) indicates to use the the server default value</param>
        /// <param name="collationName">(optional) A string with the name of the collation to use</param>
        /// <returns>A reference to the new Tabular database, as an AMO database object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Oledb"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "datasource")]
        public static AMO.Database TabularDatabaseAdd(AMO.Server server,
                                                      string databaseName,
                                                      string datasourceOledbConnectionString = null,
                                                      string connectionName = null,
                                                      int lcid = 0,
                                                      string collationName = null,
                                                      int dbCompatibilityLevel = 0)
        {
            //  Major steps in creating a tabular database
            //  - Validate required input arguments
            //  - Create local copy of database
            //  - Create connection object in database
            //      - Create dsv object in database; handled by 'ConnectionAddRelationalDataSource()'
            //  - Add database to database collection in server
            //  - Update server instance
            //
            //  Note:   There are no validations for duplicated names, invalid names or
            //          similar scenarios. It is expected the server will take care of them and
            //          throw exceptions on any invalid situation.
            //
            // Note:    There are no validations on the 'datasourceOledbConnectionString' 
            //          The user is responsible for accuracy and usability of the string.
            //
            // Note:    In AMO only well formed OleDb connection strings are supported
            //          The 'Provider' key word must be the first keyword in the connection string
            //          
            //
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



            //  Validate required input arguments
            if (server == null) throw new ArgumentNullException("server");
            if (databaseName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("databaseName");
            if (!IsServerCompatibilityLevelCorrect(server)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);
            if ((dbCompatibilityLevel!=0) && !IsCompatibilityLevelCorrect(dbCompatibilityLevel)) throw new ArgumentException(Resources.InvalidCompatibilityLevelOperationException);

            databaseName = databaseName.Trim();
            //  Create local copy of database
            //  In this sample code, only InMemory tabular databases are created
            using (AMO.Database newDatabase = server.Databases.Add(databaseName))
            {
                newDatabase.StorageEngineUsed = AMO.StorageEngineUsed.InMemory;
                newDatabase.DirectQueryMode = AMO.DirectQueryMode.InMemory;
                newDatabase.CompatibilityLevel = (dbCompatibilityLevel == 0) ? server.DefaultCompatibilityLevel : dbCompatibilityLevel;
                if (lcid != 0) newDatabase.Language = lcid;
                if (!collationName.IsNullOrEmptyOrWhitespace())
                    newDatabase.Collation = collationName;


                //  Create connection object in database
                if (!datasourceOledbConnectionString.IsNullOrEmptyOrWhitespace())
                    ConnectionAddRelationalDataSource(newDatabase, datasourceOledbConnectionString, connectionName, updateInstance: false);

                //  Update server instance
                newDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

                return newDatabase;
            }
        }

        private static bool IsServerCompatibilityLevelCorrect(AMO.Server server)
        {
            return (server != null) && IsCompatibilityLevelCorrect(server.DefaultCompatibilityLevel);
        }

        private static bool IsDatabaseCompatibilityLevelCorrect(AMO.Database db)
        {
            return (db != null) 
                && IsServerCompatibilityLevelCorrect(db.Parent) 
                && (db.Parent.DefaultCompatibilityLevel >= db.CompatibilityLevel)
                && IsCompatibilityLevelCorrect(db.CompatibilityLevel);
        }

        private static bool IsCompatibilityLevelCorrect(int compatLevel)
        {
            return (compatLevel==((int)CompatibilityLevel.SQL2012RTM)) ||(compatLevel==((int)CompatibilityLevel.SQL2012SP1));
        }

    }
}
