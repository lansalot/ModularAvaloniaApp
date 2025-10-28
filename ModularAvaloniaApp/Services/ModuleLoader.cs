using ModularAvaloniaApp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ModularAvaloniaApp.Services
{
    /// <summary>
    /// Service responsible for discovering and loading modules from DLL files
    /// </summary>
    public class ModuleLoader
    {
        private readonly IMessageBus _messageBus;
        private readonly List<IModule> _loadedModules = new();
        private readonly string _modulesPath;

        public ModuleLoader(IMessageBus messageBus, string? modulesPath = null)
        {
            _messageBus = messageBus;
            _modulesPath = modulesPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules");
        }

        /// <summary>
        /// Gets all currently loaded modules
        /// </summary>
        public IReadOnlyList<IModule> LoadedModules => _loadedModules.AsReadOnly();

        /// <summary>
        /// Discover and load all modules from the modules directory
        /// </summary>
        public void LoadAllModules()
        {
            if (!Directory.Exists(_modulesPath))
            {
                Console.WriteLine($"Modules directory not found: {_modulesPath}");
                Directory.CreateDirectory(_modulesPath);
                return;
            }

            var dllFiles = Directory.GetFiles(_modulesPath, "*.dll", SearchOption.TopDirectoryOnly);
            
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    LoadModuleFromFile(dllFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load module from {dllFile}: {ex.Message}");
                }
            }

            Console.WriteLine($"Loaded {_loadedModules.Count} modules from {dllFiles.Length} DLL files.");
        }

        /// <summary>
        /// Load a specific module from a DLL file
        /// </summary>
        private void LoadModuleFromFile(string dllPath)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var moduleType in moduleTypes)
            {
                try
                {
                    if (Activator.CreateInstance(moduleType) is IModule module)
                    {
                        // Check if module with same ID is already loaded
                        if (_loadedModules.Any(m => m.ModuleId == module.ModuleId))
                        {
                            Console.WriteLine($"Module with ID '{module.ModuleId}' is already loaded, skipping.");
                            continue;
                        }

                        // Initialize the module
                        module.Initialize(_messageBus);
                        _loadedModules.Add(module);

                        // Publish module loaded message
                        _messageBus.Publish(MessageTopics.ModuleLoaded, new ModuleLifecycleMessage
                        {
                            ModuleId = module.ModuleId,
                            ModuleName = module.ModuleName
                        });

                        Console.WriteLine($"Loaded module: {module.ModuleName} (ID: {module.ModuleId}, Version: {module.Version})");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to instantiate module type {moduleType.Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Unload a specific module
        /// </summary>
        public bool UnloadModule(string moduleId)
        {
            var module = _loadedModules.FirstOrDefault(m => m.ModuleId == moduleId);
            if (module == null)
                return false;

            try
            {
                module.Shutdown();
                _loadedModules.Remove(module);

                // Publish module unloaded message
                _messageBus.Publish(MessageTopics.ModuleUnloaded, new ModuleLifecycleMessage
                {
                    ModuleId = module.ModuleId,
                    ModuleName = module.ModuleName
                });

                Console.WriteLine($"Unloaded module: {module.ModuleName} (ID: {module.ModuleId})");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unloading module {moduleId}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unload all modules
        /// </summary>
        public void UnloadAllModules()
        {
            var modulesToUnload = _loadedModules.ToList();
            foreach (var module in modulesToUnload)
            {
                UnloadModule(module.ModuleId);
            }
        }

        /// <summary>
        /// Get a module by its ID
        /// </summary>
        public IModule? GetModule(string moduleId)
        {
            return _loadedModules.FirstOrDefault(m => m.ModuleId == moduleId);
        }
    }
}