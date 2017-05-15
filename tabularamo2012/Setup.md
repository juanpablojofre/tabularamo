# Tabular AMO 2012 Setup

## Introduction

‘Tabular AMO’ is a set of two samples with the purpose of demonstrating how to create a tabular model, as a developer, using AMO. The solution consists of a sample library of functions to manage tabular models and another sample that shows how to use the library to create a tabular model.

‘AdventureWorks Tabular AMO’ is the C# sample program that builds a Tabular model using the AMO2Tabular V2 library (AMO2Tabular). The main purpose of the sample is to illustrate how you would create a tabular model, as a developer, using a programming language and AMO2Tabular.
‘AMO2Tabular’ is a sample of a library designed to manage tabular models using AMO. AMO2Tabular functions range from creating a tabular database, to modify columns, to create hierarchies and to manage partitions; but not limited to only those functions. AMO2Tabular does not intend to be the complete library; rather to exemplify how most operations should be built, leaving other operations to the user to implement them.

The sample and the learning are in the source code; more than in compiling and executing the code. The execution of the ‘AdventureWorks Tabular AMO’ code is the proof that the library works.

The sample creates a tabular model; it starts from creating a tabular database, then goes through the process of creating tables and all related elements until finalizing with some security elements; also, at the end, the sample exercises a little with partition management. 

The created model is similar to ‘AdventureWorks Tabular Model SQL Server 2012’ (available to download from Codeplex at http://msftdbprodsamples.codeplex.com/downloads/get/353143). 

By creating a model similar to an existing one (and one that you can easily download), you can verify that the results obtained by using the library are the same as those you would obtain by designing the model using Microsoft SQL Server Data Tools.

An important note here is to say, that by design: models created using the AMO2Tabular V2 library cannot be used in Microsoft SQL Server Data Tools (formerly known as BIDS). Models created using AMO2Tabular, can be queried and used from Microsoft SQL Server Management Studio.

## Requirements

### Software

SQL Server 2012 (Standard, Enterprise or Developer edition)
Visual C# 2010 Express or Visual Studio 2010 (Professional, Premium or Ultimate edition)
Visual Studio 2010 SP1

### Environment

SQL Server 2012 RDBMS Engine installed in local machine.
SQL Server 2012 Analysis Services installed in local machine and running in Tabular mode.
‘AdventureWorksDW2012’ relational database installed; download from here http://msftdbprodsamples.codeplex.com/downloads/get/165405
(optional) ‘AdventureWorks Tabular Model SQL Server 2012’ installed; download from here http://msftdbprodsamples.codeplex.com/downloads/get/353143

## Install

If you are reading this note you already downloaded the required files and had unzipped them; in case you haven’t unzipped the files, do it now to your preferred location for developer projects.
Open the ‘Tabular AMO.sln’ solution file with Visual Studio or by double clicking on it.
Build the solution (will build the library -AMO2Tabular- and the sample -AdventureWorks Tabular AMO-) either by selecting build the solution from the Build menu or by pressing [CTRL+SHIFT+B].
Run the solution. If Microsoft SQL Server is installed, both instances (rdbms and tabular) are running and the user executing the sample has read access to ‘AdventureWorksDW2012’ and has server administrator privileges to the tabular instance, the sample should run with no problems.
Please note that the security section, in AdventureWorks Tabular AMO, is grayed. See the ‘Setup and Execute’ guide for more details.