using System;
using EdlinSoftware.JsonPatch.Pointers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch
{
    /// <summary>
    /// Visitor interface for <see cref="JsonPatchDefinition"/> class.
    /// </summary>
    public interface IJsonPatchDefinitionVisitor
    {
        /// <summary>
        /// Visits add patch definition.
        /// </summary>
        /// <param name="path">Path to add to.</param>
        /// <param name="value">Value to add.</param>
        void VisitAdd(JsonPointer path, object value);
    }

    /// <summary>
    /// Represents some Json patch definition.
    /// </summary>
    [JsonConverter(typeof(JsonPatchDefinitionConverter))]
    public abstract class JsonPatchDefinition
    {
        /// <summary>
        /// Path to the modification place.
        /// </summary>
        public JsonPointer Path { get; set; }

        /// <summary>
        /// Implementation of Visitor pattern.
        /// </summary>
        /// <param name="visitor">Visitor.</param>
        /// <exception cref="ArgumentNullException">Visitor should not be null.</exception>
        public abstract void Visit(IJsonPatchDefinitionVisitor visitor);
    }

    public sealed class JsonPatchAddDefinition : JsonPatchDefinition
    {
        /// <summary>
        /// Value to add.
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc />
        public override void Visit(IJsonPatchDefinitionVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            visitor.VisitAdd(Path, Value);
        }
    }

    public sealed class JsonPatchDefinitionConverter : JsonConverter<JsonPatchDefinition>
    {
        private sealed class JsonPatchDefinitionConverterWriter : IJsonPatchDefinitionVisitor
        {
            private readonly JsonWriter _writer;
            private readonly JsonSerializer _serializer;

            public JsonPatchDefinitionConverterWriter(
                JsonWriter writer,
                JsonSerializer serializer)
            {
                _writer = writer ?? throw new ArgumentNullException(nameof(writer));
                _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            }

            public void VisitAdd(JsonPointer path, object value)
            {
                _writer.WriteStartObject();
                _writer.WritePropertyName("op");
                _writer.WriteValue("add");
                _writer.WritePropertyName("path");
                _writer.WriteValue(path.ToString());
                _writer.WritePropertyName("value");
                _serializer.Serialize(_writer, value);
                _writer.WriteEndObject();
            }
        }

        public override void WriteJson(JsonWriter writer, JsonPatchDefinition value, JsonSerializer serializer)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "Can't serialize null patch definitions.");

            value.Visit(new JsonPatchDefinitionConverterWriter(writer, serializer));
        }

        public override JsonPatchDefinition ReadJson(JsonReader reader, Type objectType, JsonPatchDefinition existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);

            if (token == null) throw new InvalidOperationException("There is no patch definition.");

            if (token.Type != JTokenType.Object) throw new InvalidOperationException("Patch definition should be a Json object.");

            JObject patchDefinitionJObject = token as JObject;
            if (patchDefinitionJObject == null) throw new InvalidOperationException("Patch definition should be a Json object.");

            var operation = GetMandatoryPropertyValue<string>(patchDefinitionJObject, "op") ?? "";

            switch (operation.ToLowerInvariant())
            {
                case "add":
                    {
                        return new JsonPatchAddDefinition
                        {
                            Path = GetMandatoryPropertyValue<string>(patchDefinitionJObject, "path"),
                            Value = patchDefinitionJObject.GetValue("value")
                        };
                    }
                default:
                    throw new InvalidOperationException($"Unknown value of 'op' property: '{operation}'");
            }
        }

        private T GetMandatoryPropertyValue<T>(JObject patchDefinitionJObject, string propertyName)
        {
            if (!patchDefinitionJObject.ContainsKey(propertyName)) throw new InvalidOperationException($"Patch definition must contain '{propertyName}' property");

            return patchDefinitionJObject.Value<T>(propertyName);
        }
    }
}