/*=====================================================================
  
    File:      AMO2Tabular.RelationshipFunctions.cs

    Summary:   This part of the AMO2Tabular class contains all
               functions related to manage and manipulate 
               Relationships in a tabular model
 
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tabularDatabase"></param>
        /// <param name="pkTableName"></param>
        /// <param name="pkColumnName"></param>
        /// <param name="foreignTableName"></param>
        /// <param name="foreignColumnName"></param>
        /// <param name="active"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "pk")]
        public static void RelationshipAdd(AMO.Database tabularDatabase,
                                           string pkTableName,
                                           string pkColumnName,
                                           string foreignTableName,
                                           string foreignColumnName,
                                           bool active = true,
                                           bool updateInstance = true)
        {
            //  Terminology note: 
            //  -   the Foreign side of the relationship is named the From end in AMO
            //  -   the PK side of the relationship in named the To end in AMO
            //
            //  Relationships flow FROM the foreign side of the relationship 
            //  TO the primary key side of the relationship in AMO
            //      ==> Relationship information is stored in the Foreign Dimension object
            //            

            //  Major steps in adding a relationship
            //
            //  - Validate required input arguments and other initial preparations
            //  - Verify relationship doesn't exist before creating it
            //  - Add relationship to the Foreign dimension object
            //  - Set relationship to active, if requested (default setting)
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (pkTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkTableName");
            if (pkColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkColumnName");
            if (foreignTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignTableName");
            if (foreignColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignColumnName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            pkTableName = pkTableName.Trim();
            pkColumnName = pkColumnName.Trim();
            foreignTableName = foreignTableName.Trim();
            foreignColumnName = foreignColumnName.Trim();

            //  -   Obtain Id's
            string pkTableId = tabularDatabase.Dimensions.GetByName(pkTableName).ID;
            string pkColumnId = tabularDatabase.Dimensions[pkTableId].Attributes.GetByName(pkColumnName).ID;
            string foreignTableId = tabularDatabase.Dimensions.GetByName(foreignTableName).ID;
            string foreignColumnId = tabularDatabase.Dimensions[foreignTableId].Attributes.GetByName(foreignColumnName).ID;
            #endregion

            //  Verify relationship existence
            //  --> throw exception if relationship already exists
            if (RelationshipExists(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName))
                throw new InvalidOperationException(Resources.RelationshipAlreadyExistInvalidOperationException);

            //  Add relationship
            //  First, create the INACTIVE relationship definitions in the MultipleValues end of the relationship
            string newRelationshipID = Guid.NewGuid().ToString();
            AMO.Relationship newRelationship = tabularDatabase.Dimensions[foreignTableId].Relationships.Add(newRelationshipID);

            newRelationship.FromRelationshipEnd.DimensionID = foreignTableId;
            newRelationship.FromRelationshipEnd.Attributes.Add(foreignColumnId);
            newRelationship.FromRelationshipEnd.Multiplicity = AMO.Multiplicity.Many;
            newRelationship.FromRelationshipEnd.Role = string.Empty;
            newRelationship.ToRelationshipEnd.DimensionID = pkTableId;
            newRelationship.ToRelationshipEnd.Attributes.Add(pkColumnId);
            newRelationship.ToRelationshipEnd.Multiplicity = AMO.Multiplicity.One;
            newRelationship.ToRelationshipEnd.Role = string.Empty;

            //  Set relationship to active
            if (active)
                RelationshipAlterActive(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName, active, false);

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

        }

        public static void RelationshipDrop(AMO.Database tabularDatabase,
                                            string pkTableName,
                                            string pkColumnName,
                                            string foreignTableName,
                                            string foreignColumnName,
                                            bool updateInstance = true)
        {
            //  Terminology note: 
            //  -   the Foreign side of the relationship is named the From end in AMO
            //  -   the PK side of the relationship in named the To end in AMO
            //
            //  Relationships flow FROM the foreign side of the relationship 
            //  TO the primary key side of the relationship in AMO
            //      ==> Relationship information is stored in the Foreign Dimension object
            //            

            //  Major steps in adding a relationship
            //
            //  - Validate required input arguments and other initial preparations
            //  - Verify relationship exist; throw error if it doesn't exist
            //  - Remove Active part first
            //  - Remove In-Active part lastly
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (pkTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkTableName");
            if (pkColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkColumnName");
            if (foreignTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignTableName");
            if (foreignColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignColumnName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            pkTableName = pkTableName.Trim();
            pkColumnName = pkColumnName.Trim();
            foreignTableName = foreignTableName.Trim();
            foreignColumnName = foreignColumnName.Trim();

            //  -   Obtain Id's
            string foreignTableId = tabularDatabase.Dimensions.GetByName(foreignTableName).ID;
            #endregion

            //  Verify relationship existence
            //  --> throw an exception if relationship doesn't exist
            string relationshipId = RelationshipTryGetRelationshipId(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName);
            if (relationshipId.IsNullOrEmptyOrWhitespace())
                throw new NotSupportedException(Resources.RelationshipDoesNotExistInvalidOperationException);

            //  Remove Active part
            if (RelationshipIsActive(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName))
                RelationshipAlterActive(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName, false);

            //  Remove In-Active part
            tabularDatabase.Dimensions[foreignTableId].Relationships.Remove(relationshipId);

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        public static void RelationshipAlterActive(AMO.Database tabularDatabase,
                                                   string pkTableName,
                                                   string pkColumnName,
                                                   string foreignTableName,
                                                   string foreignColumnName,
                                                   bool active,
                                                   bool updateInstance = true)
        {
            //  Terminology note: 
            //  -   the Foreing side of the relationship is named the From end in AMO
            //  -   the PK side of the relationship in named the To end in AMO
            //
            //  Relationships flow FROM the foreign side of the relationship 
            //  TO the primary key side of the relationship in AMO
            //      ==> [Inactive] Relationship information is stored in the Foreign Dimension object in a relationship object
            //      ==> [Active] Relationship information is stored in the MeasureGroup object of the table in a ReferenceMeasureGroupDimension object
            //          

            //  Major steps in adding a relationship
            //
            //  - Validate required input arguments and other initial preparations
            //  - If Activating:
            //      - Verify relationship exist before Activating
            //          -   If relationship doesn't exist throw an error
            //      - Verify no multipath or alternate path will be created by activating this relationship
            //      - If ReferenceMeasureGroupDimension, for the PK table, doesn't exist add it to the Dimensions collection in the MeasureGroup
            //          -   replicate attributes (columns) from Dimension
            //      - If ReferenceMeasureGroupDimension exists, update relationshipID and IntermediateGranularity
            //      - To the Dimensions collection in the MeasureGroup
            //          - Add all other intermediate relationships from 'Ancestors' paths
            //          - Add all other intermediate relationships to the 'Descendants' paths --> Update MeasureGroups in the 'Descendants' paths
            //
            //
            //       T1       T2         T5               T6         T7
            //      ----     -----      -----            -----      -----
            //      |a1| <-- |a2 |      |a5 |            |a6 |      |a7 |
            //      |  |     |a2i| <--  |a5i|   /|       |   |      |a7i|
            //      |  |     |   |      |   |  / ------| |a6i| <--  |   |
            //      ----     -----      |   | /        | |   |      -----
            //                          |   | \  Rel56 | |   |
            //       T3       T4        |   |  \ ------| |   |       T8
            //      ----     -----      |   |   \|       |   |      -----
            //      |a3| <-- |a4 |      |   |            |   | <--  |a8 |
            //      |  |     |a4i| <--  |a5j|            |   |      |a8i|
            //      |  |     |   |      |   |            |   |      |   |
            //      ----     -----      -----            -----      -----
            //
            //      \_________  ____________/         \________  ________/
            //                \/                               \/
            //             Ancestors                      Descendants
            //
            //
            //  When adding Rel56, as active, to the model, on top of everything that already exists, you have to:
            //  
            //  --> Add a ReferenceMeasureGroupDimension or updating existing one: (T5, T6.a6i) will include: Materialization:=Regular, RelationshipId:=<relationship id>
            //
            //  --> After adding (T5, T6.a6i) to T6, you have to:
            //
            //                                           Add              Add             Add
            //                                          to T6            to T7           to T8
            //                                         ......           ......          ......       
            //                                                          (T5, T6.a6i)    (T5, T6.a6i)
            //                                         (T2, T5.a5i)     (T2, T5.a5i)    (T2, T5.a5i)
            //                                         (T1, T2.a2i)     (T1, T2.a2i)    (T1, T2.a2i)
            //                                         (T4, T5.a5j)     (T4, T5.a5j)    (T4, T5.a5j)
            //                                         (T3, T4.a4i)     (T3, T4.a4i)    (T3, T4.a4i)
            //
            //  --> as reference dimensions to the MeasureGroup
            //  --> because the relationships between T6<--T7 and T6<--T8 already exist, the materialization and relationship 
            //      definitions were added at the time the relationship were created ==> no need to do anything like 
            //      on T6.
            //
            //  - If De-Activating
            //      - Verify relationship exist before De-Activating
            //      - If relationship doesn't exist throw an error
            //      - Remove All ReferenceMeasureGroupDimension, for the PK table and Down Below tables, from MeasureGroup
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //
            //  Note:   Only one DataSourceView is used in Tabular Models 
            //          ==> tabularDatabase.DataSourceViews[0] represents the DSV of the model
            //  
            //  Note:   Only one Cube is used in Tabular Models 
            //          ==> tabularDatabase.Cubes[0] represents the cube in the model
            //
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
            if (pkTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkTableName");
            if (pkColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkColumnName");
            if (foreignTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignTableName");
            if (foreignColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignColumnName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            pkTableName = pkTableName.Trim();
            pkColumnName = pkColumnName.Trim();
            foreignTableName = foreignTableName.Trim();
            foreignColumnName = foreignColumnName.Trim();

            //  -   Obtain Id's
            string pkTableId = tabularDatabase.Dimensions.GetByName(pkTableName).ID;
            string pkColumnId = tabularDatabase.Dimensions[pkTableId].Attributes.GetByName(pkColumnName).ID;
            string foreignTableId = tabularDatabase.Dimensions.GetByName(foreignTableName).ID;
            string foreignColumnId = tabularDatabase.Dimensions[foreignTableId].Attributes.GetByName(foreignColumnName).ID;

            #endregion

            RelationshipGraph currentRelationshipsGraph;

            //  Branch on 'active'
            //  -   true ==> set relationship to Active
            //  -   false ==> set relationship to Inactive
            switch (active)
            {
                case true:
                    #region set relationship to Active
                    //  Verify relationship existence
                    //  --> Throw an error if relationship doesn't exist

                    //  -   Define placeholder for the relationship id; it's needed later
                    string relationshipId = RelationshipTryGetRelationshipId(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName);

                    if (relationshipId.IsNullOrEmptyOrWhitespace())
                        throw new InvalidOperationException(Resources.RelationshipDoesNotExistInvalidOperationException);

                    //  Verify that activating this relationship does not violate the rule of no alternate paths when it will be created
                    //      Build a graph of the actual state of relationships
                    currentRelationshipsGraph = new RelationshipGraph(tabularDatabase);
                    if (currentRelationshipsGraph.RelationshipAlternatePathExists(pkTableId, foreignTableId))
                        throw new InvalidOperationException(Resources.RelationshipViolatesAlternatePathRuleInvalidOperationException);

                    //  The 'activeness' of a relationship is defined by the existence of a ReferenceMeasureGroupDimension
                    //  that include Materialization as Regular
                    using (AMO.Cube modelCube = tabularDatabase.Cubes[0])
                    using (AMO.MeasureGroup foreignTableMG = modelCube.MeasureGroups[foreignTableId])
                    {

                        if (!foreignTableMG.Dimensions.Contains(pkTableId))
                        {
                            //  Creating the ReferenceMeasureGroupDimension that defines the 'Activeness' of a relationship
                            RelationshipAddReferenceMeasureGroupDimension(tabularDatabase, foreignTableId, new RelationshipPair(new FullName(pkTableId, pkColumnId), new FullName(foreignTableId, foreignColumnId)), relationshipId);

                            //  Adding Intermediate relationships for all tables in the paths from PrimaryKeyEndBelow for direct ForeignKeyEnd
                            foreach (RelationshipPair primaryKeyDownRelationship in currentRelationshipsGraph.RelationshipsListPrimaryKeyDown(pkTableId))
                                RelationshipAddReferenceMeasureGroupDimension(tabularDatabase, foreignTableId, primaryKeyDownRelationship);

                            //  Adding Intermediate relationships for all tables in the paths from PrimaryKeyEndBelow in each of the tables
                            //  that are ForeignKeyEndAbove
                            foreach (string foreignKeyTableUpId in currentRelationshipsGraph.TableListForeignKeyUp(foreignTableId))
                            {
                                using (AMO.MeasureGroup foreignKeyUpMG = tabularDatabase.Cubes[0].MeasureGroups[foreignKeyTableUpId])
                                {
                                    RelationshipAddReferenceMeasureGroupDimension(tabularDatabase, foreignKeyUpMG.ID, new RelationshipPair(new FullName(pkTableId, pkColumnId), new FullName(foreignTableId, foreignColumnId)));
                                    foreach (RelationshipPair primaryKeyDownRelationship in currentRelationshipsGraph.RelationshipsListPrimaryKeyDown(pkTableId))
                                        RelationshipAddReferenceMeasureGroupDimension(tabularDatabase, foreignKeyUpMG.ID, primaryKeyDownRelationship);
                                }
                            }

                        }
                        else
                        {
                            //  Because the ReferenceMeasureGroupDimension already exists, this probably means just a change on the foreign key used as Active
                            using (AMO.ReferenceMeasureGroupDimension currentReferenceMGDim = (AMO.ReferenceMeasureGroupDimension)foreignTableMG.Dimensions[pkTableId])
                            {
                                currentReferenceMGDim.RelationshipID = relationshipId;
                                currentReferenceMGDim.IntermediateGranularityAttributeID = foreignColumnId;
                            }
                        }

                    }
                    currentRelationshipsGraph.Clear();
                    break;
                    #endregion
                case false:
                    #region set relationship to Inactive
                    //  Verify relationship existence
                    //  --> If relationship doesn't exist throw an error
                    if (!RelationshipExists(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName))
                        throw new InvalidOperationException(Resources.RelationshipDoesNotExistInvalidOperationException);

                    //      Build a graph of the actual state of relationships
                    currentRelationshipsGraph = new RelationshipGraph(tabularDatabase);


                    //  Remove the ReferenceMeasureGroupDimension to remove the 'Activeness'
                    tabularDatabase.Cubes[0].MeasureGroups[foreignTableId].Dimensions.Remove(pkTableId);

                    //  Removing Intermediate relationships for all tables in the paths from PrimaryKeyEndBelow for direct ForeignKeyEnd
                    foreach (string primaryKeyDownTableId in currentRelationshipsGraph.TableListPrimaryKeyDown(pkTableId))
                        tabularDatabase.Cubes[0].MeasureGroups[foreignTableId].Dimensions.Remove(primaryKeyDownTableId);

                    //  Adding Intermediate relationships for all tables in the paths from PrimaryKeyEndBelow in each of the tables
                    //  that are ForeignKeyEndAbove
                    foreach (string foreignKeyTableUpId in currentRelationshipsGraph.TableListForeignKeyUp(foreignTableId))
                    {
                            tabularDatabase.Cubes[0].MeasureGroups[foreignKeyTableUpId].Dimensions.Remove(pkTableId);
                            foreach (string primaryKeyDownTableId in currentRelationshipsGraph.TableListPrimaryKeyDown(pkTableId))
                                tabularDatabase.Cubes[0].MeasureGroups[foreignKeyTableUpId].Dimensions.Remove(primaryKeyDownTableId);
                    }

                    currentRelationshipsGraph.Clear();
                    break;
                    #endregion
            }

            //  Update server instance
            if (updateInstance)
                tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

        }

        /// <summary>
        /// Verifies the relationship between the given columns in the model exists
        /// </summary>
        /// <param name="tabularDatabase"></param>
        /// <param name="pkTableName"></param>
        /// <param name="pkColumnName"></param>
        /// <param name="foreignTableName"></param>
        /// <param name="foreignColumnName"></param>
        /// <returns></returns>
        private static string RelationshipTryGetRelationshipId(AMO.Database tabularDatabase,
                                                               string pkTableName,
                                                               string pkColumnName,
                                                               string foreignTableName,
                                                               string foreignColumnName)
        {
            //  Terminology note: 
            //  -   the Foreign side of the relationship is named the From end in AMO
            //  -   the PK side of the relationship in named the To end in AMO
            //
            //  Relationships flow FROM the foreign side of the relationship 
            //  TO the primary key side of the relationship in AMO
            //      ==> [Inactive] Relationship information is stored in the Foreign Dimension object
            //            

            //  Major steps in verifying the existence of a relationship
            //
            //  - Validate required input arguments and other initial preparations
            //  - Iterate over all relationships on the Foreing Table to find a match
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (pkTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkTableName");
            if (pkColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkColumnName");
            if (foreignTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignTableName");
            if (foreignColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignColumnName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            pkTableName = pkTableName.Trim();
            pkColumnName = pkColumnName.Trim();
            foreignTableName = foreignTableName.Trim();
            foreignColumnName = foreignColumnName.Trim();

            //  -   Obtain Id's
            string pkTableId = tabularDatabase.Dimensions.GetByName(pkTableName).ID;
            string pkColumnId = tabularDatabase.Dimensions[pkTableId].Attributes.GetByName(pkColumnName).ID;
            string foreignTableId = tabularDatabase.Dimensions.GetByName(foreignTableName).ID;
            string foreignColumnId = tabularDatabase.Dimensions[foreignTableId].Attributes.GetByName(foreignColumnName).ID;
            #endregion


            //  Iterate over all relationship objects on the foreign table
            foreach (AMO.Relationship currentRelationship in tabularDatabase.Dimensions[foreignTableId].Relationships)
            {
                if ((0 == string.Compare(currentRelationship.FromRelationshipEnd.Attributes[0].AttributeID, foreignColumnId, StringComparison.OrdinalIgnoreCase))
                    && (0 == string.Compare(currentRelationship.ToRelationshipEnd.DimensionID, pkTableId, StringComparison.OrdinalIgnoreCase))
                    && (0 == string.Compare(currentRelationship.ToRelationshipEnd.Attributes[0].AttributeID, pkColumnId, StringComparison.OrdinalIgnoreCase)))
                {
                    return currentRelationship.ID;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// Verifies if there exists a relationship between the given columns in the model
        /// </summary>
        /// <param name="tabularDatabase"></param>
        /// <param name="pkTableName"></param>
        /// <param name="pkColumnName"></param>
        /// <param name="foreignTableName"></param>
        /// <param name="foreignColumnName"></param>
        /// <returns></returns>
        public static bool RelationshipExists(AMO.Database tabularDatabase,
                                               string pkTableName,
                                               string pkColumnName,
                                               string foreignTableName,
                                               string foreignColumnName)
        {
            //  This is a shortcut wrapper to verify if a relationship exists
            //  whenever you don't need the relationshipId

            return !RelationshipTryGetRelationshipId(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName).IsNullOrEmptyOrWhitespace();
        }

        /// <summary>
        /// Verifies if an existing relationship is active
        /// </summary>
        /// <param name="tabularDatabase"></param>
        /// <param name="pkTableName"></param>
        /// <param name="pkColumnName"></param>
        /// <param name="foreignTableName"></param>
        /// <param name="foreignColumnName"></param>
        /// <returns></returns>
        public static bool RelationshipIsActive(AMO.Database tabularDatabase,
                                                   string pkTableName,
                                                   string pkColumnName,
                                                   string foreignTableName,
                                                   string foreignColumnName)
        {
            //  Terminology note: 
            //  -   the Foreign side of the relationship is named the From end in AMO
            //  -   the PK side of the relationship in named the To end in AMO
            //
            //  Relationships flow FROM the foreign side of the relationship 
            //  TO the primary key side of the relationship in AMO
            //      ==> [Inactive] Relationship information is stored in the Foreign Dimension object
            //            

            //  Major steps in verifying if relationship is active
            //
            //  - Validate required input arguments and other initial preparations
            //  - Validate relationship exists
            //  - Verify the existence of a ReferenceMeasureGroupDimension for (pkTableName, pkColumnName) in measure group for foreignTableName
            //      Note:   The 'activeness' of a relationship is defined by the existence of a ReferenceMeasureGroupDimension
            //
            //  Note:   In AMO, strings as indexers refer to the ID of the object, not the name
            //

            #region Validate input arguments and other initial preparations
            //  Validate required input arguments
            if (tabularDatabase == null) throw new ArgumentNullException(TabularDatabaseStringName);
            if (pkTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkTableName");
            if (pkColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("pkColumnName");
            if (foreignTableName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignTableName");
            if (foreignColumnName.IsNullOrEmptyOrWhitespace()) throw new ArgumentNullException("foreignColumnName");
            if (!IsDatabaseCompatibilityLevelCorrect(tabularDatabase)) throw new InvalidOperationException(Resources.InvalidCompatibilityLevelOperationException);

            //  Other initial preparations
            //  -   Cleaning and preparing name variables
            pkTableName = pkTableName.Trim();
            pkColumnName = pkColumnName.Trim();
            foreignTableName = foreignTableName.Trim();
            foreignColumnName = foreignColumnName.Trim();

            //  -   Obtain Id's
            string pkTableId = tabularDatabase.Dimensions.GetByName(pkTableName).ID;
            string pkColumnId = tabularDatabase.Dimensions[pkTableId].Attributes.GetByName(pkColumnName).ID;
            string foreignTableId = tabularDatabase.Dimensions.GetByName(foreignTableName).ID;
            string foreignColumnId = tabularDatabase.Dimensions[foreignTableId].Attributes.GetByName(foreignColumnName).ID;
            #endregion



            string relationshipId = RelationshipTryGetRelationshipId(tabularDatabase, pkTableName, pkColumnName, foreignTableName, foreignColumnName);
            if (relationshipId.IsNullOrEmptyOrWhitespace())
                throw new InvalidOperationException(Resources.RelationshipDoesNotExistInvalidOperationException);


            return tabularDatabase.Cubes[0].MeasureGroups[foreignTableId].Dimensions.Contains(pkTableId)
                   && 0 == string.Compare(((AMO.ReferenceMeasureGroupDimension)tabularDatabase.Cubes[0].MeasureGroups[foreignTableId].Dimensions[pkTableId]).RelationshipID, relationshipId, StringComparison.OrdinalIgnoreCase)
                   && 0 == string.Compare(((AMO.ReferenceMeasureGroupDimension)tabularDatabase.Cubes[0].MeasureGroups[foreignTableId].Dimensions[pkTableId]).IntermediateGranularityAttributeID, foreignColumnId, StringComparison.OrdinalIgnoreCase);
        }

        private static void RelationshipAddReferenceMeasureGroupDimension(AMO.Database tabularDatabase,
                                                                            string currentMeasureGroupId,
                                                                            RelationshipPair relationshipPair,
                                                                            string relationshipId = null)
        {
            string foreignTableId = relationshipPair.ForeignKeyEnd.TableId;
            string foreignColumnId = relationshipPair.ForeignKeyEnd.ColumnId;
            string pkTableId = relationshipPair.PrimaryKeyEnd.TableId;
            string pkColumnId = relationshipPair.PrimaryKeyEnd.ColumnId;

            //  Creating the ReferenceMeasureGroupDimension that defines the 'Activeness' of a relationship
            using (AMO.Cube modelCube = tabularDatabase.Cubes[0])
            using (AMO.MeasureGroup currentMG = modelCube.MeasureGroups[currentMeasureGroupId])
            using (AMO.ReferenceMeasureGroupDimension newReferenceMGDim = new AMO.ReferenceMeasureGroupDimension())
            {
                newReferenceMGDim.CubeDimensionID = pkTableId;
                newReferenceMGDim.IntermediateCubeDimensionID = foreignTableId;
                newReferenceMGDim.IntermediateGranularityAttributeID = foreignColumnId;
                //  Replicating attributes (columns) from dimension
                foreach (AMO.CubeAttribute PKAttribute in modelCube.Dimensions[pkTableId].Attributes)
                {
                    using (AMO.MeasureGroupAttribute PKMGAttribute = newReferenceMGDim.Attributes.Add(PKAttribute.AttributeID))
                    using (AMO.DataItem dataItem = new AMO.DataItem(pkTableId, PKAttribute.AttributeID, PKAttribute.Attribute.KeyColumns[0].DataType))
                    using (AMO.ColumnBinding columnBinding = new AMO.ColumnBinding(pkTableId, PKAttribute.AttributeID))
                    {
                        PKMGAttribute.KeyColumns.Add(dataItem);
                        PKMGAttribute.KeyColumns[0].Source = columnBinding;
                    }
                }
                newReferenceMGDim.Attributes[pkColumnId].Type = AMO.MeasureGroupAttributeType.Granularity;

                //  If relationship is not null or empty then this is a direct relationship and 
                //  has to have Materialization.Regular
                if (!relationshipId.IsNullOrEmptyOrWhitespace())
                {
                    newReferenceMGDim.Materialization = AMO.ReferenceDimensionMaterialization.Regular;
                    newReferenceMGDim.RelationshipID = relationshipId;
                }
                else
                {
                    newReferenceMGDim.Materialization = AMO.ReferenceDimensionMaterialization.Indirect;
                }

                //  Adding the ReferenceMeasureGroupDimension to the measure group
                currentMG.Dimensions.Add(newReferenceMGDim);
            }
        }



    }
}
