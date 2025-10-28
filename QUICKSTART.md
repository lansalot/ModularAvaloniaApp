# Quick Start Guide - Modular Avalonia App

## Running the Application

1. **Build the solution:**
   ```bash
   dotnet build
   ```

2. **Run the application:**
   ```bash
   cd ModularAvaloniaApp
   dotnet run
   ```

3. **What you should see:**
   - A window with 4 sample icons in the top bar
   - A green "📦 Demo" button (from the loaded module)
   - Live time display in the center
   - Module controls in the right panel including:
     - "Demo Module" title
     - Time display from the module
     - Three tool buttons
     - Click counter

## Testing the Features

### Main Application Features
- **Time Display**: The time in the center updates every second
- **Sample Icons**: Click any of the 4 sample icons at the top (outputs to console)

### Module Features
- **Module Icon**: Click the "📦 Demo" button to:
  - Open a popup window
  - Increment the click counter
  - See console output
  
- **Module Tools**: Click any of the three tool buttons in the right panel:
  - Each publishes a message to the message bus
  - Outputs to console showing which tool was clicked

- **Time Subscription**: The right panel shows the current time, demonstrating that the module successfully subscribes to the `time.current` topic

## Console Output

Watch the console to see:
- Module loading messages
- Module initialization confirmation
- Time subscription confirmation
- Click events from icons and tools
- Message bus activity

## Project Files

Key files to examine:

1. **`ModularAvaloniaApp/Interfaces/IModule.cs`** - Module interface definition
2. **`ModularAvaloniaApp/Interfaces/IMessageBus.cs`** - Message bus interface
3. **`ModularAvaloniaApp/Services/MessageBus.cs`** - Message bus implementation
4. **`ModularAvaloniaApp/Services/ModuleLoader.cs`** - Module loading logic
5. **`ModularAvaloniaApp/MainWindow.axaml.cs`** - Main window implementation
6. **`SampleModule/Class1.cs`** - Example module implementation

## Creating Additional Modules

To add more modules:

1. Create a new class library project:
   ```bash
   dotnet new classlib -n YourModule -f net9.0
   ```

2. Add project references and Avalonia packages to `YourModule.csproj`:
   ```xml
   <ItemGroup>
     <ProjectReference Include="..\ModularAvaloniaApp\ModularAvaloniaApp.csproj" />
   </ItemGroup>
   <ItemGroup>
     <PackageReference Include="Avalonia" Version="11.3.0" />
     <PackageReference Include="Avalonia.Desktop" Version="11.3.0" />
   </ItemGroup>
   ```

3. Implement `IModule` interface in your class

4. Add your module to the solution:
   ```bash
   dotnet sln add YourModule/YourModule.csproj
   ```

5. Update the `ModularAvaloniaApp.csproj` CopyModules target to include your new module:
   ```xml
   <ItemGroup>
     <ModuleFiles Include="$(MSBuildProjectDirectory)\..\YourModule\bin\$(Configuration)\$(TargetFramework)\YourModule.dll" />
   </ItemGroup>
   ```

6. Rebuild and run!

## Troubleshooting

### Module Not Loading
- Check that the DLL is in the `bin/Debug/net9.0/modules` folder
- Verify your class implements `IModule`
- Check console for error messages

### Module Not Displaying
- Ensure `GetTopBarIcon()` returns a valid control
- Ensure `GetRightColumnControls()` returns valid controls
- Check that `Initialize()` doesn't throw exceptions

### Time Not Updating in Module
- Verify subscription to `MessageTopics.CurrentTime`
- Ensure handler is on UI thread (use `Dispatcher.UIThread.Post()`)
- Check console for error messages

## Next Steps

Experiment with:
- Creating custom message topics
- Making modules communicate with each other
- Adding more UI elements to modules
- Implementing module configuration
- Creating specialized module types (e.g., for different purposes)
