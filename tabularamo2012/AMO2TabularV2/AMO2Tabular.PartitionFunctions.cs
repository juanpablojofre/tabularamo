/*=====================================================================
  
    File:      AMO2Tabular.PartitionFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Partitions in a tabular model
 
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
using System.Collections.Generic;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        public static void PartitionAdd(AMO.Database tabularDatabase,
                                        string tableName,
                                        string partitionName,
                                        string selectStatement,
                                        bool updateInstance = true,
                                        AMO.ProcessType? processPartition = null,
                                        string alternateConnectionName = null)
        {
            //  Major steps in adding a Partition to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Defining Datasource to use
            //  - Adding Partition to Measure Group

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


            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (partitionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("partitionName");
            if (selectStatement.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("selectStatement");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            partitionName = partitionName.Trim();
            selectStatement = selectStatement.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain DataSourceId
            string dataSourceId;
            if (alternateConnectionName == null)
            {
                dataSourceId = tabularDatabase.DataSourceViews[0].DataSourceID;
            }
            else
            {
                dataSourceId = tabularDatabase.DataSources.GetByName(alternateConnectionName.Trim()).ID;
            }


            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName])
            using (AMO.Partition partition = new AMO.Partition(partitionName, partitionName))
            {
                partition.StorageMode = AMO.StorageMode.InMemory;
                partition.ProcessingMode = AMO.ProcessingMode.Regular;
                partition.Source = new AMO.QueryBinding(dataSourceId, selectStatement);
                partition.Type = AMO.PartitionType.Data;
                tableMeasureGroup.Partitions.Add(partition);
            }
            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

            //  Process default partition
            if (processPartition != null)
            {
                //  Throw exception if server instance is outdated and user requests process
                if (!updateInstance)
                    throw new InvalidOperationException(Resources.ProcessRequestedForOutdatedModelInvalidOperationException);

                //  Now the partition can be processed according to the user request
                PartitionProcess(tabularDatabase, tableName, partitionName, processPartition.Value);
            }

        }

        public static void PartitionDrop(AMO.Database tabularDatabase,
                                         string tableName,
                                         string partitionName,
                                         bool updateInstance = true)
        {
            //  Major steps in droping a Partition from a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Dropping Partition to Measure Group

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

            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (partitionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("partitionName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            partitionName = partitionName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;


            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName])
            using (AMO.Partition partition = tableMeasureGroup.Partitions.GetByName(partitionName))
            {
                tableMeasureGroup.Partitions.Remove(partition);
            }
            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void PartitionAlterName(AMO.Database tabularDatabase,
                                              string tableName,
                                              string oldPartitionName,
                                              string newPartitionName,
                                              bool updateInstance = true)
        {
            //  Major steps in renaming or AlteringName to a Partition in a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Renaming Partition in Measure Group

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

            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (oldPartitionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("oldPartitionName");
            if (newPartitionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("newPartitionName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            oldPartitionName = oldPartitionName.Trim();
            newPartitionName = newPartitionName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;


            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName])
            using (AMO.Partition partition = tableMeasureGroup.Partitions.GetByName(oldPartitionName))
            {
                partition.Name = newPartitionName;
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void PartitionAlterSelectStatement(AMO.Database tabularDatabase,
                                                         string tableName,
                                                         string partitionName,
                                                         string selectStatement,
                                                         bool updateInstance = true)
        {
            //  Major steps in updating or Altering the Select Statement of a Partition in a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Update Query Binding in Measure Group
            //
            //  Note:   This function only updates the select statement; does not change the DataSource or Connection
            //          of the partition
            //
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

            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (partitionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("partitionName");
            if (selectStatement.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("selectStatement");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            partitionName = partitionName.Trim();
            selectStatement = selectStatement.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;


            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName])
            using (AMO.Partition partition = tableMeasureGroup.Partitions.GetByName(partitionName))
            {
                AMO.QueryBinding qb = partition.Source as AMO.QueryBinding;
                partition.Source = new AMO.QueryBinding(qb.DataSourceID, selectStatement);
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void PartitionProcess(AMO.Database tabularDatabase,
                                            string tableName,
                                            string partitionName,
                                            AMO.ProcessType processValue)
        {
            //  Major steps in Processing a Partition in a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Process partition
            //
            //  Note:   There are no validations on the ProcessType requested and whatever value is passed it's used
            //
            //  Note:   For partitions, in tabular models, the following ProcessType values are 'valid' or have sense:
            //          -   ProcessDefault  ==> verifies if data (at partition level) or recalc is required and issues coresponding internal process tasks
            //          -   ProcessFull     ==> forces data upload (on given partition) and recalc, regardless of table status
            //          -   ProcessData     ==> forces data upload only (on given partition); does not issue an internal recalc process task
            //          -   ProcessClear    ==> clears all table data (on given partition)
            //
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

            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (partitionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("partitionName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            partitionName = partitionName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  Process
            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName])
            using (AMO.Partition partition = tableMeasureGroup.Partitions.GetByName(partitionName))
            {
                partition.Process(processValue);
            }
        }

        public static void PartitionAlterConnection(AMO.Database tabularDatabase,
                                                    string tableName,
                                                    string partitionName,
                                                    string connectionName,
                                                    bool updateInstance = true)
        {
            //  Major steps in updating or Altering the Connection or DataSource of a Partition in a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Update Query Binding in Measure Group
            //
            //  Note:   This function only updates the DataSource or Connection; does not change the select statement
            //          of the partition
            //
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

            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (partitionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("partitionName");
            if (connectionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("connectionName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            partitionName = partitionName.Trim();
            connectionName = connectionName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;
            string connectionId = tabularDatabase.DataSources.GetByName(connectionName).ID;

            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName])
            using (AMO.Partition partition = tableMeasureGroup.Partitions.GetByName(partitionName))
            {
                AMO.QueryBinding qb = partition.Source as AMO.QueryBinding;
                partition.Source = new AMO.QueryBinding(connectionId, qb.QueryDefinition);
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void PartitionAlterMerge(AMO.Database tabularDatabase,
                                               string tableName,
                                               string destinationPartitionName,
                                               bool updateInstance = true,
                                               params string[] sourcePartitionNames)
        {
            //  Major steps in Merging Partitions from a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Obtain source partitions Ids
            //  - Obtain destination partition
            //  - Merge source partitions into destination partition
            //  - Drop/delete source partitions (to eliminate duplication)

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

            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (destinationPartitionName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("destinationPartitionName");
            bool sourcePartitionNamesEmpty = true;
            foreach (string sourcePartitionName in sourcePartitionNames)
            {
                if (!sourcePartitionName.IsNullOrEmptyOrWhitespace())
                {
                    sourcePartitionNamesEmpty = false;
                    break;
                }
            }
            if (sourcePartitionNamesEmpty) throw new ArgumentNullException("sourcePartitionNames");

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            destinationPartitionName = destinationPartitionName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            //  -   Obtain source partitions Ids
            //  -   Obtain destination partition
            //  -   Merge source partitions into destination partition
            //  -   Drop/delete source partitions (to eliminate duplication)
            List<string> sourcePartitionIds = new List<string>(); ;
            List<AMO.Partition> sourcePartitions = new List<AMO.Partition>();
            using (AMO.MeasureGroup tableMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[datasourceTableName])
            using(AMO.Partition destinationPartition = tableMeasureGroup.Partitions.GetByName(destinationPartitionName))
            {
                foreach (string sourcePartitionName in sourcePartitionNames)
                {
                    if (!sourcePartitionName.IsNullOrEmptyOrWhitespace())
                    {
                        sourcePartitionIds.Add(tableMeasureGroup.Partitions.GetByName(sourcePartitionName).ID);
                        sourcePartitions.Add(tableMeasureGroup.Partitions.GetByName(sourcePartitionName));
                    }
                }
                destinationPartition.Merge(sourcePartitions);
                foreach (string sourcePartitionId in sourcePartitionIds)
                {
                    tableMeasureGroup.Partitions.Remove(sourcePartitionId, true);
                }
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        // *****************************************************************************


        public static void PartitionAddWithNewConnection(AMO.Database tabularDatabase,
                                                         string datasourceConnectionString,
                                                         string connectionName,
                                                         string tableName,
                                                         string partitionName,
                                                         string SelectStatement,
                                                         bool updateInstance = true,
                                                         AMO.ProcessType? processPartition = null,
                                                         string alternateConnectionName = null)
        {
            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

            //  Process default partition
            if (processPartition != null)
            {
                //  Throw exception if server instance is outdated and user requests process
                if (!updateInstance)
                    throw new InvalidOperationException(Resources.ProcessRequestedForOutdatedModelInvalidOperationException);

                //  Now the partition can be processed according to the user request
                PartitionProcess(tabularDatabase, tableName, partitionName, processPartition.Value);
            }

            const string functionName = "PartitionAddWithNewConnection";
            //
            //ToDo: Add function code
            //
            throw new NotImplementedException(string.Format(Resources.NotImplementedExceptionFunction, functionName));
        }

    }
}
