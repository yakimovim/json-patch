using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.JsonPatch.Utilities
{
    internal static class JsonUtilities
    {
        public static JToken GetJToken(this object value, JsonSerializer serializer)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));

            if(value == null)
                return JToken.Parse("null");

            if (value is JToken jToken)
                return jToken;

            return JToken.FromObject(value, serializer);
        }
    }
}