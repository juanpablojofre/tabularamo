/*=====================================================================
  
    File:      AMO2Tabular.HierarchyFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Hierarchies in a tabular model
 
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
        public static void HierarchyAdd(AMO.Database tabularDatabase,
                                        string tableName,
                                        string hierarchyName,
                                        bool updateInstance = true,
                                        params LevelInfo[] levelInfo)
        {
            //  Major steps in adding a Hierarchy to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding 'Empty' Hierarchy to dimension
            //  - Adding Levels to Hierarchy

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
            if (hierarchyName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(HierarchyStringName);
            if (levelInfo == null || levelInfo.Length == 0) throw new ArgumentNullException(LevelInfo);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            hierarchyName = hierarchyName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;
            #endregion

            //  Add 'empty' hierarchy
            using (AMO.Hierarchy currentHierarchy = tabularDatabase.Dimensions[datasourceTableName].Hierarchies.Add(hierarchyName, hierarchyName))
            {
                currentHierarchy.AllMemberName = string.Format(CultureInfo.InvariantCulture, "(All of {0})", hierarchyName);
            }

            //  Add levels
            AMO2Tabular.HierarchyAlterLevelsAdd(tabularDatabase, tableName, hierarchyName, false, levelInfo);

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }


        public static void HierarchyAlterLevelsAdd(AMO.Database tabularDatabase,
                                                  string tableName,
                                                  string hierarchyName,
                                                  bool updateInstance = true,
                                                  params LevelInfo[] levelInfo)
        {
            //  Major steps in adding Levels to a Hierarchy in a table 
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding Levels to Hierarchy

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
            if (hierarchyName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(HierarchyStringName);
            if (levelInfo == null || levelInfo.Length == 0) throw new ArgumentNullException(LevelInfo);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            hierarchyName = hierarchyName.Trim();
            #endregion

            //  [Codeplex issue # 8]
            //  [JPJofre, 2012-10-18]
            //  [Description: using (AMO.Hierarchy currentHierarchy = tableDimension.Hierarchies[hierarchyName]); 
            //  ...   it should be using (AMO.Hierarchy currentHierarchy = tableDimension.Hierarchies.GetByName(hierarchyName))]
            //  [Suggested fix: replace with tableDimension.Hierarchies.GetByName(hierarchyName)]


            //  Add levels
            using(AMO.Dimension tableDimension = tabularDatabase.Dimensions.GetByName(tableName))
            using (AMO.Hierarchy currentHierarchy = tableDimension.Hierarchies.GetByName(hierarchyName))
            {
                //  Add levels to the hierarchy
                foreach (LevelInfo level in levelInfo)
                {
                    string attributeId = tableDimension.Attributes.GetByName(level.DefiningLevelColumnName).ID;
                    currentHierarchy.Levels.Add(level.LevelName).SourceAttributeID = attributeId;
                }
            }
            
            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void HierarchyAlterLevelAdd(AMO.Database tabularDatabase,
                                                  string tableName,
                                                  string hierarchyName,
                                                  string definingLevelColumnName,
                                                  string levelName,
                                                  bool updateInstance = true)
        {
            //  Major steps in adding a Level to a Hierarchy in a table 
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Add Level to Hierarchy

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
            if (hierarchyName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(HierarchyStringName);
            if (definingLevelColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(DefiningLevelColumnName);
            if (levelName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(LevelName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            hierarchyName = hierarchyName.Trim();
            #endregion

            //  [Codeplex issue # 8]
            //  [JPJofre, 2012-10-18]
            //  [Description: using (AMO.Hierarchy currentHierarchy = tabularDatabase.Dimensions[tableName].Hierarchies.Add(hierarchyName, hierarchyName)); 
            //  ...    this creates a NEW hierarchy object instead of obtaining one!!!]
            //  [Suggested fix: replace Add() with FindByName()]

            
            //  Add level
            using (AMO.Hierarchy currentHierarchy = tabularDatabase.Dimensions[tableName].Hierarchies.FindByName(hierarchyName))
            {
                    string attributeId = tabularDatabase.Dimensions.GetByName(tableName).Attributes.GetByName(definingLevelColumnName).ID;
                    currentHierarchy.Levels.Add(levelName).SourceAttributeID = attributeId;
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static string[] HierarchiesEnumerate(AMO.Database tabularDatabase, string tableName)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();

            //  -   Obtain table name in DSV
            string datasourceTableName = tabularDatabase.Dimensions.GetByName(tableName).ID;

            #endregion

            return (from hierarchy in tabularDatabase.Dimensions[datasourceTableName].Hierarchies.Cast<AMO.Hierarchy>()
                    select hierarchy.Name).ToArray();

        }

    }
}
