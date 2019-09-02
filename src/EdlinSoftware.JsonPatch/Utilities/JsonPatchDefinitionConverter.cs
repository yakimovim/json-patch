using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch.Utilities
{
    using static JsonOperations;

    public sealed class JsonPatchDefinitionConverter : JsonConverter<JsonPatchDefinition>
    {
        private static readonly IReadOnlyDictionary<JsonPatchTypes, Type> KnownJsonPatchTypes;

        static JsonPatchDefinitionConverter()
        {
            var jsonPatchBaseType = typeof(JsonPatchDefinition);

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
            JsonPatchDefinition value,
            JsonSerializer serializer)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            value.WriteToJson(writer, serializer);
        }

        public override JsonPatchDefinition ReadJson(
            JsonReader reader,
            Type objectType,
            JsonPatchDefinition existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);

            if (token == null) throw new JsonPatchException("There is no patch definition.");

            if (token.Type != JTokenType.Object) throw new JsonPatchException(JsonPatchMessages.PatchDefinitionShouldBeJsonObject);

            JObject patchDefinitionJObject = token as JObject;
            if (patchDefinitionJObject == null) throw new JsonPatchException(JsonPatchMessages.PatchDefinitionShouldBeJsonObject);

            var operation = GetMandatoryPropertyValue<string>(patchDefinitionJObject, "op") ?? "";

            var patchType = (JsonPatchTypes)Enum.Parse(typeof(JsonPatchTypes), operation, ignoreCase: true);
            if (!Enum.IsDefined(typeof(JsonPatchTypes), patchType))
                throw new JsonPatchException($"Unknown value of 'op' property: '{operation}'");

            if (!KnownJsonPatchTypes.TryGetValue(patchType, out var jsonPatchType))
                throw new JsonPatchException($"Patch operation '{operation}' is not supported");

            var jsonPatchDefinition = (JsonPatchDefinition)Activator.CreateInstance(jsonPatchType);
            jsonPatchDefinition.FillFromJson(patchDefinitionJObject);
            return jsonPatchDefinition;
        }
    }

}