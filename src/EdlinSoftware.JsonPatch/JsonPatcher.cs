using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[assembly: InternalsVisibleTo("EdlinSoftware.JsonPatch.Tests")]

namespace EdlinSoftware.JsonPatch
{
    /// <summary>
    /// Facade for JSON patching.
    /// </summary>
    public static class JsonPatcher
    {
        public static JToken PatchTokenCopy(JToken initial, IReadOnlyList<JsonPatchDefinition> patchDefinitions)
        {
            var copy = initial.DeepClone();

            patchDefinitions = patchDefinitions ?? new JsonPatchDefinition[0];

            foreach (var jsonPatchDefinition in patchDefinitions)
            {
                jsonPatchDefinition.Apply(ref copy);
            }

            return copy;
        }

        public static T PatchObjectCopy<T>(T obj, IReadOnlyList<JsonPatchDefinition> patchDefinitions)
        {
            var token = JToken.FromObject(obj);

            var patchedCopy = PatchTokenCopy(token, patchDefinitions);

            return patchedCopy.ToObject<T>();
        }
    }

    internal static class Utilities
    {
        public static JToken GetJToken(this object value, JsonSerializer serializer = null)
        {
            if(value == null)
                return JToken.Parse("null");

            if (value is JToken jToken)
                return jToken;

            return JToken.FromObject(value, serializer ?? JsonSerializer.CreateDefault());
        }
    }
}