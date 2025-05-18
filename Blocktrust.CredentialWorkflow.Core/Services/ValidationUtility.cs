using System.Text.Json;
using System.Text.RegularExpressions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;
using Jint;

namespace Blocktrust.CredentialWorkflow.Core.Services;

/// <summary>
/// Utility class for credential validation rules
/// </summary>
public static class ValidationUtility
{
    #region W3C Validation Methods
    
    public static (bool IsValid, string? ErrorMessage) ValidateRequiredField(JsonDocument credential, ValidationRule rule)
    {
        var path = rule.Configuration;
        var pathParts = path.Split('.');
        var element = credential.RootElement;
        
        if (element.ValueKind == JsonValueKind.Null)
        {
            return (false, $"Required field '{path}' is null");
        }
        
        // Try direct path traversal first
        bool directPathSuccess = true;
        var directPathElement = element;
        
        foreach (var part in pathParts)
        {
            // Check if this part references an array index
            var arrayIndexMatch = Regex.Match(part, @"^(.*)\[(\d+)\]$");
            if (arrayIndexMatch.Success)
            {
                var arrayName = arrayIndexMatch.Groups[1].Value;
                var index = int.Parse(arrayIndexMatch.Groups[2].Value);
                
                if (!directPathElement.TryGetProperty(arrayName, out var arrayElement) || 
                    arrayElement.ValueKind != JsonValueKind.Array ||
                    index >= arrayElement.GetArrayLength())
                {
                    directPathSuccess = false;
                    break;
                }
                
                directPathElement = arrayElement[index];
                continue;
            }
            
            if (!directPathElement.TryGetProperty(part, out var child))
            {
                directPathSuccess = false;
                break;
            }
            
            directPathElement = child;
        }
        
        if (directPathSuccess)
        {
            return (true, null);
        }
        
        // If direct path failed, try with 'vc' prefix for JWT credentials
        if (!pathParts[0].Equals("vc", StringComparison.OrdinalIgnoreCase) && 
            element.TryGetProperty("vc", out var vcElement))
        {
            bool vcPathSuccess = true;
            var vcPathElement = vcElement;
            
            foreach (var part in pathParts)
            {
                // Check if this part references an array index
                var arrayIndexMatch = Regex.Match(part, @"^(.*)\[(\d+)\]$");
                if (arrayIndexMatch.Success)
                {
                    var arrayName = arrayIndexMatch.Groups[1].Value;
                    var index = int.Parse(arrayIndexMatch.Groups[2].Value);
                    
                    if (!vcPathElement.TryGetProperty(arrayName, out var arrayElement) || 
                        arrayElement.ValueKind != JsonValueKind.Array ||
                        index >= arrayElement.GetArrayLength())
                    {
                        vcPathSuccess = false;
                        break;
                    }
                    
                    vcPathElement = arrayElement[index];
                    continue;
                }
                
                if (!vcPathElement.TryGetProperty(part, out var child))
                {
                    vcPathSuccess = false;
                    break;
                }
                
                vcPathElement = child;
            }
            
            if (vcPathSuccess)
            {
                return (true, null);
            }
        }
        
        return (false, $"Required field '{path}' is missing");
    }
    
    public static (bool IsValid, string? ErrorMessage) ValidateFormat(JsonDocument credential, ValidationRule rule)
    {
        // Split only on the first colon
        var firstColonIndex = rule.Configuration.IndexOf(':');
        if (firstColonIndex < 0)
        {
            return (false, "Invalid format rule configuration");
        }
        
        var path = rule.Configuration.Substring(0, firstColonIndex);
        var format = rule.Configuration.Substring(firstColonIndex + 1);
        var pathParts = path.Split('.');
        
        if (!TryGetElementByPath(credential.RootElement, pathParts, out var element))
        {
            return (false, $"Field '{path}' not found");
        }
        
        switch (format.ToUpper())
        {
            case "ISO8601":
                if (element.ValueKind != JsonValueKind.String || 
                    !DateTime.TryParse(element.GetString(), out _))
                {
                    return (false, $"Field '{path}' is not a valid ISO8601 date");
                }
                break;
                
            case "EMAIL":
                if (element.ValueKind != JsonValueKind.String || 
                    !IsValidEmail(element.GetString()))
                {
                    return (false, $"Field '{path}' is not a valid email format");
                }
                break;
                
            case "URL":
                if (element.ValueKind != JsonValueKind.String || 
                    !Uri.TryCreate(element.GetString(), UriKind.Absolute, out _))
                {
                    return (false, $"Field '{path}' is not a valid URL format");
                }
                break;
                
            case "DID":
                if (element.ValueKind != JsonValueKind.String || 
                    !element.GetString().StartsWith("did:"))
                {
                    return (false, $"Field '{path}' is not a valid DID format");
                }
                break;
                
            default:
                return (false, $"Unsupported format '{format}'");
        }
        
        return (true, null);
    }
    
    public static (bool IsValid, string? ErrorMessage) ValidateRange(JsonDocument credential, ValidationRule rule)
    {
        // Split only on the first colon
        var firstColonIndex = rule.Configuration.IndexOf(':');
        if (firstColonIndex < 0)
        {
            return (false, "Invalid range rule configuration");
        }
        
        var path = rule.Configuration.Substring(0, firstColonIndex);
        
        // Handle range with negative numbers more carefully
        string rangeStr = rule.Configuration.Substring(firstColonIndex + 1);
        double min, max;
        
        // Find the last hyphen which separates the min and max values
        int lastHyphenIndex = rangeStr.LastIndexOf('-');
        if (lastHyphenIndex <= 0 || lastHyphenIndex == rangeStr.Length - 1)
        {
            return (false, "Invalid range format");
        }
        
        // Extract min and max parts
        string minStr = rangeStr.Substring(0, lastHyphenIndex);
        string maxStr = rangeStr.Substring(lastHyphenIndex + 1);
        
        // Parse the values
        if (!double.TryParse(minStr, out min) || !double.TryParse(maxStr, out max))
        {
            return (false, "Invalid range values");
        }
        
        var pathParts = path.Split('.');
        
        if (!TryGetElementByPath(credential.RootElement, pathParts, out var element))
        {
            return (false, $"Field '{path}' not found");
        }
        
        double value;
        if (element.ValueKind == JsonValueKind.Number)
        {
            try
            {
                value = element.GetDouble();
            }
            catch (InvalidOperationException)
            {
                return (false, $"Field '{path}' cannot be retrieved as a number");
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
        {
            var stringValue = element.GetString();
            if (!double.TryParse(stringValue, out value))
            {
                return (false, $"Field '{path}' is a string but cannot be parsed as a number");
            }
        }
        else
        {
            return (false, $"Field '{path}' is neither a number nor a string");
        }
        
        if (value < min || value > max)
        {
            return (false, $"Field '{path}' value {value} is outside range {min}-{max}");
        }
        
        return (true, null);
    }
    
    public static (bool IsValid, string? ErrorMessage) ValidateValue(JsonDocument credential, ValidationRule rule)
    {
        // Split only on the first colon to allow expected values to contain colons
        var firstColonIndex = rule.Configuration.IndexOf(':');
        if (firstColonIndex < 0)
        {
            return (false, "Invalid value rule configuration. Expected format: 'path:expectedValue'");
        }
        
        var path = rule.Configuration.Substring(0, firstColonIndex);
        var expectedValue = rule.Configuration.Substring(firstColonIndex + 1);
        var pathParts = path.Split('.');
        
        if (!TryGetElementByPath(credential.RootElement, pathParts, out var element))
        {
            return (false, $"Field '{path}' not found");
        }
        
        string? actualValue = null;
        
        // Get the actual value based on JSON element type
        switch(element.ValueKind)
        {
            case JsonValueKind.String:
                actualValue = element.GetString();
                break;
            case JsonValueKind.Number:
                if (element.TryGetInt32(out var intVal))
                    actualValue = intVal.ToString();
                else if (element.TryGetInt64(out var longVal))
                    actualValue = longVal.ToString();
                else if (element.TryGetDouble(out var doubleVal))
                    actualValue = doubleVal.ToString();
                break;
            case JsonValueKind.True:
                actualValue = "true";
                break;
            case JsonValueKind.False:
                actualValue = "false";
                break;
            case JsonValueKind.Null:
                actualValue = "null";
                break;
            default:
                return (false, $"Field '{path}' value type not supported for exact matching");
        }
        
        if (actualValue != expectedValue)
        {
            return (false, $"Field '{path}' has value '{actualValue}' but expected '{expectedValue}'");
        }
        
        return (true, null);
    }
    
    public static (bool IsValid, string? ErrorMessage) ValidateValueArray(JsonDocument credential, ValidationRule rule)
    {
        // Split only on the first colon
        var firstColonIndex = rule.Configuration.IndexOf(':');
        if (firstColonIndex < 0)
        {
            return (false, "Invalid value array rule configuration. Expected format: 'path:value1,value2,value3'");
        }
        
        var path = rule.Configuration.Substring(0, firstColonIndex);
        var allowedValuesString = rule.Configuration.Substring(firstColonIndex + 1);
        var allowedValues = allowedValuesString.Split(',').Select(v => v.Trim()).ToArray();
        
        if (allowedValues.Length == 0)
        {
            return (false, "No allowed values specified");
        }
        
        var pathParts = path.Split('.');
        
        if (!TryGetElementByPath(credential.RootElement, pathParts, out var element))
        {
            return (false, $"Field '{path}' not found");
        }
        
        string? actualValue = null;
        
        // Get the actual value based on JSON element type
        switch(element.ValueKind)
        {
            case JsonValueKind.String:
                actualValue = element.GetString();
                break;
            case JsonValueKind.Number:
                if (element.TryGetInt32(out var intVal))
                    actualValue = intVal.ToString();
                else if (element.TryGetInt64(out var longVal))
                    actualValue = longVal.ToString();
                else if (element.TryGetDouble(out var doubleVal))
                    actualValue = doubleVal.ToString();
                break;
            case JsonValueKind.True:
                actualValue = "true";
                break;
            case JsonValueKind.False:
                actualValue = "false";
                break;
            case JsonValueKind.Null:
                actualValue = "null";
                break;
            default:
                return (false, $"Field '{path}' value type not supported for array matching");
        }
        
        if (!allowedValues.Contains(actualValue))
        {
            return (false, $"Field '{path}' value '{actualValue}' is not one of the allowed values: {string.Join(", ", allowedValues)}");
        }
        
        return (true, null);
    }

    public static (bool IsValid, string? ErrorMessage) ValidateCustomRule(JsonDocument credential, ValidationRule rule)
    {
        // Custom validation rules are not implemented yet
        return (false, "Custom validation rules are not implemented yet");
    }
    
    #endregion
    
    #region Custom JavaScript Validation Methods
    
    public static (bool IsValid, string? ErrorMessage) ValidateJavaScriptExpression(object data, CustomValidationRule rule)
    {
        try
        {
            // Validate expression length and format
            string expression = rule.Expression?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(expression))
                return (false, "Expression cannot be empty");
                
            if (expression.Length > 300)
                return (false, "Expression exceeds maximum allowed length (300 characters)");
                
            if (expression.Contains('\n') || expression.Contains('\r'))
                return (false, "Multi-line expressions are not allowed");
            
            // Block dangerous patterns - comprehensive blocklist approach
            string[] dangerousPatterns = new[] {
                // Code execution
                @"\beval\s*\(", @"new\s+Function", @"\bsetTimeout\s*\(", @"\bsetInterval\s*\(", 
                
                // Network access
                @"\bfetch\s*\(", @"\bxhr\b", @"\bXMLHttpRequest\b", @"\bWebSocket\b", 
                @"https?://", @"ftp://", @"ws[s]?://", @"\bURL\b", @"\bURLSearchParams\b",
                
                // Module loading
                @"\brequire\s*\(", @"\bimport\s*\(", @"\bimport\s+", @"from\s+['\""]",
                
                // DOM access (shouldn't exist in Jint, but being thorough)
                @"\bdocument\b", @"\bwindow\b", @"\blocation\b", @"\bnavigator\b",
                
                // Object prototype tampering
                @"\b__proto__\b", @"\bObject\.prototype\b", @"\bObject\.defineProperty\b",
                
                // System/environment access
                @"\bprocess\b", @"\bglobal\b", @"\bconsole\b",
                
                // Potential infinite loops
                @"while\s*\([^)]*true", @"for\s*\([^;]*;\s*;", @"do\s*{.*}\s*while\s*\(\s*true\s*\)"
            };
            
            foreach (var pattern in dangerousPatterns)
            {
                if (Regex.IsMatch(expression, pattern, RegexOptions.IgnoreCase))
                    return (false, "Expression contains potentially unsafe operations");
            }
            
            // Create engine with strict execution limits
            var engine = new Engine(options => {
                options.LimitRecursion(3)                     // Extremely low recursion limit
                      .MaxStatements(20)                      // Restrict to few statements
                      .TimeoutInterval(TimeSpan.FromMilliseconds(50)) // Very short timeout
                      .DebugMode(false)                       // No debugging
                      .Strict(true);                          // Enforce strict mode
            });
            
            // Serialize the data if necessary
            if (data is JsonElement jsonElement)
            {
                string jsonString = JsonSerializer.Serialize(jsonElement);
                // Use safer approach without string interpolation that could be exploited
                engine.Execute("var data = " + JsonSerializer.Serialize(jsonString) + ";");
                engine.Execute("data = JSON.parse(data);");
            }
            else
            {
                // Set up the JS environment for non-JsonElement data
                engine.SetValue("data", data);
            }
            
            // Force expression to return boolean
            string safeExpression = $"!!({expression})";
            var isValid = engine.Evaluate(safeExpression).AsBoolean();
            
            return isValid ? (true, null) : (false, rule.ErrorMessage);
        }
        catch (Exception ex)
        {
            // Sanitize error message to avoid leaking implementation details
            return (false, "Expression evaluation failed: Invalid expression");
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
    
    private static bool TryGetElementByPath(JsonElement root, string[] pathParts, out JsonElement result)
    {
        // Try direct path first
        bool directPathSuccess = true;
        var directPathElement = root;
        
        foreach (var part in pathParts)
        {
            // Check if this part references an array index
            var arrayIndexMatch = Regex.Match(part, @"^(.*)\[(\d+)\]$");
            if (arrayIndexMatch.Success)
            {
                var arrayName = arrayIndexMatch.Groups[1].Value;
                var index = int.Parse(arrayIndexMatch.Groups[2].Value);
                
                if (!directPathElement.TryGetProperty(arrayName, out var arrayElement) || 
                    arrayElement.ValueKind != JsonValueKind.Array ||
                    index >= arrayElement.GetArrayLength())
                {
                    directPathSuccess = false;
                    break;
                }
                
                directPathElement = arrayElement[index];
                continue;
            }
            
            if (!directPathElement.TryGetProperty(part, out var child))
            {
                directPathSuccess = false;
                break;
            }
            
            directPathElement = child;
        }
        
        if (directPathSuccess)
        {
            result = directPathElement;
            return true;
        }
        
        // If direct path failed, try with 'vc' prefix for JWT credentials
        if (!pathParts[0].Equals("vc", StringComparison.OrdinalIgnoreCase) && 
            root.TryGetProperty("vc", out var vcElement))
        {
            bool vcPathSuccess = true;
            var vcPathElement = vcElement;
            
            foreach (var part in pathParts)
            {
                // Check if this part references an array index
                var arrayIndexMatch = Regex.Match(part, @"^(.*)\[(\d+)\]$");
                if (arrayIndexMatch.Success)
                {
                    var arrayName = arrayIndexMatch.Groups[1].Value;
                    var index = int.Parse(arrayIndexMatch.Groups[2].Value);
                    
                    if (!vcPathElement.TryGetProperty(arrayName, out var arrayElement) || 
                        arrayElement.ValueKind != JsonValueKind.Array ||
                        index >= arrayElement.GetArrayLength())
                    {
                        vcPathSuccess = false;
                        break;
                    }
                    
                    vcPathElement = arrayElement[index];
                    continue;
                }
                
                if (!vcPathElement.TryGetProperty(part, out var child))
                {
                    vcPathSuccess = false;
                    break;
                }
                
                vcPathElement = child;
            }
            
            if (vcPathSuccess)
            {
                result = vcPathElement;
                return true;
            }
        }
        
        result = default;
        return false;
    }
    
    #endregion
}