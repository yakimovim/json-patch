using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EdlinSoftware.JsonPatch.Utilities;
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
            JsonSerializer serializer = null,
            ErrorHandlingTypes globalErrorHandling = ErrorHandlingTypes.Throw)
        {
            serializer = serializer ?? JsonSerializer.CreateDefault();

            var copy = initial.DeepClone();

            patchDefinitions = patchDefinitions ?? new JsonPatchDefinition[0];

            foreach (var jsonPatchDefinition in patchDefinitions)
            {
                var result = jsonPatchDefinition.Apply(ref copy, serializer);
                if (result.IsFailure)
                {
                    var errorHandlingType =
                        ((IErrorHandlingTypeProvider)jsonPatchDefinition).ErrorHandlingType
                        ?? globalErrorHandling;
                    if (errorHandlingType == ErrorHandlingTypes.Throw)
                    {
                        throw new JsonPatchException(result.Error);
                    }
                }
            }

            return copy;
        }

        public static JToken PatchTokenCopy(
            JToken initial,
            IReadOnlyList<JsonPatchDefinition> patchDefinitions,
            JsonSerializerSettings serializerSettings,
            ErrorHandlingTypes globalErrorHandling = ErrorHandlingTypes.Throw)
        {
            return PatchTokenCopy(
                initial,
                patchDefinitions,
                JsonSerializer.Create(serializerSettings),
                globalErrorHandling
            );
        }

        public static T PatchObjectCopy<T>(
            T obj,
            IReadOnlyList<JsonPatchDefinition> patchDefinitions,
            JsonSerializer serializer = null,
            ErrorHandlingTypes globalErrorHandling = ErrorHandlingTypes.Throw)
        {
            serializer = serializer ?? JsonSerializer.CreateDefault();

            var token = JToken.FromObject(obj, serializer);

            var patchedCopy = PatchTokenCopy(token, patchDefinitions, serializer, globalErrorHandling);

            return patchedCopy.ToObject<T>(serializer);
        }

        public static T PatchObjectCopy<T>(
            T obj,
            IReadOnlyList<JsonPatchDefinition> patchDefinitions,
            JsonSerializerSettings serializerSettings,
            ErrorHandlingTypes globalErrorHandling = ErrorHandlingTypes.Throw)
        {
            return PatchObjectCopy(
                obj,
                patchDefinitions,
                JsonSerializer.Create(serializerSettings),
                globalErrorHandling
            );
        }
    }
}