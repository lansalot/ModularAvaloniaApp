using Avalonia.Controls;
using Avalonia.Media;
using ModularAvaloniaApp.Interfaces;
using System;
using System.Collections.Generic;

namespace SecondModule
{
    public class ReaderModule : IModule
    {
        private IMessageBus? _messageBus;
        private TextBlock? _receivedTextDisplay;
        private TextBlock? _debugDisplay;
        private string _lastReceivedText = "Waiting for data...";
        private int _messageCount = 0;

        public string ModuleId => "reader-module-2";
        public string ModuleName => "Reader Module";
        public string Version => "1.0.0";

        public void Initialize(IMessageBus messageBus)
        {
            _messageBus = messageBus;
            
            // Subscribe to text changes from the first module
            _messageBus.Subscribe<TextDataMessage>("demo.text.changed", OnDemoTextChanged);
            
            ModularAvaloniaApp.Services.DebugLogger.Log($"Reader Module initialized and subscribed to demo.text.changed");
        }

        public Control GetTopBarIcon()
        {
            var button = new Button
            {
                Content = "📖 Reader",
                Width = 80,
                Height = 40,
                Background = new SolidColorBrush(Color.FromRgb(106, 27, 154)),
                Foreground = Brushes.White,
                Margin = new Avalonia.Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
                BorderThickness = new Avalonia.Thickness(1)
            };
            
            button.Click += (sender, e) => OnTopBarIconClicked();
            
            return button;
        }

        public IEnumerable<Control> GetLeftColumnControls()
        {
            var controls = new List<Control>();

            // Module title
            controls.Add(new TextBlock
            {
                Text = "Reader Module",
                FontWeight = FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 10, 0, 5),
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            });

            // Display received text
            _receivedTextDisplay = new TextBlock
            {
                Text = _lastReceivedText,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(206, 145, 120)),
                Margin = new Avalonia.Thickness(0, 0, 0, 10),
                TextWrapping = TextWrapping.Wrap
            };
            controls.Add(_receivedTextDisplay);

            controls.Add(new TextBlock
            {
                Text = "Reading from Demo Module",
                FontSize = 9,
                Foreground = new SolidColorBrush(Color.FromRgb(156, 156, 156)),
                Margin = new Avalonia.Thickness(0, 0, 0, 5),
                TextWrapping = TextWrapping.Wrap
            });

            // Debug info
            _debugDisplay = new TextBlock
            {
                Text = $"Messages received: {_messageCount}",
                FontSize = 8,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)),
                Margin = new Avalonia.Thickness(0, 0, 0, 15),
                TextWrapping = TextWrapping.Wrap
            };
            controls.Add(_debugDisplay);

            // Icon buttons for the left column
            for (int i = 1; i <= 4; i++)
            {
                var iconButton = new Button
                {
                    Content = $"🔹 Item {i}",
                    Width = 150,
                    Height = 30,
                    Background = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
                    Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                    Margin = new Avalonia.Thickness(0, 2, 0, 2),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(99, 99, 102)),
                    BorderThickness = new Avalonia.Thickness(1)
                };
                
                var itemNumber = i; // Capture for closure
                iconButton.Click += (sender, e) => 
                {
                    Console.WriteLine($"Reader Module Item {itemNumber} clicked!");
                    
                    // Publish click event
                    _messageBus?.Publish("reader.item.clicked", new { ItemNumber = itemNumber, Timestamp = DateTime.Now });
                };
                
                controls.Add(iconButton);
            }

            return controls;
        }

        public IEnumerable<Control>? GetRightColumnControls()
        {
            // This module only uses the left column
            return null;
        }

        public void OnTopBarIconClicked()
        {
            Console.WriteLine($"Reader Module top icon clicked!");
            
            // Show window with received data
            ShowReaderWindow();
        }

        private void OnDemoTextChanged(TextDataMessage message)
        {
            try
            {
                _messageCount++;
                string text = message.Text ?? "";
                _lastReceivedText = string.IsNullOrEmpty(text) ? "Waiting for data..." : $"Received: {text}";
                
                ModularAvaloniaApp.Services.DebugLogger.Log($"Reader Module received text: '{text}' (Message #{_messageCount})");
                
                // Update UI on UI thread
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (_receivedTextDisplay != null)
                    {
                        _receivedTextDisplay.Text = _lastReceivedText;
                    }
                    if (_debugDisplay != null)
                    {
                        _debugDisplay.Text = $"Messages received: {_messageCount}";
                    }
                });
            }
            catch (Exception ex)
            {
                ModularAvaloniaApp.Services.DebugLogger.Log($"Reader Module error processing text: {ex.Message}");
            }
        }

        private void ShowReaderWindow()
        {
            var window = new Window
            {
                Title = "Reader Module Window",
                Width = 350,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30))
            };

            var content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 10
            };

            content.Children.Add(new TextBlock
            {
                Text = "Reader Module",
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            });

            content.Children.Add(new TextBlock
            {
                Text = "This module reads data from the Demo Module's text box.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(156, 220, 254))
            });

            content.Children.Add(new TextBlock
            {
                Text = "Current data:",
                FontWeight = FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 10, 0, 0),
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
            });

            content.Children.Add(new TextBlock
            {
                Text = _lastReceivedText,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(206, 145, 120)),
                FontSize = 14
            });

            var closeButton = new Button
            {
                Content = "Close",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 15, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(14, 99, 156)),
                Foreground = Brushes.White,
                Padding = new Avalonia.Thickness(20, 8)
            };
            closeButton.Click += (sender, e) => window.Close();
            content.Children.Add(closeButton);

            window.Content = content;
            window.Show();
        }

        public void Shutdown()
        {
            // Unsubscribe from events
            if (_messageBus != null)
            {
                _messageBus.Unsubscribe<TextDataMessage>("demo.text.changed", OnDemoTextChanged);
            }
            
            Console.WriteLine("Reader Module shutdown completed");
        }
    }
}
