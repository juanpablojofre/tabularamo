# Tabular AMO 2012
Is about creating and managing tabular models using the AMO api.
It is a developer’s sample, for those interested in managing Analysis Services.

The sample is made of two project parts:

The first part is a library of functions to manage tabular models
-AMO2Tabular V2-.

The second part is a sample to build a tabular model
-AdventureWorks Tabular AMO 2012- using the AMO2Tabular library;
the created model is similar to the 'AdventureWorks Tabular Model 2012',
available here [AdventureWorks DW Tabular Model SQL Server 2012](../AdventureWorksDW2012 Tabular/AdventureWorks DW Tabular Model SQL Server 2012).

The intentions around the AMO2Tabular library were to provide the most
complete guide on how to write AMO code to manage the different logical
objects in a tabular model.
Not all functions to manage tabular models are implemented in the library;
but, I expect the necessary knowledge to complete the library is included in
the AMO2Tabular source code.
Also, you are expected to contribute to the library; once you find that
certain functionality was not included and you have to implement that
functionality, please consider extending the code to expand the library.

The scope of the library comprehensive, in the sense that it has functionality
for all tabular objects. However, as mentioned earlier, it is not extensive;
it does not necessarily cover all possible operations on every tabular object.
If coding certain operation, on a particular object, is not an obvious solution,
you can be certain that I have included that operation in the library; what
remains to be seen is what you consider obvious vs. what I do (I just hope there
is not too much difference here).

The 'AdventureWorks Tabular AMO 2012' sample was designed with two purposes in
mind. The first purpose was for it to be a test bed for the library; where most
of the functionality of the library could be tested. The second purpose was to
have a sample on how to use the library to build and manage a tabular model;
also, the model created by the sample should have enough complexities to be
a good showcase of the capabilities of the library.
