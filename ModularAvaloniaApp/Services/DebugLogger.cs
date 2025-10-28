using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.ObjectModel;

namespace ModularAvaloniaApp.Services
{
    /// <summary>
    /// Simple debug logger that shows messages in a window
    /// </summary>
    public static class DebugLogger
    {
        private static Window? _debugWindow;
        private static ListBox? _listBox;
        private static ObservableCollection<string> _messages = new();

        public static void Log(string message)
        {
            var timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
            
            // Also write to console
            Console.WriteLine(timestampedMessage);
            
            // Add to debug window if it exists
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                _messages.Add(timestampedMessage);
                
                // Keep only last 100 messages
                while (_messages.Count > 100)
                {
                    _messages.RemoveAt(0);
                }
            });
        }

        public static void ShowDebugWindow()
        {
            if (_debugWindow != null)
            {
                _debugWindow.Activate();
                return;
            }

            _debugWindow = new Window
            {
                Title = "Debug Console",
                Width = 600,
                Height = 400,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30))
            };

            var stackPanel = new StackPanel { Margin = new Avalonia.Thickness(10) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Debug Console",
                FontSize = 16,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                Margin = new Avalonia.Thickness(0, 0, 0, 10)
            });

            _listBox = new ListBox
            {
                ItemsSource = _messages,
                Background = new SolidColorBrush(Color.FromRgb(37, 37, 38)),
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                FontFamily = new FontFamily("Consolas, Courier New, monospace"),
                FontSize = 11
            };

            var scrollViewer = new ScrollViewer
            {
                Content = _listBox,
                Height = 300
            };

            stackPanel.Children.Add(scrollViewer);

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Margin = new Avalonia.Thickness(0, 10, 0, 0),
                Spacing = 10
            };

            var clearButton = new Button
            {
                Content = "Clear",
                Background = new SolidColorBrush(Color.FromRgb(14, 99, 156)),
                Foreground = Brushes.White,
                Padding = new Avalonia.Thickness(15, 5)
            };
            clearButton.Click += (s, e) => _messages.Clear();

            var closeButton = new Button
            {
                Content = "Close",
                Background = new SolidColorBrush(Color.FromRgb(62, 62, 66)),
                Foreground = Brushes.White,
                Padding = new Avalonia.Thickness(15, 5)
            };
            closeButton.Click += (s, e) => _debugWindow?.Close();

            buttonPanel.Children.Add(clearButton);
            buttonPanel.Children.Add(closeButton);
            stackPanel.Children.Add(buttonPanel);

            _debugWindow.Content = stackPanel;

            _debugWindow.Closed += (s, e) => _debugWindow = null;

            _debugWindow.Show();
        }
    }
}
