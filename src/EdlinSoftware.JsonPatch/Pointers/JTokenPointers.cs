using System;
using System.Diagnostics;
using EdlinSoftware.JsonPatch.Utilities;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch.Pointers
{
    internal abstract class JTokenPointer
    {
        public static Result<JTokenPointer> Get(JToken token, JsonPointer path)
        {
            if(path.IsRootPointer)
                return Result.Ok<JTokenPointer>(JRootPointer.Instance);

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
                                return Result.Fail<JTokenPointer>($"Unable to find index '{arrayIndex}' in an array at '{referenceToken.GetParentPointer()}'.");
                            token = jArray[arrayIndex];
                        }
                        else if (referenceToken == "-")
                        {
                            if (jArray.Count == 0)
                                return Result.Fail<JTokenPointer>($"Unable to find last element in an empty array at '{referenceToken.GetParentPointer()}'.");
                            token = jArray[jArray.Count - 1];
                        }
                        else
                        {
                            return Result.Fail<JTokenPointer>($"Unable to find '{referenceToken}' property of an array at '{referenceToken.GetParentPointer()}'.");
                        }
                        break;
                    default:
                        return Result.Fail<JTokenPointer>($"Value at '{referenceToken.GetParentPointer()}' is of primitive type '{token.Type}' and can't participate in a pointer.");
                }

                if (token == null)
                    return Result.Fail<JTokenPointer>($"Unable to find path '{referenceToken.GetPointer()}'.");
            }

            switch (token)
            {
                case JObject jObject:
                    return JObjectPointer.Get(jObject, path.LastReferenceToken).OnSuccess(p => (JTokenPointer) p);
                case JArray jArray:
                    return JArrayPointer.Get(jArray, path.LastReferenceToken).OnSuccess(p => (JTokenPointer) p);
                default:
                    return Result.Fail<JTokenPointer>($"Unable to build pointer for '{path.GetParentPointerForLastReferenceToken()}'.");
            }
        }
    }

    internal class JRootPointer : JTokenPointer
    {
        public static readonly JRootPointer Instance = new JRootPointer();

        [DebuggerStepThrough]
        private JRootPointer() { }
    }

    internal class JArrayPointer : JTokenPointer
    {
        private readonly JArray _jArray;
        private readonly string _pathPart;

        private JArrayPointer(JArray jArray, JsonPointer.ReferenceToken pathPart)
        {
            _jArray = jArray;
            _pathPart = pathPart;
        }

        public static Result<JArrayPointer> Get(JArray jArray, JsonPointer.ReferenceToken pathPart)
        {
            if (jArray == null) throw new ArgumentNullException(nameof(jArray));
            if (pathPart == null) throw new ArgumentNullException(nameof(pathPart));

            if (pathPart != "-")
            {
                if (!int.TryParse(pathPart, out var arrayIndex))
                    return Result.Fail<JArrayPointer>($"Unable to find '{pathPart}' property of an array at '{pathPart.GetParentPointer()}'.");

                if (arrayIndex < 0 || arrayIndex > jArray.Count)
                    return Result.Fail<JArrayPointer>($"Unable to find index '{arrayIndex}' in an array at '{pathPart.GetParentPointer()}'.");
            }

            return Result.Ok(new JArrayPointer(jArray, pathPart));
        }

        public Result<JToken> GetValue()
        {
            var arrayIndex = _pathPart == "-"
                ? _jArray.Count - 1
                : int.Parse(_pathPart);

            if (arrayIndex < 0 || arrayIndex >= _jArray.Count)
                return Result.Fail<JToken>($"Unable to get absent '{_pathPart}' element from an array.");

            return Result.Ok(_jArray[arrayIndex]);
        }

        public Result SetValue(JToken value)
        {
            if (_pathPart == "-")
            {
                _jArray.Add(value);
            }
            else
            {
                _jArray.Insert(int.Parse(_pathPart), value);
            }

            return Result.Ok();
        }

        public Result SetManyValues(JToken value)
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

                return Result.Ok();
            }

            return SetValue(value);
        }

        public Result RemoveValue()
        {
            var arrayIndex = _pathPart == "-"
                ? _jArray.Count - 1
                : int.Parse(_pathPart);

            if(arrayIndex < 0 || arrayIndex >= _jArray.Count)
                return Result.Fail($"Unable to remove absent '{_pathPart}' element from an array.");

            _jArray.RemoveAt(arrayIndex);

            return Result.Ok();
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

        private JObjectPointer(JObject jObject, JsonPointer.ReferenceToken pathPart)
        {
            _jObject = jObject;
            _pathPart = pathPart;
        }

        public static Result<JObjectPointer> Get(JObject jObject, JsonPointer.ReferenceToken pathPart)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));
            if (pathPart == null) throw new ArgumentNullException(nameof(pathPart));

            return Result.Ok(new JObjectPointer(jObject, pathPart));
        }

        public Result<JToken> GetValue()
        {
            if (!_jObject.ContainsKey(_pathPart))
                return Result.Fail<JToken>($"Unable to get absent value of '{_pathPart}' key from an object.");

            return Result.Ok(_jObject[_pathPart]);
        }

        public Result SetValue(JToken value)
        {
            _jObject[_pathPart] = value;
            return Result.Ok();
        }

        public Result RemoveValue()
        {
            if(!_jObject.ContainsKey(_pathPart))
                return Result.Fail($"Unable to remove absent '{_pathPart}' key from an object.");

            _jObject.Remove(_pathPart);

            return Result.Ok();
        }

        public void Deconstruct(out JObject jObject, out string pathPart)
        {
            jObject = _jObject;
            pathPart = _pathPart;
        }
    }
}