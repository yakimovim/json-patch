using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EdlinSoftware.JsonPatch.Pointers;
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

            var patcher = new JsonTokenPatchImplementation();

            foreach (var jsonPatchDefinition in patchDefinitions)
            {
                copy = patcher.ApplyPatch(copy, jsonPatchDefinition);
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

    internal class JsonTokenPatchImplementation : IJsonPatchDefinitionVisitor
    {
        private JToken _objectToPatch;

        public JToken ApplyPatch(JToken token, JsonPatchDefinition patchDefinition)
        {
            if (patchDefinition == null) throw new ArgumentNullException(nameof(patchDefinition));

            _objectToPatch = token ?? throw new ArgumentNullException(nameof(token));
            patchDefinition.Visit(this);
            return _objectToPatch;
        }

        public void VisitAdd(JsonPointer path, object value)
        {
            var pointer = JTokenPointer.Get(_objectToPatch, path);

            switch (pointer)
            {
                case JRootPointer jRootPointer:
                {
                    _objectToPatch = value.GetJToken();
                    break;
                }
                case JObjectPointer jObjectPointer:
                {
                    var (jObject, pathPart) = jObjectPointer;

                    jObject[pathPart] = value.GetJToken();

                    break;
                }
                case JArrayPointer jArrayPointer:
                {
                    var (jArray, pathPart) = jArrayPointer;

                    if (pathPart == "-")
                    {
                        jArray.Add(value.GetJToken());
                    }
                    else if (int.TryParse(pathPart, out var arrayIndex))
                    {
                        jArray.Insert(arrayIndex, value.GetJToken());
                    }
                    else
                    {
                        
                    }

                    break;
                }
                default:
                    throw new InvalidOperationException("Unknown type of path pointer.");
            }
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