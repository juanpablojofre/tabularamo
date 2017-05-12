/*=====================================================================
  
    File:      GetDatabaseSchema.SQLOLEDB.cs

    Summary:   This part of GetDatabaseSchema implements the  
               specifics of getting the database schema for 
               a SQLOLEDB provider
 
               This is part of the AMO2Tabular.HelperFunctions 
 
               AMO to Tabular (AMO2Tabular) is sample code to show and 
               explain how to use AMO to handle Tabular model objects. 
               The sample can be seen as a sample library of functions
               with the necessary code to execute each particular 
               action or operation over a logical tabular object. 

    Authors:   JuanPablo Jofre (jpjofre@microsoft.com)
    Date:	   21-Apr-2012
  
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
using System.Data;
using System.Data.OleDb;
using System.Globalization;


namespace MicrosoftSql2012Samples.Amo2Tabular
{


    public static partial class AMO2Tabular
    {
        #region SQL OLE DB constants
        const string sqlOleDbTablesSelect =
            @"SELECT TABLE_NAME, TABLE_TYPE " +
            @"FROM INFORMATION_SCHEMA.TABLES " +
            @"WHERE TABLE_NAME <> 'sysdiagrams'" + //In MS SQL, [sysdiagrams] is a user table -not a system table-; therefore, needs to be filtered: better here
            @"ORDER BY TABLE_NAME ";

        const string sqlOleDbColumnsSelect =
            @"SELECT TABLE_NAME, " +
            @"       COLUMN_NAME, " +
            @"       ORDINAL_POSITION, " +
            @"       COLUMN_DEFAULT, " +
            @"       IS_NULLABLE, " +
            @"       DATA_TYPE, " +
            @"       CHARACTER_MAXIMUM_LENGTH, " +
            @"       CHARACTER_OCTET_LENGTH, " +
            @"       NUMERIC_PRECISION, " +
            @"       NUMERIC_PRECISION_RADIX, " +
            @"       NUMERIC_SCALE, " +
            @"       DATETIME_PRECISION, " +
            @"       CHARACTER_SET_NAME, " +
            @"       COLLATION_NAME " +
            @"FROM INFORMATION_SCHEMA.COLUMNS " +
            @"WHERE TABLE_NAME <> 'sysdiagrams'" + //In MS SQL, [sysdiagrams] is a user table -not a system table-; therefore, needs to be filtered: better here
            @"ORDER BY TABLE_NAME, ORDINAL_POSITION ";

        const string sqlOleDbRelationshipsSelect =
            @"SELECT RC.CONSTRAINT_NAME + ' --> ' + RC.UNIQUE_CONSTRAINT_NAME AS [Relationship], " +
            @"       CUFK.TABLE_NAME AS [Foreign Table], " +
            @"       FKKU.ORDINAL_POSITION [FK Key Usage Ordinal], " +
            @"       CUFK.COLUMN_NAME AS [Foreign Column], " +
            @"       CUPK.TABLE_NAME AS [PK Table], " +
            @"       PKKU.ORDINAL_POSITION [PK Key Usage Ordinal], " +
            @"       CUPK.COLUMN_NAME AS [PK Column] " +
            @"FROM Information_Schema.Referential_Constraints RC " +
            @"JOIN Information_Schema.Constraint_Column_Usage CUFK on RC.CONSTRAINT_NAME = CUFK.CONSTRAINT_NAME " +
            @"JOIN Information_Schema.Constraint_Column_Usage CUPK on RC.UNIQUE_CONSTRAINT_NAME = CUPK.CONSTRAINT_NAME " +
            @"JOIN Information_Schema.Key_Column_Usage FKKU on CUFK.CONSTRAINT_NAME = FKKU.CONSTRAINT_NAME AND CUFK.COLUMN_NAME = FKKU.COLUMN_NAME " +
            @"JOIN Information_Schema.Key_Column_Usage PKKU on CUPK.CONSTRAINT_NAME = PKKU.CONSTRAINT_NAME AND CUPK.COLUMN_NAME = PKKU.COLUMN_NAME " +
            @"WHERE FKKU.ORDINAL_POSITION = PKKU.ORDINAL_POSITION " +
            @"ORDER BY 1, CUFK.TABLE_NAME, FKKU.ORDINAL_POSITION, CUFK.COLUMN_NAME";

        const string sqlOleDbPrimaryKeysSelect =
            @"SELECT TC.CONSTRAINT_TYPE, TC.CONSTRAINT_NAME, KCU.TABLE_NAME, KCU.ORDINAL_POSITION, KCU.COLUMN_NAME " +
            @"FROM Information_Schema.Table_Constraints TC " +
            @"JOIN Information_Schema.Key_Column_Usage KCU  on TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME " +
            @"WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY' " +
            @"ORDER BY KCU.TABLE_NAME, KCU.ORDINAL_POSITION, KCU.COLUMN_NAME";

        const string sqlOleDbAlternateUniqueKeySelect =
            @"SELECT TC.CONSTRAINT_TYPE, TC.CONSTRAINT_NAME, KCU.TABLE_NAME, KCU.ORDINAL_POSITION, KCU.COLUMN_NAME " +
            @"FROM Information_Schema.Table_Constraints TC " +
            @"JOIN Information_Schema.Key_Column_Usage KCU  on TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME " +
            @"WHERE TC.CONSTRAINT_TYPE = 'UNIQUE' " +
            @"ORDER BY KCU.TABLE_NAME, TC.CONSTRAINT_NAME, KCU.ORDINAL_POSITION, KCU.COLUMN_NAME";
        #endregion

        /// <summary>
        /// Obtains a basic database schema from a Microsoft SQL Server database
        /// </summary>
        /// <param name="sqlOleDbConnectionString">The connection string to a MS SQlServer database</param>
        /// <returns>A Dataset object with the schema of the database. No data is loaded.</returns>
        private static DataSet GetDatabaseSchemaSQLOLEDB(string sqlOleDbConnectionString)
        {
            //  Major steps in obtaining the schema from the database
            //
            //  -   Build basic metadata tables from where to derive the schema
            //      -   Tables table
            //      -   Columns table
            //      -   Relationships table
            //      -   PrimaryKeys table
            //      -   AlternateUniquekeys table
            //  -   Add each table to the dataset
            //      -   when adding columns use MapMsSqlType() to map MS SQL data types to .Net types
            //  -   Create Table (use the TableAdd function to create the table)
            //
            //  Note:   There are no validations for the connection string received

            using (DataSet dsv = new DataSet())
            using (DataTable tables = new DataTable("Tables"))
            using (DataTable columns = new DataTable("Columns"))
            using (DataTable relationships = new DataTable("Relationships"))
            using (DataTable primaryKeys = new DataTable("PrimaryKeys"))
            using (DataTable uniqueKeys = new DataTable("UniqueKeys"))
            {

                #region Build Metadata: Tables, Relationships, Primary Keys and alternate Unique Keys
                using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                using (OleDbConnection cnx = new OleDbConnection(sqlOleDbConnectionString))
                {
                    cnx.Open();
                    adapter.SelectCommand = new OleDbCommand(sqlOleDbTablesSelect, cnx);
                    adapter.Fill(tables);
                }

                using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                using (OleDbConnection cnx = new OleDbConnection(sqlOleDbConnectionString))
                {
                    cnx.Open();
                    adapter.SelectCommand = new OleDbCommand(sqlOleDbColumnsSelect, cnx);
                    adapter.Fill(columns);
                }

                using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                using (OleDbConnection cnx = new OleDbConnection(sqlOleDbConnectionString))
                {
                    cnx.Open();
                    adapter.SelectCommand = new OleDbCommand(sqlOleDbRelationshipsSelect, cnx);
                    adapter.Fill(relationships);
                }

                using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                using (OleDbConnection cnx = new OleDbConnection(sqlOleDbConnectionString))
                {
                    cnx.Open();
                    adapter.SelectCommand = new OleDbCommand(sqlOleDbPrimaryKeysSelect, cnx);
                    adapter.Fill(primaryKeys);
                }

                using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                using (OleDbConnection cnx = new OleDbConnection(sqlOleDbConnectionString))
                {
                    cnx.Open();
                    adapter.SelectCommand = new OleDbCommand(sqlOleDbAlternateUniqueKeySelect, cnx);
                    adapter.Fill(uniqueKeys);
                }
                #endregion

                #region Add Table Schemas
                //  I have found that the FillSchema function does not map correctly some of MS SQL data types
                //  ==> The table schema has to be built by hand
                //  -   Data types are mapped using MapMsSqlType()

                //  Iterating over all user tables in database
                foreach (DataRow row in tables.Rows)
                {
                    using (DataTable table = new DataTable(row["TABLE_NAME"].ToString()))
                    {
                        //  Obtaining calculatedColumn information for current table
                        DataRow[] columnsInfo = columns.Select(string.Format(CultureInfo.InvariantCulture, "TABLE_NAME = '{0}'", table.TableName), "ORDINAL_POSITION ASC");

                        for (int i = 0; i < columnsInfo.Length; i++)
                        {
                            using (DataColumn column = new DataColumn(columnsInfo[i]["COLUMN_NAME"].ToString()))
                            {
                                column.DataType = MapMsSqlType(columnsInfo[i]["DATA_TYPE"].ToString());
                                //  Obtaining Max string length... passing it as out parameter
                                int maxLength;
                                if ((column.DataType == typeof(string)) && int.TryParse(columnsInfo[i]["CHARACTER_MAXIMUM_LENGTH"].ToString(), out maxLength))
                                    column.MaxLength = maxLength;

                                column.AllowDBNull = (0 == string.Compare(columnsInfo[i]["IS_NULLABLE"].ToString(), "YES", StringComparison.OrdinalIgnoreCase)) ? true : false;
                                table.Columns.Add(column);
                            }
                        }
                        dsv.Tables.Add(table);
                    }
                }
                #endregion

                #region Add Primary Keys
                //  Iterating over all tables in DSV
                foreach (DataTable table in dsv.Tables)
                {
                    //  Get the columns that form the PK from the Information_Schema, for the given table in DSV
                    DataRow[] expectedKeys = primaryKeys.Select(string.Format(CultureInfo.InvariantCulture, "TABLE_NAME = '{0}'", table.TableName), "ORDINAL_POSITION ASC");

                    if (expectedKeys.Length == 0)
                    {// The table should not have PK according to Information_Schema; set DSV accordingly
                        table.PrimaryKey = null;
                    }
                    else
                    {// Add PK to table
                        DataColumn[] pk = new DataColumn[expectedKeys.Length];
                        for (int i = 0; i < expectedKeys.Length; i++)
                        {
                            pk[i] = table.Columns[expectedKeys[i]["COLUMN_NAME"].ToString()];
                        }
                        table.PrimaryKey = pk;
                    }

                }
                #endregion

                #region Add relationships
                //
                //  Note:   In Dataset notation the Primary Key side of a relationship is called Parent side
                //          following the same logic the Foreign Key side of the relationship is called Child side
                //      *   The naming convention used in this section of the function uses the Dataset nomenclature
                //          -   Primary Key ==> ParentColumns
                //          -   Foreign Key ==> ChildColumns
                //
                //  Algorithm strategy: In one pass read all relationships and create them in the the DSV dataset
                //                      -   Collect relationship columns until relationship name is different
                //                      -   Add collected relationship columns to DSV dataset with the former relationship name
                //                      -   Clear calculatedColumn collections and reset relationship name
                //                      -   Add current columns to collections and repeat loop
                //


                string relationshipName = relationships.Rows.Count > 0 ? relationships.Rows[0]["Relationship"].ToString() : null;   //Assign initial value to relationshipName from the first relationship in the list
                List<DataColumn> parentColumns = new List<DataColumn>();
                List<DataColumn> childColumns = new List<DataColumn>();
                for (int i = 0; i < relationships.Rows.Count; i++)
                {
                    if (relationshipName != relationships.Rows[i]["Relationship"].ToString())
                    {
                        //  Add relationship from previous collected information
                        dsv.Relations.Add(relationshipName, parentColumns.ToArray(), childColumns.ToArray());
                        parentColumns.Clear();
                        childColumns.Clear();
                        relationshipName = relationships.Rows[i]["Relationship"].ToString();
                    }
                    //  Save current record info
                    parentColumns.Add(dsv.Tables[relationships.Rows[i]["PK Table"].ToString()].Columns[relationships.Rows[i]["PK Column"].ToString()]);
                    childColumns.Add(dsv.Tables[relationships.Rows[i]["Foreign Table"].ToString()].Columns[relationships.Rows[i]["Foreign Column"].ToString()]);
                }
                dsv.Relations.Add(relationshipName, parentColumns.ToArray(), childColumns.ToArray()); //Add last relationship to DSV

                #endregion

                #region Add alternate unique keys to DSV
                //  Iterating over all tables in DSV
                foreach (DataTable table in dsv.Tables)
                {
                    //  Get the columns that form the Alternate Unique Key from the Information_Schema, for the given table in DSV
                    DataRow[] expectedKeys = uniqueKeys.Select(string.Format(CultureInfo.InvariantCulture, "TABLE_NAME = '{0}'", table.TableName), "CONSTRAINT_NAME ASC, ORDINAL_POSITION ASC");

                    //  Add unique constraints from Information_Schema to the table
                    if (expectedKeys.Length > 0)
                    {
                        string constraintName = expectedKeys[0]["CONSTRAINT_NAME"].ToString();
                        List<DataColumn> uniqueColumns = new List<DataColumn>();

                        for (int i = 0; i < expectedKeys.Length; i++)
                        {
                            if (constraintName != expectedKeys[i]["CONSTRAINT_NAME"].ToString())
                            {
                                table.Constraints.Add(new UniqueConstraint(constraintName, uniqueColumns.ToArray()));
                                constraintName = expectedKeys[i]["CONSTRAINT_NAME"].ToString();
                                uniqueColumns.Clear();
                            }
                            uniqueColumns.Add(table.Columns[expectedKeys[i]["COLUMN_NAME"].ToString()]);
                        }
                        table.Constraints.Add(new UniqueConstraint(constraintName, uniqueColumns.ToArray()));
                    }
                }
                #endregion

                return dsv;
            }
        }

        private static Type MapMsSqlType(string msSqlType)
        {
            switch (msSqlType)
            {
                case "bigint":
                    return typeof(Int64);
                case "binary":
                    return typeof(byte[]);
                case "bit":
                    return typeof(bool);
                case "char":
                    return typeof(string);
                case "date":
                case "datetime":
                case "datetime2":
                    return typeof(DateTime);
                case "datetimeoffset":
                    return typeof(DateTimeOffset);
                case "decimal":
                    return typeof(decimal);
                case "float":
                    return typeof(float);
                case "geography":
                case "geometry":
                case "hierarchyid":
                    return typeof(object);
                case "image":
                    return typeof(byte[]);
                case "int":
                    return typeof(Int32);
                case "money":
                    return typeof(decimal);
                case "nchar":
                    return typeof(string);
                case "ntext":
                    return typeof(string);
                case "numeric":
                    return typeof(decimal);
                case "nvarchar":
                    return typeof(string);
                case "real":
                    return typeof(Single);
                case "smalldatetime":
                    return typeof(DateTime);
                case "smallint":
                    return typeof(Int16);
                case "smallmoney":
                    return typeof(decimal);
                case "sql_variant":
                    return typeof(object);
                case "sysname":
                    return typeof(string);
                case "text":
                    return typeof(string);
                case "time":
                    return typeof(TimeSpan);
                case "timestamp":
                    return typeof(byte[]);
                case "tinyint":
                    return typeof(byte);
                case "uniqueidentifier":
                    return typeof(Guid);
                case "varbinary":
                    return typeof(byte[]);
                case "varchar":
                    return typeof(string);
                case "xml":
                    return typeof(string);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}