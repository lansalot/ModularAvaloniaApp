using Avalonia.Controls;
using System.Collections.Generic;

namespace ModularAvaloniaApp.Interfaces
{
    /// <summary>
    /// Interface that all loadable modules must implement
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Unique identifier for this module
        /// </summary>
        string ModuleId { get; }
        
        /// <summary>
        /// Display name for this module
        /// </summary>
        string ModuleName { get; }
        
        /// <summary>
        /// Version of this module
        /// </summary>
        string Version { get; }
        
        /// <summary>
        /// Initialize the module with access to the message bus
        /// </summary>
        /// <param name="messageBus">The application's message bus for communication</param>
        void Initialize(IMessageBus messageBus);
        
        /// <summary>
        /// Get the control that represents this module's icon in the top bar
        /// </summary>
        /// <returns>Control to display in the top icon row</returns>
        Control GetTopBarIcon();
        
        /// <summary>
        /// Get the list of controls to display in the right column
        /// </summary>
        /// <returns>List of controls for the right side panel</returns>
        IEnumerable<Control> GetRightColumnControls();
        
        /// <summary>
        /// Called when the module's top bar icon is clicked
        /// </summary>
        void OnTopBarIconClicked();
        
        /// <summary>
        /// Called when the module should clean up resources
        /// </summary>
        void Shutdown();
    }
}