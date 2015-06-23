Reuseable analysis library and Visual Studio extension with hundreds of open source refactorings and code issue fixes.

[Twitter](https://twitter.com/vsrefactoring) - [StackOverflow](http://stackoverflow.com/questions/tagged/nrefactory) - [Wiki](https://github.com/icsharpcode/NRefactory/wiki)

## What is NRefactory?

NRefactory is a library geared towards anyone that needs IDE services - think code completion, refactoring, code fixes and more. 
It is being used by IDE projects such as [SharpDevelop](https://github.com/icsharpcode/SharpDevelop) and 
[MonoDevelop](https://github.com/mono/monodevelop), or [OmniSharp](https://github.com/OmniSharp) that provides IDE-like features 
in various cross platform editors.

## The Visual Studio Extension / NuGet Package (X-Platform too!)

Those have been extracted and moved to [vsrefactoringessentials.com](http://vsrefactoringessentials.com) and the
[Refactoring Essentials repository](https://github.com/icsharpcode/RefactoringEssentials) respectively.

Note that the code base is cross-platform, MonoDevelop uses the very same refactorings and code fixes / analyzers!

## Got a bit of history for me?

NRefactory is not exactly a new kid on the block. It started a very long time in SharpDevelop, was at times developed in parallel 
in MonoDevelop, and roundabout 2010 the two teams joined forces to bring NRefactory5 to life as a full open source stack of IDE services 
(MonoDevelop 3 and SharpDevelop 5 ship(ped) with this version). NR6 is leveraging Roslyn instead of homegrown stack components to make 
development easier and faster. After all, open source isn't about duplicating effort - at least in our book.

## The license?

MIT. Doesn't get any simpler than that.
