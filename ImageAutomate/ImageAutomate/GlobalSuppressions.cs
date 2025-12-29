// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;
using System.Resources;

[assembly: NeutralResourcesLanguage("en-US")]

// Suppress localization warnings for UI strings - this is a desktop app not requiring localization
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Desktop application not requiring localization")]

// Suppress generic exception catching in UI event handlers where user feedback is appropriate
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "UI event handlers need to catch all exceptions to provide user feedback")]

// Suppress EventHandler<string?> warning - string is acceptable for simple file path events
[assembly: SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "Simple event with string payload is acceptable")]

// Suppress static member suggestions for UI methods that may need instance access in future
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "UI methods may need instance access in future")]

// Suppress return type optimization for UI methods using Control base type
[assembly: SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Control base type is appropriate for UI flexibility")]

// Suppress internal type suggestion for WinForms components that need to be public for designer
[assembly: SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "WinForms components require public visibility for designer support")]

// Suppress explicit default value initialization
[assembly: SuppressMessage("Style", "CA1805:Do not initialize unnecessarily", Justification = "Explicit initialization improves clarity")]

// Suppress List<T> return type warning for collection APIs
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "List<T> is appropriate for internal workspace service APIs")]

// Suppress IDisposable warning for singleton service
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Singleton service lifetime is application lifetime")]

// Suppress P/Invoke security warnings for well-known Windows APIs
[assembly: SuppressMessage("Security", "CA5392:Use DefaultDllImportSearchPaths attribute for P/Invokes", Justification = "Using well-known Windows API")]

// Suppress sealed type suggestion
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types", Justification = "Class may be extended in future")]

// Suppress dead code warning for double-check locking pattern
[assembly: SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "Double-check locking pattern")]

// Suppress string comparison suggestions
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison for clarity", Justification = "Simple string operations")]
[assembly: SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Case-insensitive search pattern")]
[assembly: SuppressMessage("Performance", "CA1862:Use the StringComparison method overloads", Justification = "Simple string operations")]

// Suppress ConfigureAwait for WinForms context
[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Windows Forms application")]
