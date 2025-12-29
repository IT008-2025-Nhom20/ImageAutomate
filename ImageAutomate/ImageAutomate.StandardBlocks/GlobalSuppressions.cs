// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.

using System.Diagnostics.CodeAnalysis;

// Suppress explicit default value initializations for clarity
[assembly: SuppressMessage("Style", "CA1805:Do not initialize unnecessarily", Justification = "Explicit initialization improves clarity")]

// Suppress enum design warnings for external API compatibility
[assembly: SuppressMessage("Design", "CA1008:Enums should have zero value", Justification = "Enum values match external API requirements")]
[assembly: SuppressMessage("Design", "CA1027:Mark enums with FlagsAttribute", Justification = "Not flags enums, values are bit depths")]
[assembly: SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Enum values match external API naming")]
