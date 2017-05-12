/*=====================================================================
  
    File:      GetDatabaseSchema.SYBASE OLEDB PROVIDER.cs

    Summary:   This part of GetDatabaseSchema implements the  
               specifics of getting the database schema for 
               a SYBASE OLEDB provider
 
               This is part of the AMO2Tabular.HelperFunctions 
 
               AMO to Tabular (AMO2Tabular) is sample code to show and 
               explain how to use AMO to handle Tabular model objects. 
               The sample can be seen as a sample library of functions
               with the necessary code to execute each particular 
               action or operation over a logical tabular object. 

    Authors:   JuanPablo Jofre (jpjofre@microsoft.com)
    Date:	   21-Apr-2012
  
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
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.Sql;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using System.Globalization;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        private static DataSet GetDatabaseSchemaSYBASE(string sybaseOleDbConnectionString)
        {
            const string functionName = "GetDatabaseSchemaSYBASE";
            //
            //ToDo: Add function code
            //
            throw new NotImplementedException(string.Format(Resources.NotImplementedExceptionFunction, functionName));
        }
    }
}