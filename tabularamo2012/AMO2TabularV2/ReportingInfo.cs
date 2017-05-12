/*=====================================================================
  
    File:      ReportingInfo.cs

    Summary:   ReportingInfo is a structure that holds all reporting 
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


namespace MicrosoftSql2012Samples.Amo2Tabular
{
    public struct ReportingInfo
    {
        bool? DefaultImage { get; set; }
        bool? DefaultLabel { get; set; }
        string ImageURL { get; set; }
        bool? KeepUniqueRows { get; set; }
        SummarizeBy SummarizeBy { get; set; }
        string TableDetailPosition { get; set; }
    }

}
