/*=====================================================================
  
    File:      AdventureWorksTabularAMO2012.cs

    Summary:   AdventureWorks Tabular Model AMO 2012 (AdventureWorksTabularAMO)
               is sample code to show and explain how to use the AMO2Tabular 
               library to build a replica of ‘AdventureWorks Tabular Model 
               SQL Server 2012’. 
               The sample should try to use as much as possible functions from 
               the library, as the sample is the test case scenario for the 
               library. 

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

//  #define localUsersCreated


using System;
using System.Globalization;
using System.Reflection;
using MicrosoftSql2012Samples.AdventureWorksTabularAMO.Properties;
using MicrosoftSql2012Samples.Amo2Tabular;
using AMO = Microsoft.AnalysisServices;

namespace MicrosoftSql2012Samples.AdventureWorksTabularAMO
{
    class AdventureWorksTabularAMO2012
    {

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "CurrencyKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DueDateKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DateKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EmployeeKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "GeographyKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OrderDateKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ProductCategoryKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ProductKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ProductSubcategoryKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PromotionKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RelationshipAdd"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "SalesTerritoryKey"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ShipDateKey")]
        static void Main()
        {
            using (AMO.Server server = new AMO.Server())
            {
                server.Connect(Resources.Localhost);

                //  Creating Database object and adding Connection object, in one step
                using (AMO.Database AMO2TabularDb = AMO2Tabular.TabularDatabaseAdd(server,
                                                                                    Resources.AdventureWorksTabularAmoDbName,
                                                                                    Resources.dataSourceOleDb,
                                                                                    Resources.AdventureWorksTabularAmoDbName,
                                                                                    dbCompatibilityLevel:1103))
                {
                    //  Create tabular model schema 
                    //  --> Add tables, table componentes and update to match model
                    //      --> Add / Update Calculated Columns
                    //      --> Add / Update Measures and KPIs
                    //      --> Add / Update Hierarchies
                    //  --> Add / Update Relationships
                    //  --> Add objects with dependencies
                    //  --> Add Perpectives
                    //  --> Add roles, users and RLS
                    //  --> Update server with local schema
                    //  --> Process the model

                    #region  Add tables, table componentes and update to match model
                    //  -   Date is the first table created in this sample; 
                    //      so, needs to convey the cube name and use the TableAddFirstTable
                    CreateDateTable(AMO2TabularDb, Resources.cubeName);

                    CreateProductCategoryTable(AMO2TabularDb);

                    CreateProductSubcategoryTable(AMO2TabularDb);

                    CreateProductTable(AMO2TabularDb);

                    CreateCurrencyTable(AMO2TabularDb);

                    CreatePromotionTable(AMO2TabularDb);

                    CreateGeographyTable(AMO2TabularDb);

                    CreateEmployeeTable(AMO2TabularDb);

                    CreateCustomerTable(AMO2TabularDb);

                    CreateResellerTable(AMO2TabularDb);

                    CreateResellerSalesTable(AMO2TabularDb);

                    CreateInternetSalesTable(AMO2TabularDb);

                    CreateSalesTerritoryTable(AMO2TabularDb);

                    CreateProductInventoryTable(AMO2TabularDb);

                    CreateSalesQuotaTable(AMO2TabularDb);
                    #endregion

                    #region  Add Relationships
                    //  Note: By AMO2tabular default, and following tabular model best coding practices, each RelationshipAdd invocation updates the server instance
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Currency", "Currency Id", "Internet Sales", "Currency Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Currency", "Currency Id", "Internet Sales", "Currency Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Currency", "Currency Id", "Reseller Sales", "Currency Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Currency", "Currency Id", "Reseller Sales", "Currency Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Customer", "Customer Id", "Internet Sales", "Customer Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Customer", "Customer Id", "Internet Sales", "Customer Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Date", "Date Id", "Internet Sales", "Order Date Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Date", "Date Id", "Internet Sales", "Order Date Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Date", "Date Id", "Internet Sales", "Ship Date Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Date", "Date Id", "Internet Sales", "Ship Date Id", false);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Date", "Date Id", "Internet Sales", "DueDate Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Date", "Date Id", "Internet Sales", "Due Date Id", false);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Date", "Date Id", "Product Inventory", "Date Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Date", "Date Id", "Product Inventory", "Date Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Date", "Date Id", "Reseller Sales", "OrderDate Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Date", "Date Id", "Reseller Sales", "Order Date Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Date", "Date Id", "Reseller Sales", "ShipDate Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Date", "Date Id", "Reseller Sales", "Ship Date Id", false);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Date", "Date Id", "Reseller Sales", "DueDate Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Date", "Date Id", "Reseller Sales", "Due Date Id", false);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Date", "Date Id", "Sales Quota", "Date Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Date", "Date Id", "Sales Quota", "Date Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Employee", "Employee Id", "Reseller Sales", "Employee Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Employee", "Employee Id", "Reseller Sales", "Employee Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Employee", "Employee Id", "Sales Quota", "Employee Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Employee", "Employee Id", "Sales Quota", "Employee Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Geography", "Geography Id", "Customer", "Geography Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Geography", "Geography Id", "Customer", "Geography Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Geography", "Geography Id", "Reseller", "Geography Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Geography", "Geography Id", "Reseller", "Geography Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Product Category", "Product Category Id", "Product Subcategory", "ProductCategory Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Product Category", "Product Category Id", "Product Subcategory", "Product Category Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Product Subcategory", "Product Subcategory Id", "Product", "ProductSubcategory Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Product Subcategory", "Product Subcategory Id", "Product", "Product Subcategory Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Product", "Product Id", "Internet Sales", "Product Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Product", "Product Id", "Internet Sales", "Product Id");


                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Product", "Product Id", "Product Inventory", "Product Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Product", "Product Id", "Product Inventory", "Product Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Product", "Product Id", "Reseller Sales", "Product Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Product", "Product Id", "Reseller Sales", "Product Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Promotion", "Promotion Id", "Internet Sales", "Promotion Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Promotion", "Promotion Id", "Internet Sales", "Promotion Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Promotion", "Promotion Id", "Reseller Sales", "Promotion Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Promotion", "Promotion Id", "Reseller Sales", "Promotion Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Reseller", "Reseller Id", "Reseller Sales", "Reseller Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Reseller", "Reseller Id", "Reseller Sales", "Reseller Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Sales Territory", "Sales Territory Id", "Employee", "Sales Territory Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Sales Territory", "Sales Territory Id", "Employee", "Sales Territory Id", false);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Sales Territory", "Sales Territory Id", "Geography", "Sales Territory Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Sales Territory", "Sales Territory Id", "Geography", "Sales Territory Id", false);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Sales Territory", "Sales Territory Id", "Internet Sales", "Sales Territory Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Sales Territory", "Sales Territory Id", "Internet Sales", "Sales Territory Id");

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingRelationshipAddConsoleMessage, "Sales Territory", "Sales Territory Id", "Reseller Sales", "Sales Territory Id"));
                    AMO2Tabular.RelationshipAdd(AMO2TabularDb, "Sales Territory", "Sales Territory Id", "Reseller Sales", "Sales Territory Id");
                    #endregion

                    #region Add objects with dependencies on relationships
                    //  Objects on 'Employee' with dependencies
                    string tableName = "Employee";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingObjectsWithDependenciesConsoleMessage, tableName));
                    //  Add Calculated Columns
                    AMO2Tabular.CalculatedColumnAdd(AMO2TabularDb, tableName, "Hierarchy", "PATH(Employee[Employee Id], Employee[Manager Id])", false);

                    //  Objects on 'Product' with dependencies
                    tableName = "Product";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingObjectsWithDependenciesConsoleMessage, tableName));
                    //      Add Calculated Columns
                    AMO2Tabular.CalculatedColumnAdd(AMO2TabularDb, tableName, "Product SubCategory Name", "RELATED('Product SubCategory'[Product Subcategory Name])", true);
                    AMO2Tabular.CalculatedColumnAdd(AMO2TabularDb, tableName, "Product Category Name", "RELATED('Product Category'[Product Category Name])", true);

                    //      Add Hierarchies
                    AMO2Tabular.HierarchyAdd(AMO2TabularDb, tableName, "Category", true, new LevelInfo[] { new LevelInfo("Product Category Name", "Category"), 
                                                                                                           new LevelInfo("Product Subcategory Name", "Subcategory"),
                                                                                                           new LevelInfo("Model Name", "Model"),
                                                                                                           new LevelInfo("Product Name", "Product")});

                    //  Objects on 'Product Inventory' with dependencies
                    tableName = "Product Inventory";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingObjectsWithDependenciesConsoleMessage, tableName));
                    //      Add Calculated Columns
                    AMO2Tabular.CalculatedColumnAdd(AMO2TabularDb, tableName, "Product-Date Optimal Inventory Value", "[Unit Cost]*RELATED('Product'[Reorder Point])");
                    AMO2Tabular.CalculatedColumnAdd(AMO2TabularDb, tableName, "Product-Date Max Inventory Value", "[Unit Cost]*RELATED('Product'[Safety Stock Level])");
                    AMO2Tabular.CalculatedColumnAdd(AMO2TabularDb, tableName, "Product-Date OverStocked", "IF([Units Balance]>RELATED('Product'[Safety Stock Level]),1,0)");
                    AMO2Tabular.CalculatedColumnAdd(AMO2TabularDb, tableName, "Product-Date UnderStocked", "IF([Units Balance]<RELATED('Product'[Reorder Point]),1,0)");

                    //      Add Measures
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Inventory Value", "CALCULATE(SUM([Product-Date Inventory Value]),LASTDATE('Product Inventory'[Movement Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Units", "CALCULATE(SUM([Units Balance]),LASTDATE('Product Inventory'[Movement Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Inventory Optimal Value", "CALCULATE(SUM([Product-Date Optimal Inventory Value]),LASTDATE('Product Inventory'[Movement Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Inventory Maximum Value", "CALCULATE(SUM([Product-Date Max Inventory Value]),LASTDATE('Product Inventory'[Movement Date]))");

                    //  Add KPIs (adding first the underlying measure)
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Inventory Value Performance", "IF([Total Inventory Value]<[Total Inventory Optimal Value],-(1-([Total Inventory Value]/[Total Inventory Optimal Value])),([Total Inventory Value]-[Total Inventory Optimal Value])/([Total Inventory Maximum Value]-[Total Inventory Optimal Value]))");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Total Inventory Value Performance", "0", "Case When IsEmpty(KpiValue(\"Total Inventory Value Performance\")) Then Null When KpiValue(\"Total Inventory Value Performance\") < -0.2 Then -1 When KpiValue(\"Total Inventory Value Performance\") >= -0.2 And KpiValue(\"Total Inventory Value Performance\") < 0 Then 0 When KpiValue(\"Total Inventory Value Performance\") >= 0 And KpiValue(\"Total Inventory Value Performance\") < 0.6 Then 1 When KpiValue(\"Total Inventory Value Performance\") >= 0.6 And KpiValue(\"Total Inventory Value Performance\") < 1 Then 0 Else -1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Total Inventory Value Performance", "0", "IF(ISBLANK([Total Inventory Value Performance]), BLANK(), IF([Total Inventory Value Performance] < -0.2, -1, IF([Total Inventory Value Performance] < 0, 0, IF([Total Inventory Value Performance] < 0.6, 1, IF([Total Inventory Value Performance] < 1, 0, -1)))))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Products OverStocked", "CALCULATE(SUM([Product-Date OverStocked]),LASTDATE('Product Inventory'[Movement Date]))");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                    AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Products OverStocked", "0", "Case When IsEmpty(KpiValue(\"Products OverStocked\")) Then Null When KpiValue(\"Products OverStocked\") < 1 Then 1 When KpiValue(\"Products OverStocked\") >= 1 And KpiValue(\"Products OverStocked\") < 5 Then 0 Else -1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Products OverStocked", "0", "IF(ISBLANK([Products OverStocked]), BLANK(), IF([Products OverStocked] < 1, 1, IF([Products OverStocked] < 5, 0, -1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Products UnderStocked", "CALCULATE(SUM([Product-Date UnderStocked]),LASTDATE('Product Inventory'[Movement Date]))");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Products UnderStocked", "0", "Case When IsEmpty(KpiValue(\"Products UnderStocked\")) Then Null When KpiValue(\"Products UnderStocked\") < 5 Then 1 When KpiValue(\"Products UnderStocked\") >= 5 And KpiValue(\"Products UnderStocked\") < 15 Then 0 Else -1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Products UnderStocked", "0", "IF(ISBLANK([Products UnderStocked]), BLANK(), IF([Products UnderStocked] < 5, 1, IF([Products UnderStocked] < 15, 0, -1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Products with Negative Stock", "CALCULATE(SUM([Product-Date Negative Stock]),LASTDATE('Product Inventory'[Movement Date]))");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Products with Negative Stock", "0", "Case When IsEmpty(KpiValue(\"Products with Negative Stock\")) Then Null When KpiValue(\"Products with Negative Stock\") < 0.1 Then 1 When KpiValue(\"Products with Negative Stock\") >= 0.1 And KpiValue(\"Products with Negative Stock\") < 5 Then 0 Else -1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Products with Negative Stock", "0", "IF(ISBLANK([Products with Negative Stock]), BLANK(), IF([Products with Negative Stock] < 0.1, 1, IF([Products with Negative Stock] < 5, 0, -1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }


                    //  Objects on 'Internet Sales' with dependencies
                    tableName = "Internet Sales";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingObjectsWithDependenciesConsoleMessage, tableName));
                    //      Add Measures
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Internet Previous Quarter Gross Profit", "CALCULATE([Internet Total Gross Profit], PREVIOUSQUARTER('Date'[Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Internet Current Quarter Gross Profit", "TOTALQTD([Internet Total Gross Profit], 'Date'[Date])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Internet Previous Quarter Gross Profit Proportion to QTD", "[Internet Previous Quarter Gross Profit]*([Days In Current Quarter to Date]/[Days In Current Quarter])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Internet Previous Quarter Sales", "CALCULATE([Internet Total Sales], PREVIOUSQUARTER('Date'[Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Internet Current Quarter Sales", "TOTALQTD([Internet Total Sales],'Date'[Date])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Internet Previous Quarter Sales Proportion to QTD", "[Internet Previous Quarter Sales]*([Days In Current Quarter to Date]/[Days In Current Quarter])");

                    //      Add KPIs (adding first the underlying measure)
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Internet Current Quarter Sales Performance", "IFERROR([Internet Current Quarter Sales]/[Internet Previous Quarter Sales Proportion to QTD],BLANK())");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Internet Current Quarter Sales Performance", "1.1", "Case When IsEmpty(KpiValue(\"Internet Current Quarter Sales Performance\")) Then Null When KpiValue(\"Internet Current Quarter Sales Performance\") < 0.8 Then -1 When KpiValue(\"Internet Current Quarter Sales Performance\") >= 0.8 And KpiValue(\"Internet Current Quarter Sales Performance\") < 1.07 Then 0 Else 1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Internet Current Quarter Sales Performance", "1.1", "IF(ISBLANK([Internet Current Quarter Sales Performance]), BLANK(), IF([Internet Current Quarter Sales Performance] < 0.8, -1, IF([Internet Current Quarter Sales Performance] < 1.07, 0, 1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Internet Current Quarter Gross Profit Performance", "IF([Internet Previous Quarter Gross Profit Proportion to QTD]<>0,([Internet Current Quarter Gross Profit]-[Internet Previous Quarter Gross Profit Proportion to QTD])/[Internet Previous Quarter Gross Profit Proportion to QTD],BLANK())");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Internet Current Quarter Gross Profit Performance", "1.25", "Case When IsEmpty(KpiValue(\"Internet Current Quarter Gross Profit Performance\")) Then Null When KpiValue(\"Internet Current Quarter Gross Profit Performance\") < 0.8 Then -1 When KpiValue(\"Internet Current Quarter Gross Profit Performance\") >= 0.8 And KpiValue(\"Internet Current Quarter Gross Profit Performance\") < 1.03 Then 0 Else 1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Internet Current Quarter Gross Profit Performance", "1.25", "IF(ISBLANK([Internet Current Quarter Gross Profit Performance]), BLANK(), IF([Internet Current Quarter Gross Profit Performance] < 0.8, -1, IF([Internet Current Quarter Gross Profit Performance] < 1.03, 0, 1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    //  Objects on 'Reseller Sales' with dependencies
                    tableName = "Reseller Sales";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingObjectsWithDependenciesConsoleMessage, tableName));
                    //      Add Measures
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Previous Quarter Gross Profit", "CALCULATE([Reseller Total Gross Profit], PREVIOUSQUARTER('Date'[Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Current Quarter Gross Profit", "TOTALQTD([Reseller Total Gross Profit], 'Date'[Date])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Previous Quarter Gross Profit Proportion to QTD", "[Reseller Previous Quarter Gross Profit]*([Days In Current Quarter to Date]/[Days In Current Quarter])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Previous Quarter Sales", "CALCULATE([Reseller Total Sales], PREVIOUSQUARTER('Date'[Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Current Quarter Sales", "TOTALQTD([Reseller Total Sales],'Date'[Date])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Previous Quarter Sales Proportion to QTD", "[Reseller Previous Quarter Sales]*([Days In Current Quarter]/[Days In Current Quarter])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Total Sales - Sales Territory sliced by Reseller", "CALCULATE(SUM('Reseller Sales'[Sales Amount]), USERELATIONSHIP(Reseller[ResellerKey], 'Reseller Sales'[ResellerKey]), USERELATIONSHIP(Geography[GeographyKey], Reseller[GeographyKey]), USERELATIONSHIP('Sales Territory'[SalesTerritoryKey], Geography[SalesTerritoryKey]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Total Sales - Sales Territory sliced by Employee", "CALCULATE(SUM('Reseller Sales'[Sales Amount]),USERELATIONSHIP(Employee[EmployeeKey],'Reseller Sales'[EmployeeKey]),USERELATIONSHIP('Sales Territory'[SalesTerritoryKey],Employee[SalesTerritoryKey]))");

                    //      Add KPIs (adding first the underlying measure)
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Current Quarter Sales Performance", "IFERROR([Reseller Current Quarter Sales]/[Reseller Previous Quarter Sales Proportion to QTD],BLANK())");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Reseller Current Quarter Sales Performance", "1.1", "Case When IsEmpty(KpiValue(\"Reseller Current Quarter Sales Performance\")) Then Null When KpiValue(\"Reseller Current Quarter Sales Performance\") < 0.8 Then -1 When KpiValue(\"Reseller Current Quarter Sales Performance\") >= 0.8 And KpiValue(\"Reseller Current Quarter Sales Performance\") < 1.07 Then 0 Else 1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Reseller Current Quarter Sales Performance", "1.1", "IF(ISBLANK([Reseller Current Quarter Sales Performance]), BLANK(), IF([Reseller Current Quarter Sales Performance] < 0.8, -1, IF([Reseller Current Quarter Sales Performance] < 1.07, 0, 1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Reseller Current Quarter Gross Profit Performance", "IF([Reseller Previous Quarter Gross Profit Proportion to QTD]<>0,([Reseller Current Quarter Gross Profit]-[Reseller Previous Quarter Gross Profit Proportion to QTD])/[Reseller Previous Quarter Gross Profit Proportion to QTD],BLANK())");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Reseller Current Quarter Gross Profit Performance", "1.25", "Case When IsEmpty(KpiValue(\"Reseller Current Quarter Gross Profit Performance\")) Then Null When KpiValue(\"Reseller Current Quarter Gross Profit Performance\") < 0.8 Then -1 When KpiValue(\"Reseller Current Quarter Gross Profit Performance\") >= 0.8 And KpiValue(\"Reseller Current Quarter Gross Profit Performance\") < 1.03 Then 0 Else 1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Reseller Current Quarter Gross Profit Performance", "1.25", "IF(ISBLANK([Reseller Current Quarter Gross Profit Performance]), BLANK(), IF([Reseller Current Quarter Gross Profit Performance] < 0.8, -1, IF([Reseller Current Quarter Gross Profit Performance] < 1.03, 0, 1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    //  Objects on 'Sales Territory' with dependencies
                    tableName = "Sales Territory";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingObjectsWithDependenciesConsoleMessage, tableName));
                    //      Add Measures
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Distinct Count Sales Orders", "[Reseller Distinct Count Sales Order] + [Internet Distinct Count Sales Order]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Sales", "[Reseller Total Sales] + [Internet Total Sales]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Discount Amount", "[Reseller Total Discount Amount] + [Internet Total Discount Amount]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Product Cost", "[Reseller Total Product Cost] + [Internet Total Product Cost]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Tax Amount", "[Reseller Total Tax Amount] + [Internet Total Tax Amount]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Freight", "[Reseller Total Freight] + [Internet Total Freight]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Gross Profit", "[Reseller Total Gross Profit] + [Internet Total Gross Profit]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Units Sold", "[Reseller Total Units] + [Internet Total Units]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Order Lines Count", "[Reseller Order Lines Count] + [Internet Order Lines Count]");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Previous Quarter Gross Profit", "CALCULATE([Total Gross Profit], PREVIOUSQUARTER('Date'[Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Current Quarter Gross Profit", "TOTALQTD([Total Gross Profit],'Date'[Date])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Previous Quarter Gross Profit Proportion to QTD", "[Total Previous Quarter Gross Profit]*([Days In Current Quarter to Date]/[Days In Current Quarter])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Previous Quarter Sales", "CALCULATE([Total Sales], PREVIOUSQUARTER('Date'[Date]))");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Current Quarter Sales", "TOTALQTD([Total Sales],'Date'[Date])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Previous Quarter Sales Proportion to QTD", "[Total Previous Quarter Sales]*([Days In Current Quarter to Date]/[Days In Current Quarter])");
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Sales - Sales Territory sliced by Employee", "[Reseller Total Sales - Sales Territory sliced by Employee] + [Internet Total Sales]");

                    //      Add KPIs (adding first the underlying measure)
                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Current Quarter Sales Performance", "IFERROR([Total Current Quarter Sales]/[Total Previous Quarter Sales Proportion to QTD],BLANK())");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Total Current Quarter Sales Performance", "1.1", "Case When IsEmpty(KpiValue(\"Total Current Quarter Sales Performance\")) Then Null When KpiValue(\"Total Current Quarter Sales Performance\") < 0.8 Then -1 When KpiValue(\"Total Current Quarter Sales Performance\") >= 0.8 And KpiValue(\"Total Current Quarter Sales Performance\") < 1.07 Then 0 Else 1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Total Current Quarter Sales Performance", "1.1", "IF(ISBLANK([Total Current Quarter Sales Performance]), BLANK(), IF([Total Current Quarter Sales Performance] < 0.8, -1, IF([Total Current Quarter Sales Performance] < 1.07, 0, 1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    AMO2Tabular.MeasureAdd(AMO2TabularDb, tableName, "Total Current Quarter Gross Profit Performance", "IF([Total Previous Quarter Gross Profit Proportion to QTD]<>0,([Total Current Quarter Gross Profit]-[Total Previous Quarter Gross Profit Proportion to QTD])/[Total Previous Quarter Gross Profit Proportion to QTD],BLANK())");
                    switch ((CompatibilityLevel)AMO2TabularDb.CompatibilityLevel)
                    {
                        case CompatibilityLevel.SQL2012RTM:
                    AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Total Current Quarter Gross Profit Performance", "1.25", "Case When IsEmpty(KpiValue(\"Total Current Quarter Gross Profit Performance\")) Then Null When KpiValue(\"Total Current Quarter Gross Profit Performance\") < 0.8 Then -1 When KpiValue(\"Total Current Quarter Gross Profit Performance\") >= 0.8 And KpiValue(\"Total Current Quarter Gross Profit Performance\") < 1.03 Then 0 Else 1 End", "Three Symbols UnCircled Colored");
                            break;
                        case CompatibilityLevel.SQL2012SP1:
                            AMO2Tabular.KpiAdd(AMO2TabularDb, tableName, "Total Current Quarter Gross Profit Performance", "1.25", "IF(ISBLANK([Total Current Quarter Gross Profit Performance]), BLANK(), IF([Total Current Quarter Gross Profit Performance] < 0.8, -1, IF([Total Current Quarter Gross Profit Performance] < 1.03, 0, 1)))", "Three Symbols UnCircled Colored");
                            break;
                        default:
                            break;
                    }

                    #endregion

                    #region Add Perspectives
                    string perspectiveName = "Inventory";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingPerspectiveAddConsoleMessage, perspectiveName));
                    AMO2Tabular.PerspectiveAdd(AMO2TabularDb, perspectiveName, false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Date", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product Category", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product Subcategory", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product Inventory", false);

                    perspectiveName = "Internet Operation";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingPerspectiveAddConsoleMessage, perspectiveName));
                    AMO2Tabular.PerspectiveAdd(AMO2TabularDb, perspectiveName, false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Currency", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Customer", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Date", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Geography", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Internet Sales", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product Category", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product Subcategory", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Promotion", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Sales Territory", false);

                    perspectiveName = "Reseller Operation";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingPerspectiveAddConsoleMessage, perspectiveName));
                    AMO2Tabular.PerspectiveAdd(AMO2TabularDb, perspectiveName, false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Currency", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Date", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Employee", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Geography", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product Category", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product Subcategory", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Product", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Promotion", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Reseller", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Reseller Sales", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Sales Quota", false);
                    AMO2Tabular.PerspectiveAlterTableAdd(AMO2TabularDb, perspectiveName, "Sales Territory", false);

                    //  Updating server instance
                    AMO2TabularDb.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

                    #endregion

                    #region Add Roles, Users and RLS
                    //  ************************************************************************************************************
                    //
                    //  Important note:
                    //
                    //  The following section requieres the following users created in the local machine:
                    //  To test the Admins role:
                    //  -   Administrator
                    //
                    //  To test the Analysts role:
                    //  -   sqlDBA
                    //
                    //  To test the Operators role:
                    //  -   sqlService
                    //
                    //  To test the Users role
                    //  -   To test Reseller Sales, some of the following users, from the Employees table:
                    //      brian3, stephen0, michael9, linda3, jillian0, garrett1, tsvi0, pamela0, shu0, josé1, david8, tete0, amy0, jae0, ranjit0, rachel0, syed0, lynn0
                    //  .   I tested with: brian3, syed0, lynn0
                    //
                    //  -   To test Product Inventory, some of the following users, from the Employees table:
                    //      alejandro0, alex0, alice0, andrew0, andrew1, andy0, angela0, anibal0, annik0, barbara0, baris0, barry0, belinda0, benjamin0, betsy0, bjorn0, bob0, bonnie0, brandon0, brenda0, brian0, brian2, britta0, bryan0, carol0, carole0, chad0, charles0, chris0, chris2, christopher0, cristian0, cynthia0, danielle0, david1, david2, david3, david4, david7, denise0, diane0, diane2, don0, doris0, douglas0, ebru0, ed0, elizabeth0, eric0, eric1, eugene0, eugene1, fadi0, frank0, frank1, frank3, fred0, gabe0, garrett0, gary0, george0, greg0, guy1, hanying0, houman0, hung-fu0, ivo0, jack0, jack1, james0, james1, jan0, janeth0, jason0, jay0, jeff0, jeffrey0, jianshuo0, jim0, jinghao0, jo0, john0, john1, john2, john3, john4, jolynn0, jose0, jun0, karan0, kathie0, katie0, ken1, kendall0, kevin1, kevin2, kim1, kimberly0, kirk0, kitti0, kok-ho0, krishna0, lane0, laura0, linda0, linda1, lionel0, lolan0, lori0, lorraine0, maciej0, mandar0, marc0, margie0, mark1, mary1, merav0, michael0, michael1, michael2, michael3, michael4, michael5, michael7, michiko0, mihail0, min0, mindaugas0, nancy0, nicole0, nitin0, nuan0, olinda0, patrick0, patrick1, paul0, paul1, paula1, pete0, prasanna0, rajesh0, randy0, raymond0, rebecca0, reed0, reuben0, rob1, robert0, rostislav0, russell0, russell1, ruth0, ryan0, samantha0, sameer0, sandeep0, sandra0, scott0, shammi0, shane0, shelley0, sidney0, simon0, stefen0, steve0, steven0, stuart0, suchitra0, suroor0, susan1, sylvester0, tawana0, taylor0, terrence0, thomas0, tom0, yuhong0, yvonne0, zheng0
                    //  .   I tested with: ed0 
                    //
                    //  -   To test Internet Sales, some of the following users, from Employees table:
                    //      brian3, david0, james1, jean0, ken0, laura1, peter0, terri0
                    //  .   I tested with: ken0, laura1
                    //
                    //  To test the Excluded role:
                    //  -   TestUser
                    //
                    //  ************************************************************************************************************

                    //  Note: the following users must exist in the local machine or an exception is thrown
#if localUsersCreated
                    string roleName = "Admins";
                    string roleDescription = "All administrators";
                    Tuple<bool, bool, bool> privileges = new Tuple<bool, bool, bool>(true, true, true); //  Read, Process, Administrator
                    string[] members = { @".\AmoAdmin" };
                    Tuple<string, string>[] rlss = null;
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingRolesConsoleMessage, roleName));
                    buildRoles(AMO2TabularDb, true, roleName, privileges, roleDescription, members, rlss);

                    roleName = "Analysts";
                    roleDescription = "All users responsible for the status of the model";
                    privileges = new Tuple<bool, bool, bool>(true, true, false);    //  Read, Process, Administrator
                    members = new string[] { @".\AmoAnalyst" };
                    rlss = null;
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingRolesConsoleMessage, roleName));
                    buildRoles(AMO2TabularDb, true, roleName, privileges, roleDescription, members, rlss);

                    roleName = "Operators";
                    roleDescription = "All users responsible for updating the data";
                    privileges = new Tuple<bool, bool, bool>(false, true, false);    //  Read, Process, Administrator
                    members = new string[] { @".\AmoOperator" };
                    rlss = null;
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingRolesConsoleMessage, roleName));
                    buildRoles(AMO2TabularDb, true, roleName, privileges, roleDescription, members, rlss);

                    roleName = "Users";
                    roleDescription = "All  users allowed to query the model";
                    privileges = new Tuple<bool, bool, bool>(true, false, false);    //  Read, Process, Administrator
                    members = new string[] { @".\brian3", @".\syed0", @".\lynn0", @".\ed0", @".\ken0", @".\laura1" };
                    rlss = new Tuple<string, string>[]{new Tuple<string, string>("Reseller Sales", "PATHCONTAINS( LOOKUPVALUE('Employee'[Hierarchy], 'Employee'[Employee Id], 'Reseller Sales'[Employee Id]), lookupvalue('Employee'[Employee Id], 'Employee'[Login Id], \"adventure-works\\\" & right(username(), len(username()) - search(\"\\\",username(),1,1))))"),
                                                       new Tuple<string, string>("Product Inventory", "lookupvalue('Employee'[Department Name], 'Employee'[Login Id], \"adventure-works\\\" & right(username(), len(username())-search(\"\\\",username(),1,1))) = \"Production\"")};
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingRolesConsoleMessage, roleName));
                    buildRoles(AMO2TabularDb, true, roleName, privileges, roleDescription, members, rlss);


                    roleName = "Excluded";
                    roleDescription = "All users excluded from model";
                    privileges = new Tuple<bool, bool, bool>(false, false, false);    //  Read, Process, Administrator
                    members = new string[] { @".\TestUser" };
                    rlss = null;
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CreatingRolesConsoleMessage, roleName));
                    buildRoles(AMO2TabularDb, true, roleName, privileges, roleDescription, members, rlss);
#endif
                    #endregion

                    //  Process newly created database
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingProcessDatabaseConsoleMessage));
                    AMO2TabularDb.Process(AMO.ProcessType.ProcessDefault);
                    
                    #region Manage Partitions
                    //DefiningPartitionsConsoleMessage
                    tableName = "Internet Sales";
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.DefiningPartitionsConsoleMessage, "Internet Sales - 2005"));

                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2005 Q3", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2005 AND D.CalendarQuarter = 3", true);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2005 Q4", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2005 AND D.CalendarQuarter = 4", true);
                    AMO2Tabular.PartitionDrop(AMO2TabularDb, tableName, "Internet Sales", true);
                    AMO2Tabular.PartitionProcess(AMO2TabularDb, tableName, "Internet Sales 2005 Q3", AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionProcess(AMO2TabularDb, tableName, "Internet Sales 2005 Q4", AMO.ProcessType.ProcessFull);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.DefiningPartitionsConsoleMessage, "Internet Sales - 2006"));

                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2006 Q1", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2006 AND D.CalendarQuarter = 1", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2006 Q2", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2006 AND D.CalendarQuarter = 2", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2006 Q3", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2006 AND D.CalendarQuarter = 3", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2006 Q4", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2006 AND D.CalendarQuarter = 4", true, AMO.ProcessType.ProcessFull);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.DefiningPartitionsConsoleMessage, "Internet Sales - 2007 .. 2010"));

                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2007", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2007", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2008", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2008", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2009", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2009", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Internet Sales 2010", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2010", true, AMO.ProcessType.ProcessFull);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.MergingPartitionsConsoleMessage, "Internet Sales - 2005 Q3 .. Q4"));

                    AMO2Tabular.PartitionAlterMerge(AMO2TabularDb, tableName, "Internet Sales 2005 Q3", true, "Internet Sales 2005 Q4");
                    AMO2Tabular.PartitionAlterName(AMO2TabularDb, tableName, "Internet Sales 2005 Q3", "Internet Sales 2005", true);
                    AMO2Tabular.PartitionAlterSelectStatement(AMO2TabularDb, tableName, "Internet Sales 2005", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2005", true);
                    AMO2TabularDb.Process(AMO.ProcessType.ProcessRecalc);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.MergingPartitionsConsoleMessage, "Internet Sales - 2006 Q1 .. Q4"));

                    AMO2Tabular.PartitionAlterMerge(AMO2TabularDb, tableName, "Internet Sales 2006 Q1", true, "Internet Sales 2006 Q2", "Internet Sales 2006 Q3", "Internet Sales 2006 Q4");
                    AMO2Tabular.PartitionAlterName(AMO2TabularDb, tableName, "Internet Sales 2006 Q1", "Internet Sales 2006", true);
                    AMO2Tabular.PartitionAlterSelectStatement(AMO2TabularDb, tableName, "Internet Sales 2006", "SELECT I.* FROM FactInternetSales I join DimDate D on  I.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2006", true);
                    AMO2TabularDb.Process(AMO.ProcessType.ProcessRecalc);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.DefiningPartitionsConsoleMessage, "Reseller Sales - 2005 .. 2010"));
                    tableName = "Reseller Sales";
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Reseller Sales 2005", "SELECT R.* FROM FactResellerSales R join DimDate D on  R.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2005", true);
                    AMO2Tabular.PartitionDrop(AMO2TabularDb, tableName, "Reseller Sales", true);
                    AMO2Tabular.PartitionProcess(AMO2TabularDb, tableName, "Reseller Sales 2005", AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Reseller Sales 2006", "SELECT R.* FROM FactResellerSales R join DimDate D on  R.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2006", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Reseller Sales 2007", "SELECT R.* FROM FactResellerSales R join DimDate D on  R.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2007", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Reseller Sales 2008", "SELECT R.* FROM FactResellerSales R join DimDate D on  R.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2008", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Reseller Sales 2009", "SELECT R.* FROM FactResellerSales R join DimDate D on  R.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2009", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Reseller Sales 2010", "SELECT R.* FROM FactResellerSales R join DimDate D on  R.[OrderDateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2010", true, AMO.ProcessType.ProcessFull);

                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.DefiningPartitionsConsoleMessage, "Product Inventory - 2005 .. 2010"));
                    tableName = "Product Inventory";
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Product Inventory 2005", "SELECT P.* FROM [FactProductInventory] P join DimDate D on  P.[DateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2005", true);
                    AMO2Tabular.PartitionDrop(AMO2TabularDb, tableName, "Product Inventory", true);
                    AMO2Tabular.PartitionProcess(AMO2TabularDb, tableName, "Product Inventory 2005", AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Product Inventory 2006", "SELECT P.* FROM [FactProductInventory] P join DimDate D on  P.[DateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2006", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Product Inventory 2007", "SELECT P.* FROM [FactProductInventory] P join DimDate D on  P.[DateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2007", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Product Inventory 2008", "SELECT P.* FROM [FactProductInventory] P join DimDate D on  P.[DateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2008", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Product Inventory 2009", "SELECT P.* FROM [FactProductInventory] P join DimDate D on  P.[DateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2009", true, AMO.ProcessType.ProcessFull);
                    AMO2Tabular.PartitionAdd(AMO2TabularDb, tableName, "Product Inventory 2010", "SELECT P.* FROM [FactProductInventory] P join DimDate D on  P.[DateKey] = D.[DateKey] WHERE D.[CalendarYear] = 2010", true, AMO.ProcessType.ProcessFull);
                    #endregion

                    #region Report Objects
                    //Tables
                    Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.TablesConsoleMessage));
                    foreach (string tName in AMO2Tabular.TablesEnumerate(AMO2TabularDb))
                    {
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.TablesReportConsoleMessage, tName));
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.ColumnsConsoleMessage));
                        foreach (string cName in AMO2Tabular.ColumnsEnumerate(AMO2TabularDb, tName))
                        {
                            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.TableElementsReportConsoleMessage, cName));
                        }
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.CalculatedColumnsConsoleMessage));
                        foreach (string ccName in AMO2Tabular.CalculatedColumnsEnumerate(AMO2TabularDb, tName))
                        {
                            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.TableElementsReportConsoleMessage, ccName));
                        }
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.MeasuresConsoleMessage));
                        foreach (string mName in AMO2Tabular.MeasuresEnumerate(AMO2TabularDb, tName))
                        {
                            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.TableElementsReportConsoleMessage, mName));
                        }
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.KpisConsoleMessage));
                        foreach (string kName in AMO2Tabular.KpisEnumerate(AMO2TabularDb, tName))
                        {
                            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.TableElementsReportConsoleMessage, kName));
                        }
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.HierarchiesConsoleMessage));
                        foreach (string hName in AMO2Tabular.HierarchiesEnumerate(AMO2TabularDb, tName))
                        {
                            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.TableElementsReportConsoleMessage, hName));
                        }
                        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.LineSeparatorConsoleMessage));
                    }
                    #endregion
                }
            }
        }

        private static void CreateCurrencyTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            //  Create table object in one large step and finalize updating the server instance
            //  Note: Explicitly passing updateInstance = false in all function calls

            //  Create Table
            AMO2Tabular.TableAdd(tabularDatabase, "DimCurrency", "Currency", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Currency", "CurrencyKey", "Currency Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Currency", "CurrencyAlternateKey", "Currency Alternate Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Currency", "CurrencyName", "Currency Name", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateCustomerTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            //  Create table object in one large step and finalize updating the server instance
            //  Note: Explicitly passing updateInstance = false

            //  Create Table
            AMO2Tabular.TableAdd(tabularDatabase, "DimCustomer", "Customer", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "CustomerKey", "Customer Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "GeographyKey", "Geography Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "CustomerAlternateKey", "Customer Alternate Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "FirstName", "First Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "MiddleName", "Middle Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "LastName", "Last Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "NameStyle", "Name Style", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "BirthDate", "Birth Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "MaritalStatus", "Marital Status", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "EmailAddress", "Email Address", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "YearlyIncome", "Yearly Income", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "TotalChildren", "Total Children", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "NumberChildrenAtHome", "Number Children At Home", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "EnglishEducation", "Education", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "EnglishOccupation", "Occupation", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "HouseOwnerFlag", "Owns House", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "NumberCarsOwned", "Number of Cars Owned", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "AddressLine1", "Address Line 1", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "AddressLine2", "Address Line 2", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "Phone", "Phone Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "DateFirstPurchase", "Date of First Purchase", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Customer", "CommuteDistance", "Commute Distance", false);

            //  Remove not used columns from datasource (different than hidding; removed columns cannot be used in expressions, hidden ones yes)
            AMO2Tabular.ColumnDrop(tabularDatabase, "Customer", "SpanishEducation", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Customer", "FrenchEducation", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Customer", "SpanishOccupation", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Customer", "FrenchOccupation", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateDateTable(AMO.Database tabularDatabase, string cubeName)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAddFirstTable(tabularDatabase, cubeName, "DimDate", "Date", false, null, true, null, "FullDateAlternateKey");

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "DateKey", "Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "FullDateAlternateKey", "Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "DayNumberOfWeek", "Day Number Of Week", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "EnglishDayNameOfWeek", "Day of Week", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "DayNumberOfMonth", "Day Of Month", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "DayNumberOfYear", "Day Of Year", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "WeekNumberOfYear", "Week Number Of Year", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "EnglishMonthName", "Month Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "MonthNumberOfYear", "Month", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "CalendarQuarter", "Calendar Quarter", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "CalendarYear", "Calendar Year", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "CalendarSemester", "Calendar Semester", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "FiscalQuarter", "Fiscal Quarter", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "FiscalYear", "Fiscal Year", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Date", "FiscalSemester", "Fiscal Semester", false);

            //  Remove not used columns from datasource (different than hiding; removed columns cannot be used in expressions, hidden ones yes)
            AMO2Tabular.ColumnDrop(tabularDatabase, "Date", "SpanishDayNameOfWeek", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Date", "FrenchDayNameOfWeek", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Date", "SpanishMonthName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Date", "FrenchMonthName", false);

            //  Add Measures
            AMO2Tabular.MeasureAdd(tabularDatabase, "Date", "Days In Current Quarter to Date", "COUNTROWS( DATESQTD( 'Date'[Date])) =COUNTROWS( DATESQTD( 'Date'[Date]))", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Date", "Days In Current Quarter", "COUNTROWS( DATESBETWEEN( 'Date'[Date], STARTOFQUARTER( LASTDATE('Date'[Date])), ENDOFQUARTER('Date'[Date])))", false);

            //  Add Hierarchies
            AMO2Tabular.HierarchyAdd(tabularDatabase, "Date", "Calendar", false, new LevelInfo[] { new LevelInfo("Calendar Year", "Year"), 
                                                                                                   new LevelInfo("Calendar Semester", "Semester"),
                                                                                                   new LevelInfo("Calendar Quarter", "Quarter"),
                                                                                                   new LevelInfo("Month Name", "Month"),
                                                                                                   new LevelInfo("Day Of Month", "Day")});

            AMO2Tabular.HierarchyAdd(tabularDatabase, "Date", "Fiscal", false, new LevelInfo[] { new LevelInfo("Calendar Year", "Fiscal Year"), 
                                                                                                 new LevelInfo("Calendar Semester", "Fiscal Semester"),
                                                                                                 new LevelInfo("Calendar Quarter", "Fiscal Quarter"),
                                                                                                 new LevelInfo("Month Name", "Month"),
                                                                                                 new LevelInfo("Day Of Month", "Day")});

            AMO2Tabular.HierarchyAdd(tabularDatabase, "Date", "Production Calendar", false, new LevelInfo[] { new LevelInfo("Calendar Year", " Year"), 
                                                                                                              new LevelInfo("Week Number Of Year", "Week"),
                                                                                                              new LevelInfo("Day Of Week", "Day")});


            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);

        }

        private static void CreateEmployeeTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "DimEmployee", "Employee", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "EmployeeKey", "Employee Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "ParentEmployeeKey", "Manager Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "EmployeeNationalIDAlternateKey", "Employee National Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "SalesTerritoryKey", "Sales Territory Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "FirstName", "First Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "LastName", "Last Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "MiddleName", "Middle Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "NameStyle", "Name Style", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "HireDate", "Hire Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "BirthDate", "Birth Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "LoginID", "Login ID", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "EmailAddress", "Email Address", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "Phone", "Phone Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "MaritalStatus", "Marital Status", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "EmergencyContactName", "Emergency Contact Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "EmergencyContactPhone", "Emergency Contact Phone", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "SalariedFlag", "Is Salaried", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "PayFrequency", "Pay Frequency", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "BaseRate", "Base Rate", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "VacationHours", "Vacation Hours", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "SickLeaveHours", "Sick Leave Hours", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "CurrentFlag", "Is Current", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "SalesPersonFlag", "Is Sales Person", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "DepartmentName", "Department Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "StartDate", "Start Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Employee", "EndDate", "End Date", false);

            //  Remove not used columns from datasource (different than hidding; removed columns cannot be used in expressions, hidden ones yes)
            AMO2Tabular.ColumnDrop(tabularDatabase, "Employee", "ParentEmployeeNationalIDAlternateKey", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateGeographyTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "DimGeography", "Geography", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Geography", "GeographyKey", "Geography Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Geography", "StateProvinceCode", "State Province Code", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Geography", "StateProvinceName", "State Province Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Geography", "CountryRegionCode", "Country Region Code", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Geography", "EnglishCountryRegionName", "Country Region Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Geography", "PostalCode", "Postal Code", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Geography", "SalesTerritoryKey", "Sales Territory Id", false);

            //  Remove not used columns from datasource (different than hidding; removed columns cannot be used in expressions, hidden ones yes)
            AMO2Tabular.ColumnDrop(tabularDatabase, "Geography", "SpanishCountryRegionName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Geography", "FrenchCountryRegionName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Geography", "IpAddressLocator", false);

            //  Add Hierarchies
            AMO2Tabular.HierarchyAdd(tabularDatabase, "Geography", "Geography", false, new LevelInfo[] { new LevelInfo("Country Region Name", "Country Region"), 
                                                                                                         new LevelInfo("State Province Name", "State Province"),
                                                                                                         new LevelInfo("City", "City")});

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateProductTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "DimProduct", "Product", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "ProductKey", "Product Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "ProductAlternateKey", "Product Alternate Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "ProductSubcategoryKey", "Product Subcategory Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "WeightUnitMeasureCode", "Weight Unit Code", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "SizeUnitMeasureCode", "Size Unit Code", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "EnglishProductName", "Product Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "StandardCost", "Standard Cost", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "FinishedGoodsFlag", "Is Finished Product", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "SafetyStockLevel", "Safety Stock Level", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "ReorderPoint", "Reorder Point", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "ListPrice", "List Price", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "SizeRange", "Size Range", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "DaysToManufacture", "Days  ToManufacture", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "ProductLine", "Product Line", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "DealerPrice", "Dealer Price", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "ModelName", "Model Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "LargePhoto", "Large Photo", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "EnglishDescription", "Description", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "StartDate", "Product Start Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "EndDate", "Product End Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product", "Status", "Product Status", false);

            //  Remove not used columns from datasource (different than hidding; removed columns cannot be used in expressions, hidden ones yes)
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "SpanishProductName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "FrenchProductName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "FrenchDescription", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "ChineseDescription", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "ArabicDescription", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "HebrewDescription", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "ThaiDescription", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "GermanDescription", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "JapaneseDescription", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product", "TurkishDescription", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateProductCategoryTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "DimProductCategory", "Product Category", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Category", "ProductCategoryKey", "Product Category Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Category", "ProductCategoryAlternateKey", "Product Category Alternate Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Category", "EnglishProductCategoryName", "Product Category Name", false);

            //  Remove not used columns from datasource (different than hidding; removed columns cannot be used in expressions, hidden ones yes)
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product Category", "SpanishProductCategoryName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product Category", "FrenchProductCategoryName", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateProductSubcategoryTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "DimProductSubcategory", "Product Subcategory", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Subcategory", "ProductSubcategoryKey", "Product Subcategory Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Subcategory", "ProductSubcategoryAlternateKey", "Product Subcategory Alternate Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Subcategory", "EnglishProductSubcategoryName", "Product Subcategory Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Subcategory", "ProductCategoryKey", "Product Category Id", false);

            //  Remove not used columns from datasource (different than hidding; removed columns cannot be used in expressions, hidden ones yes)
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product Subcategory", "SpanishProductSubcategoryName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Product Subcategory", "FrenchProductSubcategoryName", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreatePromotionTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "DimPromotion", "Promotion", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "PromotionKey", "Promotion Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "PromotionAlternateKey", "Promotion Alternate Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "EnglishPromotionName", "Promotion Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "DiscountPct", "Discount Pct", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "EnglishPromotionType", "Promotion Type", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "EnglishPromotionCategory", "Promotion Category", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "StartDate", "Promotion Start Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "EndDate", "Promotion End Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "MinQty", "Min Quantity", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Promotion", "MaxQty", "Max Quantity", false);

            //  Remove not used columns from datasource (different than hidding; removed columns cannot be used in expressions, hidden ones yes)
            AMO2Tabular.ColumnDrop(tabularDatabase, "Promotion", "SpanishPromotionName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Promotion", "FrenchPromotionName", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Promotion", "SpanishPromotionType", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Promotion", "FrenchPromotionType", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Promotion", "SpanishPromotionCategory", false);
            AMO2Tabular.ColumnDrop(tabularDatabase, "Promotion", "FrenchPromotionCategory", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateResellerTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "DimReseller", "Reseller", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "ResellerKey", "Reseller Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "GeographyKey", "Geography Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "ResellerAlternateKey", "Reseller Alternate Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "Phone", "Reseller Phone", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "BusinessType", "Business Type", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "ResellerName", "Reseller Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "NumberEmployees", "Number Employees", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "OrderFrequency", "Order Frequency", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "OrderMonth", "Order Month", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "FirstOrderYear", "First Order Year", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "LastOrderYear", "Last Order Year", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "ProductLine", "Product Line", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "AddressLine1", "Address Line 1", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "AddressLine2", "Address Line 2", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "AnnualSales", "Annual Sales", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "BankName", "Bank Name", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "MinPaymentType", "Min Payment Type", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "MinPaymentAmount", "Min Payment Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "AnnualRevenue", "Annual Revenue", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller", "YearOpened", "Year Opened", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateSalesTerritoryTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "DimSalesTerritory", "Sales Territory", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Territory", "SalesTerritoryKey", "Sales Territory Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Territory", "SalesTerritoryAlternateKey", "Sales Territory Alternate Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Territory", "SalesTerritoryRegion", "Sales Territory Region", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Territory", "SalesTerritoryCountry", "Sales Territory Country", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Territory", "SalesTerritoryGroup", "Sales Territory Group", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Territory", "SalesTerritoryImage", "Sales Territory Image", false);

            //  Add Hierarchies
            AMO2Tabular.HierarchyAdd(tabularDatabase, "Sales Territory", "Territory", false, new LevelInfo[] { new LevelInfo("Sales Territory Group", "Group"), 
                                                                                                               new LevelInfo("Sales Territory Country", "Country"),
                                                                                                               new LevelInfo("Sales Territory Region", "Region")});


            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateInternetSalesTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            //  Create table object in one large step and finalize updating the server instance
            //  Note: Explicitly passing updateInstance = false

            //  Create Table
            AMO2Tabular.TableAdd(tabularDatabase, "FactInternetSales", "Internet Sales", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "ProductKey", "Product Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "OrderDateKey", "Order Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "DueDateKey", "Due Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "ShipDateKey", "Ship Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "CustomerKey", "Customer Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "PromotionKey", "Promotion Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "CurrencyKey", "Currency Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "SalesTerritoryKey", "Sales Territory Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "SalesOrderNumber", "Sales Order Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "SalesOrderLineNumber", "Sales Order Line Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "RevisionNumber", "Revision Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "OrderQuantity", "Order Quantity", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "UnitPrice", "Unit Price", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "ExtendedAmount", "Extended Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "UnitPriceDiscountPct", "Unit Price Discount Pct", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "DiscountAmount", "Discount Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "ProductStandardCost", "Product Standard Cost", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "TotalProductCost", "Total Product Cost", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "SalesAmount", "Sales Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "TaxAmt", "Tax Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "CarrierTrackingNumber", "Carrier Tracking Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "CustomerPONumber", "Customer PO Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "OrderDate", "Order Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "DueDate", "Due Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Internet Sales", "ShipDate", "Ship Date", false);


            //  Format columns

            //  Add Calculated Columns
            AMO2Tabular.CalculatedColumnAdd(tabularDatabase, "Internet Sales", "Gross Profit", "[Sales Amount]-[Total Product Cost]", false);

            //  Add Measures
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Distinct Count Sales Order", "DISTINCTCOUNT([Sales Order Number])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Total Sales", "SUM([Sales Amount])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Total Discount Amount", "SUM([Discount Amount])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Total Product Cost", "SUM([Total Product Cost])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Total Tax Amount", "SUM([Tax Amount])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Total Freight", "SUM([Freight])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Total Gross Profit", "SUM([Gross Profit])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Total Units", "SUM([Order Quantity])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Internet Sales", "Internet Order Lines Count", "COUNT([Sales Order Line Number])", false);

            //  Format Measures
            AMO2Tabular.MeasureAlterFormat(tabularDatabase, "Internet Sales", "Internet Total Sales", @"'\$#,0.00;(\$#,0.00);\$#,0.00'", false);
            AMO2Tabular.MeasureAlterFormat(tabularDatabase, "Internet Sales", "Internet Total Discount Amount", "'$ *#,##0.00'", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateProductInventoryTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "FactProductInventory", "Product Inventory", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Inventory", "ProductKey", "Product Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Inventory", "DateKey", "Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Inventory", "MovementDate", "Movement Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Inventory", "UnitCost", "Unit Cost", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Inventory", "UnitsIn", "Units In", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Inventory", "UnitsOut", "Units Out", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Product Inventory", "UnitsBalance", "Units Balance", false);


            //  Add Calculated Columns
            AMO2Tabular.CalculatedColumnAdd(tabularDatabase, "Product Inventory", "Product-Date Inventory Value", "[Unit Cost]*[Units Balance]", false);
            AMO2Tabular.CalculatedColumnAdd(tabularDatabase, "Product Inventory", "Product-Date Negative Stock", "IF([Units Balance]<0,1,0)", true);

            //  Add Measures
            AMO2Tabular.MeasureAdd(tabularDatabase, "Product Inventory", "Total Units In", "SUM([Units In])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Product Inventory", "Total Units Out", "SUM([Units Out])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Product Inventory", "Total Units Movement", "[Total Units In]-[Total Units Out]", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateResellerSalesTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "FactResellerSales", "Reseller Sales", false);

            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "ProductKey", "Product Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "OrderDateKey", "Order Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "DueDateKey", "Due Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "ShipDateKey", "Ship Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "ResellerKey", "Reseller Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "EmployeeKey", "Employee Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "PromotionKey", "Promotion Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "CurrencyKey", "Currency Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "SalesTerritoryKey", "Sales Territory Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "SalesOrderNumber", "Sales Order Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "SalesOrderLineNumber", "Sales Order Line Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "RevisionNumber", "Revision Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "OrderQuantity", "Order Quantity", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "UnitPrice", "Unit Price", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "ExtendedAmount", "Extended Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "UnitPriceDiscountPct", "Unit Price Discount Pct", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "DiscountAmount", "Discount Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "ProductStandardCost", "Product Standard Cost", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "TotalProductCost", "Total Product Cost", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "SalesAmount", "Sales Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "TaxAmt", "Tax Amount", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "CarrierTrackingNumber", "Carrier Tracking Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "CustomerPONumber", "Reseller PO Number", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "OrderDate", "Order Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "DueDate", "Due Date", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Reseller Sales", "ShipDate", "Ship Date", false);

            //  Add Calculated Columns
            AMO2Tabular.CalculatedColumnAdd(tabularDatabase, "Reseller Sales", "Gross Profit", "[Sales Amount]-[Total Product Cost]", false);



            //  Add Measures
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Distinct Count Sales Order", "DISTINCTCOUNT([Sales Order Number])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Total Sales", "SUM([Sales Amount])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Total Discount Amount", "SUM([Discount Amount])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Total Product Cost", "SUM([Total Product Cost])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Total Tax Amount", "SUM([Tax Amount])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Total Freight", "SUM([Freight])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Total Gross Profit", "SUM([Gross Profit])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Total Units", "SUM([Order Quantity])", false);
            AMO2Tabular.MeasureAdd(tabularDatabase, "Reseller Sales", "Reseller Order Lines Count", "COUNT([Sales Order Line Number])", false);

            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void CreateSalesQuotaTable(AMO.Database tabularDatabase)
        {
            System.Console.WriteLine(string.Format(CultureInfo.InvariantCulture, Resources.StartingFunctionCallConsoleMessage, MethodBase.GetCurrentMethod().Name));

            AMO2Tabular.TableAdd(tabularDatabase, "FactSalesQuota", "Sales Quota", false);


            //  Update Column names
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Quota", "SalesQuotaKey", "Sales Quota Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Quota", "EmployeeKey", "Employee Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Quota", "DateKey", "Date Id", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Quota", "CalendarYear", "Calendar Year", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Quota", "CalendarQuarter", "Calendar Quarter", false);
            AMO2Tabular.ColumnAlterColumnName(tabularDatabase, "Sales Quota", "SalesAmountQuota", "Sales Amount Quota", false);


            //  Updating server instance
            tabularDatabase.Update(AMO.UpdateOptions.ExpandFull, AMO.UpdateMode.UpdateOrCreate);
        }

        private static void buildRoles(AMO.Database tabularDb, bool updateInstance, string roleName, Tuple<bool, bool, bool> privileges, string description, string[] members, Tuple<string, string>[] rlss)
        {
            AMO2Tabular.RoleAdd(tabularDb, roleName, privileges.Item1, privileges.Item2, privileges.Item3, description, updateInstance);
            if (privileges != null)
                foreach (string member in members)
                    AMO2Tabular.RoleMemberAdd(tabularDb, roleName, member, updateInstance);
            if (rlss != null)
                foreach (Tuple<string, string> rls in rlss)
                    AMO2Tabular.RlsAdd(tabularDb, roleName, rls.Item1, rls.Item2, updateInstance);
        }

    }
}
