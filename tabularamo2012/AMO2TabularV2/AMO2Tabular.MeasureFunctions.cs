/*=====================================================================
  
    File:      AMO2Tabular.MeasureFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Measures in a tabular model
 
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
using System.Text;
using System.Text.RegularExpressions;
using MicrosoftSql2012Samples.Amo2Tabular.Properties;
using AMO = Microsoft.AnalysisServices;




namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public static partial class AMO2Tabular
    {
        public static void MeasureAdd(AMO.Database tabularDatabase,
                                      string tableName,
                                      string measureName,
                                      string daxExpression,
                                      bool updateInstance = true,
                                      ColumnInfo? measureProperties = null)
        {
            //  Major steps in adding a measure to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Verify if the header of the Measures script needs to be created; create it if needed.
            //  - Store the existing MdxScript in a 'temporary' variable
            //  - Append the new Measure to the 'temporary' variable
            //  - Replace existing MdxScript with 'temporary' variable

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
            if (daxExpression.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(DaxExpressionStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);
            if (MeasureExists(tabularDatabase, tableName, measureName)) throw new InvalidOperationException(Resources.MeasureAlreadyExistsInvalidOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            measureName = measureName.Trim();
            daxExpression = daxExpression.Trim();
            #endregion

            switch ((CompatibilityLevel)tabularDatabase.CompatibilityLevel)
            {
                case CompatibilityLevel.SQL2012RTM:
                    MeasureAdd_2012RTM(tabularDatabase, tableName, measureName, daxExpression, measureProperties);
                    break;

                case CompatibilityLevel.SQL2012SP1:
                    MeasureAdd_2012SP1(tabularDatabase, tableName, measureName, daxExpression, measureProperties);
                    break;

                default:
                    throw new NotSupportedException(Resources.InvalidCompatibilityLevelOperationException);
            }


            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);


        }

        public static void MeasureAlterFormat(AMO.Database tabularDatabase,
                                              string tableName,
                                              string measureName,
                                              string format,
                                              bool updateInstance = true)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (measureName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(MeasureStringName);
            //  Explicitly checking for not null only; the user might want to clear current format and give an empty string
            if (format == null) throw new ArgumentNullException("format");

            //  Validate required initial conditions
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);
            if (!MeasureExists(tabularDatabase, tableName, measureName)) throw new InvalidOperationException(Resources.MeasureDoesntExistsInvalidOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            measureName = measureName.Trim();
            format = format.Trim();
            #endregion

            switch ((CompatibilityLevel)tabularDatabase.CompatibilityLevel)
            {
                case CompatibilityLevel.SQL2012RTM:
                    MeasureAlterFormat_2012RTM(tabularDatabase, tableName, measureName, format);
                    break;

                case CompatibilityLevel.SQL2012SP1:
                    MeasureAlterFormat_2012SP1(tabularDatabase, tableName, measureName, format);
                    break;

                default:
                    throw new NotSupportedException(Resources.InvalidCompatibilityLevelOperationException);
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void MeasureDrop(AMO.Database tabularDatabase,
                                       string tableName,
                                       string measureName,
                                       bool updateInstance = true)
        {
            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (measureName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(MeasureStringName);

            //  Validate required initial conditions
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);
            if (!MeasureExists(tabularDatabase, tableName, measureName)) throw new InvalidOperationException(Resources.MeasureDoesntExistsInvalidOperationException);


            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            measureName = measureName.Trim();
            #endregion

            switch ((CompatibilityLevel)tabularDatabase.CompatibilityLevel)
            {
                case CompatibilityLevel.SQL2012RTM:
                    MeasureDrop_2012RTM(tabularDatabase, tableName, measureName);
                    break;

                case CompatibilityLevel.SQL2012SP1:
                    MeasureDrop_2012SP1(tabularDatabase, tableName, measureName);
                    break;

                default:
                    throw new NotSupportedException(Resources.InvalidCompatibilityLevelOperationException);
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

        }

        public static bool MeasureExists(AMO.Database tabularDatabase,
                                               string tableName,
                                               string measureName)
        {

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (tableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(TableStringName);
            if (measureName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException(MeasureStringName);
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);


            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            tableName = tableName.Trim();
            measureName = measureName.Trim();
            #endregion

            switch ((CompatibilityLevel)tabularDatabase.CompatibilityLevel)
            {
                case CompatibilityLevel.SQL2012RTM:
                    return MeasureExists_2012RTM(tabularDatabase, tableName, measureName);

                case CompatibilityLevel.SQL2012SP1:
                    return MeasureExists_2012SP1(tabularDatabase, tableName, measureName);

                default:
                    throw new NotSupportedException(Resources.InvalidCompatibilityLevelOperationException);
            }

        }

        public static string[] MeasuresEnumerate(AMO.Database tabularDatabase, string tableName)
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
            switch ((CompatibilityLevel)tabularDatabase.CompatibilityLevel)
            {
                case CompatibilityLevel.SQL2012RTM:
                    return MeasuresEnumerate_2012RTM(tabularDatabase, tableName);

                case CompatibilityLevel.SQL2012SP1:
                    return MeasuresEnumerate_2012SP1(tabularDatabase, tableName);

                default:
                    throw new NotSupportedException(Resources.InvalidCompatibilityLevelOperationException);
            }

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

        private static void MeasureAdd_2012RTM(AMO.Database tabularDatabase,
                                                string tableName,
                                                string measureName,
                                                string daxExpression,
                                                ColumnInfo? measureProperties = null)
        {
            //  Major steps in adding a measure to a table in the database
            //
            //  - Validate required input arguments
            //  - Other Initial preparations
            //  - Verify if the header of the Measures script needs to be created; create it if needed.
            //  - Store the existing MdxScript in a 'temporary' variable
            //  - Append the new Measure to the 'temporary' variable
            //  - Replace existing MdxScript with 'temporary' variable

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
            #region SQL2012 RTM compatibility level
            StringBuilder measuresCommand = new StringBuilder();

            using (AMO.MdxScript mdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {

                //  Verify if the header of the Measures script needs to be created
                //  Note:   The following header is known to be needed by MS designer client tools 
                if (mdxScript.Commands.Count == 1)
                {
                    measuresCommand.Append(MdxScriptHeader);
                    mdxScript.Commands.Add(new AMO.Command(measuresCommand.ToString()));

                }
                else
                {
                    measuresCommand.Append(mdxScript.Commands[1].Text);
                }

                //  Add the Measure to the list of commands in MdxScripts
                measuresCommand.AppendLine(string.Format(CultureInfo.InvariantCulture, createMeasureTemplate, tableName, measureName, daxExpression));

                //  Replpace the existing script with the updated one
                mdxScript.Commands[1].Text = measuresCommand.ToString();

                //  Update Measure porperties
                using (AMO.CalculationProperty cp = new AMO.CalculationProperty(measureName, AMO.CalculationType.Member))
                {
                    if (measureProperties != null)
                    {
                        cp.FormatString = measureProperties.Value.DataFormat;
                        cp.Description = measureProperties.Value.Description;
                        if (measureProperties.Value.Visible != null)
                            cp.Visible = measureProperties.Value.Visible.Value;
                    }
                    mdxScript.CalculationProperties.Add(cp);
                }
            }
            #endregion

        }

        private static void MeasureAdd_2012SP1(AMO.Database tabularDatabase,
                                                string tableName,
                                                string measureName,
                                                string daxExpression,
                                                ColumnInfo? measureProperties = null)
        {
            //  Major steps in adding a measure to a table in the database
            //
            //  - Verify if the header of the Measures script needs to be created; create it if needed.
            //  - Store the existing MdxScript in a 'temporary' variable
            //  - Append the new Measure to the 'temporary' variable
            //  - Replace existing MdxScript with 'temporary' variable

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
            //  Note:   In 2012 SP1 one Command script is used per measure 
            //          ==> This is different from 2012 RTM behavior, where all measures and KPIs were stored in Commands[1]
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
            StringBuilder measuresText = new StringBuilder();

            using (AMO.MdxScript mdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {
                AMO.Command measureCommand = new AMO.Command();
                //  The header of the Measures script needs to be created
                //  Note:   The following header is known to be needed by MS designer client tools 
                measuresText.Append(MdxScriptHeader);

                //  Add the Measure to the Text of the command
                measuresText.AppendLine(string.Format(CultureInfo.InvariantCulture, createMeasureTemplate, tableName, measureName, daxExpression));

                //  Replpace the existing script with the updated one
                measureCommand.Text = measuresText.ToString();


                // Add measure command to mdxScript
                mdxScript.Commands.Add(measureCommand);


                //  Update Measure porperties
                using (AMO.CalculationProperty cp = new AMO.CalculationProperty(measureName, AMO.CalculationType.Member))
                {
                    if (measureProperties != null)
                    {
                        cp.FormatString = measureProperties.Value.DataFormat;
                        cp.Description = measureProperties.Value.Description;
                        if (measureProperties.Value.Visible != null)
                            cp.Visible = measureProperties.Value.Visible.Value;
                    }
                    mdxScript.CalculationProperties.Add(cp);
                }
            }

        }

        private static void MeasureAlterFormat_2012RTM(AMO.Database tabularDatabase,
                                          string tableName,
                                          string measureName,
                                          string format)
        {
            tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName].CalculationProperties[measureName].FormatString = format;
        }

        private static void MeasureAlterFormat_2012SP1(AMO.Database tabularDatabase,
                                          string tableName,
                                          string measureName,
                                          string format)
        {
            tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName].CalculationProperties[measureName].FormatString = format;
        }

        private static void MeasureDrop_2012RTM(AMO.Database tabularDatabase,
                               string tableName,
                               string measureName)
        {
            //  Note:   Because of the way KPIs are implemented in tabular models 2012, as a promotion of the measure;
            //          Then, when you want to delete a measure, you also have to delete the dependent KPI (if exists)


            //  Verifying and deleting KPI if it exists
            if (KpiExist(tabularDatabase, tableName, measureName))
                KpiDrop(tabularDatabase, tableName, measureName, false);

            using (AMO.MdxScript modelMdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {
                modelMdxScript.CalculationProperties.Remove(measureName);
                //  To be sure that changing the command expression doesn't affect the search...
                //      The search is performed over the AMO command text --> modelCommand.Text
                //      But the removal of text is done over the copy --> measureRemovedCommands

                AMO.Command modelCommand = modelMdxScript.Commands[1];
                Regex commandTypeExpression = new Regex(MeasurePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

                bool found = false;
                string measureRemovedCommands = null;
                foreach (Match match in commandTypeExpression.Matches(modelCommand.Text))
                {
                    if (string.Compare(measureName, match.Groups[MeasureStringName].Value, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        measureRemovedCommands = modelCommand.Text.Replace(match.Groups[0].Value, string.Empty);
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
                    throw new InvalidOperationException(Resources.MeasureDoesntExistsInvalidOperationException);
                }
            }
        }

        private static void MeasureDrop_2012SP1(AMO.Database tabularDatabase,
                           string tableName,
                           string measureName)
        {
            //  Note:   Because of the way KPIs are implemented in tabular models 2012, as a promotion of the measure;
            //          Then, when you want to delete a measure, you also have to delete the dependent KPI (if exists)


            //  Verifying and deleting KPI if it exists
            if (KpiExist(tabularDatabase, tableName, measureName))
                KpiDrop(tabularDatabase, tableName, measureName, false);

            using (AMO.MdxScript modelMdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {
                modelMdxScript.CalculationProperties.Remove(measureName);


                Regex commandTypeExpression = new Regex(MeasurePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                foreach (AMO.Command modelCommand in modelMdxScript.Commands)
                {

                    if (commandTypeExpression.IsMatch(modelCommand.Text) && (string.Compare(measureName, commandTypeExpression.Match(modelCommand.Text).Groups[MeasureStringName].Value, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        modelMdxScript.Commands.Remove(modelCommand);
                        break;
                    }
                }
            }
        }

        private static bool MeasureExists_2012RTM(AMO.Database tabularDatabase,
                                           string tableName,
                                           string measureName)
        {
            Regex commandTypeExpression = new Regex(MeasurePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName].Commands.Count > 1)
            {
                foreach (Match match in commandTypeExpression.Matches(tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName].Commands[1].Text))
                {
                    if (string.Compare(measureName, match.Groups[MeasureStringName].Value, StringComparison.InvariantCultureIgnoreCase) == 0)
                        return true;
                }
            }
            return false;
        }

        private static bool MeasureExists_2012SP1(AMO.Database tabularDatabase,
                                           string tableName,
                                           string measureName)
        {
            using (AMO.MdxScript mdxScript = tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName])
            {
                Regex measureTypeExpression = new Regex(MeasurePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                foreach (AMO.Command modelCommand in mdxScript.Commands)
                {

                    if (measureTypeExpression.IsMatch(modelCommand.Text) && (string.Compare(measureName, measureTypeExpression.Match(modelCommand.Text).Groups[MeasureStringName].Value, StringComparison.InvariantCultureIgnoreCase) == 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string[] MeasuresEnumerate_2012RTM(AMO.Database tabularDatabase, string tableName)
        {
            Regex commandTypeExpression = new Regex(MeasurePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return (from measureMatch in commandTypeExpression.Matches(tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName].Commands[1].Text).Cast<Match>()
                    where string.Compare(measureMatch.Groups[TableStringName].Value.Replace("'", string.Empty), tableName, StringComparison.OrdinalIgnoreCase) == 0
                    select measureMatch.Groups[MeasureStringName].Value).ToArray();
        }

        private static string[] MeasuresEnumerate_2012SP1(AMO.Database tabularDatabase, string tableName)
        {
            Regex commandTypeExpression = new Regex(MeasurePattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return (from measure in tabularDatabase.Cubes[0].MdxScripts[MdxScriptStringName].Commands.Cast<AMO.Command>()
                    where string.Compare(commandTypeExpression.Match(measure.Text).Groups[TableStringName].Value.Replace("'", string.Empty), tableName, StringComparison.OrdinalIgnoreCase) == 0
                        select commandTypeExpression.Match(measure.Text).Groups[MeasureStringName].Value).ToArray();
        }
    }
}
