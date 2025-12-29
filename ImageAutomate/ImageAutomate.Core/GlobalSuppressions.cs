// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.

using System.Diagnostics.CodeAnalysis;

// Suppress explicit default value initializations for clarity
[assembly: SuppressMessage("Style", "CA1805:Do not initialize unnecessarily", Justification = "Explicit initialization improves clarity")]

// Suppress generic exception catching for plugin loading and serialization
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Plugin/serialization error handling requires catching all exceptions")]

// Suppress EventHandler type suggestion for simple events
[assembly: SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "Action delegate is appropriate for simple events")]

// Suppress internal class suggestions
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class may be used via reflection")]
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types", Justification = "Class may be extended in future")]

// Suppress empty interface warning for marker interfaces
[assembly: SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Marker interface for pipeline sink identification")]

// Suppress ConfigureAwait for WinForms context
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Windows Forms application")]
