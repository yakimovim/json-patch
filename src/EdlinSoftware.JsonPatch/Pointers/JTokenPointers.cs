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

            foreach (JsonPointer.ReferenceToken referenceToken in path.GetReferenceTokensExceptLast())
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
                                throw new InvalidOperationException($"Unable to find index '{arrayIndex}' in an array at '{referenceToken.GetParentPointer()}'.");
                            token = jArray[arrayIndex];
                        }
                        else if (referenceToken == "-")
                        {
                            if (jArray.Count == 0)
                                throw new InvalidOperationException($"Unable to find last element in an empty array at '{referenceToken.GetParentPointer()}'.");
                            token = jArray[jArray.Count - 1];
                        }
                        else
                        {
                            throw new InvalidOperationException($"Unable to find '{referenceToken}' property of an array at '{referenceToken.GetParentPointer()}'.");
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Value at '{referenceToken.GetParentPointer()}' is of primitive type '{token.Type}' and can't participate in a pointer.");
                }

                if (token == null)
                    throw new InvalidOperationException($"Unable to find path '{referenceToken.GetPointer()}'.");
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

        public JArrayPointer(JArray jArray, JsonPointer.ReferenceToken pathPart)
        {
            _jArray = jArray ?? throw new ArgumentNullException(nameof(jArray));
            _pathPart = pathPart ?? throw new ArgumentNullException(nameof(pathPart));

            if (_pathPart != "-")
            {
                if(!int.TryParse(_pathPart, out var arrayIndex))
                    throw new InvalidOperationException($"Unable to find '{_pathPart}' property of an array at '{pathPart.GetParentPointer()}'.");

                if (arrayIndex < 0 || arrayIndex > _jArray.Count)
                    throw new InvalidOperationException($"Unable to find index '{arrayIndex}' in an array at '{pathPart.GetParentPointer()}'.");
            }
        }

        public JToken GetValue()
        {
            var arrayIndex = _pathPart == "-"
                ? _jArray.Count - 1
                : int.Parse(_pathPart);

            if (arrayIndex < 0 || arrayIndex >= _jArray.Count)
                throw new InvalidOperationException($"Unable to get absent '{_pathPart}' element from an array.");

            return _jArray[arrayIndex];
        }

        public void SetValue(JToken value)
        {
            if (_pathPart == "-")
            {
                _jArray.Add(value);
            }
            else
            {
                _jArray.Insert(int.Parse(_pathPart), value);
            }
        }

        public void SetManyValues(JToken value)
        {
            if (value is JArray valueArray)
            {
                var arrayIndex = _pathPart == "-"
                    ? _jArray.Count
                    : int.Parse(_pathPart);

                foreach (var valueArrayItem in valueArray)
                {
                    _jArray.Insert(arrayIndex++, valueArrayItem);
                }
            }
            else
            {
                SetValue(value);
            }
        }

        public void RemoveValue()
        {
            var arrayIndex = _pathPart == "-"
                ? _jArray.Count - 1
                : int.Parse(_pathPart);

            if(arrayIndex < 0 || arrayIndex >= _jArray.Count)
                throw new InvalidOperationException($"Unable to remove absent '{_pathPart}' element from an array.");

            _jArray.RemoveAt(arrayIndex);
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

        public JObjectPointer(JObject jObject, JsonPointer.ReferenceToken pathPart)
        {
            _jObject = jObject ?? throw new ArgumentNullException(nameof(jObject));
            _pathPart = pathPart ?? throw new ArgumentNullException(nameof(pathPart));
        }

        public JToken GetValue()
        {
            if (!_jObject.ContainsKey(_pathPart))
                throw new InvalidOperationException($"Unable to get absent value of '{_pathPart}' key from an object.");

            return _jObject[_pathPart];
        }

        public void SetValue(JToken value)
        {
            _jObject[_pathPart] = value;
        }

        public void RemoveValue()
        {
            if(!_jObject.ContainsKey(_pathPart))
                throw new InvalidOperationException($"Unable to remove absent '{_pathPart}' key from an object.");

            _jObject.Remove(_pathPart);
        }

        public void Deconstruct(out JObject jObject, out string pathPart)
        {
            jObject = _jObject;
            pathPart = _pathPart;
        }
    }
}