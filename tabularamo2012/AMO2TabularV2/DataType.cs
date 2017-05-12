/*=====================================================================
  
    File:      DataType.cs

    Summary:   DataType is an enumeration used in AMO2Tabular to 
               represent all available data types in tabular models

    Authors:   JuanPablo Jofre (jpjofre@microsoft.com)
    Date:      04-Apr-2012
  
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


namespace MicrosoftSql2012Samples.Amo2Tabular
{
    public enum DataType
    {
        Unsupported = -1,
        Default = 0,
        WholeNumber = System.Data.OleDb.OleDbType.BigInt,
        Decimal = System.Data.OleDb.OleDbType.Double,
        Currency = System.Data.OleDb.OleDbType.Currency,
        Date = System.Data.OleDb.OleDbType.Date,
        YesNo = System.Data.OleDb.OleDbType.Boolean,
        Text = System.Data.OleDb.OleDbType.WChar,
        Image = System.Data.OleDb.OleDbType.Binary
    }
}
