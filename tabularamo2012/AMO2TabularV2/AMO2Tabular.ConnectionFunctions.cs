/*=====================================================================
  
    File:      AMO2Tabular.ConnectionFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Connections in a tabular model
 
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
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        /// <summary>
        ///     Adds a connection object (aka DataSource object in AMO) and a 
        ///     DataSourceView, if it doesn't exist, to a the local copy of 
        ///     the tabular database, using the given OleDb connection string.
        /// </summary>
        /// <param name="tabularDatabase">A reference to an AMO server object</param>
        /// <param name="datasourceOledbConnectionString">A well formed OleDb connection string to the relational data source</param>
        /// <param name="connectionName">A string with the name on the connection object</param>
        /// <param name="userName">(optional) A string with the user name; when given, it modifies impersonation from service account to user account</param>
        /// <param name="password">(optional) A string with the user password; WARNING: this string is not encrypted</param>
        public static void ConnectionAddRelationalDataSource(AMO.Database tabularDatabase,
                                                             string datasourceOledbConnectionString,
                                                             string connectionName,
                                                             bool updateInstance = true,
                                                             string userName = null,
                                                             string password = null)
        {
            //  Major steps in creating a connection object or AMO.DataSource
            //  - Validate required input arguments
            //  - Create local copy of data source object (the connection object)
            //  - Verify there are no other DataSourceView view object
            //      - Create dsv object in database
            //      - Get source database schema
            //  - Add DataSource object to database
            //  - Add DSV (if created) to database
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

            bool dsvAdded = false;


            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (datasourceOledbConnectionString.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("datasourceOledbConnectionString");
            if (connectionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("connectionName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);


            //  Create local copy of data source
            using (AMO.DataSource connection = new AMO.RelationalDataSource(connectionName, connectionName))
            using (AMO.DataSourceView dsv = new AMO.DataSourceView(connectionName, connectionName))
            {
                connection.ConnectionString = datasourceOledbConnectionString;
                //If user name is null or blank, use default ImpersonateServiceAccount
                if (userName.IsNullOrEmptyOrWhitespace())
                    connection.ImpersonationInfo = new AMO.ImpersonationInfo(AMO.ImpersonationMode.ImpersonateServiceAccount);
                else
                {// At this point, there is no verification that the given credentials will work
                    connection.ImpersonationInfo = new AMO.ImpersonationInfo(AMO.ImpersonationMode.ImpersonateAccount, userName.Trim(), password);
                }


                //  Verify DSV existence and create one if needed
                if (tabularDatabase.DataSourceViews.Count == 0)
                {// No DSV in the database

                    //  Note: DSV added and populated it with the entire relational database schema
                    dsvAdded = true;
                    dsv.Schema = GetDatabaseSchema(datasourceOledbConnectionString);
                    dsv.DataSourceID = connection.ID;
                }


                // Add DataSource and DataSourceView objects to database
                tabularDatabase.DataSources.Add(connection);
                if (dsvAdded) tabularDatabase.DataSourceViews.Add(dsv);
            }
            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

        }
    }
}
