using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EdlinSoftware.JsonPatch.Pointers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch
{
    using static JsonOperations;

    internal enum JsonPatchTypes
    {
        Add,
        Remove,
        Replace,
        Move,
        Copy,
        Test
    }

    [AttributeUsage(AttributeTargets.Class)]
    internal class PatchTypeAttribute : Attribute
    {
        public JsonPatchTypes PatchType { get; }

        public PatchTypeAttribute(JsonPatchTypes patchType)
        {
            PatchType = patchType;
        }
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
        /// Writes this object to JSON.
        /// </summary>
        /// <param name="writer">JSON writer.</param>
        /// <param name="serializer">JSON serializer.</param>
        internal void WriteToJson(JsonWriter writer, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("op");
            writer.WriteValue(GetOperationName());
            writer.WritePropertyName("path");
            writer.WriteValue(Path.ToString());
            WriteAdditionalJsonProperties(writer, serializer);
            writer.WriteEndObject();
        }

        private string GetOperationName()
        {
            return GetType()
                .GetTypeInfo()
                .GetCustomAttribute<PatchTypeAttribute>()
                .PatchType
                .ToString()
                .ToLowerInvariant();
        }

        /// <summary>
        /// Writes additional properties (except 'op' and 'path') of this object to JSON.
        /// </summary>
        /// <param name="writer">JSON writer.</param>
        /// <param name="serializer">JSON serializer.</param>
        protected virtual void WriteAdditionalJsonProperties(JsonWriter writer, JsonSerializer serializer) { }

        /// <summary>
        /// Fills properties of this object from JSON.
        /// </summary>
        /// <param name="jObject">JSON object.</param>
        internal void FillFromJson(JObject jObject)
        {
            Path = GetMandatoryPropertyValue<string>(jObject, "path");
            FillAdditionalPropertiesFromJson(jObject);
        }

        /// <summary>
        /// Fills additional (except <see cref="Path"/>) properties of this object from JSON.
        /// </summary>
        /// <param name="jObject">JSON object.</param>
        protected virtual void FillAdditionalPropertiesFromJson(JObject jObject) { }

        /// <summary>
        /// Applies this patch to the token.
        /// </summary>
        /// <param name="token">JSON token.</param>
        internal abstract void Apply(ref JToken token);
    }

    [PatchType(JsonPatchTypes.Add)]
    public sealed class JsonPatchAddDefinition : JsonPatchDefinition
    {
        /// <summary>
        /// Value to add.
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc />
        protected override void WriteAdditionalJsonProperties(JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName("value");
            serializer.Serialize(writer, Value);
        }

        /// <inheritdoc />
        protected override void FillAdditionalPropertiesFromJson(JObject jObject)
        {
            Value = jObject.GetValue("value");
        }

        /// <inheritdoc />
        internal override void Apply(ref JToken token)
        {
            var pointer = JTokenPointer.Get(token, Path);

            switch (pointer)
            {
                case JRootPointer _:
                    {
                        token = Value.GetJToken();
                        break;
                    }
                case JObjectPointer jObjectPointer:
                    {
                        jObjectPointer.SetValue(Value);
                        break;
                    }
                case JArrayPointer jArrayPointer:
                    {
                        jArrayPointer.SetValue(Value);
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unknown type of path pointer.");
            }
        }
    }

    [PatchType(JsonPatchTypes.Remove)]
    public sealed class JsonPatchRemoveDefinition : JsonPatchDefinition
    {
        /// <inheritdoc />
        internal override void Apply(ref JToken token)
        {
            var pointer = JTokenPointer.Get(token, Path);

            switch (pointer)
            {
                case JRootPointer _:
                    {
                        token = null;
                        break;
                    }
                case JObjectPointer jObjectPointer:
                    {
                        jObjectPointer.RemoveValue();
                        break;
                    }
                case JArrayPointer jArrayPointer:
                    {
                        jArrayPointer.RemoveValue();
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unknown type of path pointer.");
            }
        }
    }

    [PatchType(JsonPatchTypes.Replace)]
    public sealed class JsonPatchReplaceDefinition : JsonPatchDefinition
    {
        /// <summary>
        /// Value to replace.
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc />
        protected override void WriteAdditionalJsonProperties(JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName("value");
            serializer.Serialize(writer, Value);
        }

        /// <inheritdoc />
        protected override void FillAdditionalPropertiesFromJson(JObject jObject)
        {
            Value = jObject.GetValue("value");
        }

        /// <inheritdoc />
        internal override void Apply(ref JToken token)
        {
            var pointer = JTokenPointer.Get(token, Path);

            switch (pointer)
            {
                case JRootPointer _:
                    {
                        token = Value.GetJToken();
                        break;
                    }
                case JObjectPointer jObjectPointer:
                    {
                        jObjectPointer.RemoveValue();
                        jObjectPointer.SetValue(Value);
                        break;
                    }
                case JArrayPointer jArrayPointer:
                    {
                        jArrayPointer.RemoveValue();
                        jArrayPointer.SetValue(Value);
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unknown type of path pointer.");
            }
        }
    }

    [PatchType(JsonPatchTypes.Move)]
    public sealed class JsonPatchMoveDefinition : JsonPatchDefinition
    {
        /// <summary>
        /// JSON pointer to the place to take value from.
        /// </summary>
        public JsonPointer From { get; set; }

        /// <inheritdoc />
        protected override void WriteAdditionalJsonProperties(JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName("from");
            writer.WriteValue(From.ToString());
        }

        /// <inheritdoc />
        protected override void FillAdditionalPropertiesFromJson(JObject jObject)
        {
            From = GetMandatoryPropertyValue<string>(jObject, "from");
        }

        /// <inheritdoc />
        internal override void Apply(ref JToken token)
        {
            if (From.IsPrefixOf(Path))
                throw new InvalidOperationException("Unable to move parent JSON to a child.");

            var fromPointer = JTokenPointer.Get(token, From);
            var toPointer = JTokenPointer.Get(token, Path);

            var tokenToMove = RemoveTokenFromSourceAndReturnIt(fromPointer);

            switch (toPointer)
            {
                case JRootPointer _:
                    {
                        token = tokenToMove;
                        break;
                    }
                case JObjectPointer jObjectPointer:
                    {
                        jObjectPointer.SetValue(tokenToMove);
                        break;
                    }
                case JArrayPointer jArrayPointer:
                    {
                        jArrayPointer.SetValue(tokenToMove);
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unknown type of path pointer.");
            }
        }

        private JToken RemoveTokenFromSourceAndReturnIt(JTokenPointer fromPointer)
        {
            switch (fromPointer)
            {
                case JObjectPointer jObjectPointer:
                    {
                        var token = jObjectPointer.GetValue();
                        jObjectPointer.RemoveValue();
                        return token;
                    }
                case JArrayPointer jArrayPointer:
                    {
                        var token = jArrayPointer.GetValue();
                        jArrayPointer.RemoveValue();
                        return token;
                    }
                default:
                    throw new InvalidOperationException("Can't move JSON");
            }
        }
    }

    [PatchType(JsonPatchTypes.Copy)]
    public sealed class JsonPatchCopyDefinition : JsonPatchDefinition
    {
        /// <summary>
        /// JSON pointer to the place to take value from.
        /// </summary>
        public JsonPointer From { get; set; }

        /// <inheritdoc />
        protected override void WriteAdditionalJsonProperties(JsonWriter writer, JsonSerializer serializer)
        {
            writer.WritePropertyName("from");
            writer.WriteValue(From.ToString());
        }

        /// <inheritdoc />
        protected override void FillAdditionalPropertiesFromJson(JObject jObject)
        {
            From = GetMandatoryPropertyValue<string>(jObject, "from");
        }

        /// <inheritdoc />
        internal override void Apply(ref JToken token)
        {
            var fromPointer = JTokenPointer.Get(token, From);
            var toPointer = JTokenPointer.Get(token, Path);

            var tokenToCopy = GetSourceTokenCopy(token, fromPointer);

            switch (toPointer)
            {
                case JRootPointer _:
                    {
                        token = tokenToCopy;
                        break;
                    }
                case JObjectPointer jObjectPointer:
                    {
                        jObjectPointer.SetValue(tokenToCopy);
                        break;
                    }
                case JArrayPointer jArrayPointer:
                    {
                        jArrayPointer.SetValue(tokenToCopy);
                        break;
                    }
                default:
                    throw new InvalidOperationException("Unknown type of path pointer.");
            }
        }

        private JToken GetSourceTokenCopy(JToken rootToken, JTokenPointer fromPointer)
        {
            switch (fromPointer)
            {
                case JRootPointer _:
                    {
                        return rootToken.DeepClone();
                    }
                case JObjectPointer jObjectPointer:
                    {
                        return jObjectPointer.GetValue().DeepClone();
                    }
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.GetValue().DeepClone();
                    }
                default:
                    throw new InvalidOperationException("Can't copy JSON");
            }
        }
    }

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

        public override void WriteJson(JsonWriter writer, JsonPatchDefinition value, JsonSerializer serializer)
        {
            if (value == null) throw new ArgumentNullException(nameof(value), "Can't serialize null patch definitions.");

            value.WriteToJson(writer, serializer);
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

            var patchType = (JsonPatchTypes)Enum.Parse(typeof(JsonPatchTypes), operation, ignoreCase: true);
            if (!Enum.IsDefined(typeof(JsonPatchTypes), patchType))
                throw new InvalidOperationException($"Unknown value of 'op' property: '{operation}'");

            if (!KnownJsonPatchTypes.TryGetValue(patchType, out var jsonPatchType))
                throw new InvalidOperationException($"Patch operation '{operation}' is not supported");

            var jsonPatchDefinition = (JsonPatchDefinition)Activator.CreateInstance(jsonPatchType);
            jsonPatchDefinition.FillFromJson(patchDefinitionJObject);
            return jsonPatchDefinition;
        }
    }

    internal static class JsonOperations
    {
        public static T GetMandatoryPropertyValue<T>(JObject patchDefinitionJObject, string propertyName)
        {
            if (!patchDefinitionJObject.ContainsKey(propertyName)) throw new InvalidOperationException($"Patch definition must contain '{propertyName}' property");

            return patchDefinitionJObject.Value<T>(propertyName);
        }
    }

}