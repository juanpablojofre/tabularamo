/*=====================================================================
  
    File:      AMO2Tabular.KpiFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               KPIs in a tabular model
 
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
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        public static void KpiAdd(AMO.Database tabularDatabase,
                                  string tableName,
                                  string measureName,
                                  string goalExpression,
                                  string statusExpression,
                                  string statusGraphicImageName,
                                  bool updateInstance = true)
        {

            //  Major steps in adding a KPI to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Verify if the header of the Measures script needs to be created; create it if needed.
            //  - Store the existing MdxScript in a 'temporary' variable
            //  - Append the new KPI elements to the 'temporary' variable
            //  - Replace existing MdxScript with 'temporary' variable
            //  - Update Calculation Properties for KPI elements
            //
            //  Important Conceptual Note:
            //  In Tabular Models a KPI is a Measure that has been promoted to KPI; so, the underlying measure still
            //  exists and cannot be hide
            //
            //  Note: Measures are dynamic objects that live inside an MdxScript object in the cube
            //
            //  IMPORTANT Note: Measures must not be created as native OLAP measure objects, from the MeasureGroup object.
            //      !!  Native OLAP measures cannot be:
            //          •	Referenced by DAX queries
            //          •	Referenced by calculated columns
            //          •	Referenced by other DAX measures

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
            //          ==> tabularDatabase.Cubes[0] represents the cube of the model
            //  
            //  Note:   Only one Commands script is used in Tabular Models to hold all ALL Measures and KPIs of the model
            //          ==> tabularDatabase.Cubes[0].MdxScripts["MdxScript"].Commands[1].Text contains ALL Measures and KPIs of the model
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
            if (measureName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(MeasureStringName);
            if (goalExpression.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("goalExpression");
            if (statusExpression.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("statusExpression");
            if (statusGraphicImageName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("statusGraphicImageName");

            //  Validate required initial conditions
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);
            if (!MeasureExists(tabularDatabase, tableName, measureName)) throw new InvalidOperationException(Resources.MeasureDoesntExistsInvalidOperationException);


            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            measureName = measureName.Trim();
            goalExpression = goalExpression.Trim();
            statusExpression = statusExpression.Trim();
            statusGraphicImageName = statusGraphicImageName.Trim();
            #endregion

            switch ((CompatibilityLevel)tabularDatabase.CompatibilityLevel)
            {
                case CompatibilityLevel.SQL2012RTM:
                    KpiAdd_2012RTM(tabularDatabase, tableName, measureName, goalExpression, statusExpression, statusGraphicImageName);
                    break;

                case CompatibilityLevel.SQL2012SP1:
                    KpiAdd_2012SP1(tabularDatabase, tableName, measureName, goalExpression, statusExpression, statusGraphicImageName);
                    break;

                default:
                    throw new NotSupportedException(Resources.InvalidCompatibilityLevelOperationException);
            }


            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);


        }


        public static void KpiDrop(AMO.Database tabularDatabase,
                              string tableName,
                              string kpiName,
                              bool updateInstance = true)
        {
            //  Major steps in dropping a KPI from a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Store the existing MdxScript in a 'temporary' variable
            //  - Remove the KPI element from the 'temporary' variable
            //  - Remove the Status element from the 'temporary' variable
            //  - Remove the Goal element from the 'temporary' variable
            //  - Remove the Trend element from the 'temporary' variable
            //  - Replace existing MdxScript with 'temporary' variable
            //  - Remove from Calculation Properties the KPI, Status, Goal and Trend elements
            //
            //  Important Conceptual Note:
            //  In Tabular Models a KPI is a Measure that has been promoted to KPI. Deleting a KPI
            //  doesn't delete the undelying measure.
            //
            //  Note: Measures are dynamic objects that live inside an MdxScript object in the cube
            //
            //  IMPORTANT Note: Measures must not be created as native OLAP measure objects, from the MeasureGroup object.
            //      !!  Native OLAP measures cannot be:
            //          •	Referenced by DAX queries
            //          •	Referenced by calculated columns
            //          •	Referenced by other DAX measures

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
            //          ==> tabularDatabase.Cubes[0] represents the cube of the model
            //  
            //  Note:   Only one Commands script is used in Tabular Models to hold all ALL Measures and KPIs of the model
            //          ==> tabularDatabase.Cubes[0].MdxScripts["MdxScript"].Commands[1].Text contains ALL Measures and KPIs of the model
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
            if (kpiName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("kpiName");

            //  Validate required initial conditions
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);
            if (!MeasureExists(tabularDatabase, tableName, kpiName)) throw new InvalidOperationException(Resources.MeasureDoesntExistsInvalidOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            kpiName = kpiName.Trim();
            #endregion

            switch ((CompatibilityLevel)tabularDatabase.CompatibilityLevel)
            {
                case CompatibilityLevel.SQL2012RTM:
                    KpiDrop_2012RTM(tabularDatabase, tableName, kpiName);
                    break;

                case CompatibilityLevel.SQL2012SP1:
                    KpiDrop_2012SP1(tabularDatabase, tableName, kpiName);
                    break;

                default:
                    throw new NotSupportedException(Resources.InvalidCompatibilityLevelOperationException);
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static bool KpiExist(AMO.Database tabularDatabase,
                              string tableName,
                              string kpiName)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (kpiName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("kpiName");

            //  Validate required initial conditions
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            kpiName = kpiName.Trim();
            #endregion

            switch ((CompatibilityLevel)tabularDatabase.CompatibilityLevel)
            {
                case CompatibilityLevel.SQL2012RTM:
                    return KpiExist_2012RTM(tabularDatabase, tableName, kpiName);                    

                case CompatibilityLevel.SQL2012SP1:
                    return KpiExist_2012SP1(tabularDatabase, tableName, kpiName);

                default:
                    throw new NotSupportedException(Resources.InvalidCompatibilityLevelOperationException);
            }
        }

        public static string[] KpisEnumerate(AMO.Database tabularDatabase,
                              string tableName)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);

            //  Validate required initial conditions
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            #endregion

            return (from measureName in MeasuresEnumerate(tabularDatabase, tableName)
                   where KpiExist(tabularDatabase, tableName, measureName)
                   select measureName).ToArray();
        }


        /*****************************************************************************************************************************************************
         * 
         * PRIVATE -- VERSION SPECIFIC FUNCTIONALITY
         * 
         * >   SQL 2012 RTM
         * >   SQL 2012 SP1
         * 
         * ***************************************************************************************************************************************************
         */

        private static void KpiAdd_2012RTM(AMO.Database tabularDatabase,
                              string tableName,
                              string measureName,
                              string goalExpression,
                              string statusExpression,
                              string statusGraphicImageName)
        {

            //  Major steps in adding a KPI to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Verify if the header of the Measures script needs to be created; create it if needed.
            //  - Store the existing MdxScript in a 'temporary' variable
            //  - Append the new KPI elements to the 'temporary' variable
            //  - Replace existing MdxScript with 'temporary' variable
            //  - Update Calculation Properties for KPI elements
            //
            //  Important Conceptual Note:
            //  In Tabular Models a KPI is a Measure that has been promoted to KPI; so, the underlying measure still
            //  exists and cannot be hide
            //
            //  Note: Measures are dynamic objects that live inside an MdxScript object in the cube
            //
            //  IMPORTANT Note: Measures must not be created as native OLAP measure objects, from the MeasureGroup object.
            //      !!  Native OLAP measures cannot be:
            //          •	Referenced by DAX queries
            //          •	Referenced by calculated columns
            //          •	Referenced by other DAX measures

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
            //          ==> tabularDatabase.Cubes[0] represents the cube of the model
            //  
            //  Note:   In 2012 RTM, only one Commands script is used in Tabular Models to hold all ALL Measures and KPIs of the model
            //          ==> tabularDatabase.Cubes[0].MdxScripts["MdxScript"].Commands[1].Text contains ALL Measures and KPIs of the model
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


            using (AMO.MdxScript mdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {

                StringBuilder kpiCommand = new StringBuilder(mdxScript.Commands[1].Text);

                //  Add the Goal 'Member' to the list of commands in MdxScripts
                kpiCommand.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                    createGoalMemberTemplate,
                                                    tableName,
                                                    measureName,
                                                    goalExpression)
                                      );

                //  Add the Status 'Member' to the list of commands in MdxScripts
                kpiCommand.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                    createStatusMemberTemplate,
                                                    tableName,
                                                    measureName,
                                                    statusExpression)
                                     );

                //  Add the Trend 'Member' to the list of commands in MdxScripts
                kpiCommand.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                   createTrendMemberTemplate,
                                                   tableName,
                                                   measureName)
                                     );

                //  Add the KPI 'object' to the list of commands in MdxScripts
                kpiCommand.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                    createKpiTemplate,
                                                    tableName,
                                                    measureName,
                                                    statusGraphicImageName));


                //  Replpace the existing script with the updated one
                mdxScript.Commands[1].Text = kpiCommand.ToString();


                //Adding Calculation Property for the Goal member
                using (AMO.CalculationProperty cp = new AMO.CalculationProperty(string.Format(CultureInfo.InvariantCulture, goalMeasureNameTemplate, measureName), AMO.CalculationType.Member))
                {
                    cp.Visible = false;
                    mdxScript.CalculationProperties.Add(cp);
                }

                //Adding Calculation Property for the Status member
                using (AMO.CalculationProperty cp = new AMO.CalculationProperty(string.Format(CultureInfo.InvariantCulture, statusMeasureNameTemplate, measureName), AMO.CalculationType.Member))
                {
                    cp.Visible = false;
                    mdxScript.CalculationProperties.Add(cp);
                }

                //Adding Calculation Property for the Trend member
                using (AMO.CalculationProperty cp = new AMO.CalculationProperty(string.Format(CultureInfo.InvariantCulture, trendMeasureNameTemplate, measureName), AMO.CalculationType.Member))
                {
                    cp.Visible = false;
                    mdxScript.CalculationProperties.Add(cp);
                }

                //Adding Calculation Property for the KPI
                using (AMO.CalculationProperty cp = new AMO.CalculationProperty(string.Format(CultureInfo.InvariantCulture, kpiNameTemplate, measureName), AMO.CalculationType.Member))
                {
                    cp.Visible = true;
                    mdxScript.CalculationProperties.Add(cp);
                }
            }
        }

        private static void KpiAdd_2012SP1(AMO.Database tabularDatabase,
                          string tableName,
                          string measureName,
                          string goalExpression,
                          string statusExpression,
                          string statusGraphicImageName)
        {
            //  Major steps in adding a KPI to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Store the existing MdxScript in a 'temporary' variable
            //  - Append the new KPI elements to the 'temporary' variable
            //  - Replace existing MdxScript with 'temporary' variable
            //  - Update Calculation Properties for KPI elements
            //
            //  Important Conceptual Note:
            //  In Tabular Models a KPI is a Measure that has been promoted to KPI; so, the underlying measure still
            //  exists and cannot be hide
            //
            //  Note: Measures are dynamic objects that live inside an MdxScript object in the cube
            //
            //  IMPORTANT Note: Measures must not be created as native OLAP measure objects, from the MeasureGroup object.
            //      !!  Native OLAP measures cannot be:
            //          •	Referenced by DAX queries
            //          •	Referenced by calculated columns
            //          •	Referenced by other DAX measures

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
            //          ==> tabularDatabase.Cubes[0] represents the cube of the model
            //  
            //  Note:   In 2012 SP1 and beyond, each measure and related KPI are stored in different Command script objects; one command script object per measure
            //          ==> tabularDatabase.Cubes[0].MdxScripts["MdxScript"].Commands[n].Text contains the n-th Measure and related KPIs of the model
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

            using (AMO.MdxScript mdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {
                Regex commandTypeExpression = new Regex(MeasurePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                foreach (AMO.Command modelCommand in mdxScript.Commands)
                {
                    if (commandTypeExpression.IsMatch(modelCommand.Text) && (string.Compare(measureName, commandTypeExpression.Match(modelCommand.Text).Groups[MeasureStringName].Value, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        StringBuilder kpiCommand = new StringBuilder(modelCommand.Text);

                        //  Add the Goal 'Measure' to the list of commands in MdxScripts
                        kpiCommand.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                            createGoalMeasureTemplate,
                                                            tableName,
                                                            measureName,
                                                            goalExpression)
                                              );

                        //  Add the Status 'Measure' to the list of commands in MdxScripts
                        kpiCommand.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                            createStatusMeasureTemplate,
                                                            tableName,
                                                            measureName,
                                                            statusExpression)
                                             );

                        //  Add the KPI 'object' to the list of commands in MdxScripts
                        kpiCommand.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                                            createKpiTemplate2,
                                                            tableName,
                                                            measureName,
                                                            statusGraphicImageName));


                        //  Replpace the existing script with the updated one
                        modelCommand.Text = kpiCommand.ToString();


                        //Adding Calculation Property for the Goal member
                        using (AMO.CalculationProperty cp = new AMO.CalculationProperty(string.Format(CultureInfo.InvariantCulture, goalMeasureNameTemplate, measureName), AMO.CalculationType.Member))
                        {
                            cp.Visible = false;
                            mdxScript.CalculationProperties.Add(cp);
                        }

                        //Adding Calculation Property for the Status member
                        using (AMO.CalculationProperty cp = new AMO.CalculationProperty(string.Format(CultureInfo.InvariantCulture, statusMeasureNameTemplate, measureName), AMO.CalculationType.Member))
                        {
                            cp.Visible = false;
                            mdxScript.CalculationProperties.Add(cp);
                        }

                        //Adding Calculation Property for the KPI
                        using (AMO.CalculationProperty cp = new AMO.CalculationProperty(string.Format(CultureInfo.InvariantCulture, kpiNameTemplate, measureName), AMO.CalculationType.Member))
                        {
                            cp.Visible = true;
                            mdxScript.CalculationProperties.Add(cp);
                        }


                        break;
                    }
                }

            }
        }

        private static void KpiDrop_2012RTM(AMO.Database tabularDatabase,
                              string tableName,
                              string kpiName)
        {
            //  Major steps in dropping a KPI from a table in the database
            //
            //  - Store the existing MdxScript in a 'temporary' variable
            //  - Remove the KPI element from the 'temporary' variable
            //  - Remove the Status element from the 'temporary' variable
            //  - Remove the Goal element from the 'temporary' variable
            //  - Remove the Trend element from the 'temporary' variable
            //  - Replace existing MdxScript with 'temporary' variable
            //  - Remove from Calculation Properties the KPI, Status, Goal and Trend elements
            //
            //  Important Conceptual Note:
            //  In Tabular Models a KPI is a Measure that has been promoted to KPI. Deleting a KPI
            //  doesn't delete the undelying measure.
            //
            //  Note: Measures are dynamic objects that live inside an MdxScript object in the cube
            //
            //  IMPORTANT Note: Measures must not be created as native OLAP measure objects, from the MeasureGroup object.
            //      !!  Native OLAP measures cannot be:
            //          •	Referenced by DAX queries
            //          •	Referenced by calculated columns
            //          •	Referenced by other DAX measures

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
            //          ==> tabularDatabase.Cubes[0] represents the cube of the model
            //  
            //  Note:   Only one Commands script is used in Tabular Models to hold all ALL Measures and KPIs of the model
            //          ==> tabularDatabase.Cubes[0].MdxScripts["MdxScript"].Commands[1].Text contains ALL Measures and KPIs of the model
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

            using (AMO.MdxScript modelMdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {
                AMO.Command modelCommand = modelMdxScript.Commands[1];
                AMO.CalculationPropertyCollection modelCalculationProperties = modelMdxScript.CalculationProperties;

                //  Remove KPI properties
                modelCalculationProperties.Remove(kpiName);

                Regex commandTypeExpression = new Regex(KpiPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

                //  To be sure that changing the command expression doesn't affect the search...
                //      The search is performed over the AMO command text --> modelCommand.Text
                //      But the removal of text is done over the copy --> measureRemovedCommands
                bool found = false;
                string measureRemovedCommands = modelCommand.Text;
                foreach (Match match in commandTypeExpression.Matches(modelCommand.Text))   //  search
                {
                    string matchKpiName = match.Groups["kpiName"].Value.Replace("[", string.Empty);
                    matchKpiName = matchKpiName.Replace("]", string.Empty);
                    if (string.Compare(kpiName, matchKpiName, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        measureRemovedCommands = measureRemovedCommands.Replace(match.Groups[0].Value, string.Empty);   //  removal
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    modelCommand.Text = measureRemovedCommands;
                }
                else
                {
                    throw new InvalidOperationException(Resources.KpiDoesNotExistInvalidOperationException);
                }

                commandTypeExpression = new Regex(MemberPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);


                //  Note:   Here we assume Goal, Status and Trend members were added following the
                //          defined practice for MS SQL Server 2012 Tabular models of giving them the
                //          names according to the following patterns:

                //  Goal 'Member' name in MdxScripts: 
                string goalMemberName = string.Format(CultureInfo.InvariantCulture, "[_{0} Goal]", kpiName);

                //  Status 'Member' name in MdxScripts:
                string statusMemberName = string.Format(CultureInfo.InvariantCulture, "[_{0} Status]", kpiName);

                //  Trend 'Member' name in MdxScripts:
                string trendMemberName = string.Format(CultureInfo.InvariantCulture, "[_{0} Trend]", kpiName);


                //  To be sure that changing the command expression doesn't affect the search...
                //      The search is performed over the AMO command text --> modelCommand.Text
                //      But the removal of text is done over the copy --> measureRemovedCommands

                found = false;
                measureRemovedCommands = modelCommand.Text;
                foreach (Match match in commandTypeExpression.Matches(modelCommand.Text))   //  search
                {
                    string matchMemberName = match.Groups["memberName"].Value;

                    //  Remove the Goal 'Member' from the list of commands in MdxScripts 
                    //  Remove the Status 'Member' from the list of commands in MdxScripts
                    //  Remove the Trend 'Member' from the list of commands in MdxScripts
                    if ((string.Compare(goalMemberName, matchMemberName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        || (string.Compare(statusMemberName, matchMemberName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        || (string.Compare(trendMemberName, matchMemberName, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        measureRemovedCommands = measureRemovedCommands.Replace(match.Groups[0].Value, string.Empty);   //  removal
                        found = true;
                    }
                }

                if (found)
                {
                    modelCommand.Text = measureRemovedCommands;
                }

                //Removing the Status member Calculation Property 
                modelCalculationProperties.Remove(statusMemberName);

                //Removing the Trend member Calculation Property
                modelCalculationProperties.Remove(trendMemberName);

                //Removing the Goal member Calculation Property 
                modelCalculationProperties.Remove(goalMemberName);
            }
        }

        private static void KpiDrop_2012SP1(AMO.Database tabularDatabase,
                          string tableName,
                          string kpiName)
        {
            using (AMO.MdxScript mdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {

                Regex kpiTypeExpression = new Regex(KpiPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

                foreach (AMO.Command modelCommand in mdxScript.Commands)
                {
                    if (kpiTypeExpression.IsMatch(modelCommand.Text))
                    {
                        string matchKpiName = kpiTypeExpression.Match(modelCommand.Text).Groups["kpiName"].Value.Replace("[", string.Empty);
                        matchKpiName = matchKpiName.Replace("]", string.Empty);

                        if (string.Compare(kpiName, matchKpiName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            AMO.CalculationPropertyCollection modelCalculationProperties = mdxScript.CalculationProperties;

                            //  Remove KPI properties
                            modelCalculationProperties.Remove(kpiName);

                            //  To be sure that changing the command expression doesn't affect the search...
                            //      The search is performed over the AMO command text --> modelCommand.Text
                            //      But the removal of text is done over the copy --> measureRemovedCommands
                            bool found = false;
                            string measureRemovedCommands = modelCommand.Text.Replace(kpiTypeExpression.Match(modelCommand.Text).Groups[0].Value,string.Empty);

                            Regex commandTypeExpression = new Regex(MemberPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);


                            //  Note:   Here we assume Goal, Status and Trend members were added following the
                            //          defined practice for MS SQL Server 2012 Tabular models of giving them the
                            //          names according to the following patterns:

                            //  Goal 'Member' name in MdxScripts: 
                            string goalMemberName = string.Format(CultureInfo.InvariantCulture, "[_{0} Goal]", kpiName);

                            //  Status 'Member' name in MdxScripts:
                            string statusMemberName = string.Format(CultureInfo.InvariantCulture, "[_{0} Status]", kpiName);

                            //  Trend 'Member' name in MdxScripts:
                            string trendMemberName = string.Format(CultureInfo.InvariantCulture, "[_{0} Trend]", kpiName);


                            //  To be sure that changing the command expression doesn't affect the search...
                            //      The search is performed over the AMO command text --> modelCommand.Text
                            //      But the removal of text is done over the copy --> measureRemovedCommands

                            found = false;
                            measureRemovedCommands = modelCommand.Text;
                            foreach (Match match in commandTypeExpression.Matches(modelCommand.Text))   //  search
                            {
                                string matchMemberName = match.Groups["memberName"].Value;

                                //  Remove the Goal 'Member' from the list of commands in MdxScripts 
                                //  Remove the Status 'Member' from the list of commands in MdxScripts
                                if ((string.Compare(goalMemberName, matchMemberName, StringComparison.InvariantCultureIgnoreCase) == 0)
                                    || (string.Compare(statusMemberName, matchMemberName, StringComparison.InvariantCultureIgnoreCase) == 0))
                                {
                                    measureRemovedCommands = measureRemovedCommands.Replace(match.Groups[0].Value, string.Empty);   //  removal
                                    found = true;
                                }
                            }

                            if (found)
                            {
                                modelCommand.Text = measureRemovedCommands;
                            }

                            //Removing the Status member Calculation Property 
                            modelCalculationProperties.Remove(statusMemberName);

                            //Removing the Goal member Calculation Property 
                            modelCalculationProperties.Remove(goalMemberName);
                        }
                    }
                }
            }
        }

        private static bool KpiExist_2012RTM(AMO.Database tabularDatabase,
                      string tableName,
                      string kpiName)
        {
            Regex kpiTypeExpression = new Regex(KpiPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

            foreach (Match match in kpiTypeExpression.Matches(tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName].Commands[1].Text))
            {
                string matchKpiName = match.Groups["kpiName"].Value.Replace("[", string.Empty);
                matchKpiName = matchKpiName.Replace("]", string.Empty);
                if (string.Compare(kpiName, matchKpiName, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool KpiExist_2012SP1(AMO.Database tabularDatabase,
                  string tableName,
                  string kpiName)
        {
            using (AMO.MdxScript mdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {

                Regex kpiTypeExpression = new Regex(KpiPattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

                foreach (AMO.Command modelCommand in mdxScript.Commands)
                {
                    if (kpiTypeExpression.IsMatch(modelCommand.Text) )
                    {
                        string matchKpiName = kpiTypeExpression.Match(modelCommand.Text).Groups["kpiName"].Value.Replace("[", string.Empty);
                        matchKpiName = matchKpiName.Replace("]", string.Empty);

                        if (string.Compare(kpiName, matchKpiName, StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }



    }



}
