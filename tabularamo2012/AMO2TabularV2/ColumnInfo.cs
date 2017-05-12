/*=====================================================================
  
    File:      ColumnInfo.cs

    Summary:   ColumnInfo is a structure that holds all optional 
               properties to a calculatedColumn in a tabular model. 

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
using System.Globalization;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public struct ColumnInfo
    {
        public string Name {get; internal set;}
        public string DataSourceName { get; internal set; }
        public DataType DataType {get; internal set;}
        public string DataFormat { get; internal set; }
        public string Description { get; internal set; }
        public bool? Visible { get; internal set; }
        public string SortByColumn { get; internal set; }
        public string CurrencySymbol { get; internal set; }
        public int? DecimalPlaces { get; internal set; }
        public bool? ShowThousandSeparator { get; internal set; }

        public ColumnInfo(string name, string dataSourceName, DataType dataType, string dataFormat, string description, bool? visible, string sortByColumn, string currencySymbol, int? decimalPlaces, bool? showThousandSeparator)
            : this()
        {
            Name = name;
            DataSourceName = dataSourceName;
            DataType = dataType;
            DataFormat = dataFormat;
            Description = description;
            Visible = visible;
            SortByColumn = sortByColumn;
            CurrencySymbol = currencySymbol;
            DecimalPlaces = decimalPlaces;
            ShowThousandSeparator = showThousandSeparator;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}", Name, DataSourceName, DataType, DataFormat, Description, Visible, SortByColumn, CurrencySymbol, DecimalPlaces, ShowThousandSeparator);
        }
    }

}
