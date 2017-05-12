/*=====================================================================
  
    File:      AMO2Tabular.PerspectiveFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Perspective in a tabular model
 
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
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        public static void PerspectiveAdd(AMO.Database tabularDatabase,
                                          string perspectiveName,
                                          bool updateInstance = true)
        {
            //  Major steps in adding a Perspective to the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding Perspective to 'Database'; in this case the database is
            //    represented by the cube. The different views (perspectives) of
            //    the database are stored in, the whole or universe, the cube

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
            if (perspectiveName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(PerspectiveStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            perspectiveName = perspectiveName.Trim();
            #endregion

            //  Add Perspective
            using (AMO.Perspective perspective = new AMO.Perspective(perspectiveName, perspectiveName))
            {
                tabularDatabase.Cubes[0].Perspectives.Add(perspective);
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void PerspectiveAlterTableAdd(AMO.Database tabularDatabase,
                                                    string perspectiveName,
                                                    string tableName,
                                                    bool updateInstance = true)
        {
            //  Major steps in adding a Table to a Perspective in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding Table to perspective
            //  - Adding columns to table; if column already added, skip it.
            //  - Adding calculated columns; skip any already added
            //  - Adding measures; skip any already added
            //  - Adding hierarchies; skip any already added

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
            if (perspectiveName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(PerspectiveStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            perspectiveName = perspectiveName.Trim();
            tableName = tableName.Trim();
            #endregion

            //  Add table to Perspective
            using (AMO.Perspective perspective = tabularDatabase.Cubes[0].Perspectives.GetByName(perspectiveName))
            {
                perspective.Dimensions.Add(tabularDatabase.Cubes[0].Dimensions.GetByName(tableName).ID);
            }



            //  Add table elements to Perspective
            //  Adding columns
            foreach (string columnName in ColumnsEnumerate(tabularDatabase, tableName))
            {
                if (!PerspectiveContainsColumn(tabularDatabase, perspectiveName, tableName, columnName))
                    PerspectiveAlterColumnAdd(tabularDatabase, perspectiveName, tableName, columnName, false);
            }

            //  Adding calculated columns
            foreach (string calculatedColumnName in CalculatedColumnsEnumerate(tabularDatabase, tableName))
            {
                if (!PerspectiveContainsColumn(tabularDatabase, perspectiveName, tableName, calculatedColumnName))
                    PerspectiveAlterColumnAdd(tabularDatabase, perspectiveName, tableName, calculatedColumnName, false);
            }

            //  Adding measures (also adds KPIs, as KPIs are promoted measures)
            foreach (string measureName in MeasuresEnumerate(tabularDatabase, tableName))
            {
                if (!PerspectiveContainsMeasure(tabularDatabase, perspectiveName, measureName))
                    PerspectiveAlterMeasureAdd(tabularDatabase, perspectiveName, tableName, measureName, false);
            }

            //  Adding hierarchies
            foreach (string hierarchyName in HierarchiesEnumerate(tabularDatabase, tableName))
            {
                if (!PerspectiveContainsHierarchy(tabularDatabase, perspectiveName, tableName, hierarchyName))
                    PerspectiveAlterHierarchyAdd(tabularDatabase, perspectiveName, tableName, hierarchyName, false);
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void PerspectiveAlterColumnAdd(AMO.Database tabularDatabase,
                                                     string perspectiveName,
                                                     string tableName,
                                                     string columnName,
                                                     bool updateInstance = true)
        {
            //  Major steps in adding a Column to a Perspective in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding column of table to perspective
            //      Note:   There is no need for a different function to add
            //              Calculated Columns to a perspective; since calculated columns
            //              are enumerated as columns.
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
            if (perspectiveName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(PerspectiveStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            perspectiveName = perspectiveName.Trim();
            tableName = tableName.Trim();
            columnName = columnName.Trim();
            #endregion

            using (AMO.Dimension tableDimension = tabularDatabase.Dimensions.GetByName(tableName))
            {
                string tableId = tableDimension.ID;
                string columnId = tableDimension.Attributes.GetByName(columnName).ID;

                using (AMO.Perspective perspective = tabularDatabase.Cubes[0].Perspectives.GetByName(perspectiveName))
                using (AMO.PerspectiveDimension perspectiveTable = perspective.Dimensions[tableId])
                {
                    perspectiveTable.Attributes.Add(columnId);
                }
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void PerspectiveAlterMeasureAdd(AMO.Database tabularDatabase,
                                                      string perspectiveName,
                                                      string tableName,
                                                      string measureName,
                                                      bool updateInstance = true)
        {
            //  Major steps in adding a Measure to a Perspective in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding measure to perspective
            //      Note:   A Measure is added to a perspective without its related 'table'.
            //              Measures are added to the perspective as part of the Calculations collection
            //              of the perspective
            //      Note:   At the same time if the measure has been promoted to KPI, the corresponding KPI
            //              is added to the KPIs collection of the perspective
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

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (perspectiveName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(PerspectiveStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (measureName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(MeasureStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            perspectiveName = perspectiveName.Trim();
            tableName = tableName.Trim();
            measureName = measureName.Trim();
            #endregion


            using (AMO.Perspective perspective = tabularDatabase.Cubes[0].Perspectives.GetByName(perspectiveName))
            using (AMO.PerspectiveCalculation measureCalculation = perspective.Calculations.Add(measureName, AMO.PerspectiveCalculationType.Member))
            {
                if (KpiExist(tabularDatabase, tableName, measureName))
                {
                    perspective.Kpis.Add(measureName);
                }
            }


            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void PerspectiveAlterHierarchyAdd(AMO.Database tabularDatabase,
                                                        string perspectiveName,
                                                        string tableName,
                                                        string hierarchyName,
                                                        bool updateInstance = true)
        {
            //  Major steps in adding a Hierarchy to a Perspective in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding hierarchy of table to perspective
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
            if (perspectiveName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(PerspectiveStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (hierarchyName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(HierarchyStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            perspectiveName = perspectiveName.Trim();
            tableName = tableName.Trim();
            hierarchyName = hierarchyName.Trim();
            #endregion

            using (AMO.Dimension tableDimension = tabularDatabase.Dimensions.GetByName(tableName))
            {
                string tableId = tableDimension.ID;
                string hierarchyId = tableDimension.Hierarchies.GetByName(hierarchyName).ID;

                using (AMO.Perspective perspective = tabularDatabase.Cubes[0].Perspectives.GetByName(perspectiveName))
                using (AMO.PerspectiveDimension perspectiveTable = perspective.Dimensions[tableId])
                {
                    perspectiveTable.Hierarchies.Add(hierarchyId);
                }
            }


            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static bool PerspectiveContainsColumn(AMO.Database tabularDatabase,
                                                        string perspectiveName,
                                                        string tableName,
                                                        string columnName)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (perspectiveName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(PerspectiveStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (columnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(ColumnStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            perspectiveName = perspectiveName.Trim();
            tableName = tableName.Trim();
            columnName = columnName.Trim();
            #endregion

            using (AMO.Dimension tableDimension = tabularDatabase.Dimensions.GetByName(tableName))
            {
                string tableId = tableDimension.ID;
                string columnId = tableDimension.Attributes.GetByName(columnName).ID;

                return tabularDatabase.Cubes[0].Perspectives.GetByName(perspectiveName).Dimensions[tableId].Attributes.Contains(columnId);
            }
        }

        public static bool PerspectiveContainsMeasure(AMO.Database tabularDatabase,
                                                        string perspectiveName,
                                                        string measureName)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (perspectiveName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(PerspectiveStringName);
            if (measureName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(MeasureStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            perspectiveName = perspectiveName.Trim();
            measureName = measureName.Trim();
            #endregion


            return tabularDatabase.Cubes[0].Perspectives.GetByName(perspectiveName).Calculations.Contains(measureName);
        }

        public static bool PerspectiveContainsHierarchy(AMO.Database tabularDatabase,
                                                string perspectiveName,
                                                string tableName,
                                                string hierarchyName)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (perspectiveName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(PerspectiveStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (hierarchyName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(HierarchyStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            perspectiveName = perspectiveName.Trim();
            tableName = tableName.Trim();
            hierarchyName = hierarchyName.Trim();
            #endregion

            using (AMO.Dimension tableDimension = tabularDatabase.Dimensions.GetByName(tableName))
            {
                string tableId = tableDimension.ID;
                string hierarchyId = tableDimension.Hierarchies.GetByName(hierarchyName).ID;

                return tabularDatabase.Cubes[0].Perspectives.GetByName(perspectiveName).Dimensions[tableId].Hierarchies.Contains(hierarchyId);
            }
        }


    }
}
