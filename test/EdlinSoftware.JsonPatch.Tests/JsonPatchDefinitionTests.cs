using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace EdlinSoftware.JsonPatch.Tests
{
    public class JsonPatchDefinitionTests
    {
        public static IEnumerable<object[]> GetSerializationData()
        {
            yield return new object[]
            {
                new JsonPatchAddDefinition
                {
                    Path = "/var/boo",
                    Value = null
                },
                "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":null}"
            };
            yield return new object[]
            {
                new JsonPatchAddDefinition
                {
                    Path = "/var/boo",
                    Value = 3
                },
                "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":3}"
            };
            yield return new object[]
            {
                new JsonPatchAddDefinition
                {
                    Path = "/var/boo",
                    Value = new { skip = 30 }
                },
                "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}"
            };
            yield return new object[]
            {
                new JsonPatchAddDefinition
                {
                    Path = "/var/boo",
                    Value = JToken.Parse("{\"skip\":30}")
                },
                "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}"
            };
            yield return new object[]
            {
                new JsonPatchRemoveDefinition()
                {
                    Path = "/var/boo",
                },
                "{\"op\":\"remove\",\"path\":\"/var/boo\"}"
            };
            yield return new object[]
            {
                new JsonPatchReplaceDefinition
                {
                    Path = "/var/boo",
                    Value = null
                },
                "{\"op\":\"replace\",\"path\":\"/var/boo\",\"value\":null}"
            };
            yield return new object[]
            {
                new JsonPatchReplaceDefinition
                {
                    Path = "/var/boo",
                    Value = 3
                },
                "{\"op\":\"replace\",\"path\":\"/var/boo\",\"value\":3}"
            };
            yield return new object[]
            {
                new JsonPatchReplaceDefinition
                {
                    Path = "/var/boo",
                    Value = new { skip = 30 }
                },
                "{\"op\":\"replace\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}"
            };
            yield return new object[]
            {
                new JsonPatchReplaceDefinition
                {
                    Path = "/var/boo",
                    Value = JToken.Parse("{\"skip\":30}")
                },
                "{\"op\":\"replace\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}"
            };
            yield return new object[]
            {
                new JsonPatchMoveDefinition()
                {
                    From = "/foo/bar",
                    Path = "/var/boo"
                },
                "{\"op\":\"move\",\"path\":\"/var/boo\",\"from\":\"/foo/bar\"}"
            };
        }

        [Theory]
        [MemberData(nameof(GetSerializationData))]
        public void Serialize(JsonPatchDefinition patchDefinition, string expectedResult)
        {
            var actualResult = JsonConvert.SerializeObject(patchDefinition);

            JsonStringsAreEqual(expectedResult, actualResult);
        }

        public static IEnumerable<object[]> GetDeserializationDataForAdd()
        {
            yield return new object[]
            {
                "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":null}",
                "/var/boo",
                "null"
            };
            yield return new object[]
            {
                "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":3}",
                "/var/boo",
                "3"
            };
            yield return new object[]
            {
                "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}",
                "/var/boo",
                "{\"skip\":30}"
            };
        }

        [Theory]
        [MemberData(nameof(GetDeserializationDataForAdd))]
        public void Deserialize_AddPatch(string json, string expectedPath, string expectedJsonValue)
        {
            var jsonPatch = JsonConvert.DeserializeObject<JsonPatchDefinition>(json);

            var patch = jsonPatch.ShouldBeOfType<JsonPatchAddDefinition>();

            patch.Path.ShouldBe(expectedPath);
            ValueShouldBeEqualTo(patch.Value, expectedJsonValue);
        }

        [Fact]
        public void Deserialize_RemovePatch()
        {
            var json = "{\"op\":\"remove\",\"path\":\"/var/boo\"}";

            var jsonPatch = JsonConvert.DeserializeObject<JsonPatchDefinition>(json);

            var patch = jsonPatch.ShouldBeOfType<JsonPatchRemoveDefinition>();

            patch.Path.ShouldBe("/var/boo");
        }

        public static IEnumerable<object[]> GetDeserializationDataForReplace()
        {
            yield return new object[]
            {
                "{\"op\":\"replace\",\"path\":\"/var/boo\",\"value\":null}",
                "/var/boo",
                "null"
            };
            yield return new object[]
            {
                "{\"op\":\"replace\",\"path\":\"/var/boo\",\"value\":3}",
                "/var/boo",
                "3"
            };
            yield return new object[]
            {
                "{\"op\":\"replace\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}",
                "/var/boo",
                "{\"skip\":30}"
            };
        }

        [Theory]
        [MemberData(nameof(GetDeserializationDataForReplace))]
        public void Deserialize_ReplacePatch(string json, string expectedPath, string expectedJsonValue)
        {
            var jsonPatch = JsonConvert.DeserializeObject<JsonPatchDefinition>(json);

            var patch = jsonPatch.ShouldBeOfType<JsonPatchReplaceDefinition>();

            patch.Path.ShouldBe(expectedPath);
            ValueShouldBeEqualTo(patch.Value, expectedJsonValue);
        }

        [Fact]
        public void Deserialize_MovePatch()
        {
            var json = "{\"op\":\"move\",\"path\":\"/var/boo\",\"from\":\"/foo/bar\"}";

            var jsonPatch = JsonConvert.DeserializeObject<JsonPatchDefinition>(json);

            var patch = jsonPatch.ShouldBeOfType<JsonPatchMoveDefinition>();

            patch.Path.ShouldBe("/var/boo");
            patch.From.ShouldBe("/foo/bar");
        }

        private void JsonStringsAreEqual(string expectedResult, string actualResult)
        {
            actualResult = JToken.Parse(actualResult).ToString();
            expectedResult = JToken.Parse(expectedResult).ToString();

            actualResult.ShouldBe(expectedResult);
        }

        private void ValueShouldBeEqualTo(object value, string expectedJsonValue)
        {
            var valueToken = value.ShouldBeAssignableTo<JToken>();
            valueToken.ToString().ShouldBe(JToken.Parse(expectedJsonValue).ToString());
        }
    }
}