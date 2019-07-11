using System.Collections.Generic;
using Newtonsoft.Json.Linq;
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

        public static IEnumerable<object[]> GetRemovePatchData()
        {
            yield return new object[] { JToken.Parse("{ \"var\": 3 }"), "/var", "{}" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", "{}" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/1", "[1,3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", "[1,3]" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/-", "[1,2]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", "[1,2]" };
            yield return new object[] { new Person { Name = "Ivan" }, "/Name", "{\"Age\":0}" };
        }

        [Theory]
        [MemberData(nameof(GetRemovePatchData))]
        public void Remove(object input, string path, string expectedJson)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchRemoveDefinition
                {
                    Path = path,
                }
            };

            var output = Patch(input, patchDefinitions);

            output.ShouldBeJson(expectedJson);
        }

        public static IEnumerable<object[]> GetReplacePatchData()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", JToken.Parse("3"), "{ \"var\": 3 }" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", 3, "{ \"var\": 3 }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", JToken.Parse("3"), "{ \"var\": 3 }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", 3, "{ \"var\": 3 }" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", JToken.Parse("7"), "[1, 7, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", 7, "[1, 7, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", JToken.Parse("7"), "[1, 7, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", 7, "[1, 7, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", JToken.Parse("7"), "[1, 2, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", 7, "[1, 2, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", JToken.Parse("7"), "[1, 2, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", 7, "[1, 2, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "", JToken.Parse("{\"skip\":7}"), "{\"skip\":7}" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "", new { skip = 7 }, "{\"skip\":7}" };
            yield return new object[] { new[] { 1, 2, 3 }, "", JToken.Parse("{\"skip\":7}"), "{\"skip\":7}" };
            yield return new object[] { new[] { 1, 2, 3 }, "", new { skip = 7 }, "{\"skip\":7}" };
            yield return new object[] { new Person { Name = "Andrey" }, "/Name", "Ivan", "{\"Age\":0,\"Name\":\"Ivan\"}" };
        }

        [Theory]
        [MemberData(nameof(GetReplacePatchData))]
        public void Replace(object input, string path, object value, string expectedJson)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchReplaceDefinition
                {
                    Path = path,
                    Value = value
                }
            };

            var output = Patch(input, patchDefinitions);

            output.ShouldBeJson(expectedJson);
        }

        public static IEnumerable<object[]> GetMovePatchData()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", "/boo", "{ \"boo\": 5 }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", "/boo", "{ \"boo\": 5 }" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", "/-", "[1, 3, 2]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/0", "/1", "[2, 1, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", "/-", "[1, 3, 2]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/0", "/1", "[2, 1, 3]" };
            yield return new object[] { new { Name = "Andrey" }, "/Name", "/FirstName", "{\"FirstName\":\"Andrey\"}" };
        }

        [Theory]
        [MemberData(nameof(GetMovePatchData))]
        public void Move(object input, string from, string path, string expectedJson)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchMoveDefinition
                {
                    Path = path,
                    From = from
                }
            };

            var output = Patch(input, patchDefinitions);

            output.ShouldBeJson(expectedJson);
        }

    }
}