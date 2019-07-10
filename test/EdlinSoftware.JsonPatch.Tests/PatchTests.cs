using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace EdlinSoftware.JsonPatch.Tests
{
    using static TestUtilities;

    public class PatchTests
    {
        public static IEnumerable<object[]> GetAddPatchData()
        {
            yield return new object[] { JToken.Parse("{}"), "/var", JToken.Parse("3"), "{ \"var\": 3 }" };
            yield return new object[] { JToken.Parse("{}"), "/var", 3, "{ \"var\": 3 }" };
            yield return new object[] { new Dictionary<string, object>(), "/var", JToken.Parse("3"), "{ \"var\": 3 }" };
            yield return new object[] { new Dictionary<string, object>(), "/var", 3, "{ \"var\": 3 }" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", JToken.Parse("3"), "{ \"var\": 3 }" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", 3, "{ \"var\": 3 }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", JToken.Parse("3"), "{ \"var\": 3 }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", 3, "{ \"var\": 3 }" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", JToken.Parse("7"), "[1, 7, 2, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", 7, "[1, 7, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", JToken.Parse("7"), "[1, 7, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", 7, "[1, 7, 2, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", JToken.Parse("7"), "[1, 2, 3, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", 7, "[1, 2, 3, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", JToken.Parse("7"), "[1, 2, 3, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", 7, "[1, 2, 3, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/3", JToken.Parse("7"), "[1, 2, 3, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/3", 7, "[1, 2, 3, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/3", JToken.Parse("7"), "[1, 2, 3, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/3", 7, "[1, 2, 3, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "", JToken.Parse("{\"skip\":7}"), "{\"skip\":7}" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "", new { skip = 7 }, "{\"skip\":7}" };
            yield return new object[] { new[] { 1, 2, 3 }, "", JToken.Parse("{\"skip\":7}"), "{\"skip\":7}" };
            yield return new object[] { new[] { 1, 2, 3 }, "", new { skip = 7 }, "{\"skip\":7}" };
            yield return new object[] { new Person { Name = "Andrey" }, "/Name", "Ivan", "{\"Name\":\"Ivan\",\"Age\":0}" };
        }

        [Theory]
        [MemberData(nameof(GetAddPatchData))]
        public void Add(object input, string path, object value, string expectedJson)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchAddDefinition
                {
                    Path = path,
                    Value = value
                }
            };

            var output = Patch(input, patchDefinitions);

            output.ShouldBeJson(expectedJson);
        }
    }
}