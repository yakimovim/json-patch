using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EdlinSoftware.JsonPatch.Pointers;
using EdlinSoftware.JsonPatch.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch
{
    using static JsonOperations;

    internal enum JsonPatchTypes
    {
        Add,
        AddMany,
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
        /// <param name="serializer">JSON serializer.</param>
        internal abstract Result Apply(ref JToken token, JsonSerializer serializer);
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
        internal override Result Apply(ref JToken token, JsonSerializer serializer)
        {
            var pointer = JTokenPointer.Get(token, Path);
            if (pointer.IsFailure) return Result.Fail(pointer.Error);

            var jValue = Value.GetJToken(serializer);

            switch (pointer.Value)
            {
                case JRootPointer _:
                    {
                        token = jValue;
                        return Result.Ok();
                    }
                case JObjectPointer jObjectPointer:
                    {
                        return jObjectPointer.SetValue(jValue);
                    }
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.SetValue(jValue);
                    }
                default:
                    throw new JsonPatchException(JsonPatchMessages.UnknownPathPointer);
            }
        }
    }

    [PatchType(JsonPatchTypes.AddMany)]
    public sealed class JsonPatchAddManyDefinition : JsonPatchDefinition
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
        internal override Result Apply(ref JToken token, JsonSerializer serializer)
        {
            var pointer = JTokenPointer.Get(token, Path);
            if (pointer.IsFailure) return Result.Fail(pointer.Error);

            switch (pointer.Value)
            {
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.SetManyValues(Value.GetJToken(serializer));
                    }
                default:
                    return Result.Fail("'addmany' patch should work only with arrays.");
            }
        }
    }

    [PatchType(JsonPatchTypes.Remove)]
    public sealed class JsonPatchRemoveDefinition : JsonPatchDefinition
    {
        /// <inheritdoc />
        internal override Result Apply(ref JToken token, JsonSerializer serializer)
        {
            var pointer = JTokenPointer.Get(token, Path);
            if (pointer.IsFailure) return Result.Fail(pointer.Error);

            switch (pointer.Value)
            {
                case JRootPointer _:
                    {
                        token = null;
                        return Result.Ok();
                    }
                case JObjectPointer jObjectPointer:
                    {
                        return jObjectPointer.RemoveValue();
                    }
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.RemoveValue();
                    }
                default:
                    throw new JsonPatchException(JsonPatchMessages.UnknownPathPointer);
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
        internal override Result Apply(ref JToken token, JsonSerializer serializer)
        {
            var pointer = JTokenPointer.Get(token, Path);
            if (pointer.IsFailure) return Result.Fail(pointer.Error);

            var jValue = Value.GetJToken(serializer);

            switch (pointer.Value)
            {
                case JRootPointer _:
                    {
                        token = jValue;
                        return Result.Ok();
                    }
                case JObjectPointer jObjectPointer:
                    {
                        return jObjectPointer.RemoveValue()
                            .OnSuccess(() => jObjectPointer.SetValue(jValue));
                    }
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.RemoveValue()
                            .OnSuccess(() => jArrayPointer.SetValue(jValue));
                    }
                default:
                    throw new JsonPatchException(JsonPatchMessages.UnknownPathPointer);
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
        internal override Result Apply(ref JToken token, JsonSerializer serializer)
        {
            if (From.IsPrefixOf(Path))
                return Result.Fail("Unable to move parent JSON to a child.");

            var fromPointer = JTokenPointer.Get(token, From);
            if (fromPointer.IsFailure) return Result.Fail(fromPointer.Error);
            var toPointer = JTokenPointer.Get(token, Path);
            if (toPointer.IsFailure) return Result.Fail(toPointer.Error);

            var tokenToMove = RemoveTokenFromSourceAndReturnIt(fromPointer.Value);
            if (tokenToMove.IsFailure) return Result.Fail(tokenToMove.Error);

            switch (toPointer.Value)
            {
                case JRootPointer _:
                    {
                        token = tokenToMove.Value;
                        return Result.Ok();
                    }
                case JObjectPointer jObjectPointer:
                    {
                        return jObjectPointer.SetValue(tokenToMove.Value);
                    }
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.SetValue(tokenToMove.Value);
                    }
                default:
                    throw new JsonPatchException(JsonPatchMessages.UnknownPathPointer);
            }
        }

        private Result<JToken> RemoveTokenFromSourceAndReturnIt(JTokenPointer fromPointer)
        {
            switch (fromPointer)
            {
                case JObjectPointer jObjectPointer:
                    {
                        var token = jObjectPointer.GetValue();

                        return token
                            .OnSuccess(() => jObjectPointer.RemoveValue())
                            .OnSuccess(() => token.Value);
                    }
                case JArrayPointer jArrayPointer:
                    {
                        var token = jArrayPointer.GetValue();

                        return token
                            .OnSuccess(() => jArrayPointer.RemoveValue())
                            .OnSuccess(() => token.Value);
                    }
                default:
                    return Result.Fail<JToken>("Can't move entire JSON");
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
        internal override Result Apply(ref JToken token, JsonSerializer serializer)
        {
            var fromPointer = JTokenPointer.Get(token, From);
            if (fromPointer.IsFailure) return Result.Fail(fromPointer.Error);
            var toPointer = JTokenPointer.Get(token, Path);
            if (toPointer.IsFailure) return Result.Fail(toPointer.Error);

            var tokenToCopy = GetSourceTokenCopy(token, fromPointer.Value);
            if (tokenToCopy.IsFailure) return Result.Fail(tokenToCopy.Error);

            switch (toPointer.Value)
            {
                case JRootPointer _:
                    {
                        token = tokenToCopy.Value;
                        return Result.Ok();
                    }
                case JObjectPointer jObjectPointer:
                    {
                        return jObjectPointer.SetValue(tokenToCopy.Value);
                    }
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.SetValue(tokenToCopy.Value);
                    }
                default:
                    throw new JsonPatchException(JsonPatchMessages.UnknownPathPointer);
            }
        }

        private Result<JToken> GetSourceTokenCopy(JToken rootToken, JTokenPointer fromPointer)
        {
            switch (fromPointer)
            {
                case JRootPointer _:
                    {
                        return Result.Ok(rootToken.DeepClone());
                    }
                case JObjectPointer jObjectPointer:
                    {
                        return jObjectPointer.GetValue()
                            .OnSuccess(t => t.DeepClone());
                    }
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.GetValue()
                            .OnSuccess(t => t.DeepClone());
                    }
                default:
                    throw new JsonPatchException(JsonPatchMessages.UnknownPathPointer);
            }
        }
    }

    [PatchType(JsonPatchTypes.Test)]
    public sealed class JsonPatchTestDefinition : JsonPatchDefinition
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
        internal override Result Apply(ref JToken token, JsonSerializer serializer)
        {
            var pointer = JTokenPointer.Get(token, Path);
            if (pointer.IsFailure) return Result.Fail(pointer.Error);

            var expectedToken = Value.GetJToken(serializer);

            switch (pointer.Value)
            {
                case JRootPointer _:
                    {
                        if (!JToken.DeepEquals(token, expectedToken))
                            return Result.Fail("JSON patch test failed.");
                        return Result.Ok();
                    }
                case JObjectPointer jObjectPointer:
                    {
                        return jObjectPointer.GetValue()
                            .OnSuccess(actualToken =>
                            {
                                if (!JToken.DeepEquals(actualToken, expectedToken))
                                    return Result.Fail("JSON patch test failed.");
                                return Result.Ok();
                            });
                    }
                case JArrayPointer jArrayPointer:
                    {
                        return jArrayPointer.GetValue()
                            .OnSuccess(actualToken =>
                            {
                                if (!JToken.DeepEquals(actualToken, expectedToken))
                                    return Result.Fail("JSON patch test failed.");
                                return Result.Ok();
                            });
                    }
                default:
                    throw new JsonPatchException(JsonPatchMessages.UnknownPathPointer);
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
            if (value == null) throw new ArgumentNullException(nameof(value));

            value.WriteToJson(writer, serializer);
        }

        public override JsonPatchDefinition ReadJson(JsonReader reader, Type objectType, JsonPatchDefinition existingValue,
            bool hasExistingValue, JsonSerializer serializer)
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

    internal static class JsonOperations
    {
        public static T GetMandatoryPropertyValue<T>(JObject patchDefinitionJObject, string propertyName)
        {
            if (!patchDefinitionJObject.ContainsKey(propertyName)) throw new JsonPatchException($"Patch definition must contain '{propertyName}' property");

            return patchDefinitionJObject.Value<T>(propertyName);
        }
    }

}