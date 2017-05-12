/*=====================================================================
  
    File:      AMO2Tabular.ColumnFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
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
using System.Globalization;
using System.Linq;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;




namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        //  For images, there is a pre-defined max size: 0x4000000
        //  --> 2^27 or 67,108,864 (bytes)
        public const int tabularImageMaxSize = 0x4000000;

        //  For strings, there is a pre-defined max size: 0x20000
        //  --> 2^17 or 131,072 (bytes)
        public const int tabularStringMaxSize = 0x20000;



        public static void ColumnAdd(AMO.Database tabularDatabase,
                                     string tableName,
                                     string datasourceColumnName,
                                     string columnName = null,
                                     bool updateInstance = true,
                                     ColumnInfo? columnProperties = null,
                                     ReportingInfo? reportingProperties = null)
        {
            //  Major steps in adding a Column to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding Column as attribute to dimension
            //  - Adding Column as attribute to degenerated dimension in Measure Group
            //  - Set Column properties according to optional parameters

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

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (datasourceColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(DatasourceColumStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            datasourceColumnName = datasourceColumnName.Trim();
            tableName = tableName.Trim();
            columnName = columnName.IsNullOrEmptyOrWhitespace() ? datasourceColumnName : columnName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain "RowNumber" Column Id
            string rowNumberColumnId = string.Empty;
            foreach (AMO.DimensionAttribute da in tabularDatabase.Dimensions[datasourceTableName].Attributes)
            {
                if (da.Type == AMO.AttributeType.RowNumber)
                {
                    rowNumberColumnId = da.ID;
                    break;
                }
            }

            //  -   Before attempting to add Column, to table, verify the data type is supported
            //      ==> Obtain OleDbType type equivalent from dsv Column data type
            System.Data.OleDb.OleDbType columnDataType;

            if (columnProperties != null && columnProperties.Value.DataType != DataType.Default && columnProperties.Value.DataType != DataType.Unsupported)
            {
                //  Accept user input
                columnDataType = (System.Data.OleDb.OleDbType)columnProperties.Value.DataType;
            }
            else
            {
                DataType d = mapToSupportedTabularDataTypes(tabularDatabase.DataSourceViews[0].Schema.Tables[datasourceTableName].Columns[datasourceColumnName].DataType);
                if (d != DataType.Unsupported)
                {
                    columnDataType = (System.Data.OleDb.OleDbType)d;
                }
                else
                {
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Resources.DataTypeNotSupportedException, tabularDatabase.DataSourceViews[0].Schema.Tables[datasourceTableName].Columns[datasourceColumnName].DataType.FullName));
                }
            }
            #endregion

            //  Adding Column as attribute to dimension
            //      Note: datasourceTableName == tableId; because parity with DS object names needs to be kept
            using (AMO.DimensionAttribute columnToAttributeName = tabularDatabase.Dimensions[datasourceTableName].Attributes.Add(columnName, datasourceColumnName))
            using (AMO.DataItem keyColumn = new AMO.DataItem(datasourceTableName, datasourceColumnName, columnDataType))
            using (AMO.DataItem nameColumn = new AMO.DataItem(datasourceTableName, datasourceColumnName, columnDataType == System.Data.OleDb.OleDbType.Binary ? System.Data.OleDb.OleDbType.Binary : System.Data.OleDb.OleDbType.WChar))
            using (AMO.ColumnBinding columnBinding = new AMO.ColumnBinding(datasourceTableName, datasourceColumnName))
            {
                //  Define attribute (Column properties)
                columnToAttributeName.Usage = AMO.AttributeUsage.Regular;
                columnToAttributeName.OrderBy = AMO.OrderBy.Key;

                //  For images, there is a pre-defined max size: tabularImageMaxSize
                if (columnDataType == System.Data.OleDb.OleDbType.Binary)
                {
                    keyColumn.DataSize = tabularImageMaxSize;
                    nameColumn.DataSize = tabularImageMaxSize;
                }

                //  For strings, there is a pre-defined max size: tabularStringMaxSize
                if (columnDataType == System.Data.OleDb.OleDbType.WChar)
                {
                    keyColumn.DataSize = tabularStringMaxSize;
                    nameColumn.DataSize = tabularStringMaxSize;
                }
                keyColumn.Source = columnBinding.Clone();
                nameColumn.Source = columnBinding;

                //  Establish relationship to "RowNumber" Column
                //  -   If attribute is PrimaryKey/AlternateUniqueKey set Cardinality to One
                //  -   else, set Cardinality to Many
                AMO.AttributeRelationship currentAttributeRelationship = tabularDatabase.Dimensions[datasourceTableName].Attributes[rowNumberColumnId].AttributeRelationships.Add(datasourceColumnName);
                currentAttributeRelationship.OverrideBehavior = AMO.OverrideBehavior.None;
                if (IsUnique(tabularDatabase.DataSourceViews[0].Schema, datasourceTableName, datasourceColumnName))
                {
                    currentAttributeRelationship.Cardinality = AMO.Cardinality.One;
                    keyColumn.NullProcessing = AMO.NullProcessing.Error;
                    columnToAttributeName.KeyUniquenessGuarantee = true;
                }
                else
                {
                    currentAttributeRelationship.Cardinality = AMO.Cardinality.Many;
                    keyColumn.NullProcessing = AMO.NullProcessing.Preserve;
                    columnToAttributeName.KeyUniquenessGuarantee = false;
                }

                //  Update dimension attribute with collected information on keyColumn and nameColumn
                columnToAttributeName.NameColumn = nameColumn;
                columnToAttributeName.KeyColumns.Add(keyColumn);

            }


            //  Adding Column as attribute to degenerated dimension in Measure Group
            using (AMO.DegenerateMeasureGroupDimension defaultMGDim = (AMO.DegenerateMeasureGroupDimension)tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName].Dimensions[datasourceTableName])
            using (AMO.MeasureGroupAttribute mga = new AMO.MeasureGroupAttribute(datasourceColumnName))
            using (AMO.DataItem keyColumn = new AMO.DataItem(datasourceTableName, datasourceColumnName, columnDataType))
            {
                //  Set NullProcessing to error for Unique columns
                if (IsUnique(tabularDatabase.DataSourceViews[0].Schema, datasourceTableName, datasourceColumnName))
                {
                    keyColumn.NullProcessing = AMO.NullProcessing.Error;
                }
                else
                {
                    keyColumn.NullProcessing = AMO.NullProcessing.Preserve;
                }

                //  Set image size to predefined max size: tabularImageMaxSize
                if (columnDataType == System.Data.OleDb.OleDbType.Binary)
                {
                    keyColumn.DataSize = tabularImageMaxSize;
                }

                //  Set string size to predefined max size
                if (columnDataType == System.Data.OleDb.OleDbType.WChar)
                {
                    keyColumn.DataSize = tabularStringMaxSize;
                }


                //  Update MG attribute
                mga.KeyColumns.Add(keyColumn);
                defaultMGDim.Attributes.Add(mga);
            }

            //  [Codeplex issue # 6]
            //  [JPJofre, 2012-10-18]
            //  [Description: ColumnAdd() missing columnProperties section as in CalculatedColumnAdd()]
            //  [Suggested fix: Include code from CalculatedColumnAdd()]


            //  Set/update optional Column properties
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

            //ToDo: set optional reporting properties: reportingProperties


            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

        }

        public static void ColumnDrop(AMO.Database tabularDatabase,
                                     string tableName,
                                     string columnName,
                                     bool updateInstance = true)
        {
            //  Major steps in removing a Column from a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Remove Column from degenerated dimension in Measure Group
            //  - Remove Column from dimension

            //
            //  Note: There are no validations for invalid names or
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

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            columnName = columnName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain Column ID
            string datasourceColumnName = tabularDatabase.Dimensions[datasourceTableName].Attributes.GetByName(columnName).ID;

            //  -   Validate Column to remove is not the RowNumber Column
            if (tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].Type == AMO.AttributeType.RowNumber)
                throw new InvalidOperationException(Resources.CannotRemoveRowNumberColumnInvalidOperationException);
            #endregion


            //  Removing Column, as attribute, from degenerated dimension in Measure Group
            ((AMO.DegenerateMeasureGroupDimension)tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName].Dimensions[datasourceTableName]).Attributes.Remove(datasourceColumnName);


            //  Removing Column, as attribute, from dimension
            tabularDatabase.Dimensions[datasourceTableName].Attributes.Remove(datasourceColumnName);


            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void ColumnAlterSetPrimaryKey(AMO.Database tabularDatabase,
                                                    string tableName,
                                                    string columnName,
                                                    bool updateInstance = true)
        {
            //  Major steps in setting the Primary Key of a table in the database
            //
            //  - Validate required input arguments and other initial preparations
            //  - Set One:One relationship between RowNumber Column and PrimaryKey/AlternateUniqueKey attribute
            //  - Set NullProcessing to Error to avoid null values in Primary Key Column
            //
            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
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


            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            columnName = columnName.Trim();

            //  -   Obtain table and column name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;
            string datasourceColumnName = tabularDatabase.Dimensions[datasourceTableName].Attributes[columnName].ID;


            //  Verify columnName is a unique constraint (either Unique or Primary Key)

            if (!IsUnique(tabularDatabase.DataSourceViews[0].Schema, datasourceTableName, datasourceColumnName)) 
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.DateColumnIsNotUniqueInvalidOperationException, columnName));

            //  -   Obtain "RowNumber" Column Id
            string rowNumberColumnId = string.Empty;
            foreach (AMO.DimensionAttribute da in tabularDatabase.Dimensions[datasourceTableName].Attributes)
            {
                if (da.Type == AMO.AttributeType.RowNumber)
                {
                    rowNumberColumnId = da.ID;
                    break;
                }
            }


            //  [Codeplex issue # 4]
            //  [JPJofre, 2012-10-18]
            //  [Description: ColumnAlterSetPrimaryKey() doesn't verify 'columnName' is different than rowNumber column name]
            //  [Suggested fix: verify 'columnName' is different than rowNumber column name and throw an exception if they are equal]

            if (0 == string.Compare(rowNumberColumnId, columnName, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException(ColumnStringName);

            //  Set One:One relationship between RowNumber Column and PrimaryKey/AlternateUniqueKey attribute
            tabularDatabase.Dimensions[datasourceTableName].Attributes[rowNumberColumnId].AttributeRelationships[columnName].Cardinality = AMO.Cardinality.One;

            //  Set Attribute KeyColumn NullProcessing to error, to avoid null values in keys
            //
            //  Note: Both, dimension and measure group, objects are updated
            tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].KeyColumns[0].NullProcessing = AMO.NullProcessing.Error;
            tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].KeyUniquenessGuarantee = true;
            ((AMO.DegenerateMeasureGroupDimension)tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName].Dimensions[datasourceTableName]).Attributes[datasourceColumnName].KeyColumns[0].NullProcessing = AMO.NullProcessing.Error;

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void ColumnAlterDataType(AMO.Database tabularDatabase,
                                               string tableName,
                                               string columnName,
                                               DataType dataType,
                                               bool updateInstance = true)
        {
            //  Major steps in altering a Column data type in a table from the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Update Column data type in degenerated dimension in Measure Group
            //  - Update Column data type in dimension

            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
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

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            columnName = columnName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain Column ID
            string datasourceColumnName = tabularDatabase.Dimensions[datasourceTableName].Attributes.GetByName(columnName).ID;

            //  -   Validate Column to update is not the RowNumber Column
            if (tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].Type == AMO.AttributeType.RowNumber)
                throw new InvalidOperationException(Resources.CannotUpdateModifyRowNumberColumnInvalidOperationException);

            //  -   Before attempting to update Column data type verify the data type is supported
            System.Data.OleDb.OleDbType columnDataType;

            if (dataType != DataType.Default && dataType != DataType.Unsupported)
            {
                //  Accept user input
                columnDataType = (System.Data.OleDb.OleDbType)dataType;
            }
            else
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Resources.DataTypeNotSupportedException, tabularDatabase.DataSourceViews[0].Schema.Tables[datasourceTableName].Columns[datasourceColumnName].DataType.FullName));
            }

            #endregion


            //  Update Column data type, as attribute data type, in degenerated dimension in Measure Group
            using (AMO.MeasureGroupAttribute mga = ((AMO.DegenerateMeasureGroupDimension)tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName].Dimensions[datasourceTableName]).Attributes[datasourceColumnName])
            using (AMO.DataItem keyColumn = mga.KeyColumns[0])
            {
                keyColumn.DataType = columnDataType;
                //  Set image size to predefined max size
                if (columnDataType == System.Data.OleDb.OleDbType.Binary)
                {
                    keyColumn.DataSize = tabularImageMaxSize;
                }

                //  Set string size to predefined max size
                if (columnDataType == System.Data.OleDb.OleDbType.WChar)
                {
                    keyColumn.DataSize = tabularStringMaxSize;
                }
            }

            //  Update Column data type, as attribute data type, in dimension
            using (AMO.DimensionAttribute da = tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName])
            using (AMO.DataItem keyColumn = da.KeyColumns[0])
            using (AMO.DataItem nameColumn = da.NameColumn)
            {
                keyColumn.DataType = nameColumn.DataType = columnDataType;
                //  Set image size to predefined max size
                if (columnDataType == System.Data.OleDb.OleDbType.Binary)
                {
                    keyColumn.DataSize = nameColumn.DataSize = tabularImageMaxSize;
                }

                //  Set string size to predefined max size
                if (columnDataType == System.Data.OleDb.OleDbType.WChar)
                {
                    keyColumn.DataSize = nameColumn.DataSize = tabularStringMaxSize;
                }
            }


            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void ColumnAlterColumnName(AMO.Database tabularDatabase,
                                                 string tableName,
                                                 string oldColumnName,
                                                 string newColumnName,
                                                 bool updateInstance = true)
        {
            //  Major steps renaming a Column in a table from the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Update Column name in dimension

            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
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

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (oldColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("oldColumnName");
            if (newColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("newColumnName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            oldColumnName = oldColumnName.Trim();
            newColumnName = newColumnName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain Column ID
            string datasourceColumnName = tabularDatabase.Dimensions[datasourceTableName].Attributes.GetByName(oldColumnName).ID;

            //  -   Validate Column to update is not the RowNumber Column
            if (tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].Type == AMO.AttributeType.RowNumber)
                throw new InvalidOperationException(Resources.CannotUpdateModifyRowNumberColumnInvalidOperationException);
            #endregion


            //  Update Column name, as attribute name, in dimension
            tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].Name = newColumnName;

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void ColumnAlterVisibility(AMO.Database tabularDatabase,
                                                 string tableName,
                                                 string columnName,
                                                 bool visible,
                                                 bool updateInstance = true)
        {
            //  Major steps in altering the Column visibility in a table from the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Update Column visibility in dimension

            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
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

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            columnName = columnName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain Column ID
            string datasourceColumnName = tabularDatabase.Dimensions[datasourceTableName].Attributes.GetByName(columnName).ID;

            //  -   Validate Column to update is not the RowNumber Column
            if (tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].Type == AMO.AttributeType.RowNumber)
                throw new InvalidOperationException(Resources.CannotUpdateModifyRowNumberColumnInvalidOperationException);
            #endregion


            //  Update Column visibility, as attribute AttributeHierarchyVisible, in dimension
            tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].AttributeHierarchyVisible = visible;

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void ColumnAlterFormat(AMO.Database tabularDatabase,
                                             string tableName,
                                             string columnName,
                                             string format,
                                             bool updateInstance = true)
        {
            //  Major steps in altering the Column format in a table from the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Update Column format in dimension

            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
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

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Explicitly, we don not validate if format is empty... this allows the user to clear the format string
            if (format == null) throw new ArgumentNullException("format");

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            columnName = columnName.Trim();
            format = format.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain Column ID
            string datasourceColumnName = tabularDatabase.Dimensions[datasourceTableName].Attributes.GetByName(columnName).ID;

            //  -   Validate Column to update is not the RowNumber Column
            if (tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].Type == AMO.AttributeType.RowNumber)
                throw new InvalidOperationException(Resources.CannotUpdateModifyRowNumberColumnInvalidOperationException);
            #endregion


            //  Update Column format, as attribute format, in dimension
            tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].FormatString = format;

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void ColumnAlterSortByColumnName(AMO.Database tabularDatabase,
                                                       string tableName,
                                                       string columnName,
                                                       string sortByColumnName,
                                                       bool updateInstance = true)
        {
            //  Major steps in altering the Column sort by Column in a table from the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Update Column sort by Column in dimension
            //  Note:   if the user pass an empty string 
            //          ==> we understand the user wants to remove the sort by Column attribute and revert to sort by table key

            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //
            //  Note:   Microsoft design tools use the following pattern to keep track of the
            //          datasource matching elements:
            //          DataSourceView->TableName <---> Dimension.ID, MeasureGroup.ID
            //          DataSourceView->ColumnName <---> Dimension->ColumnID, MeasureGroup.DegeneratedDimension->ColumnID
            //          So far, this sample follows the same pattern.
            //
            //          WARNING:    Breaking the above pattern when creating your 
            //                      own AMO to Tabular functions might lead to 
            //                      unpredictable behavior when using Microsoft
            //                      Design tools in your models.

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Explicitly, we don not validate if sortByColumnName is empty or blank... this allows the user to clear the sortByColumnName
            if (sortByColumnName == null) throw new ArgumentNullException("sortByColumnName");

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            columnName = columnName.Trim();
            sortByColumnName = sortByColumnName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain Column ID
            string datasourceColumnName = tabularDatabase.Dimensions[datasourceTableName].Attributes.GetByName(columnName).ID;
            string datasourceSortByColumnName = tabularDatabase.Dimensions[datasourceTableName].Attributes.GetByName(sortByColumnName).ID;

            //  -   Validate Column to update is not the RowNumber Column
            if (tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].Type == AMO.AttributeType.RowNumber)
                throw new InvalidOperationException(Resources.CannotUpdateModifyRowNumberColumnInvalidOperationException);
            #endregion


            //  Update Column sort by Column in dimension
            //  Note:   if the user pass an empty string 
            //          ==> the user wants to remove the sort by Column attribute and revert to sort by table key
            if (!sortByColumnName.IsNullOrEmptyOrWhitespace())
            {
                tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].OrderBy = AMO.OrderBy.AttributeKey;
                tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].OrderByAttributeID = datasourceSortByColumnName;
            }
            else
            {
                tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].OrderBy = AMO.OrderBy.Key;
                tabularDatabase.Dimensions[datasourceTableName].Attributes[datasourceColumnName].OrderByAttributeID = string.Empty;
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static bool ColumnExist(AMO.Database tabularDatabase,
                                        string tableName,
                                        string columnName)
        {
            //  Major steps in determinint if a column exists
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Check if column exists through Attributes.ContainsName(columnName)
            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
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

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            columnName = columnName.Trim();

            #endregion

            return tabularDatabase.Dimensions.GetByName(tableName).Attributes.ContainsName(columnName);
        }
        
        public static string[] ColumnsEnumerate(AMO.Database tabularDatabase, string tableName)
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

            return (from column in tabularDatabase.Dimensions.GetByName(tableName).Attributes.Cast<AMO.DimensionAttribute>()
                    where !(column.NameColumn.Source is AMO.ExpressionBinding)
                    select column.Name).ToArray();

        }



    }
}
