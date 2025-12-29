// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.

using System.Diagnostics.CodeAnalysis;

// Suppress explicit default value initializations for clarity
[assembly: SuppressMessage("Style", "CA1805:Do not initialize unnecessarily", Justification = "Explicit initialization improves clarity")]

// Suppress generic exception catching for execution error handling
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Execution context requires catching all exceptions for proper error reporting")]

// Suppress ConfigureAwait warnings - this is a Windows Forms app
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Windows Forms application with UI context")]

// Suppress unused internal class warning
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Class may be used in future")]

// Suppress GetExceptions method to property suggestion
[assembly: SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "Method name better indicates computation")]