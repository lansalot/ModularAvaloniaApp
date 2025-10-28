using Avalonia.Controls;
using Avalonia.Media;
using ModularAvaloniaApp.Interfaces;
using System;
using System.Collections.Generic;

namespace SampleModule
{
    public class DemoModule : IModule
    {
        private IMessageBus? _messageBus;
        private TextBlock? _timeDisplay;
        private int _clickCount = 0;

        public string ModuleId => "demo-module-1";
        public string ModuleName => "Demo Module";
        public string Version => "1.0.0";

        public void Initialize(IMessageBus messageBus)
        {
            _messageBus = messageBus;
            
            // Subscribe to time updates
            _messageBus.Subscribe<TimeUpdateMessage>(MessageTopics.CurrentTime, OnTimeUpdate);
            
            Console.WriteLine($"Demo Module initialized and subscribed to time updates");
        }

        public Control GetTopBarIcon()
        {
            var button = new Button
            {
                Content = "📦 Demo",
                Width = 80,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(14, 99, 156)),
                Foreground = Brushes.White,
                Margin = new Avalonia.Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
                BorderThickness = new Avalonia.Thickness(1)
            };
            
            button.Click += (sender, e) => OnTopBarIconClicked();
            
            return button;
        }

        public IEnumerable<Control> GetRightColumnControls()
        {
            var controls = new List<Control>();

            // Module title
            controls.Add(new TextBlock
            {
                Text = "Demo Module",
                FontWeight = FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 10, 0, 5),
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            });

            // Time display from main app
            _timeDisplay = new TextBlock
            {
                Text = "Waiting for time...",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(78, 201, 176)),
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            };
            controls.Add(_timeDisplay);

            // Sample icon buttons
            for (int i = 1; i <= 3; i++)
            {
                var iconButton = new Button
                {
                    Content = $"🔧 Tool {i}",
                    Width = 150,
                    Height = 30,
                    Background = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
                    Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                    Margin = new Avalonia.Thickness(0, 2, 0, 2),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(99, 99, 102)),
                    BorderThickness = new Avalonia.Thickness(1)
                };
                
                var toolNumber = i; // Capture for closure
                iconButton.Click += (sender, e) => 
                {
                    Console.WriteLine($"Demo Module Tool {toolNumber} clicked!");
                    
                    // Publish a custom message that other modules could listen to
                    _messageBus?.Publish("demo.tool.clicked", new { ToolNumber = toolNumber, Timestamp = DateTime.Now });
                };
                
                controls.Add(iconButton);
            }

            // Click counter display
            var clickCountDisplay = new TextBlock
            {
                Text = $"Icon clicks: {_clickCount}",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(156, 220, 254)),
                Margin = new Avalonia.Thickness(0, 10, 0, 0)
            };
            controls.Add(clickCountDisplay);

            return controls;
        }

        public void OnTopBarIconClicked()
        {
            _clickCount++;
            Console.WriteLine($"Demo Module top icon clicked! (Total clicks: {_clickCount})");
            
            // Publish click event for other modules
            _messageBus?.Publish("demo.icon.clicked", new { ClickCount = _clickCount, Timestamp = DateTime.Now });
            
            // Show a simple message window
            ShowDemoWindow();
        }

        private void ShowDemoWindow()
        {
            var window = new Window
            {
                Title = "Demo Module Window",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            var content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 10
            };

            content.Children.Add(new TextBlock
            {
                Text = "Hello from Demo Module!",
                FontSize = 16,
                FontWeight = FontWeight.Bold
            });

            content.Children.Add(new TextBlock
            {
                Text = $"This window was opened by clicking the module icon.\nClick count: {_clickCount}",
                TextWrapping = TextWrapping.Wrap
            });

            var closeButton = new Button
            {
                Content = "Close",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 10, 0, 0)
            };
            closeButton.Click += (sender, e) => window.Close();
            content.Children.Add(closeButton);

            window.Content = content;
            window.Show();
        }

        private void OnTimeUpdate(TimeUpdateMessage timeMessage)
        {
            // Update the time display in our right panel
            if (_timeDisplay != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    _timeDisplay.Text = $"Time: {timeMessage.FormattedTime}";
                });
            }
        }

        public void Shutdown()
        {
            // Unsubscribe from events
            if (_messageBus != null)
            {
                _messageBus.Unsubscribe<TimeUpdateMessage>(MessageTopics.CurrentTime, OnTimeUpdate);
            }
            
            Console.WriteLine("Demo Module shutdown completed");
        }
    }
}
