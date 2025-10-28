# A PoC to develop ideas around making a program modular, so that it can be extended by 3rd parties


So, I have an idea. I'd like to build a very basic Avalonia app, using dotnet 9. The simple form has the following features:

- a single window with a row of sample icons along the top. There should be no more than 4
- a simple label showing the current time, so updating every second

The idea will be that a module can be created, whether in this solution/project, that can be loaded into the existing app. Once done so, it will add its own icon to the row along the top, taking its place next to the existing ones.

This module will then create a column of icons down the right hand side of the screen

It will also consume and display the current time as published by the main application.

Before writing any code, ask me what else I can provide to make this example clearer.

Regarding module loading, having a separate DLL in a modules subfolder would be ideal

The module interface should be able to do all thee following:

- Handle click events on their icons
- Display their own UI/windows when clicked
- Communicate with other modules

The method do publish the time is open to choice, whatever works so that something can be published by consumed by modules if required. The use of "message topics" might be worthwhile, so modules can subscribe to certain events from the main app, or even from other modules. Also, care should be taken so that no module clears the events before other modules might have received them

Icon requirements and UI layout aren't too important, anything simple for the PoC will be fine.
