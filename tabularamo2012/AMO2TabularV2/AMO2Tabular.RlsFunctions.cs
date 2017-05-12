/*=====================================================================
  
    File:      AMO2Tabular.RlsFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Row Level Security objects in a tabular model
 
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
        public static void RlsAdd(AMO.Database tabularDatabase,
                                  string roleName,
                                  string tableName,
                                  string daxFilterExpression,
                                  bool updateInstance = true)
        {
            //  Major steps in adding a Row Level Security (RLS)
            //
            //  - Validate required input arguments and other initial preparations
            //  - Add RLS to Table (as dimension) and enable ReadAccess
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (roleName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(RoleStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (daxFilterExpression.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(DaxFilterExpressionStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            roleName = roleName.Trim();
            tableName = tableName.Trim();
            daxFilterExpression = daxFilterExpression.Trim();
            #endregion

            string dimensionPermissionName = string.Format(CultureInfo.InvariantCulture, "DimensionPermision_for_{0}", roleName);

            string roleId = tabularDatabase.Roles.GetByName(roleName).ID;
            using (AMO.DimensionPermission dimensionPermission = tabularDatabase.Dimensions.GetByName(tableName).DimensionPermissions.Add(roleId, dimensionPermissionName))
            {
                dimensionPermission.Read = AMO.ReadAccess.Allowed;
                dimensionPermission.AllowedRowsExpression = daxFilterExpression;
            }
            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void RlsAlter(AMO.Database tabularDatabase,
                                    string roleName,
                                    string tableName,
                                    string daxFilterExpression,
                                    bool updateInstance = true)
        {
            //  Major steps in updating/altering Row Level Security (RLS) 
            //
            //  - Validate required input arguments and other initial preparations
            //  - Update RLS expression in Table (as dimension)
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (roleName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(RoleStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (daxFilterExpression.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(DaxFilterExpressionStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            roleName = roleName.Trim();
            tableName = tableName.Trim();
            daxFilterExpression = daxFilterExpression.Trim();
            #endregion


            //  [Codeplex issue # 2]
            //  [JPJofre, 2012-10-18]
            //  [Description: tabularDatabase.Roles.GetByName(roleName).ID, returns the database role id not the DimensionsPermissions Id]
            //  [Suggested fix: TBD]

            string roleId = tabularDatabase.Roles.GetByName(roleName).ID;
            using (AMO.DimensionPermission dimensionPermission = tabularDatabase.Dimensions.GetByName(tableName).DimensionPermissions[roleId])
            {
                dimensionPermission.AllowedRowsExpression = daxFilterExpression;
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void RlsDrop(AMO.Database tabularDatabase,
                                string roleName,
                                string tableName,
                                bool updateInstance = true)
        {
            //  Major steps in deleting/droping Row Level Security (RLS) 
            //
            //  - Validate required input arguments and other initial preparations
            //  - Remove DimensionPermissions in table (as dimension)
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (roleName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(RoleStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            roleName = roleName.Trim();
            tableName = tableName.Trim();
            #endregion

            //  [Codeplex issue # 3]
            //  [JPJofre, 2012-10-18]
            //  [Description: tabularDatabase.Roles.GetByName(roleName).ID, returns the database role id not the DimensionsPermissions Id]
            //  [Suggested fix: TBD]

            string roleId = tabularDatabase.Roles.GetByName(roleName).ID;
            tabularDatabase.Dimensions.GetByName(tableName).DimensionPermissions.Remove(roleId, true);

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }
    }
}
