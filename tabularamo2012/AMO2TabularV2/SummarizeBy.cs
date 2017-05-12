/*=====================================================================
  
    File:      SummarizeBy.cs

    Summary:   SummarizeBy is an enumeration used in AMO2Tabular to 
               represent all available summarizing options under
               the Reporting Properties of a calculatedColumn

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


namespace MicrosoftSql2012Samples.Amo2Tabular
{
    public enum SummarizeBy
    {
        Default=0,
        Sum,
        Max,
        Min,
        Average,
        Count,
        DistinctCount,
        DoNotSummarize
    }

}
