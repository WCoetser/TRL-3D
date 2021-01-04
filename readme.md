# Overview

**=== NB: Work in progress ===**

TRL-3D is a 3D graphics system build on C#, OpenTK, and OpenGL 4.5 that makes it easy to generate and render 3D content programatically, or with a text editor, without having to juggle OpenGL vertex buffers or open a 3D editor.

Content is specified using batches of assertions (specifying things to render) and asynchronously processed. Events (ex. user input, screenshot) are asynchronously returned, making it possible to feed new assertions into the system. This is done using .NET multi-threaded Channel objects in order to provent the program "locking up" while rendering.

Assertions are identified by object IDs instead of internal .NET object references, making it possible to specify partially complete data, or to pass data over a network via a REST API. These assertions are transparently compiled into OpenGL objects (ex. vertex buffers, shaders etc.) The API consumer only needs to worry about the assertions.

# Licences

This project is released under the MIT license, see [licence.txt](license.txt).

For the sample project (Trl-3D.SampleApp), the following additional libraries were referenced that has different open source licences (as of time of writing):

* Serilog - Apache 2.0 licence - [https://github.com/serilog/serilog](https://github.com/serilog/serilog)
* ImageSharp - Apache 2.0 licence with commercial options - [https://github.com/SixLabors/ImageSharp](https://github.com/SixLabors/ImageSharp)
* OpenTk - MIT licence - [https://github.com/opentk/opentk](https://github.com/opentk/opentk)

The system is designed in such a way that you can substitute your own logging and image loading in case you need to use a commercial license.
