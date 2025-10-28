using ModularAvaloniaApp.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ModularAvaloniaApp.Services
{
    /// <summary>
    /// Thread-safe message bus implementation using topic-based pub-sub pattern
    /// </summary>
    public class MessageBus : IMessageBus
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<Delegate>> _subscribers = new();
        
        public void Subscribe<T>(string topic, Action<T> handler)
        {
            if (string.IsNullOrEmpty(topic) || handler == null)
                return;

            _subscribers.AddOrUpdate(topic, 
                new ConcurrentBag<Delegate> { handler },
                (key, existingBag) =>
                {
                    existingBag.Add(handler);
                    return existingBag;
                });
        }

        public void Unsubscribe<T>(string topic, Action<T> handler)
        {
            if (string.IsNullOrEmpty(topic) || handler == null)
                return;

            if (_subscribers.TryGetValue(topic, out var subscribers))
            {
                // Create a new bag without the handler to remove
                var newBag = new ConcurrentBag<Delegate>(
                    subscribers.Where(s => !ReferenceEquals(s, handler)));
                
                _subscribers.TryUpdate(topic, newBag, subscribers);
            }
        }

        public void Publish<T>(string topic, T message)
        {
            if (string.IsNullOrEmpty(topic) || message == null)
                return;

            if (_subscribers.TryGetValue(topic, out var subscribers))
            {
                // Create a snapshot of current subscribers to prevent modifications during iteration
                var currentSubscribers = subscribers.ToArray();
                
                foreach (var subscriber in currentSubscribers)
                {
                    try
                    {
                        if (subscriber is Action<T> typedHandler)
                        {
                            // Execute each handler in a try-catch to prevent one module's error 
                            // from affecting others
                            typedHandler.Invoke(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue processing other subscribers
                        Console.WriteLine($"Error in message handler for topic '{topic}': {ex.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the number of subscribers for a topic (useful for debugging)
        /// </summary>
        public int GetSubscriberCount(string topic)
        {
            return _subscribers.TryGetValue(topic, out var subscribers) ? subscribers.Count : 0;
        }
        
        /// <summary>
        /// Clear all subscribers (useful for cleanup)
        /// </summary>
        public void ClearAllSubscribers()
        {
            _subscribers.Clear();
        }
    }
}