using System;
using System.Collections.Generic;

namespace ModularAvaloniaApp.Interfaces
{
    /// <summary>
    /// Interface for the message bus system
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Subscribe to messages of a specific topic
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="topic">Topic name to subscribe to</param>
        /// <param name="handler">Handler function to call when message is received</param>
        void Subscribe<T>(string topic, Action<T> handler);
        
        /// <summary>
        /// Unsubscribe from messages of a specific topic
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="topic">Topic name to unsubscribe from</param>
        /// <param name="handler">Handler function to remove</param>
        void Unsubscribe<T>(string topic, Action<T> handler);
        
        /// <summary>
        /// Publish a message to a specific topic
        /// </summary>
        /// <typeparam name="T">Type of message</typeparam>
        /// <param name="topic">Topic to publish to</param>
        /// <param name="message">Message to publish</param>
        void Publish<T>(string topic, T message);
    }
    
    /// <summary>
    /// Standard message types used by the application
    /// </summary>
    public static class MessageTopics
    {
        public const string CurrentTime = "time.current";
        public const string ModuleLoaded = "module.loaded";
        public const string ModuleUnloaded = "module.unloaded";
        public const string ApplicationShutdown = "app.shutdown";
    }
    
    /// <summary>
    /// Time update message
    /// </summary>
    public class TimeUpdateMessage
    {
        public DateTime CurrentTime { get; set; }
        public string FormattedTime { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Module lifecycle message
    /// </summary>
    public class ModuleLifecycleMessage
    {
        public string ModuleId { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
    }
}