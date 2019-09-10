using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch.Utilities
{
    using static JsonOperations;

    public sealed class JsonPatchOperationConverter : JsonConverter<JsonPatchOperation>
    {
        private static readonly IReadOnlyDictionary<JsonPatchTypes, Type> KnownJsonPatchTypes;

        static JsonPatchOperationConverter()
        {
            var jsonPatchBaseType = typeof(JsonPatchOperation);

            KnownJsonPatchTypes = jsonPatchBaseType
                .GetTypeInfo()
                .Assembly
                .DefinedTypes
                .Where(t => t.IsSubclassOf(jsonPatchBaseType))
                .Where(t => !t.IsAbstract)
                .Select(t => new
                {
                    Type = t,
                    Attribute = t.GetCustomAttribute<PatchTypeAttribute>()
                })
                .Where(t => t.Attribute != null)
                .Select(t => new
                {
                    t.Type,
                    t.Attribute.PatchType
                })
                .ToDictionary(t => t.PatchType, t => t.Type.AsType());
        }

        public override void WriteJson(
            JsonWriter writer,
            JsonPatchOperation value,
            JsonSerializer serializer)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            value.WriteToJson(writer, serializer);
        }

        public override JsonPatchOperation ReadJson(
            JsonReader reader,
            Type objectType,
            JsonPatchOperation existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);

            if (token == null) throw new JsonPatchException("There is no patch operation.");

            if (token.Type != JTokenType.Object) throw new JsonPatchException(JsonPatchMessages.PatchOperationShouldBeJsonObject);

            JObject patchOperationJObject = token as JObject;
            if (patchOperationJObject == null) throw new JsonPatchException(JsonPatchMessages.PatchOperationShouldBeJsonObject);

            var operation = GetMandatoryPropertyValue<string>(patchOperationJObject, "op") ?? "";

            var patchType = (JsonPatchTypes)Enum.Parse(typeof(JsonPatchTypes), operation, ignoreCase: true);
            if (!Enum.IsDefined(typeof(JsonPatchTypes), patchType))
                throw new JsonPatchException($"Unknown value of 'op' property: '{operation}'");

            if (!KnownJsonPatchTypes.TryGetValue(patchType, out var jsonPatchType))
                throw new JsonPatchException($"Patch operation '{operation}' is not supported");

            var jsonPatchOperation = (JsonPatchOperation)Activator.CreateInstance(jsonPatchType);
            jsonPatchOperation.FillFromJson(patchOperationJObject);
            return jsonPatchOperation;
        }
    }

}