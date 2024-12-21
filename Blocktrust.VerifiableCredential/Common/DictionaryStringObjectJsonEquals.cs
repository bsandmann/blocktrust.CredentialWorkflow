namespace Blocktrust.VerifiableCredential.Common;

using System.Text.Json;
using System.Text.Json.Nodes;

public static class DictionaryStringObjectJsonEquals
{
    private static readonly JsonSerializerOptions JsonSerializationOptionsForEquality = new JsonSerializerOptions();

    public static bool JsonEquals<TKey, TValue>(IDictionary<TKey, TValue>? dict1, IDictionary<TKey, TValue>? dict2)
    {
        if (dict1 == null && dict2 == null) return true;
        if (dict1 == null || dict2 == null) return false;
        if (dict1.Count != dict2.Count) return false;
        bool deepEquals = true;
        
        // First we try the faster and generally correct way of doing the comparison with deepEquals
        // since this sometimes fails with just simple string I use in tests I fall back to the slower
        // implematation of serializing the dictionary to json and comparing the json strings
        foreach (var kv in dict1)
        {
            if (!dict2.TryGetValue(kv.Key, out var value)) return false;

            if (kv.Value is JsonElement jsonElement1 && value is JsonElement jsonElement2)
            {
                var node1 = JsonNode.Parse(jsonElement1.GetRawText());
                var node2 = JsonNode.Parse(jsonElement2.GetRawText());
                if (!JsonNode.DeepEquals(node1, node2)) return false;
            }
            else if (!Equals(kv.Value, value))
            {
                deepEquals = false;
            }
        }

        if (deepEquals == false)
        {
            return JsonSerializer.Serialize(dict1, JsonSerializationOptionsForEquality) == JsonSerializer.Serialize(dict2, JsonSerializationOptionsForEquality);
        }

        return true;
    }

    public static void AddToHashCode<TKey, TValue>(IDictionary<TKey, TValue>? dictionary, ref HashCode hashCode)
    {
        if (dictionary != null)
        {
            foreach (var pair in dictionary)
            {
                hashCode.Add(pair.Key);
                hashCode.Add(pair.Value);
            }
        }
    }
}