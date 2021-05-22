# Project-4

Starter code for the fourth project -- implementing the CodeGenVisitor

This repository includes a version of Program.cs that calls the Code Generation visitor immediately after the Semantics
pass finished.  It then assembles and runs the .il file generate by the CodeGenVisitor pass over the abstract syntax tree.

The provided CodeGenVisitor.cs gives you a starting point for adding in your code generation VisitNode routines.  
It generates the two-line prolog that is necessary at the beginning of a .il file.  Like SemanticsVisitor
it produces a trace as VisitNode is invoked for each node in the AST, starting at the root.  
It also include a set of WriteLine calls that generate the body of the hello.txt program, regardless of 
what test program is used to build the AST that is being processed.  This code is there so that you can see 
if all of the mechanisms are working.  You must delete it when you start adding your VisitNode routines.

There are a number of files missing, the ones that are specific to your AST creation and semantics processing.  
Visual Studio will show you which they are when you load the project.  There is a version waiting to run that I
created before removing these files.  You can run it to check that it does create and execute the 'hello world'
program even before you start adding in your files and doing a build.
