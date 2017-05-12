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

    public struct LevelInfo
    {
        public string DefiningLevelColumnName {get; internal set;}
        public string LevelName{ get; internal set; }

        public LevelInfo(string definingLevelColumnName, string levelName)
            : this()
        {
            DefiningLevelColumnName = definingLevelColumnName;
            LevelName = levelName;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}\t{1}", DefiningLevelColumnName, LevelName);
        }

    }

}
