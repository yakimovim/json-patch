using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EdlinSoftware.JsonPatch.Pointers
{
    /// <summary>
    /// Represents JSON pointer (https://tools.ietf.org/html/rfc6901).
    /// </summary>
    public class JsonPointer : IEnumerable<JsonPointer.ReferenceToken>
    {
        public sealed class ReferenceToken
        {
            private readonly JsonPointer _pointer;
            private readonly int _index;

            internal ReferenceToken(JsonPointer pointer, int index)
            {
                _pointer = pointer ?? throw new ArgumentNullException(nameof(pointer));
                _index = index;
            }

            public override string ToString() => _pointer._referenceTokens[_index];

            /// <summary>
            /// Returns JSON pointer ending on the previous reference token. 
            /// </summary>
            public JsonPointer GetParentPointer() => _pointer.GetParentPointer(_index);

            /// <summary>
            /// Returns JSON pointer ending on this reference token. 
            /// </summary>
            public JsonPointer GetPointer() => _pointer.GetParentPointer(_index + 1);

            public static implicit operator string(ReferenceToken referenceToken)
            {
                return referenceToken.ToString();
            }
        }

        private static readonly string[] RootPointer = new string[0];

        private readonly IReadOnlyList<string> _referenceTokens;

        public bool IsRootPointer => (_referenceTokens.Count == 0);

        public ReferenceToken LastReferenceToken
        {
            get
            {
                if(IsRootPointer)
                    throw new InvalidOperationException("Root pointer does not have last reference token.");

                return new ReferenceToken(this, _referenceTokens.Count - 1);
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

        public IEnumerator<ReferenceToken> GetEnumerator() => Enumerable
            .Range(0, _referenceTokens.Count)
            .Select(i => new ReferenceToken(this, i))
            .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<ReferenceToken> GetReferenceTokensExceptLast()
        {
            if(IsRootPointer)
                throw new InvalidOperationException("Root pointer does not have last reference token.");

            for (int i = 0; i < _referenceTokens.Count - 1; i++)
            {
                yield return new ReferenceToken(this, i);
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

        public bool IsPrefixOf(JsonPointer another)
        {
            if (another == null) throw new ArgumentNullException(nameof(another));

            // Root is prefix of anything.
            if (IsRootPointer) return true;

            if (_referenceTokens.Count >= another._referenceTokens.Count) return false;

            for (int i = 0; i < _referenceTokens.Count; i++)
            {
                if (_referenceTokens[i] != another._referenceTokens[i])
                    return false;
            }

            return true;
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