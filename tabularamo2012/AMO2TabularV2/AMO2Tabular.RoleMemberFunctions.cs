/*=====================================================================
  
    File:      AMO2Tabular.RoleMemberFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Role Members in a tabular model
 
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
        public static void RoleMemberAdd(AMO.Database tabularDatabase,
                                  string roleName,
                                  string windowsUserOrGroup,
                                  bool updateInstance = true)
        {
            //  Major steps in adding a RoleMember to a Role
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Adding RoleMember to Role
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
            if (windowsUserOrGroup.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("windowsUserOrGroup");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            roleName = roleName.Trim();
            windowsUserOrGroup = windowsUserOrGroup.Trim();
            #endregion

            using (AMO.Role role = tabularDatabase.Roles.GetByName(roleName))
            {
                role.Members.Add(new AMO.RoleMember(windowsUserOrGroup, getSid(windowsUserOrGroup)));
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void RoleMemberDrop(AMO.Database tabularDatabase,
                                      string roleName,
                                      string windowsUserOrGroup,
                                      bool updateInstance = true)
        {
            //  Major steps in adding a RoleMember to a Role
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Removing RoleMember from Role
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
            if (windowsUserOrGroup.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("windowsUserOrGroup");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            roleName = roleName.Trim();
            windowsUserOrGroup = windowsUserOrGroup.Trim();
            #endregion

            //  [Codeplex issue # 7]
            //  [JPJofre, 2012-10-18]
            //  [Description: RoleMemberDrop(): Improper way of obtaining RoleMember for removal]
            //  [Suggested fix: Better iterate over all role members and remove when user matches current iterator]

            string roleSid = getSid(windowsUserOrGroup);
            using (AMO.Role role = tabularDatabase.Roles.GetByName(roleName))
            {
                foreach (AMO.RoleMember roleMember in role.Members)
                {
                    if (0 == string.Compare(roleMember.Sid, roleSid, StringComparison.OrdinalIgnoreCase))
                    {
                        role.Members.Remove(roleMember);
                        break;
                    }
                }
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }
    }
}
