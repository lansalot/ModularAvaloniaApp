using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using ModularAvaloniaApp.Interfaces;
using ModularAvaloniaApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ModularAvaloniaApp;

public partial class MainWindow : Window
{
    private readonly IMessageBus _messageBus;
    private readonly ModuleLoader _moduleLoader;
    private readonly Timer _timeUpdateTimer;
    private StackPanel? _topIconBar;
    private StackPanel? _rightModulePanel;
    private TextBlock? _timeLabel;
    private StackPanel? _moduleListPanel;
    private TextBlock? _moduleCountLabel;
    private Button? _refreshModulesButton;
    private Button? _unloadAllButton;
    
    // Track which controls belong to which modules
    private readonly Dictionary<string, List<Control>> _moduleTopBarControls = new();
    private readonly Dictionary<string, List<Control>> _moduleRightPanelControls = new();

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize message bus and module loader
        _messageBus = new MessageBus();
        _moduleLoader = new ModuleLoader(_messageBus);
        
        // Get references to UI elements
        _topIconBar = this.FindControl<StackPanel>("TopIconBar");
        _rightModulePanel = this.FindControl<StackPanel>("RightModulePanel");
        _timeLabel = this.FindControl<TextBlock>("TimeLabel");
        _moduleListPanel = this.FindControl<StackPanel>("ModuleListPanel");
        _moduleCountLabel = this.FindControl<TextBlock>("ModuleCountLabel");
        _refreshModulesButton = this.FindControl<Button>("RefreshModulesButton");
        _unloadAllButton = this.FindControl<Button>("UnloadAllButton");
        
        // Wire up button events
        if (_refreshModulesButton != null)
            _refreshModulesButton.Click += (s, e) => RefreshModules();
        
        if (_unloadAllButton != null)
            _unloadAllButton.Click += (s, e) => UnloadAllModules();
        
        // Add sample icons to top bar
        AddSampleIcons();
        
        // Subscribe to module lifecycle events
        _messageBus.Subscribe<ModuleLifecycleMessage>(MessageTopics.ModuleLoaded, OnModuleLoaded);
        _messageBus.Subscribe<ModuleLifecycleMessage>(MessageTopics.ModuleUnloaded, OnModuleUnloaded);
        
        // Start time update timer
        _timeUpdateTimer = new Timer(UpdateTime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        
        // Load modules
        _moduleLoader.LoadAllModules();
        
        // Update module list
        UpdateModuleList();
    }

    private void AddSampleIcons()
    {
        if (_topIconBar == null) return;

        // Add 4 sample icons as specified
        for (int i = 1; i <= 4; i++)
        {
            var button = new Button
            {
                Content = $"Icon {i}",
                Width = 60,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                Margin = new Avalonia.Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(99, 99, 102)),
                BorderThickness = new Avalonia.Thickness(1)
            };
            
            var iconNumber = i; // Capture for closure
            button.Click += (sender, e) => 
            {
                Console.WriteLine($"Sample Icon {iconNumber} clicked!");
            };
            
            _topIconBar.Children.Add(button);
        }
    }

    private void UpdateTime(object? state)
    {
        var now = DateTime.Now;
        var formattedTime = now.ToString("HH:mm:ss");
        
        // Update UI on UI thread
        Dispatcher.UIThread.Post(() =>
        {
            if (_timeLabel != null)
            {
                _timeLabel.Text = formattedTime;
            }
        });
        
        // Publish time update message for modules
        _messageBus.Publish(MessageTopics.CurrentTime, new TimeUpdateMessage
        {
            CurrentTime = now,
            FormattedTime = formattedTime
        });
    }

    private void OnModuleLoaded(ModuleLifecycleMessage message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var module = _moduleLoader.GetModule(message.ModuleId);
            if (module == null) return;

            // Initialize tracking lists for this module
            _moduleTopBarControls[message.ModuleId] = new List<Control>();
            _moduleRightPanelControls[message.ModuleId] = new List<Control>();

            // Add module's top bar icon
            try
            {
                var topIcon = module.GetTopBarIcon();
                if (topIcon != null && _topIconBar != null)
                {
                    // Wrap in a clickable button if it's not already interactive
                    if (topIcon is not Button)
                    {
                        var wrapper = new Button
                        {
                            Content = topIcon,
                            Padding = new Avalonia.Thickness(5),
                            Margin = new Avalonia.Thickness(2)
                        };
                        wrapper.Click += (sender, e) => module.OnTopBarIconClicked();
                        _topIconBar.Children.Add(wrapper);
                        _moduleTopBarControls[message.ModuleId].Add(wrapper);
                    }
                    else
                    {
                        _topIconBar.Children.Add(topIcon);
                        _moduleTopBarControls[message.ModuleId].Add(topIcon);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding top icon for module {message.ModuleId}: {ex.Message}");
            }

            // Add module's right panel controls
            try
            {
                var rightControls = module.GetRightColumnControls();
                if (rightControls != null && _rightModulePanel != null)
                {
                    foreach (var control in rightControls)
                    {
                        _rightModulePanel.Children.Add(control);
                        _moduleRightPanelControls[message.ModuleId].Add(control);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding right panel controls for module {message.ModuleId}: {ex.Message}");
            }
            
            // Update module list
            UpdateModuleList();
        });
    }

    private void OnModuleUnloaded(ModuleLifecycleMessage message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Remove top bar controls
            if (_moduleTopBarControls.TryGetValue(message.ModuleId, out var topControls))
            {
                foreach (var control in topControls)
                {
                    _topIconBar?.Children.Remove(control);
                }
                _moduleTopBarControls.Remove(message.ModuleId);
            }

            // Remove right panel controls
            if (_moduleRightPanelControls.TryGetValue(message.ModuleId, out var rightControls))
            {
                foreach (var control in rightControls)
                {
                    _rightModulePanel?.Children.Remove(control);
                }
                _moduleRightPanelControls.Remove(message.ModuleId);
            }

            Console.WriteLine($"Module unloaded and UI cleaned up: {message.ModuleName}");
            
            // Update module list
            UpdateModuleList();
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        // Cleanup
        _timeUpdateTimer?.Dispose();
        _moduleLoader?.UnloadAllModules();
        _messageBus.Publish(MessageTopics.ApplicationShutdown, new object());
        
        base.OnClosed(e);
    }

    private void UpdateModuleList()
    {
        if (_moduleListPanel == null || _moduleCountLabel == null) return;

        _moduleListPanel.Children.Clear();

        var modules = _moduleLoader.LoadedModules;
        _moduleCountLabel.Text = $"Modules: {modules.Count}";

        if (modules.Count == 0)
        {
            _moduleListPanel.Children.Add(new TextBlock
            {
                Text = "No modules loaded. Click 'Refresh Modules' to scan for modules.",
                FontStyle = Avalonia.Media.FontStyle.Italic,
                Foreground = new SolidColorBrush(Color.FromRgb(156, 156, 156)),
                TextWrapping = TextWrapping.Wrap
            });
            return;
        }

        foreach (var module in modules)
        {
            var modulePanel = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(37, 37, 38)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
                BorderThickness = new Avalonia.Thickness(1),
                Padding = new Avalonia.Thickness(8),
                Margin = new Avalonia.Thickness(0, 2, 0, 2),
                CornerRadius = new Avalonia.CornerRadius(4)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80, GridUnitType.Pixel) });

            var infoStack = new StackPanel { Orientation = Avalonia.Layout.Orientation.Vertical };
            
            var nameText = new TextBlock
            {
                Text = module.ModuleName,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            };
            
            var detailsText = new TextBlock
            {
                Text = $"ID: {module.ModuleId} | Version: {module.Version}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(156, 220, 254))
            };

            infoStack.Children.Add(nameText);
            infoStack.Children.Add(detailsText);

            var unloadButton = new Button
            {
                Content = "Unload",
                Width = 70,
                Height = 25,
                Background = new SolidColorBrush(Color.FromRgb(199, 46, 13)),
                Foreground = Avalonia.Media.Brushes.White,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            var moduleId = module.ModuleId; // Capture for closure
            unloadButton.Click += (s, e) => UnloadModule(moduleId);

            Grid.SetColumn(infoStack, 0);
            Grid.SetColumn(unloadButton, 1);

            grid.Children.Add(infoStack);
            grid.Children.Add(unloadButton);
            modulePanel.Child = grid;

            _moduleListPanel.Children.Add(modulePanel);
        }
    }

    private void RefreshModules()
    {
        Console.WriteLine("Refreshing modules...");
        _moduleLoader.LoadAllModules();
        UpdateModuleList();
    }

    private void UnloadModule(string moduleId)
    {
        Console.WriteLine($"Unloading module: {moduleId}");
        _moduleLoader.UnloadModule(moduleId);
    }

    private void UnloadAllModules()
    {
        Console.WriteLine("Unloading all modules...");
        
        // Get list of module IDs before unloading
        var moduleIds = _moduleLoader.LoadedModules.Select(m => m.ModuleId).ToList();
        
        foreach (var moduleId in moduleIds)
        {
            _moduleLoader.UnloadModule(moduleId);
        }
        
        UpdateModuleList();
    }
}