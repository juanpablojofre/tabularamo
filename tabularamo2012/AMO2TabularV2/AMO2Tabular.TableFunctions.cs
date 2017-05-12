/*=====================================================================
  
    File:      AMO2Tabular.TableFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Tables in a tabular model
 
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
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        /// <summary>
        /// Adds the first table to the tabular model; information for all
        /// columns is taken from the DSV table definition.
        ///  
        /// Subsequent tables should be added using TableAdd function.
        /// 
        /// As a consecuence of using TableAddFirstTable an AMO.Cube is created
        /// to store needed objects that are part of the infrastructure of a 
        /// tabular model. 
        /// </summary>
        /// <param name="tabularDatabase">A reference to an AMO database object</param>
        /// <param name="cubeName">A string with the name of the AMO.Cobe object to be created</param>
        /// <param name="datasourceTableName">A string with the name of the table from where the data will come to populate the destination table</param>
        /// <param name="tableName">A string with the name of the tabular table added</param>
        /// <param name="process">(optional) An AMO.ProcessType value to indicate the type of process required on the 'table' after creation</param>
        /// <param name="hidden">(optional) A Boolean value to indicate if the table should be hidden (true)or hidden(false) in client tools</param>
        /// <param name="defaultPartitionFilterClause">(optional) A boolean expression, in SQl language, that filters the rows for the default partition of the table</param>
        /// <param name="modelDateColumn">(optional) The name of a date type calculatedColumn to be set as the unique key in the table. Also, as a consequence of defining this calculatedColumn, the entire table becomes the Date table of the model</param>
        public static void TableAddFirstTable(AMO.Database tabularDatabase,
                                              string cubeName,
                                              string datasourceTableName,
                                              string tableName,
                                              bool updateInstance = true,
                                              AMO.ProcessType? process = null,
                                              bool? visible = null,
                                              string defaultPartitionFilterClause = null,
                                              string modelDateColumn = null)
        {
            //  Major steps in creating the first table in the database
            //  NOTE:   This function also creates the Cube object required for all tables
            //
            //  -   Validate required input arguments
            //      -   Verify there are no other cubes in the database; as part of the initial conditions validations
            //
            //  -   Create local copy of cube
            //  -   Create Table (use the TableAdd function to create the table)
            //
            //  Note:   There are no validations for duplicated names, invalid names or
            //          similar scenarios. It is expected the server will take care of them and
            //          throw exceptions on any invalid situation.
            //
            //  Note:   Only one DataSourceView is used in Tabular Models 
            //          ==> tabularDatabase.DataSourceViews[0] represents the DSV of the model
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
            //  Note:   For tables, in tabular models, the following ProcessType values are 'valid' or have sense:
            //          -   ProcessDefault  ==> verifies if a data (at partition level) or recalc is required and issues coresponding internal process tasks
            //          -   ProcessFull     ==> forces data upload (on all partitions) and recalc, regardless of table status
            //          -   ProcessData     ==> forces data upload only (on all partitions); does not issue an internal recalc process task
            //          -   ProcessClear    ==> clears all table data (on all partitions)
            //

            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (cubeName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("cubeName");
            if (datasourceTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("datasourceTableName");
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);

            //  Validate other intitial conditions: Verify there are no other cubes in the database
            if (tabularDatabase.Cubes.Count > 0) throw new InvalidOperationException(Resources.CubeAlreadyExistsInvalidOperationException);

            //  Create model cube
            using (AMO.Cube cube = tabularDatabase.Cubes.Add(cubeName, cubeName))
            {
                //  Create local copy of cube
                cube.Source = new AMO.DataSourceViewBinding(tabularDatabase.DataSourceViews[0].ID);
                cube.StorageMode = AMO.StorageMode.InMemory;
                //  
                //Create initial MdxScript
                //
                AMO.MdxScript mdxScript = cube.MdxScripts.Add(MdxScriptStringName, MdxScriptStringName);
                StringBuilder initialCommand = new StringBuilder();
                initialCommand.AppendLine("CALCULATE;");
                initialCommand.AppendLine("CREATE MEMBER CURRENTCUBE.Measures.[__No measures defined] AS 1, VISIBLE = 0;");
                initialCommand.AppendLine("ALTER CUBE CURRENTCUBE UPDATE DIMENSION Measures, Default_Member = [__No measures defined];");
                mdxScript.Commands.Add(new AMO.Command(initialCommand.ToString()));
            }
            //  Create table, but do not update server instance until comming back and procesing what the user requests in 'updateInstance'
            //  Note:   As a best practice every time the object model is altered a database update needs to be issued; however, in this case
            //          to avoid multiple database updates while creating one major object (ie, Table) we are invoking TableAdd with NO updateInstance
            TableAdd(tabularDatabase, datasourceTableName, tableName, false, null, visible, defaultPartitionFilterClause, modelDateColumn);

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

            //  Only after creating the table and updating the instance we can process it  
            if (process != null)
            {
                //  Throw exception if server instance is outdated and user requests process
                if (!updateInstance)
                    throw new InvalidOperationException(Resources.ProcessRequestedForOutdatedModelInvalidOperationException);

                //  Now the table can be processed according to the user request
                TableProcess(tabularDatabase, tableName, process.Value);

            }

        }

        /// <summary>
        /// Adds a table to the tabular model; information for all
        /// columns is taken from the DSV table definition.
        /// </summary>
        /// <param name="tabularDatabase">A reference to an AMO database object</param>
        /// <param name="datasourceTableName">A string with the name of the table from where the data will come to populate the destination table</param>
        /// <param name="tableName">A string with the name of the tabular table added</param>
        /// <param name="process">(optional) An AMO.ProcessType value to indicate the type of process required on the 'table' after creation</param>
        /// <param name="hidden">(optional) A Boolean value to indicate if the table should be hidden (true) or hidden(false) in client tools</param>
        /// <param name="defaultPartitionFilterClause">(optional) A boolean expression, in SQl language, that filters the rows for the default partition of the table</param>
        /// <param name="modelDateColumn">(optional) The name of a date type calculatedColumn to be set as the unique key in the table. Also, as a consequence of defining this calculatedColumn, the entire table becomes the Date table of the model</param>
        public static void TableAdd(AMO.Database tabularDatabase,
                                    string dataSourcetableName,
                                    string tableName,
                                    bool updateInstance = true,
                                    AMO.ProcessType? process = null,
                                    bool? visible = null,
                                    string defaultPartitionFilterClause = null,
                                    string modelDateColumn = null)
        {
            //  Table creation strategy:
            //  Because a table is a combination of a Dimension and a MeasureGroup
            //  there are different ways to create the table, here we have two
            //  possibilities
            //
            //  A   Create the entire Dimension object with attributes, add the 
            //      reference to the dimension in the cube, create the entire 
            //      MeasureGroup (associated to the dimension and attributes),
            //      add the default partition to the MeasureGroup, update Primary
            //      Key attribute from source table and, finally, process 
            //      partition (if user wants it).
            //      This sequence of steps implies adding all table columns to 
            //      the dimension and later iterate again over the same columns
            //      to add them to the 'degenerated' dimension of the MeasureGroup
            //
            //  B   Create an 'empty' Dimension object (with only the rownumber 
            //      attribute), add the reference to the dimension in the cube,
            //      create the MeasureGroup object (based on the 'empty'dimension),
            //      add the default partition, add all columns in the source 
            //      table to both dimension and degenerated dimension in 
            //      MeasureGroup, update Primary Key and, finally, process
            //      partition (if user wants it).
            //      This sequence of steps implies creating the skeleton 
            //      infrastructure of a table and later add all columns using 
            //      ColumnAdd function.
            //      This is the approach used to create a table in this sample
            //
            //  Major steps in creating a table in the database
            //
            //  - Validate required input arguments
            //      - Verify there is 1 and only 1 cube in the database; as part of the initial conditions validations
            //
            //  - Add 'empty' table to cube
            //  - Add columns from DSV source information
            //  - Update Primary Key
            //  - Update Date Table
            //  - Process default partition
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
            if (dataSourcetableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("datasourceTableName");
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);


            //  Validate other intitial conditions: Verify there is only one cube in the database
            if (tabularDatabase.Cubes.Count != 1) 
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidNumberOfCubesInvalidOperationException, tabularDatabase.Cubes.Count));

            //  Add 'empty' table to model
            //  Create empty table, but do not update server instance until comming back and procesing what the user requests in 'updateInstance'
            //  Note:   As a best practice every time the object model is altered a database update needs to be issued; however, in this case
            //          to avoid multiple database updates while creating one major object (ie, Table) we are invoking ColumnAdd with NO updateInstance
            TableAddEmptyTable(tabularDatabase, dataSourcetableName, tableName, defaultPartitionFilterClause, false);

            //  Add columns from DSV source information
            //  
            //  Note:   Only columns of supported data types, in tabular models, are added
            //          --> skipping unsupported data types
            foreach (DataColumn currentColumn in tabularDatabase.DataSourceViews[0].Schema.Tables[dataSourcetableName].Columns)
            {
                if (mapToSupportedTabularDataTypes(tabularDatabase.DataSourceViews[0].Schema.Tables[dataSourcetableName].Columns[currentColumn.ColumnName].DataType) != DataType.Unsupported)
                {
                    //  Create calculatedColumn, but do not update server instance until comming back and procesing what the user requests in 'updateInstance'
                    ColumnAdd(tabularDatabase, tableName, currentColumn.ColumnName, null, false);
                }
            }


            //  Update Date Table
            if (!modelDateColumn.IsNullOrEmptyOrWhitespace())
                //  Set Date Table, but do not update server instance until comming back and procesing what the user requests in 'updateInstance'
                TableAlterSetDateTable(tabularDatabase, tableName, modelDateColumn, false);

            //  Update table visibility
            if (visible != null)
                //  Set visibility, but do not update server instance until comming back and procesing what the user requests in 'updateInstance'
                TableAlterSetVisibility(tabularDatabase, tableName, visible.Value, false);


            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

            //  Only after creating the table and updating the instance we can process it  
            if (process != null)
            {
                //  Throw exception if server instance is outdated and user requests process
                if (!updateInstance)
                    throw new InvalidOperationException(Resources.ProcessRequestedForOutdatedModelInvalidOperationException);

                //  Now the table can be processed according to the user request
                TableProcess(tabularDatabase, tableName, process.Value);
            }

        }


        /// <summary>
        /// Adds an 'empty' table to the tabular model; no calculatedColumn informationis added
        /// </summary>
        /// <param name="tabularDatabase">A reference to an AMO database object</param>
        /// <param name="datasourceTableName">A string with the name of the table from where the data will come to populate the destination table</param>
        /// <param name="tableName">A string with the name of the tabular table added</param>
        public static void TableAddEmptyTable(AMO.Database tabularDatabase,
                                                string datasourceTableName,
                                                string tableName,
                                                string defaultPartitionFilterClause = null,
                                                bool updateInstance = true
                                                )
        {
            //  Table creation strategy:
            //  Because a table is a combination of a Dimension and a MeasureGroup
            //  there are different ways to create the table, here we have two
            //  possibilities
            //
            //  A   Create the entire Dimension object with attributes, add the 
            //      reference to the dimension in the cube, create the entire 
            //      MeasureGroup (associated to the dimension and attributes),
            //      add the default partition to the MeasureGroup, update Primary
            //      Key attribute from source table and, finally, process 
            //      partition (if user wants it).
            //      This sequence of steps implies adding all table columns to 
            //      the dimension and later iterate again over the same columns
            //      to add them to the 'degenerated' dimension of the MeasureGroup
            //
            //  B   Create an 'empty' Dimension object (with only the rownumber 
            //      attribute), add the reference to the dimension in the cube,
            //      create the MeasureGroup object (based on the 'empty'dimension),
            //      add the default partition, add all columns in the source 
            //      table to both dimension and degenerated dimension in 
            //      MeasureGroup, update Primary Key and, finally, process
            //      partition (if user wants it).
            //      This sequence of steps implies creating the skeleton 
            //      infrastructure of a table and later add all columns using 
            //      ColumnAdd function.
            //      This is the approach used to create a table in this sample
            //
            //  Major steps in creating a table in the database
            //
            //  - Validate required input arguments
            //      - Verify there is 1 and only 1 cube in the database; as part of the initial conditions validations
            //
            //  - Create empty local copy of Dimension object
            //  - Add Dimension reference to cube
            //  - Add empty MeasureGroup to cube
            //  - Adding default Measure to MeasureGroup
            //  - Add 'Dimension' to MeasureGroup
            //  - Add partition to MeasureGroup
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
            if (datasourceTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("datasourceTableName");
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);


            //  Validate other intitial conditions: Verify there is only one cube in the database
            if (tabularDatabase.Cubes.Count != 1) throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidNumberOfCubesInvalidOperationException, tabularDatabase.Cubes.Count));


            //  Create empty local copy of Dimension object in two (2) steps:
            //  -   Define Dimension general properties
            //  -   Manualy add "RowNumber" calculatedColumn
            //
            //  Define Dimension general properties
            string rowNumberColumnName = string.Format(CultureInfo.InvariantCulture, "RowNumber_{0}", Guid.NewGuid()); //  Making sure the RowNumber calculatedColumn has a unique name
            using (AMO.Dimension tableDimension = tabularDatabase.Dimensions.Add(tableName, datasourceTableName))
            {
                tableDimension.Source = new AMO.DataSourceViewBinding(tabularDatabase.DataSourceViews[0].ID);
                tableDimension.StorageMode = AMO.DimensionStorageMode.InMemory;
                tableDimension.UnknownMember = AMO.UnknownMemberBehavior.AutomaticNull;
                tableDimension.UnknownMemberName = "Unknown";
                tableDimension.ErrorConfiguration = new AMO.ErrorConfiguration();
                tableDimension.ErrorConfiguration.KeyNotFound = AMO.ErrorOption.IgnoreError;
                tableDimension.ErrorConfiguration.KeyDuplicate = AMO.ErrorOption.ReportAndStop;
                tableDimension.ErrorConfiguration.NullKeyNotAllowed = AMO.ErrorOption.ReportAndStop;
                tableDimension.ProactiveCaching = new AMO.ProactiveCaching();
                TimeSpan defaultProactiveChachingTimeSpan = new TimeSpan(0, 0, -1);
                tableDimension.ProactiveCaching.SilenceInterval = defaultProactiveChachingTimeSpan;
                tableDimension.ProactiveCaching.Latency = defaultProactiveChachingTimeSpan;
                tableDimension.ProactiveCaching.SilenceOverrideInterval = defaultProactiveChachingTimeSpan;
                tableDimension.ProactiveCaching.ForceRebuildInterval = defaultProactiveChachingTimeSpan;
                tableDimension.ProactiveCaching.Source = new AMO.ProactiveCachingInheritedBinding();

                //  Manualy add a "RowNumber" attribute as the key attribute of the dimension
                //  "RowNumber" is a required calculatedColumn for a tabular model and has to be of type AMO.AttributeType.RowNumber and binding AMO.RowNumberBinding
                //  The name of the "RowNumber" attribute can be any name, as long as type and binding are correctly set
                //  By default the MS client tools set the calculatedColumn name and calculatedColumn ID of the RowNumber attribute to 
                //  "RowNumber"; and, to "InternalRowNumber" if there is a collition with a user calculatedColumn named "RowNumber"
                //  In this sample, to avoid problems with any customer table that contains a calculatedColumn named "RowNumber" and/or "InternalRowNumber"
                //  the Name and Id value of the calculatedColumn (in the dimension object) will be renamed to 
                //  'RowNumber_<NewGuid()>';
                //  For that purpose the variable rowNumberColumnId was defined above

                using (AMO.DimensionAttribute rowNumber = tableDimension.Attributes.Add(rowNumberColumnName, rowNumberColumnName))
                {
                    rowNumber.Type = AMO.AttributeType.RowNumber;
                    rowNumber.KeyUniquenessGuarantee = true;
                    rowNumber.Usage = AMO.AttributeUsage.Key;
                    rowNumber.KeyColumns.Add(new AMO.DataItem());
                    rowNumber.KeyColumns[0].DataType = System.Data.OleDb.OleDbType.Integer;
                    rowNumber.KeyColumns[0].DataSize = 4;
                    rowNumber.KeyColumns[0].NullProcessing = AMO.NullProcessing.Error;
                    rowNumber.KeyColumns[0].Source = new AMO.RowNumberBinding();
                    rowNumber.NameColumn = new AMO.DataItem();
                    rowNumber.NameColumn.DataType = System.Data.OleDb.OleDbType.WChar;
                    rowNumber.NameColumn.DataSize = 4;
                    rowNumber.NameColumn.NullProcessing = AMO.NullProcessing.ZeroOrBlank;
                    rowNumber.NameColumn.Source = new AMO.RowNumberBinding();
                    rowNumber.OrderBy = AMO.OrderBy.Key;
                    rowNumber.AttributeHierarchyVisible = false;
                }
            }

            //  Add Dimension reference to cube
            tabularDatabase.Cubes[0].Dimensions.Add(datasourceTableName, tableName, datasourceTableName);

            //  Add empty MeasureGroup to cube
            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups.Add(tableName, datasourceTableName))
            {
                tableMeasureGroup.StorageMode = AMO.StorageMode.InMemory;
                tableMeasureGroup.ProcessingMode = AMO.ProcessingMode.Regular;


                //  Add default Measure to MeasureGroup
                string defaultMeasureID = string.Concat("_Count ", tableName);
                using (AMO.Measure defaultMeasure = tableMeasureGroup.Measures.Add(defaultMeasureID, defaultMeasureID))
                using (AMO.RowBinding defaultMeasureRowBinding = new AMO.RowBinding(datasourceTableName))
                using (AMO.DataItem defaultMeasureSource = new AMO.DataItem(defaultMeasureRowBinding))
                {
                    defaultMeasure.AggregateFunction = AMO.AggregationFunction.Count;
                    defaultMeasure.DataType = AMO.MeasureDataType.BigInt;
                    defaultMeasure.Visible = false;
                    defaultMeasureSource.DataType = System.Data.OleDb.OleDbType.BigInt;
                    defaultMeasure.Source = defaultMeasureSource;
                }


                //  Add 'Dimension' to MeasureGroup
                using (AMO.DegenerateMeasureGroupDimension defaultMGDim = new AMO.DegenerateMeasureGroupDimension(tableName))
                using (AMO.MeasureGroupAttribute mga = new AMO.MeasureGroupAttribute(rowNumberColumnName))
                using (AMO.ColumnBinding rowNumberColumnBinding = new AMO.ColumnBinding(datasourceTableName, rowNumberColumnName))
                using (AMO.DataItem rowNumberKeyColumn = new AMO.DataItem(rowNumberColumnBinding))
                {
                    defaultMGDim.ShareDimensionStorage = AMO.StorageSharingMode.Shared;
                    defaultMGDim.CubeDimensionID = datasourceTableName;
                    mga.Type = AMO.MeasureGroupAttributeType.Granularity;
                    rowNumberKeyColumn.DataType = System.Data.OleDb.OleDbType.Integer;
                    mga.KeyColumns.Add(rowNumberKeyColumn);
                    defaultMGDim.Attributes.Add(mga);
                    tableMeasureGroup.Dimensions.Add(defaultMGDim);
                }


                //  Add default partition to MeasureGroup
                StringBuilder partitionSqlStatement = new StringBuilder();
                partitionSqlStatement.Append("SELECT * FROM [");
                partitionSqlStatement.Append(datasourceTableName);
                partitionSqlStatement.Append("]");
                if (defaultPartitionFilterClause != null)
                {
                    partitionSqlStatement.Append(" WHERE ");
                    partitionSqlStatement.Append(defaultPartitionFilterClause);
                }

                //  Create partition, but do not update server instance until comming back and procesing what the user requests in 'updateInstance'
                //  Note:   As a best practice every time the object model is altered a database update needs to be issued; however, in this case
                //          to avoid multiple database updates while creating one major object (ie, Table) we are invoking PartitionAdd with NO updateInstance

                PartitionAdd(tabularDatabase, tableName, tableName, partitionSqlStatement.ToString(), false);
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

        }


        public static void TableProcess(AMO.Database tabularDatabase,
                                        string tableName,
                                        AMO.ProcessType process)
        {
            //  Major steps in processing a table in the database
            //
            //  - Validate required input arguments
            //  - Process Dimension (no need to process cube or MeasureGroup)
            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  
            //  Note:   There are no validations on the Process requested and whatever value is passed it's used
            //
            //  Note:   For tables, in tabular models, the following ProcessType values are 'valid' or have sense:
            //          -   ProcessDefault  ==> verifies if a data (at partition level) or recalc is required and issues coresponding internal process tasks
            //          -   ProcessFull     ==> forces data upload (on all partitions) and recalc, regardless of table status
            //          -   ProcessData     ==> forces data upload only (on all partitions); does not issue an internal recalc process task
            //          -   ProcessClear    ==> clears all table data (on all partitions)
            //


            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);


            //  Process Dimension object only
            tabularDatabase.Dimensions[tabularDatabase.Dimensions.GetByName(tableName).ID].Process(process);
        }

        public static void TableAlterSetDateTable(AMO.Database tabularDatabase,
                                                  string tableName,
                                                  string columnName,
                                                  bool updateInstance = true)
        {
            //  Major steps in setting the Date table in the database
            //
            //  - Validate required input arguments
            //  - Set Date Column to Primary Key
            //  - Set table dimension type to Time
            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //


            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);

            //   
            //  Set Date Column to Primary Key, but do not update server instance until comming back and procesing what the user requests in 'updateInstance'
            //  Note:   As a best practice every time the object model is altered a database update needs to be issued; however, in this case
            //          to avoid multiple database updates while creating one major object (ie, Table) we are invoking ColumnAlterSetPrimaryKey with NO updateInstance

            ColumnAlterSetPrimaryKey(tabularDatabase, tableName, columnName, false);

            //  Set table dimension type to Time
            tabularDatabase.Dimensions[tabularDatabase.Dimensions.GetByName(tableName).ID].Type = AMO.DimensionType.Time;

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

        }

        public static void TableAlterSetVisibility(AMO.Database tabularDatabase,
                                                   string tableName,
                                                   bool visible,
                                                   bool updateInstance = true)
        {
            //  Major steps in setting the visibility of table in the database
            //
            //  - Validate required input arguments
            //  - In the cube, set table dimension visible attribute
            //
            //  Note: There are no validations for duplicated names, invalid names or
            //  similar scenarios. It is expected the server will take care of them and
            //  throw exceptions on any invalid situation.
            //
            //  Note:   Only one Cube is used in Tabular Models 
            //          ==> tabularDatabase.Cubes[0] represents the cube in the model
            //  


            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);


            //  In the cube, set table dimension hidden attribute
            tabularDatabase.Cubes[0].Dimensions[tabularDatabase.Dimensions.GetByName(tableName).ID].Visible = visible;

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }


        public static string[] TablesEnumerate(AMO.Database tabularDatabase)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            #endregion

            return (from dimension in tabularDatabase.Dimensions.Cast<AMO.Dimension>()
                    select dimension.Name).ToArray();

        }

        public static TableInfo[] TablesEnumerateFull(AMO.Database tabularDatabase)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            #endregion

            return (from dimension in tabularDatabase.Dimensions.Cast<AMO.Dimension>()
                    join measureGroup in tabularDatabase.Cubes[0].MeasureGroups.Cast<AMO.MeasureGroup>() on dimension.ID equals measureGroup.ID
                    join cubeDimension in tabularDatabase.Cubes[0].Dimensions.Cast<AMO.CubeDimension>() on dimension.ID equals cubeDimension.ID
                    select new TableInfo(dimension.Name,
                                         dimension.ID,
                                         dimension.Description,
                                         cubeDimension.Visible,
                                         (measureGroup.Partitions.Cast<AMO.Partition>()
                                          .Select(partition => partition.Name)).ToList()
                                         )
                   ).ToArray();
        }

    }
}
