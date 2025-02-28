using System.Text.Json;
using System.Text.RegularExpressions;
using Blocktrust.CredentialWorkflow.Core.Domain.ProcessFlow.Actions.Validation;

namespace Blocktrust.CredentialWorkflow.Core.Services;

/// <summary>
/// Utility class for credential validation rules
/// </summary>
public static class ValidationUtility
{
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
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            return (false, "Invalid format rule configuration");
        }
        
        var path = config[0];
        var format = config[1];
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
        var config = rule.Configuration.Split(':');
        if (config.Length != 2)
        {
            return (false, "Invalid range rule configuration");
        }
        
        var path = config[0];
        
        // Handle range with negative numbers more carefully
        string rangeStr = config[1];
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
    
    public static (bool IsValid, string? ErrorMessage) ValidateCustomRule(JsonDocument credential, ValidationRule rule)
    {
        // Custom validation rules are not implemented yet
        return (false, "Custom validation rules are not implemented yet");
    }
    
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
}