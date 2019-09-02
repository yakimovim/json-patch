using System;
using System.Diagnostics;
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
    public abstract class JsonPatchDefinition : IErrorHandlingTypeProvider
    {
        protected ErrorHandlingTypes? ErrorHandlingTypeStorage;

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
            if (ErrorHandlingTypeStorage.HasValue)
            {
                writer.WritePropertyName("onError");
                writer.WriteValue(ErrorHandlingTypeStorage.Value.ToString().ToLowerInvariant());
            }
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
            ErrorHandlingTypeStorage = GetErrorHandlingType(jObject);
            FillAdditionalPropertiesFromJson(jObject);
        }

        /// <summary>
        /// Returns value of error handling type from JSON object.
        /// </summary>
        /// <param name="jObject">JSON object.</param>
        protected virtual ErrorHandlingTypes? GetErrorHandlingType(JObject jObject)
        {
            if (jObject.ContainsKey("onError"))
            {
                var onErrorValue = jObject.Value<string>("onError");
                if (!Enum.TryParse(onErrorValue, true, out ErrorHandlingTypes errorHandlingType))
                {
                    var validErrorHandlingTypes = string.Join(
                        ", ",
                        Enum
                            .GetNames(typeof(ErrorHandlingTypes))
                            .Select(n => n.ToLowerInvariant())
                            .Select(n => $"'{n}'")
                            .ToArray()
                    );
                    throw new JsonPatchException(
                        $"Value '{onErrorValue}' is not a valid value for 'onError' property. Valid values are: {validErrorHandlingTypes}");
                }

                return errorHandlingType;
            }

            return null;
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

        /// <summary>
        /// Error handling type for this patch definition.
        /// </summary>
        ErrorHandlingTypes? IErrorHandlingTypeProvider.ErrorHandlingType => ErrorHandlingTypeStorage;
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

        /// <summary>
        /// Error handling type for this patch definition.
        /// </summary>
        public ErrorHandlingTypes? ErrorHandlingType
        {
            [DebuggerStepThrough]
            get => ErrorHandlingTypeStorage;
            [DebuggerStepThrough]
            set => ErrorHandlingTypeStorage = value;
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

        /// <summary>
        /// Error handling type for this patch definition.
        /// </summary>
        public ErrorHandlingTypes? ErrorHandlingType
        {
            [DebuggerStepThrough]
            get => ErrorHandlingTypeStorage;
            [DebuggerStepThrough]
            set => ErrorHandlingTypeStorage = value;
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

        /// <summary>
        /// Error handling type for this patch definition.
        /// </summary>
        public ErrorHandlingTypes? ErrorHandlingType
        {
            [DebuggerStepThrough]
            get => ErrorHandlingTypeStorage;
            [DebuggerStepThrough]
            set => ErrorHandlingTypeStorage = value;
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

        /// <summary>
        /// Error handling type for this patch definition.
        /// </summary>
        public ErrorHandlingTypes? ErrorHandlingType
        {
            [DebuggerStepThrough]
            get => ErrorHandlingTypeStorage;
            [DebuggerStepThrough]
            set => ErrorHandlingTypeStorage = value;
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

        /// <summary>
        /// Error handling type for this patch definition.
        /// </summary>
        public ErrorHandlingTypes? ErrorHandlingType
        {
            [DebuggerStepThrough]
            get => ErrorHandlingTypeStorage;
            [DebuggerStepThrough]
            set => ErrorHandlingTypeStorage = value;
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

        /// <summary>
        /// Error handling type for this patch definition.
        /// </summary>
        public ErrorHandlingTypes? ErrorHandlingType
        {
            [DebuggerStepThrough]
            get => ErrorHandlingTypeStorage;
            [DebuggerStepThrough]
            set => ErrorHandlingTypeStorage = value;
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
        protected override ErrorHandlingTypes? GetErrorHandlingType(JObject jObject)
        {
            return ErrorHandlingTypes.Throw;
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

    internal static class JsonOperations
    {
        public static T GetMandatoryPropertyValue<T>(JObject patchDefinitionJObject, string propertyName)
        {
            if (!patchDefinitionJObject.ContainsKey(propertyName)) throw new JsonPatchException($"Patch definition must contain '{propertyName}' property");

            return patchDefinitionJObject.Value<T>(propertyName);
        }
    }

}