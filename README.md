# gib
Globally Infectious Build (or something, I just liked the acronym)

For now this is just going to be prototyping work.

What I'm interested in is a new build system that can form the basis of a replacement for every other build system on the planet. Much as git took over VSC.

The aim is to be as simple to use as Make. You should be able to pop open a simple text file, add some elements, and things to do to turn X into Y, and hit run. But also to be such a powerful paradigm that the entire .NET MSBuild SDK could be reimplemented ontop of it.

But the paradigm..... that'll be different. I need a manifesto. So here we go.

For 60 some years build systems have been developed around the 'target' paradigm. You build a graph of targets, and these targets depend on other targets, and you run the build system, it evaluates from the start, and determines which targets are out of date. It then runs the whole thing to bring it up to date, and then exits.

Targets have traditionally been file based. When file X changes, you do A to produce file Y. File Y depends on file A. In original make, this was basically all you had. Consequently, people move state between targets as stowaways..... by making temporary files. Caches. Files with hashes in them, that are only updated if the original file set changes. Just to drive the trigger to cause other targets to execute. This hasn't really changed in modern build systems. Look at the .NET SDK. It's server thousand targets. Most of them just modify global variables (Properties, ItemGroups). And then you have targets throwing temporary crap into obj/ like .cache files.

And then you have various IDE systems that integrate with these build systems. Visual Studios integration into MSBuild comes to mind. The build system is full of magic global properties and item groups, and secondary magic targets, just to facilitate Visual Studio poking around in it: design-time builds. And then half of it avoids MSBuild completely, by using the Roslyn language service to feed in file modifications, prompted from the MSBuild system, but which ultimately generates a compiled model out of band for intellisense. It's huge and confusing. Authoring SDK components is a nightmare of global variables, global target names, and required magic.

Let's look at it differently, and ask some fundamental questions:

Why do we have to "run" the build each time we make a change? Why isn't it just always running?

Why does it stop at all?

Why did CI/CD systems (Azure DevOps, GitHub Actions, etc) have to develop their own out-of-band workflows in YAML, just to coordinate the execution of tasks.... which are ultimately just executions of other build systems?

Why do CI/CD systems run from the start for every commit? Why isn't it just thing that is 'always running', always emitting new output?

