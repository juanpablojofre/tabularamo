/*=====================================================================
  
    File:      AMO2Tabular.CalculatedColumnFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate Calculated
               Columns in a tabular model
 
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
using System.Linq;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;


namespace MicrosoftSql2012Samples.Amo2Tabular
{
    //  Note:
    //      CalculatedColumn functionality like Drop and Alter 
    //      is done through regular ColumnDrop and ColumnAlter functions
    //      Hence: no need to implement that functionality here

    public static partial class AMO2Tabular
    {
        public static void CalculatedColumnAdd(AMO.Database tabularDatabase,
                                               string tableName,
                                               string columnName,
                                               string daxExpression,
                                               bool updateInstance = true,
                                               ColumnInfo? columnProperties = null,
                                               ReportingInfo? reportingProperties = null,
                                               AMO.ProcessType? processType = null)
        {
            //  Major steps in adding a calculated calculatedColumn to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding calculatedColumn as attribute to dimension
            //  - Adding calculatedColumn as attribute to degenerated dimension in Measure Group
            //  - Set calculatedColumn properties according to optional parameters
            //  - Set reporting properties according to optional parameters
            //  - Process table/database according to optional parameters

            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the Name of the object
            //
            //  Note:   Only one DataSourceView is used in Tabular Models 
            //          ==> tabularDatabase.DataSourceViews[0] represents the DSV of the model
            //  
            //  Note:   Only one Cube is used in Tabular Models 
            //          ==> tabularDatabase.Cubes[0] represents the cube in the model
            //
            //  Note:   Microsoft design tools use the following pattern to keep track of the
            //          datasource matching elements:
            //          DataSourceView->TableName <---> Dimension.ID, MeasureGroup.ID
            //          DataSourceView->ColumnName <---> Dimension->ColumnID, MeasureGroup.DegeneratedDimension->CoumnID
            //          So far, this sample follows the same pattern.
            //
            //          WARNING:    Breaking the above pattern when creating your 
            //                      own AMO to Tabular functions might lead to 
            //                      unpredictable behavior when using Microsoft
            //                      Design tools in your models.
            //
            //  Note:   There are no validations on the ProcessType requested and whatever value is passed it's used
            //
            //  Note:   For Calculated Columns, in tabular models, the following ProcessType values are 'valid' or have sense:
            //          -   ProcessDefault  ==> (issued at table level) verifies if a data (at partition level) or recalc is 
            //                                  required and issues coresponding internal process tasks
            //          -   ProcessFull     ==> (issued at table level) forces data upload (on all partitions) and recalc, 
            //                                  regardless of table status
            //          -   ProcessRecalc   ==> (issued at Database level) forces a recalc of internal structures (measures, 
            //                                  calculated columns, hierarchies, etc.) at database level; doesn't load 
            //                                  new data.
            //
            //  Note:   Issuing a process request (setting parameter processType != null) forces a database update

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (daxExpression.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(DaxExpressionStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new  InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            columnName = columnName.Trim();
            daxExpression = daxExpression.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain "RowNumber" column id
            string rowNumberColumnId = string.Empty;
            foreach (AMO.DimensionAttribute da in tabularDatabase.Dimensions[datasourceTableName].Attributes)
            {
                if (da.Type == AMO.AttributeType.RowNumber)
                {
                    rowNumberColumnId = da.ID;
                    break;
                }
            }

            #endregion

            // Add calculated calculatedColumn as attribute to the Dimension
            //      Note: datasourceTableName == tableId; because parity with DS object names needs to be kept
            using (AMO.Dimension tableDimension = tabularDatabase.Dimensions[datasourceTableName])
            using (AMO.DimensionAttribute calculatedColumnDimensionAttribute = tableDimension.Attributes.Add(columnName, columnName))
            using (AMO.DataItem dataItemEmptyType = new AMO.DataItem(datasourceTableName, columnName, System.Data.OleDb.OleDbType.Empty))
            using (AMO.ExpressionBinding expressionBinding = new AMO.ExpressionBinding(daxExpression))
            using (AMO.DataItem dataItemWCharType = new AMO.DataItem(datasourceTableName, columnName, System.Data.OleDb.OleDbType.WChar))
            {
                calculatedColumnDimensionAttribute.Usage = AMO.AttributeUsage.Regular;
                calculatedColumnDimensionAttribute.KeyUniquenessGuarantee = false;
                calculatedColumnDimensionAttribute.KeyColumns.Add(dataItemEmptyType);
                calculatedColumnDimensionAttribute.KeyColumns[0].Source = expressionBinding.Clone();
                calculatedColumnDimensionAttribute.KeyColumns[0].NullProcessing = AMO.NullProcessing.Preserve;
                calculatedColumnDimensionAttribute.NameColumn = dataItemWCharType;
                calculatedColumnDimensionAttribute.NameColumn.Source = expressionBinding;
                calculatedColumnDimensionAttribute.NameColumn.NullProcessing = AMO.NullProcessing.ZeroOrBlank;
                calculatedColumnDimensionAttribute.OrderBy = AMO.OrderBy.Key;
                using (AMO.AttributeRelationship calculatedColumnDimensionAttributeRelationship = tableDimension.Attributes[rowNumberColumnId].AttributeRelationships.Add(calculatedColumnDimensionAttribute.ID))
                {
                    calculatedColumnDimensionAttributeRelationship.Cardinality = AMO.Cardinality.Many;
                    calculatedColumnDimensionAttributeRelationship.OverrideBehavior = AMO.OverrideBehavior.None;
                }
            }
            //  Add calculatedColumn as attribute to the MG, in the DegeneratedMeasureGroupDimension
            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName])
            using (AMO.DegenerateMeasureGroupDimension tableMGDimension = (AMO.DegenerateMeasureGroupDimension)tableMeasureGroup.Dimensions[datasourceTableName])
            using (AMO.MeasureGroupAttribute calculatedColumnMGAttribute = new AMO.MeasureGroupAttribute(columnName))
            using (AMO.DataItem dataItemEmptyType = new AMO.DataItem(datasourceTableName, columnName, System.Data.OleDb.OleDbType.Empty))
            using (AMO.ExpressionBinding expressionBinding = new AMO.ExpressionBinding(daxExpression))
            {
                calculatedColumnMGAttribute.KeyColumns.Add(dataItemEmptyType);
                calculatedColumnMGAttribute.KeyColumns[0].Source = expressionBinding;
                tableMGDimension.Attributes.Add(calculatedColumnMGAttribute);
            }

            //  Set/update optional calculatedColumn properties
            if (columnProperties != null)
            {
                
                if (!columnProperties.Value.DataFormat.IsNullOrEmptyOrWhitespace())
                    ColumnAlterFormat(tabularDatabase, tableName, columnName, columnProperties.Value.DataFormat);

                if (columnProperties.Value.DataType != DataType.Default && columnProperties.Value.DataType != DataType.Unsupported)
                    ColumnAlterDataType(tabularDatabase, tableName, columnName, columnProperties.Value.DataType);

                if (columnProperties.Value.Visible != null)
                    ColumnAlterVisibility(tabularDatabase, tableName, columnName, columnProperties.Value.Visible.Value);

                if (!columnProperties.Value.SortByColumn.IsNullOrEmptyOrWhitespace())
                    ColumnAlterSortByColumnName(tabularDatabase, tableName, columnName, columnProperties.Value.SortByColumn);

            }

            //  ToDo: Set/update optional reporting properties
            //if (reportingProperties != null)
            //{
            //}

            //  Update server instance
            if(updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);


            if (processType != null)
            {
                //  Throw exception if server instance is outdated and user requests process
                if (!updateInstance)
                    throw new InvalidOperationException(Resources.ProcessRequestedForOutdatedModelInvalidOperationException);


                //  Now the table, that contains the Calculated Column, can be processed according to the user request
                TableProcess(tabularDatabase, tableName, (AMO.ProcessType)processType);

                //  Calculated columns require a database level process recalc
                tabularDatabase.Process(AMO.ProcessType.ProcessRecalc);
            }

        }

        public static string[] CalculatedColumnsEnumerate(AMO.Database tabularDatabase, string tableName)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();

            #endregion

            return (from calculatedColumn in tabularDatabase.Dimensions.GetByName(tableName).Attributes.Cast<AMO.DimensionAttribute>()
                    where (calculatedColumn.NameColumn.Source is AMO.ExpressionBinding)
                    select calculatedColumn.Name).ToArray();

        }


    }
}
