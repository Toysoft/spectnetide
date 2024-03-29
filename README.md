# Introduction

## Version 2.0 is coming soon!

I have been actively working on version 2 since June 2019. The first public preview will be available in November 2019. The main themes of version 2:

- __Totally rewitten Visual Studio integration.__ I removed many annoying issues related to solution loading.
- __Multiple ZX Spectrum projects.__ You can put more than one ZX Spectrum project into a single solution. While working, you can change the active project anytime.
- __Boriel's BASIC (ZX BASIC) integration.__ Besides Z80 assembly, now, you can use ZX BASIC as your programming language to create spectacular games and applications.
- __New debugging tools.__ The ZX Spectrum emulator provides two extra options: _ULA render point indication_, and _shadow screen rendering_.
- __New, more modern project item icons.__

Preview 1 (mid-November) will have these limitations:

- ZX BASIC source code debugging is not supported yet.
- Z80 Assembler Output and Z80 Unit Test Explorer are not supported yet.
- SpectNetIDE is available only on VS 2019 (all editions supported).
- Spectrum Scripting Console App project type removed.

__Stay tuned!__

## About the Project

This project implements a ZX Spectrum integrated development environment (IDE) that is integrated into Visual Studio 2017 and 2019. 
Initially, I intended this project to be just a demo project so that I can use it in my agile software design and testing courses. However, it became a fun project.

It (will) support the following ZX Spectrum models:

* __ZX Spectrum 48K__ &mdash; _Emulator and tooling completed_
* __ZX Spectrum 128K__ &mdash; _Emulator and tooling completed_
* __ZX Spectrum +3E__ &mdash; _Emulator and tooling completed_
* __ZX Spectrum Next__ &mdash; _Emulator and tooling development is in progress_

At the moment the code is entirely written in C#. Nonetheless, I plan to implement certain parts in C++ (somewhen 
in the future, for the sake of performance), and maybe in JavaScript/TypeScript, too.

__SpectNetIde__ supports you in two main scenarios:

1. __Code discovery__. The IDE is full with tools you can use to discover/reenginer 
exsisting BASIC/Z80 assmebly code.
2. __Code creation__. You can easily create, run, debug, and export Z80 assembly code.

I cannot be grateful enough to [__Simon Brattel__](http://www.desdes.com/index.htm). Many features I have been implementing in the SpectNetIde assembler were inspired by Simon's outstanding [Zeus Z80 Assembler](http://www.desdes.com/products/oldfiles/zeus.htm). I honor his ideas and work.

## Taste the Pudding!

>You can download the VSIX installer file [here](https://marketplace.visualstudio.com/items?itemName=Dotneteer.SpectNetIde).

To get an impression about __SpectNetIde__, see these short articles with screenshots:

1. [Install SpectNetIDE](https://dotneteer.github.io/spectnetide/getting-started/install-spectnetide#article)
1. [Create your first ZX Spectrum project](https://dotneteer.github.io/spectnetide/getting-started/create-zx-spectrum-48k-project#article)
1. [Use the Keyboard Tool](https://dotneteer.github.io/spectnetide/getting-started/use-keyboard-tool#article)
1. [Create and run a simple BASIC program](https://dotneteer.github.io/spectnetide/getting-started/create-a-basic-program#article)
1. [Create and run a simple Z80 program](https://dotneteer.github.io/spectnetide/getting-started/create-a-z80-program#article)
1. [Export a Z80 program](https://dotneteer.github.io/spectnetide/getting-started/export-a-z80-program#article)
1. [LOAD a program](https://dotneteer.github.io/spectnetide/getting-started/load-a-program#article)
1. [Fast (Instant) LOAD](https://dotneteer.github.io/spectnetide/getting-started/fast-load#article)
1. [The ZX Spectrum Emulator](https://dotneteer.github.io/spectnetide/getting-started/zx-spectrum-emulator-window#article)


## Distinguishing Features

Probably is less mature than most of the ZX Spectrum emulators with longer past &mdash; 
Nevertheless, this project has some unique features:

* __Unit tests for Z80 code__. The [Z80 test language](https://dotneteer.github.io/spectnetide/documents/unit-testing-basics#article) allows you to define and run unit tests for your Z80 code.
* __Requires no configuration__. You start the IDE, create a ZX Spectrum project, and everything is ready to work.
* __The code is well commented and harnessed with unit tests__. I plan to add a lot of documentation that helps you
understand how to design and develop such an emulator. I also plan articles about Visual Studio 2017 extensibility.
* __Great development tools__. The project adds many useful features integrated into the VS 2017 IDE that 
you can develop for understanding ZX Spectrum applications and games, discover their code, and develop new ZX Spectrum apps.

* __ZX Spectrum Emulator tool window__ in the IDE
* __ZX Spectrum project type__ to keep together everything you use to create and debug your Z80 assembly apps.
    * __Disassembly tool__. The ROM comes with annotations (labels, comments, symbols, memory maps) that you can use while examining
the code. The disassembler recognizes Spectrum-specific byte codes, such as the ones used by the __`RST #08`__ and
__`RST #28`__ instructions.
    * __Registers with ULA counters__. You can follow not only the Z80 registers, but the most important information about
the ULA.
    * __Tape Explorer__ to examine `.TZX` and `.TAP` files. You can peek into the BASIC programs they contain, or disassembly the code in
the tape files.
    * __BASIC List view__ to display the BASIC program list loaded into the memory
    * __Debugging the code__ with *breakpoints, Run/Pause/Step-Into/Step-Over/Step-Out* commands
    * __Watch Memory view__ with powerful watch expressions to examine CPU and memory state while debugging.
    * __Stack view tool__. Besides the stack contents, you can see &mdash; with disassembly &mdash; the instructions that placed a particular value to the stack. 
    * __Spectrum VM state management__. You can save and load the current state of the Spectrum virtual machine to a file, or even add it
to the project hierarchy. When you start a Z80 program, the IDE uses VM states for fast code load and execution.
    * __Virtual floppy disk support__. You can create virtual floppy disks for the Spectrum +3e model, insert them into the virtual floppy
drives, eject them, make the floppies write protected, or remove the write protection. 

* __Full-blown Z80 assembly programming__. The [SpectNetIde Assembler](https://dotneteer.github.io/spectnetide/documents/main-features.html) provides you a
robust Z80 assembler and related toolset.
    * __Syntax-highlighted editor__
    * __Convenient syntax__ (for example, labels are accepted with or without a subsequent colon; alternative syntax variations, 
for example, both `jp (hl)` and `jp hl` are accepted, as well as `sub b` and `sub a,b`)
    * __String escape sequences for ZX Spectrum-specific characters__. The assembly language supports escapes for control characters like
`AT`, `TAB`, `PAPER`, &pound;, or &copy;.
    * __Loops and conditional statements__
    * __Powerful dynamic macros__
    * __Source code debugging__. You can set up breakpoint in the source code. When they are reached, the corresponding source code
is displayed. *Run/Pause/Step-Into/Step/Over* commands are available with source code, too.
    * __Z80 Program code export__. You can export the Z80 assembly code into `.TZX` and `.TAP` code files that can be immediately
LOADed into ZX Spectrum &mdash; with optional auto start support.
* __Scripting object model__. You can create [scripts](https://dotneteer.github.io/spectnetide/documents/scripting-overview#article) to automate common task with the 
[scripting object model](https://dotneteer.github.io/spectnetide/documents/scripting-object-model#article).


## Future plans

I do not want to stop here, and plan a number of exciting features:

* More emulators with development tools: ZX Spectrum +3, ZX Spectrum Next
* Compiler for a higher level language
* Integrated development tools for ZX Spectrum Next features (for example sprites with editors, etc).

## Contribution

You can contribute to the project. Please contact me by email: dotneteer@hotmail.com
