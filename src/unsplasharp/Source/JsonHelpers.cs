using System;
using System.Text.Json;

namespace Unsplasharp
{
    /// <summary>
    /// Helper methods for JSON parsing with System.Text.Json
    /// </summary>
    internal static class JsonHelpers
    {
        /// <summary>
        /// Safely gets a string value from a JsonElement
        /// </summary>
        public static string? GetString(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.Null) return null;
                if (property.ValueKind == JsonValueKind.String) return property.GetString();
                if (property.ValueKind == JsonValueKind.Number) return property.ToString();
                return property.ToString();
            }
            return null;
        }

        /// <summary>
        /// Safely gets an integer value from a JsonElement
        /// </summary>
        public static int GetInt32(this JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
                {
                    return value;
                }
                if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var stringValue))
                {
                    return stringValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely gets a nullable integer value from a JsonElement
        /// </summary>
        public static int? GetNullableInt32(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }
                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
                {
                    return value;
                }
                if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var stringValue))
                {
                    return stringValue;
                }
            }
            return null;
        }

        /// <summary>
        /// Safely gets a boolean value from a JsonElement
        /// </summary>
        public static bool GetBoolean(this JsonElement element, string propertyName, bool defaultValue = false)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.True)
                {
                    return true;
                }
                if (property.ValueKind == JsonValueKind.False)
                {
                    return false;
                }
                if (property.ValueKind == JsonValueKind.String && bool.TryParse(property.GetString(), out var stringValue))
                {
                    return stringValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely gets a double value from a JsonElement
        /// </summary>
        public static double GetDouble(this JsonElement element, string propertyName, double defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value))
                {
                    return value;
                }
                if (property.ValueKind == JsonValueKind.String && double.TryParse(property.GetString(), out var stringValue))
                {
                    return stringValue;
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Safely gets a nested JsonElement
        /// </summary>
        public static JsonElement? GetProperty(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.ValueKind == JsonValueKind.Null ? null : property;
            }
            return null;
        }

        /// <summary>
        /// Checks if a JsonElement exists and is not null
        /// </summary>
        public static bool HasProperty(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var property) && 
                   property.ValueKind != JsonValueKind.Null;
        }

        /// <summary>
        /// Gets a nested string value using dot notation (e.g., "urls.raw")
        /// </summary>
        public static string? GetNestedString(this JsonElement element, string path)
        {
            var parts = path.Split('.');
            var current = element;

            foreach (var part in parts)
            {
                if (!current.TryGetProperty(part, out current))
                {
                    return null;
                }
                if (current.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
        }

        /// <summary>
        /// Gets a nested integer value using dot notation
        /// </summary>
        public static int GetNestedInt32(this JsonElement element, string path, int defaultValue = 0)
        {
            var parts = path.Split('.');
            var current = element;

            foreach (var part in parts)
            {
                if (!current.TryGetProperty(part, out current))
                {
                    return defaultValue;
                }
                if (current.ValueKind == JsonValueKind.Null)
                {
                    return defaultValue;
                }
            }

            if (current.ValueKind == JsonValueKind.Number && current.TryGetInt32(out var value))
            {
                return value;
            }
            if (current.ValueKind == JsonValueKind.String && int.TryParse(current.GetString(), out var stringValue))
            {
                return stringValue;
            }

            return defaultValue;
        }

        /// <summary>
        /// Safely enumerates array elements
        /// </summary>
        public static JsonElement.ArrayEnumerator GetArrayEnumerator(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) && 
                property.ValueKind == JsonValueKind.Array)
            {
                return property.EnumerateArray();
            }
            
            // Return empty enumerator
            return new JsonElement().EnumerateArray();
        }

        /// <summary>
        /// Checks if element is null or undefined
        /// </summary>
        public static bool IsNullOrUndefined(this JsonElement? element)
        {
            return element == null || element.Value.ValueKind == JsonValueKind.Null || element.Value.ValueKind == JsonValueKind.Undefined;
        }
    }
}
