using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MPDCtrlX.Common;

public static class PathSanitizer
{
    public static string SanitizeFilename(string name)
    {
        // 1. Get the list of invalid characters for the current system
        // and add additional common invalid path characters.
        char[] invalidChars = Path.GetInvalidFileNameChars();

        // 2. Create a regex pattern to match invalid characters.
        // We escape the characters to ensure they are interpreted literally.
        string invalidCharsPattern = "[" + Regex.Escape(new string(invalidChars)) + "]";

        // 3. Replace all invalid characters with the replacement character.
        string sanitizedName = Regex.Replace(name, invalidCharsPattern, "_");

        // 4. Handle reserved Windows filenames (e.g., CON, PRN, NUL).
        string[] reservedNames = ["CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"];
        if (Array.Exists(reservedNames, s => s.Equals(sanitizedName, StringComparison.OrdinalIgnoreCase)))
        {
            sanitizedName = $"_{sanitizedName}_";
        }

        // 5. Trim trailing periods and spaces, which are invalid on Windows.
        sanitizedName = sanitizedName.TrimEnd('.', ' ');

        // 6. Ensure the filename isn't empty after sanitizing.
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            return "Untitled";
        }

        return sanitizedName;
    }

}
