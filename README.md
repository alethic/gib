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

So one day I saw a screenshot of Unity's Shader Graph designer. A bunch of elements, connected to each other. Piping data to each other. And you can see a live preview of the scene output as you edit it. I was working on some build thing in IKVM at the time. And the two clicked. Why aren't build systems just pipelines that run and constantly update the output as you edit the code? And then I remember work I did with GStreamer 10 to 15 years ago. You could build dynamic pipelines of connected elements. They each had source and sink pads. They could be connected to each other. You had 'bin's, which were elements that contained nested pipelines. There was a lot of dynamic stuff happening at runtime, where one element in a pipeline (a bin) could build it's nested pipeline on demand in response to negotiated content. So a big pipeline could be a bin, that auto detects input, and spawns up and connects child elements together, which could themselves be bins that dynamically spawn up child elements. So, something like 'play this .mpg file', would run a 'playbin', which had a filesrcbin, which could detect the URL and spawn up a soapsrc or httpsrc to obtain the file stream, which got sent to a demuxer, where the specific demuxer was autodetected based on the media content being emit, so it could be a demuxer for container formats.... which have multiple nested audio and or video streams... which could be various formats. Which would be hooked to dynamically generated decodebins, which discovered the format of the individual stream, which spawened up various decoders, which then got negotiated to a series of format converters, which were informed by the output device(s)... like if anything had to be done to the stream to format it to display on X or Windows, and change the format of the audio stream to go to the audio service (oss, alsa, pulse, etc). Basically, a system of a million elements, and a million nested pipelines, that was mostly auto discovered by the user specifying the minimum of facts, like a magic top-level element that pulled from HTTP and played locally.

And I thought... why don't we do build systems like that?

Why doesn't one just 'play' their build system, and then leave it running? Why don't IDEs integrate with a system that has a huge nested graph set, but is otherwise live feeding compiler information into it for Intellisense? Why do CI/CD systems spawn "jobs", and not just a pipeline that is always running, that you can watch real time? Why do they need their own CI/CD language/workflow processes, and not just nested the user's build pipeline into their own? Why do they clean and checkout from source for each change, and not just get notified of new commits, sort out exactly what changed in that commit, and push that through the pipeline?

Why are things like "the .NET SDK" not just a sub element that has some inputs and some outputs, and can otherwise be nested into user customizations?

Why can't I take the .NET SDK... and say, the Gradle build system... and run then side by side, feeding output from one into the other? Say a case where I wanted Java .jar files emitted as resources into .NET assemblies? Or even better, using IKVM, a Java build system that feeds JARs into a JAR->IL converter, which feeds back into the .NET SDK as references?

Okay. So that's the manifesto.

So there's a few things I need to experiment with here.

A pipeline model, like in Gstreamer. Where you have Elements, and Elements have Pads. Where Pads have directionality. Where Elements can be coded to recognize changes to pads. Where Elements can be put into RUNNING/STOPPED/PAUSED states or something. Where the pipeline can be RUNNING/STOPPED, and this directs the elements to stop, etc. Okay. Say we have the basics of that. We'd need a core set of very basic elements that people could reuse, and then a way to compose them.

So, let's think about that. You'd have an object model, like GStreamer, of Element, etc. People could write new code to implement Elements. This, in MSBuild, would be akin to writing a custom task. And to make this global, you'd want a way to import other people's elements. Some plugin model. I like how GitHub actions can put use: foo/bar ino it's pipeline, and directly run action code in somebody elses git repo. So, let's think about that. You should be able to have a Element in your graph that is sourced from a URL, and the plugin code is downloaded, and the Element instantiated, and inserted.

User's should be able to compose elements together into a graph. So what form would that take. This is like a 'bin' in GStreamer. This 'bin' could be an Element, but which creates a sub-graph of other elements, as described by a user description language. A DSL of some kind.... so, let's call them spec files.

Okay... we can bridge the gap with existing functionality by allowing our build system to execute shell commands like any other. So, maybe a 'sh' element is built in. stdin can be a pad. stdout/stderr can be pads. The shell script to execute can also be a pad.

So we have Elements. Elements can live at URLs. But some Elements are core to the system. specbin is one of those core Elements. Probably some basic stuff like filesrc, filesink, etc, should also be built in. But after that, the user can build custom elements. Probably 'sh' too.

So let's imagine a "spec" file which encapsulates some of this.

```

source: filesrc
  glob: **/.c

compile: sh
  cmd: gcc $(name) -o obj/$(name).o
  items: $source.out

link: sh
  cmd: ld $(compile.out.name) -o bin/myproj.exe
  items: $source.out

```

Okay. So the idea here is we build a pipeline with three elements, with names. Names on the left, element type on the right. A 'filesrc' element named 'source'. And two 'sh' elements. These all have pads. The 'filesrc' element has a 'glob' pad. Which we set to a fixed string value. So, it is set once, but never changes. But it oculd change. It's just in this example it doesn't. The 'filesrc' element runs the glob, and emits a series of events on it's 'out' pad. Each event signifies a change to the set of files. Like a file being removed, or added, or changed. The 'sh' element is coded to accept a list of items on it's 'items' pad. In this case, we link the 'items' pad of the compile 'sh' element to the 'out' pad of the source element. So, the source element, when it is running, is emitting file change events to it's out pad. And the compile 'sh' pad has those events routed into it's 'items' pad, which it can respond to. In this case, the 'sh' element is coded to execute the 'cmd' string for each item available on it's 'items' pad. Maybe that could get a bit more complicated, allowing the 'sh' task to execute different commands based on properties of the items. Or, to execute different commands when an item is removed.

Within this .spec file, there are names: 'source', 'compile' and 'link'. Names given to instances of elements. Let's call that a naming context.

And this spec file format lets you connect pads together using expressions that have those names available. $source, for instance, is a reference to the element with the name 'source' in the current naming context.






