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
            IReadOnlyList<JsonPatchOperation> patchOperations,
            JsonSerializer serializer = null,
            ErrorHandlingTypes globalErrorHandling = ErrorHandlingTypes.Throw)
        {
            serializer = serializer ?? JsonSerializer.CreateDefault();

            var copy = initial.DeepClone();

            patchOperations = patchOperations ?? new JsonPatchOperation[0];

            foreach (var jsonPatchOperation in patchOperations)
            {
                var result = jsonPatchOperation.Apply(ref copy, serializer);
                if (result.IsFailure)
                {
                    var errorHandlingType =
                        ((IErrorHandlingTypeProvider)jsonPatchOperation).ErrorHandlingType
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
            IReadOnlyList<JsonPatchOperation> patchOperations,
            JsonSerializerSettings serializerSettings,
            ErrorHandlingTypes globalErrorHandling = ErrorHandlingTypes.Throw)
        {
            return PatchTokenCopy(
                initial,
                patchOperations,
                JsonSerializer.Create(serializerSettings),
                globalErrorHandling
            );
        }

        public static T PatchObjectCopy<T>(
            T obj,
            IReadOnlyList<JsonPatchOperation> patchOperations,
            JsonSerializer serializer = null,
            ErrorHandlingTypes globalErrorHandling = ErrorHandlingTypes.Throw)
        {
            serializer = serializer ?? JsonSerializer.CreateDefault();

            var token = JToken.FromObject(obj, serializer);

            var patchedCopy = PatchTokenCopy(token, patchOperations, serializer, globalErrorHandling);

            return patchedCopy.ToObject<T>(serializer);
        }

        public static T PatchObjectCopy<T>(
            T obj,
            IReadOnlyList<JsonPatchOperation> patchOperations,
            JsonSerializerSettings serializerSettings,
            ErrorHandlingTypes globalErrorHandling = ErrorHandlingTypes.Throw)
        {
            return PatchObjectCopy(
                obj,
                patchOperations,
                JsonSerializer.Create(serializerSettings),
                globalErrorHandling
            );
        }
    }
}