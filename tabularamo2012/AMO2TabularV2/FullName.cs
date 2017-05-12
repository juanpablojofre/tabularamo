/*=====================================================================
  
    File:      LevelInfo.cs

    Summary:   LevelInfo is a structure that holds level 
               information for a hierarchy. 

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
using System;
using System.Globalization;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public struct FullName: IComparable<FullName>
    {
        public string TableId {get; internal set;}
        public string ColumnId{ get; internal set; }

        public FullName(string tableId, string columnId)
            : this()
        {
            TableId = tableId;
            ColumnId = columnId;
        }

        int IComparable<FullName>.CompareTo(FullName fn)
        {
            return string.Compare(this.ToString(), fn.ToString(), StringComparison.InvariantCultureIgnoreCase); 
        }

        public override bool Equals(object fn)
        {
            return string.Compare(this.ToString(), fn.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(FullName fn1, FullName fn2)
        {
            return fn1.Equals(fn2);
        }

        public static bool operator !=(FullName fn1, FullName fn2)
        {
            return !fn1.Equals(fn2);
        }
        
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "'{0}'[{1}]", TableId, ColumnId);
        }

    }

}
