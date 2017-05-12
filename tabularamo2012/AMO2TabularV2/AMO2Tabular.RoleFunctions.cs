/*=====================================================================
  
    File:      AMO2Tabular.RoleFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Roles in a tabular model
 
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
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        public static void RoleAdd(AMO.Database tabularDatabase,
                                   string roleName,
                                   bool readPermission,
                                   bool processPermission,
                                   bool administerPermission,
                                   string roleDescription = null,
                                   bool updateInstance = true)
        {
            //  Major steps in adding a Role to the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding Role to Database
            //  - Assigning permissions to role:
            //  -   if administerPermission
            //          user has all permssions
            //      else
            //          assign process permision accordingly to database and cube objects
            //          assign read permission accordingly to database and cube objects

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
            if (roleName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(RoleStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            roleName = roleName.Trim();
            #endregion
            string databasePermissionName = string.Format(CultureInfo.InvariantCulture, "DatabasePermision_for_{0}", roleName);
            string cubePermissionName = string.Format(CultureInfo.InvariantCulture, "CubePermision_for_{0}", roleName);
            using (AMO.Role role = tabularDatabase.Roles.Add(roleName, roleName))
            using (AMO.DatabasePermission databasePermission = tabularDatabase.DatabasePermissions.Add(roleName, databasePermissionName))
            using (AMO.CubePermission cubePermission = tabularDatabase.Cubes[0].CubePermissions.Add(roleName, cubePermissionName))
            {

                if (administerPermission)
                {
                    databasePermission.Administer = administerPermission;
                    databasePermission.Process = true;
                    databasePermission.Read = AMO.ReadAccess.Allowed;
                    databasePermission.ReadDefinition = AMO.ReadDefinitionAccess.Allowed;

                    cubePermission.Process = true;
                    cubePermission.Read = AMO.ReadAccess.Allowed;
                    cubePermission.ReadDefinition = AMO.ReadDefinitionAccess.Allowed;
                }
                else
                {
                    databasePermission.Process = processPermission;
                    cubePermission.Process = processPermission;

                    if (readPermission)
                    {
                        databasePermission.Read = AMO.ReadAccess.Allowed;
                        cubePermission.Read = AMO.ReadAccess.Allowed;
                    }
                    else
                    {
                        databasePermission.Read = AMO.ReadAccess.None;
                        cubePermission.Read = AMO.ReadAccess.None;
                    }
                }

                //  Note: in all cases no user has access to read source data
                cubePermission.ReadSourceData = AMO.ReadSourceDataAccess.None;

                if (roleDescription != null)
                    role.Description = roleDescription.Trim();
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void RoleAlterPermissions(AMO.Database tabularDatabase,
                                                string roleName,
                                                bool readPermission,
                                                bool processPermission,
                                                bool administerPermission,
                                                bool updateInstance = true)
        {
            //  Major steps in adding a Role to the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding Role to Database
            //  - Assigning permissions to role:
            //  -   if administerPermission
            //          user has all permssions
            //      else
            //          verify privilege demotion and reset accordingly
            //          assign process permision accordingly to database and cube objects
            //          assign read permission accordingly to database and cube objects

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
            if (roleName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(RoleStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            roleName = roleName.Trim();
            #endregion
            string roleId = tabularDatabase.Roles.GetByName(roleName).ID;
            using (AMO.DatabasePermission databasePermission = tabularDatabase.DatabasePermissions[roleId])
            using (AMO.CubePermission cubePermission = tabularDatabase.Cubes[0].CubePermissions[roleId])
            {
                if (administerPermission)
                {
                    //  User has been granted Administer privileges
                    databasePermission.Administer = administerPermission;
                    databasePermission.Process = true;
                    databasePermission.ReadDefinition = AMO.ReadDefinitionAccess.Allowed;
                    databasePermission.Read = AMO.ReadAccess.Allowed;

                    cubePermission.Process = true;
                    cubePermission.Read = AMO.ReadAccess.Allowed;
                    cubePermission.ReadDefinition = AMO.ReadDefinitionAccess.Allowed;

                }
                else
                {

                    if (databasePermission.Administer)
                    {
                        //  Note:   The role had administer privileges but has been demoted
                        //          ==> Removing Administer privileges
                        databasePermission.Administer = administerPermission;
                        databasePermission.ReadDefinition = AMO.ReadDefinitionAccess.None;

                        cubePermission.ReadDefinition = AMO.ReadDefinitionAccess.None;
                    }
                    if (processPermission)
                    {
                        databasePermission.Process = processPermission;
                        cubePermission.Process = processPermission;
                        
                        if (readPermission)
                        {
                            databasePermission.Read = AMO.ReadAccess.Allowed;
                            cubePermission.Read = AMO.ReadAccess.Allowed;
                        }
                        else
                        {
                            if (databasePermission.Read == AMO.ReadAccess.Allowed)
                            {
                                //  Note:   The Process role had Read privileges but has been demoted
                                //          ==> Removing Read privileges
                                databasePermission.Read = AMO.ReadAccess.None;
                                cubePermission.Read = AMO.ReadAccess.None;
                            }
                        }
                    }
                    else
                    {
                        if (databasePermission.Process)
                        {
                            //  Note:   The role had Process privileges but has been demoted
                            //          ==> Removing Process privileges
                            databasePermission.Process = processPermission;
                            cubePermission.Process = processPermission;
                        }
                        if (readPermission)
                        {
                            databasePermission.Read = AMO.ReadAccess.Allowed;
                            cubePermission.Read = AMO.ReadAccess.Allowed;
                        }
                        else
                        {
                            databasePermission.Read = AMO.ReadAccess.None;
                            databasePermission.ReadDefinition = AMO.ReadDefinitionAccess.None;

                            cubePermission.Read = AMO.ReadAccess.None;
                            cubePermission.ReadDefinition = AMO.ReadDefinitionAccess.None;
                        }
                    }
                }
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }
    }
}
