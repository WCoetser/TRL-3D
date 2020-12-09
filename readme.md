# Overview

**NB: Work in progress**

TRL-3D is a 3D graphics system build on C#, OpenTK, and OpenGL that makes it easy to generate 3D content.

Main goals:

* Specify 3D content using a term rewriting language (TRL).
* Process user input events and reference elements from the original TRL elements in event handlers.
* Use a build-in global "black board" architecture for world logic and rules.
* Differential DOM style processing through tracked term rewriting steps.
* Decoupled render logic with async render update command queue going in and events coming out.
* Standard .NET logging and depencency injection.

# Licence

This project is released under the MIT license, see [licence.txt](license.txt).
