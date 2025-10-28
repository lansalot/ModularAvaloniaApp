# Modular Avalonia Application

A proof-of-concept demonstrating a modular plugin architecture for Avalonia applications using .NET 9.

## Overview

This project showcases how to build an extensible Avalonia application that can load plugins (modules) at runtime from DLL files. Modules can:

- Add their own icons to the application's top bar
- Display controls in the right-side panel
- Subscribe to and publish messages via a message bus
- Communicate with the main application and other modules
- Handle click events and display their own windows

## Architecture

### Main Application (`ModularAvaloniaApp`)

The main application provides:

1. **Main Window** - A window with:
   - Top icon bar (with 4 sample icons + module icons)
   - Central content area with live time display
   - Right side panel for module controls

2. **Message Bus System** - A thread-safe pub-sub message bus for inter-module communication:
   - Topic-based subscriptions
   - Type-safe message handling
   - Prevents modules from interfering with each other's events

3. **Module Loader** - Discovers and loads modules from the `modules` subfolder:
   - Dynamically loads DLL files
   - Instantiates classes implementing `IModule`
   - Manages module lifecycle (initialize, load, unload)

4. **Time Publisher** - Updates every second and publishes to the `time.current` topic

### Module Interface (`IModule`)

Modules must implement the `IModule` interface:

```csharp
public interface IModule
{
    string ModuleId { get; }
    string ModuleName { get; }
    string Version { get; }
    
    void Initialize(IMessageBus messageBus);
    Control GetTopBarIcon();
    IEnumerable<Control> GetRightColumnControls();
    void OnTopBarIconClicked();
    void Shutdown();
}
```

### Message Bus

The message bus allows modules to communicate without tight coupling:

- **Subscribe**: `messageBus.Subscribe<T>(topic, handler)`
- **Publish**: `messageBus.Publish<T>(topic, message)`
- **Unsubscribe**: `messageBus.Unsubscribe<T>(topic, handler)`

Built-in topics:
- `time.current` - Current time updates (published every second)
- `module.loaded` - Fired when a module is loaded
- `module.unloaded` - Fired when a module is unloaded
- `app.shutdown` - Fired when the application is closing

## Project Structure

```
ModularAvaloniaApp/
├── Interfaces/
│   ├── IModule.cs           # Module interface definition
│   └── IMessageBus.cs       # Message bus interface and message types
├── Services/
│   ├── MessageBus.cs        # Message bus implementation
│   └── ModuleLoader.cs      # Module discovery and loading
├── MainWindow.axaml         # Main window UI
├── MainWindow.axaml.cs      # Main window logic
├── App.axaml                # Application resources
├── Program.cs               # Application entry point
└── bin/Debug/net9.0/
    └── modules/             # Module DLLs go here

SampleModule/
└── Class1.cs                # Example module implementation
```

## Building and Running

### Build the Solution

```bash
dotnet build
```

This will:
1. Build the main application
2. Build the sample module
3. Automatically copy the sample module DLL to the `modules` folder

### Run the Application

```bash
cd ModularAvaloniaApp
dotnet run
```

The application will:
1. Start and display the main window
2. Show 4 sample icons in the top bar
3. Display the current time (updating every second)
4. Automatically discover and load modules from the `modules` folder
5. Add the demo module's icon to the top bar
6. Display the demo module's controls in the right panel

## Sample Module Features

The included `DemoModule` demonstrates:

1. **Top Bar Icon** - A green "📦 Demo" button
2. **Right Panel Controls**:
   - Module title
   - Live time display (subscribed to `time.current` topic)
   - Three tool buttons
   - Click counter
3. **Click Handling** - Opens a popup window when the top icon is clicked
4. **Message Publishing** - Publishes custom events that other modules can subscribe to

## Creating Your Own Module

1. Create a new .NET 9 class library project
2. Add references to:
   - `ModularAvaloniaApp` project
   - `Avalonia` NuGet packages (v11.3.0 or higher)
3. Create a class implementing `IModule`
4. Implement all required interface members
5. Build your project
6. Copy the output DLL to the `modules` folder in the main app's output directory

Example minimal module:

```csharp
using Avalonia.Controls;
using ModularAvaloniaApp.Interfaces;
using System.Collections.Generic;

public class MyModule : IModule
{
    public string ModuleId => "my-module";
    public string ModuleName => "My Module";
    public string Version => "1.0.0";

    public void Initialize(IMessageBus messageBus)
    {
        // Subscribe to topics, store reference to message bus
    }

    public Control GetTopBarIcon()
    {
        return new Button { Content = "My Icon" };
    }

    public IEnumerable<Control> GetRightColumnControls()
    {
        return new[] { new TextBlock { Text = "Hello from my module!" } };
    }

    public void OnTopBarIconClicked()
    {
        // Handle icon click
    }

    public void Shutdown()
    {
        // Clean up resources
    }
}
```

## Key Features

### Thread-Safe Message Bus
- Each subscriber receives their own message copy
- Subscriber errors don't affect other subscribers
- Thread-safe subscription management

### Module Isolation
- Modules loaded from separate DLL files
- No hard dependencies between modules
- Clean initialization and shutdown lifecycle

### Extensibility
- Add new modules without recompiling the main app
- Modules can define custom message topics
- Modules can communicate with each other via the message bus

## Future Enhancements

Potential improvements for this PoC:

1. **Module Configuration** - Allow modules to have configuration files
2. **Hot Reload** - Watch the modules folder and load new modules at runtime
3. **Module Dependencies** - Support inter-module dependencies
4. **Module UI Regions** - More flexible UI placement options
5. **Module Permissions** - Control what modules can access
6. **Error Handling** - Better error reporting and recovery
7. **Module Marketplace** - Discover and download modules from a repository

## Technologies Used

- **.NET 9.0** - Latest .NET framework
- **Avalonia 11.3** - Cross-platform UI framework
- **C# 13** - Modern C# language features

## License

This is a proof-of-concept project for educational purposes.
