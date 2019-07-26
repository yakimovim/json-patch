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
        public static JToken PatchTokenCopy(
            JToken initial,
            IReadOnlyList<JsonPatchDefinition> patchDefinitions,
            JsonSerializer serializer = null)
        {
            serializer = serializer ?? JsonSerializer.CreateDefault();

            var copy = initial.DeepClone();

            patchDefinitions = patchDefinitions ?? new JsonPatchDefinition[0];

            foreach (var jsonPatchDefinition in patchDefinitions)
            {
                jsonPatchDefinition.Apply(ref copy, serializer);
            }

            return copy;
        }

        public static JToken PatchTokenCopy(
            JToken initial,
            IReadOnlyList<JsonPatchDefinition> patchDefinitions,
            JsonSerializerSettings serializerSettings)
        {
            return PatchTokenCopy(initial, patchDefinitions, JsonSerializer.Create(serializerSettings));
        }

        public static T PatchObjectCopy<T>(
            T obj,
            IReadOnlyList<JsonPatchDefinition> patchDefinitions,
            JsonSerializer serializer = null)
        {
            var token = JToken.FromObject(obj);

            var patchedCopy = PatchTokenCopy(token, patchDefinitions, serializer);

            return patchedCopy.ToObject<T>();
        }

        public static T PatchObjectCopy<T>(
            T obj,
            IReadOnlyList<JsonPatchDefinition> patchDefinitions,
            JsonSerializerSettings settings)
        {
            var token = JToken.FromObject(obj);

            var patchedCopy = PatchTokenCopy(token, patchDefinitions, settings);

            return patchedCopy.ToObject<T>();
        }
    }
}