using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EdlinSoftware.JsonPatch.Pointers
{
    /// <summary>
    /// Represents JSON pointer (https://tools.ietf.org/html/rfc6901).
    /// </summary>
    public class JsonPointer : IEnumerable<string>
    {
        private static readonly string[] RootPointer = new string[0];

        private readonly IReadOnlyList<string> _referenceTokens;

        public bool IsRootPointer => (_referenceTokens.Count == 0);

        public string LastReferenceToken
        {
            get
            {
                if(IsRootPointer)
                    throw new InvalidOperationException("Root pointer does not have last reference token.");

                return _referenceTokens[_referenceTokens.Count - 1];
            }
        }

        public JsonPointer(string jsonPointer)
        {
            if (jsonPointer == null) throw new ArgumentNullException(nameof(jsonPointer));

            _referenceTokens = jsonPointer == string.Empty
                ? RootPointer
                : jsonPointer
                    .Split('/')
                    .Select(UnEscape)
                    .Skip(1) // skip empty string before first '/'
                    .Select(NoEmptyParts)
                    .ToArray();
        }

        private JsonPointer(IReadOnlyList<string> referenceTokens)
        {
            _referenceTokens = (referenceTokens ?? RootPointer).ToArray();
        }

        private static string NoEmptyParts(string s)
        {
            if(s == string.Empty)
                throw new InvalidOperationException("Json pointer can't contain empty reference tokens");

            return s;
        }

        private static string UnEscape(string s)
        {
            return s
                .Replace("~1", "/")
                .Replace("~0", "~");
        }

        public IEnumerator<string> GetEnumerator() => _referenceTokens.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<(string referenceToken, int index)> GetReferenceTokensExceptLast()
        {
            if(IsRootPointer)
                throw new InvalidOperationException("Root pointer does not have last reference token.");

            for (int i = 0; i < _referenceTokens.Count - 1; i++)
            {
                var index = i;

                yield return (_referenceTokens[index], index);
            }
        }

        public override string ToString()
        {
            return IsRootPointer 
                ? string.Empty 
                : "/" + string.Join("/", _referenceTokens);
        }

        public JsonPointer GetParentPointer(int numberOfReferenceTokensInPointer)
        {
            if(numberOfReferenceTokensInPointer < 0 || numberOfReferenceTokensInPointer > _referenceTokens.Count)
                throw new InvalidOperationException($"There is no parent JSON pointer containing {numberOfReferenceTokensInPointer} reference tokens.");

            return new JsonPointer(_referenceTokens.Take(numberOfReferenceTokensInPointer).ToArray());
        }

        public JsonPointer GetParentPointerForLastReferenceToken()
        {
            if (IsRootPointer)
                throw new InvalidOperationException("Root pointer does not have last reference token.");

            return GetParentPointer(_referenceTokens.Count);
        }


        public static implicit operator JsonPointer(string jsonPointer)
        {
            return new JsonPointer(jsonPointer);
        }

        public static implicit operator string(JsonPointer jsonPointer)
        {
            return jsonPointer.ToString();
        }
    }
}