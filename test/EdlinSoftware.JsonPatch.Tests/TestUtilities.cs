﻿using System.Collections.Generic;
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

            var actualJson = actualToken.ToString();
            expectedJson = JToken.Parse(expectedJson).ToString();

            JToken.DeepEquals(actualJson, expectedJson).ShouldBeTrue(() => $"'{expectedJson}' expected but '{actualJson}' obtained");
        }

        public static void ShouldBe(this JsonPointer jsonPointer, string expectedValue)
        {
            jsonPointer.ToString().ShouldBe(expectedValue);
        }

        public static object Patch(object input, IReadOnlyList<JsonPatchDefinition> patchDefinitions)
        {
            return input is JToken inputToken
                ? JsonPatcher.PatchTokenCopy(inputToken, patchDefinitions)
                : JsonPatcher.PatchObjectCopy(input, patchDefinitions);
        }
    }
}