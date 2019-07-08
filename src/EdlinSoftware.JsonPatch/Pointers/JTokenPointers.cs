using System;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch.Pointers
{
    internal abstract class JTokenPointer
    {
        public static JTokenPointer Get(JToken token, JsonPointer path)
        {
            if(path.IsRootPointer)
                return new JRootPointer();

            foreach ((string referenceToken, int index) in path.GetReferenceTokensExceptLast())
            {
                switch (token)
                {
                    case JObject jObject:
                        token = jObject.GetValue(referenceToken);
                        break;
                    case JArray jArray:
                        if (int.TryParse(referenceToken, out var arrayIndex))
                        {
                            if (arrayIndex < 0 || arrayIndex >= jArray.Count)
                                throw new InvalidOperationException($"Unable to find index '{arrayIndex}' in an array at '{path.GetParentPointer(index)}'.");
                            token = jArray[arrayIndex];
                        }
                        else if (referenceToken == "-")
                        {
                            if (jArray.Count == 0)
                                throw new InvalidOperationException($"Unable to find last element in an empty array at '{path.GetParentPointer(index)}'.");
                            token = jArray[jArray.Count - 1];
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unable to find '{referenceToken}' property of an array at '{path.GetParentPointer(index)}'.");
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Value at '{path.GetParentPointer(index)}' is of primitive type '{token.Type}' and can't participate in a pointer.");
                }

                if (token == null)
                    throw new InvalidOperationException($"Unable to find path '{path.GetParentPointer(index + 1)}'.");
            }

            switch (token)
            {
                case JObject jObject:
                    return new JObjectPointer(jObject, path.LastReferenceToken);
                case JArray jArray:
                    return new JArrayPointer(jArray, path.LastReferenceToken);
                default:
                    throw new InvalidOperationException($"Unable to build pointer for '{path.GetParentPointerForLastReferenceToken()}'.");
            }
        }
    }

    internal class JRootPointer : JTokenPointer { }

    internal class JArrayPointer : JTokenPointer
    {
        private readonly JArray _jArray;
        private readonly string _pathPart;

        public JArrayPointer(JArray jArray, string pathPart)
        {
            _jArray = jArray ?? throw new ArgumentNullException(nameof(jArray));
            _pathPart = pathPart ?? throw new ArgumentNullException(nameof(pathPart));
        }

        public void Deconstruct(out JArray jArray, out string pathPart)
        {
            jArray = _jArray;
            pathPart = _pathPart;
        }
    }

    internal class JObjectPointer : JTokenPointer
    {
        private readonly JObject _jObject;
        private readonly string _pathPart;

        public JObjectPointer(JObject jObject, string pathPart)
        {
            _jObject = jObject ?? throw new ArgumentNullException(nameof(jObject));
            _pathPart = pathPart ?? throw new ArgumentNullException(nameof(pathPart));
        }

        public void Deconstruct(out JObject jObject, out string pathPart)
        {
            jObject = _jObject;
            pathPart = _pathPart;
        }
    }
}