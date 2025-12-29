// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.

using System.Diagnostics.CodeAnalysis;

// Suppress generic exception catching in UI rendering code
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "UI rendering needs to handle all exceptions gracefully")]

// Suppress parameter name mismatch for overridden methods
[assembly: SuppressMessage("Naming", "CA1725:Parameter names should match base declaration", Justification = "Common convention in WinForms")]

// Suppress localization warnings for UI strings
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Desktop application not requiring localization")]
