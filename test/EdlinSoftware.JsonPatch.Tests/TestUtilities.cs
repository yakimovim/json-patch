using System.Collections.Generic;
using EdlinSoftware.JsonPatch.Pointers;
using Newtonsoft.Json.Linq;
using Shouldly;

namespace EdlinSoftware.JsonPatch.Tests
{
    public static class TestUtilities
    {
        public static void ShouldBeJson(this object obj, string expectedJson)
        {
            var actualToken = obj is JToken token ? token : JToken.FromObject(obj);

            obj.ShouldBeJson(JToken.Parse(expectedJson));
        }

        public static void ShouldBeJson(this object obj, object expectedObject)
        {
            var actualToken = obj is JToken token ? token : JToken.FromObject(obj);

            var expectedToken = expectedObject is JToken expToken ? expToken : JToken.FromObject(expectedObject);

            obj.ShouldBeJson(expectedToken);
        }

        public static void ShouldBeJson(this object obj, JToken expectedToken)
        {
            var actualToken = obj is JToken token ? token : JToken.FromObject(obj);

            JToken.DeepEquals(actualToken, expectedToken).ShouldBeTrue(() => $"\nExpected JSON:\n\n{expectedToken}\n\nActual JSON:\n\n{actualToken}\n");
        }

        public static void ShouldBe(this JsonPointer jsonPointer, string expectedValue)
        {
            jsonPointer.ToString().ShouldBe(expectedValue);
        }

        public static object Patch(object input, IReadOnlyList<JsonPatchOperation> patchOperations)
        {
            return input is JToken inputToken
                ? JsonPatcher.PatchTokenCopy(inputToken, patchOperations)
                : JsonPatcher.PatchObjectCopy(input, patchOperations);
        }
    }
}