/*=====================================================================
  
    File:      TableInfo.cs

    Summary:   TableInfo is a structure that holds relevant 
               information of a table a tabular model. 

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public struct TableInfo
    {
        public string Name {get; internal set;}
        public string DataSourceName { get; internal set; }
        public string Description { get; internal set; }
        public bool Visible { get; internal set; }
        public List<string> PartitionNames { get; internal set; }

        public TableInfo(string name, string dataSourceName, string description, bool visible, List<string> partitionNames)
            : this()
        {
            Name = name;
            DataSourceName = dataSourceName;
            Description = description;
            Visible = visible;
            PartitionNames = partitionNames;
        }

        public override string ToString()
        {
            StringBuilder partitionNames = new StringBuilder();
            foreach (string partitionName in PartitionNames)
                partitionNames.AppendFormat(CultureInfo.InvariantCulture, "{0},", partitionName);
            partitionNames.Remove(partitionNames.Length - 1, 1);

            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}\t{2}\t{3}\t({4})", Name, DataSourceName, Description, Visible, partitionNames.ToString());
        }
    }

}
