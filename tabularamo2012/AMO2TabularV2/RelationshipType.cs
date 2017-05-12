/*=====================================================================
  
    File:      Relationship.cs

    Summary:   LevelInfo is a structure that holds level 
               information for a hierarchy. 

    Authors:   JuanPablo Jofre (jpjofre@microsoft.com)
    Date:      04-Apr-2012
  
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
using System.Globalization;
using System.Linq;
using AMO = Microsoft.AnalysisServices;


namespace MicrosoftSql2012Samples.Amo2Tabular
{

    public struct RelationshipPair : IComparable<RelationshipPair>
    {
        public FullName PrimaryKeyEnd { get; internal set; }
        public FullName ForeignKeyEnd { get; internal set; }

        public RelationshipPair(FullName primaryKeyEnd, FullName foreignKeyEnd)
            : this()
        {
            PrimaryKeyEnd = primaryKeyEnd;
            ForeignKeyEnd = foreignKeyEnd;
        }

        int IComparable<RelationshipPair>.CompareTo(RelationshipPair relationship)
        {
            return string.Compare(this.ToString(), relationship.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object relationship)
        {
            return string.Compare(this.ToString(), relationship.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public static bool operator ==(RelationshipPair fn1, RelationshipPair fn2)
        {
            return fn1.Equals(fn2);
        }

        public static bool operator !=(RelationshipPair fn1, RelationshipPair fn2)
        {
            return !fn1.Equals(fn2);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} <-- {1}", PrimaryKeyEnd.ToString(), ForeignKeyEnd.ToString());
        }

    }

    public class RelationshipGraphNode
    {
        public List<RelationshipPair> ForeignKeyUp { get; internal set; }
        public List<RelationshipPair> PrimaryKeyDown { get; internal set; }

        public RelationshipGraphNode()
        {
            ForeignKeyUp = new List<RelationshipPair>();
            PrimaryKeyDown = new List<RelationshipPair>();
        }
        public void Clear()
        {
            if (ForeignKeyUp != null) ForeignKeyUp.Clear();
            if (PrimaryKeyDown != null) PrimaryKeyDown.Clear();
        }

    }

    public class RelationshipGraph
    {
        Dictionary<string, RelationshipGraphNode> graph = new Dictionary<string, RelationshipGraphNode>();

        public RelationshipGraph(AMO.Database tabularDatabase)
        {
            foreach (TableInfo tableInfo in AMO2Tabular.TablesEnumerateFull(tabularDatabase))
            {
                using (AMO.MeasureGroup currentMeasureGroup = tabularDatabase.Cubes[0].MeasureGroups[tableInfo.DataSourceName])
                {
                    foreach (AMO.MeasureGroupDimension measureGroupDimension in currentMeasureGroup.Dimensions)
                    {
                        if (measureGroupDimension is AMO.ReferenceMeasureGroupDimension)
                        {
                            AMO.ReferenceMeasureGroupDimension referencedTable = measureGroupDimension as AMO.ReferenceMeasureGroupDimension;

                            string foreignTableId = referencedTable.IntermediateCubeDimensionID;
                            string foreignColumnId = referencedTable.IntermediateGranularityAttributeID;
                            string primaryKeyTableId = referencedTable.CubeDimensionID;
                            string primaryKeyColumnId = string.Empty;
                            foreach (AMO.MeasureGroupAttribute attribute in referencedTable.Attributes)
                            {
                                if (attribute.Type == AMO.MeasureGroupAttributeType.Granularity)
                                {
                                    primaryKeyColumnId = attribute.AttributeID;
                                    break;
                                }
                            }

                            FullName foreignKeyEnd = new FullName(foreignTableId, foreignColumnId);
                            FullName primaryKeyEnd = new FullName(primaryKeyTableId, primaryKeyColumnId);
                            RelationshipPair relationship = new RelationshipPair(primaryKeyEnd, foreignKeyEnd);
                            AddForeignKeyUpPair(primaryKeyTableId, relationship);
                            AddPrimaryKeyDownPair(foreignTableId, relationship);
                        }
                    }
                }
            }
        }

        public void Clear()
        {
            if (graph != null)
                foreach (RelationshipGraphNode node in graph.Values)
                    node.Clear();
            graph.Clear();

        }

        public void AddTableNode(string tableId)
        {
            graph.Add(tableId, new RelationshipGraphNode());
        }

        public void AddForeignKeyUpPair(string tableId, RelationshipPair relationshipPair)
        {
            if (!graph.ContainsKey(tableId))
                AddTableNode(tableId);
            if (!graph[tableId].ForeignKeyUp.Contains(relationshipPair))
                graph[tableId].ForeignKeyUp.Add(relationshipPair);
        }

        public void AddPrimaryKeyDownPair(string tableId, RelationshipPair relationshipPair)
        {
            if (!graph.ContainsKey(tableId))
                AddTableNode(tableId);
            if (!graph[tableId].PrimaryKeyDown.Contains(relationshipPair))
                graph[tableId].PrimaryKeyDown.Add(relationshipPair);
        }


        public List<string> TableListPrimaryKeyDown(string startingTableId)
        {
            List<string> tablesPrimaryKeyDown = new List<string>();
            if (graph.ContainsKey(startingTableId))
                foreach (RelationshipPair relationshipPair in graph[startingTableId].PrimaryKeyDown)
                {
                    tablesPrimaryKeyDown.Add(relationshipPair.PrimaryKeyEnd.TableId);
                    tablesPrimaryKeyDown.AddRange(TableListPrimaryKeyDown(relationshipPair.PrimaryKeyEnd.TableId));
                }
            return tablesPrimaryKeyDown;
        }

        public List<string> TableListForeignKeyUp(string startingTableId)
        {
            List<string> tablesForeignKeyUp = new List<string>();
            if (graph.ContainsKey(startingTableId))
                foreach (RelationshipPair relationshipPair in graph[startingTableId].ForeignKeyUp)
                {
                    tablesForeignKeyUp.Add(relationshipPair.ForeignKeyEnd.TableId);
                    tablesForeignKeyUp.AddRange(TableListForeignKeyUp(relationshipPair.ForeignKeyEnd.TableId));
                }
            return tablesForeignKeyUp;
        }

        public List<RelationshipPair> RelationshipsListPrimaryKeyDown(string startingTableId)
        {
            List<RelationshipPair> relationshipsPrimaryKeyDown = new List<RelationshipPair>();
            if (graph.ContainsKey(startingTableId))
                foreach (RelationshipPair relationshipPair in graph[startingTableId].PrimaryKeyDown)
                {
                    relationshipsPrimaryKeyDown.Add(relationshipPair);
                    relationshipsPrimaryKeyDown.AddRange(RelationshipsListPrimaryKeyDown(relationshipPair.PrimaryKeyEnd.TableId));
                }
            return relationshipsPrimaryKeyDown;
        }

        public bool RelationshipAlternatePathExists(string primaryTableId, string foreignTable)
        {
            List<string> tablesBelowPrimaryKeyEnd = TableListPrimaryKeyDown(primaryTableId);
            List<string> tablesAboveForeignKeyEnd = TableListForeignKeyUp(foreignTable);
            tablesAboveForeignKeyEnd.Add(foreignTable);
            foreach (string foreignKeyUpTableId in tablesAboveForeignKeyEnd)
            {
                List<string> candidateTablesBelowPrimaryKeyEndForForeignKeyUp = TableListPrimaryKeyDown(foreignKeyUpTableId);
                if (candidateTablesBelowPrimaryKeyEndForForeignKeyUp.Count == 0)
                    continue;
                if (candidateTablesBelowPrimaryKeyEndForForeignKeyUp.Contains(primaryTableId))
                {
                    return true;
                }
                if (candidateTablesBelowPrimaryKeyEndForForeignKeyUp.Intersect(tablesBelowPrimaryKeyEnd).Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

    }

}
