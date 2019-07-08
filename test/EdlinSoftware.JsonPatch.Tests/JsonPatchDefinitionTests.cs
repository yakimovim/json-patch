using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace EdlinSoftware.JsonPatch.Tests
{
    public class JsonPatchDefinitionTests
    {
        [Fact]
        public void Serialize_AddPatch_NullValue()
        {
            var expectedResult = "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":null}";

            var actualResult = JsonConvert.SerializeObject(new JsonPatchAddDefinition
            {
                Path = "/var/boo",
                Value = null
            });

            JsonStringsAreEqual(expectedResult, actualResult);
        }

        [Fact]
        public void Serialize_AddPatch_SimpleValue()
        {
            var expectedResult = "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":3}";

            var actualResult = JsonConvert.SerializeObject(new JsonPatchAddDefinition
            {
                Path = "/var/boo",
                Value = 3
            });

            JsonStringsAreEqual(expectedResult, actualResult);
        }

        [Fact]
        public void Serialize_AddPatch_ComplexValue()
        {
            var expectedResult = "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}";

            var actualResult = JsonConvert.SerializeObject(new JsonPatchAddDefinition
            {
                Path = "/var/boo",
                Value = new { skip = 30 }
            });

            JsonStringsAreEqual(expectedResult, actualResult);
        }

        [Fact]
        public void Serialize_AddPatch_JTokenValue()
        {
            var expectedResult = "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}";

            var actualResult = JsonConvert.SerializeObject(new JsonPatchAddDefinition
            {
                Path = "/var/boo",
                Value = JToken.Parse("{\"skip\":30}")
            });

            JsonStringsAreEqual(expectedResult, actualResult);
        }

        [Fact]
        public void Deserialize_AddPatch_NullValue()
        {
            var json = "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":null}";

            var jsonPatch = JsonConvert.DeserializeObject<JsonPatchDefinition>(json);

            var addPatch = jsonPatch.ShouldBeOfType<JsonPatchAddDefinition>();

            addPatch.Path.ShouldBe("/var/boo");
            ValueShouldBeEqualTo(addPatch.Value, "null");
        }

        [Fact]
        public void Deserialize_AddPatch_SimpleValue()
        {
            var json = "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":3}";

            var jsonPatch = JsonConvert.DeserializeObject<JsonPatchDefinition>(json);

            var addPatch = jsonPatch.ShouldBeOfType<JsonPatchAddDefinition>();

            addPatch.Path.ShouldBe("/var/boo");
            ValueShouldBeEqualTo(addPatch.Value, "3");
        }

        [Fact]
        public void Deserialize_AddPatch_ComplexValue()
        {
            var json = "{\"op\":\"add\",\"path\":\"/var/boo\",\"value\":{\"skip\":30}}";

            var jsonPatch = JsonConvert.DeserializeObject<JsonPatchDefinition>(json);

            var addPatch = jsonPatch.ShouldBeOfType<JsonPatchAddDefinition>();

            addPatch.Path.ShouldBe("/var/boo");
            ValueShouldBeEqualTo(addPatch.Value, "{\"skip\":30}");
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