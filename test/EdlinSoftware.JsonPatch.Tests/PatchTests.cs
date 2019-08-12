using System.Collections.Generic;
using EdlinSoftware.JsonPatch.Utilities;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace EdlinSoftware.JsonPatch.Tests
{
    using static TestUtilities;

    public class PatchTests
    {
        public static IEnumerable<object[]> GetAddPatchData_Success()
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
        [MemberData(nameof(GetAddPatchData_Success))]
        public void Add_Success(object input, string path, object value, string expectedJson)
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

        public static IEnumerable<object[]> GetAddPatchData_Failure_Throw()
        {
            yield return new object[] { JToken.Parse("{}"), "/foo/bar", 3, "foo" };
            yield return new object[] { JToken.Parse("[]"), "/7", 3, "7" };
        }

        [Theory]
        [MemberData(nameof(GetAddPatchData_Failure_Throw))]
        public void Add_Failure_Throw(object input, string path, object value, params string[] expectedMessageParts)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchAddDefinition
                {
                    Path = path,
                    Value = value
                }
            };

            var exception = Assert.Throws<JsonPatchException>(() => { Patch(input, patchDefinitions); });

            foreach (var expectedMessagePart in expectedMessageParts)
            {
                exception.Message.ShouldContain(expectedMessagePart);
            }
        }

        public static IEnumerable<object[]> GetAddPatchData_Failure_Skip()
        {
            yield return new object[] { JToken.Parse("{}"), "/foo/bar", 3 };
            yield return new object[] { JToken.Parse("[]"), "/7", 3 };
        }

        [Theory]
        [MemberData(nameof(GetAddPatchData_Failure_Skip))]
        public void Add_Failure_Skip(object input, string path, object value)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchAddDefinition
                {
                    Path = path,
                    Value = value,
                    ErrorHandlingType = ErrorHandlingTypes.Skip
                }
            };

            var output = Patch(input, patchDefinitions);

            input.ShouldBeJson(output);
        }

        public static IEnumerable<object[]> GetAddManyPatchData_Success()
        {
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", JToken.Parse("7"), "[1, 2, 3, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", 7, "[1, 2, 3, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", JToken.Parse("7"), "[1, 2, 3, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", 7, "[1, 2, 3, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/3", JToken.Parse("7"), "[1, 2, 3, 7]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/3", 7, "[1, 2, 3, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/3", JToken.Parse("7"), "[1, 2, 3, 7]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/3", 7, "[1, 2, 3, 7]" };

            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", JToken.Parse("[4, 5]"), "[1, 2, 3, 4, 5]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", new [] { 4, 5 }, "[1, 2, 3, 4, 5]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", JToken.Parse("[4, 5]"), "[1, 2, 3, 4, 5]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", new[] { 4, 5 }, "[1, 2, 3, 4, 5]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", JToken.Parse("[4, 5]"), "[1, 4, 5, 2, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", new[] { 4, 5 }, "[1, 4, 5, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", JToken.Parse("[4, 5]"), "[1, 4, 5, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", new[] { 4, 5 }, "[1, 4, 5, 2, 3]" };
        }

        [Theory]
        [MemberData(nameof(GetAddManyPatchData_Success))]
        public void AddMany_Success(object input, string path, object value, string expectedJson)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchAddManyDefinition
                {
                    Path = path,
                    Value = value
                }
            };

            var output = Patch(input, patchDefinitions);

            output.ShouldBeJson(expectedJson);
        }

        public static IEnumerable<object[]> GetAddManyPatchData_Failure_Throw()
        {
            yield return new object[] { JToken.Parse("{}"), "/var", 7, "only with arrays" };
            yield return new object[] { JToken.Parse("{}"), "/-", 7, "only with arrays" };
            yield return new object[] { JToken.Parse("[]"), "/var", 7, "var" };
            yield return new object[] { JToken.Parse("[]"), "/10", 7, "10" };
        }

        [Theory]
        [MemberData(nameof(GetAddManyPatchData_Failure_Throw))]
        public void AddMany_Failure_Throw(object input, string path, object value, params string[] expectedMessageParts)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchAddManyDefinition
                {
                    Path = path,
                    Value = value
                }
            };

            var exception = Assert.Throws<JsonPatchException>(() => { Patch(input, patchDefinitions); });

            foreach (var expectedMessagePart in expectedMessageParts)
            {
                exception.Message.ShouldContain(expectedMessagePart);
            }
        }

        public static IEnumerable<object[]> GetAddManyPatchData_Failure_Skip()
        {
            yield return new object[] { JToken.Parse("{}"), "/var", 7 };
            yield return new object[] { JToken.Parse("{}"), "/-", 7 };
            yield return new object[] { JToken.Parse("[]"), "/var", 7 };
            yield return new object[] { JToken.Parse("[]"), "/10", 7 };
        }

        [Theory]
        [MemberData(nameof(GetAddManyPatchData_Failure_Skip))]
        public void AddMany_Failure_Skip(object input, string path, object value)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchAddManyDefinition
                {
                    Path = path,
                    Value = value,
                    ErrorHandlingType = ErrorHandlingTypes.Skip
                }
            };

            var output = Patch(input, patchDefinitions);

            input.ShouldBeJson(output);
        }

        public static IEnumerable<object[]> GetRemovePatchData_Success()
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
        [MemberData(nameof(GetRemovePatchData_Success))]
        public void Remove_Success(object input, string path, string expectedJson)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchRemoveDefinition
                {
                    Path = path
                }
            };

            var output = Patch(input, patchDefinitions);

            output.ShouldBeJson(expectedJson);
        }

        public static IEnumerable<object[]> GetRemovePatchData_Failure_Throw()
        {
            yield return new object[] { JToken.Parse("{ \"var\": 3 }"), "/foo", "foo" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/foo", "foo" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/5", "5" };
            yield return new object[] { JToken.Parse("[]"), "/-", "-" };
        }

        [Theory]
        [MemberData(nameof(GetRemovePatchData_Failure_Throw))]
        public void Remove_Failure_Throw(object input, string path, params string[] expectedMessageParts)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchRemoveDefinition
                {
                    Path = path
                }
            };

            var exception = Assert.Throws<JsonPatchException>(() => { Patch(input, patchDefinitions); });

            foreach (var expectedMessagePart in expectedMessageParts)
            {
                exception.Message.ShouldContain(expectedMessagePart);
            }
        }

        public static IEnumerable<object[]> GetRemovePatchData_Failure_Skip()
        {
            yield return new object[] { JToken.Parse("{ \"var\": 3 }"), "/foo" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/foo" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/5" };
            yield return new object[] { JToken.Parse("[]"), "/-" };
        }

        [Theory]
        [MemberData(nameof(GetRemovePatchData_Failure_Skip))]
        public void Remove_Failure_Skip(object input, string path)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchRemoveDefinition
                {
                    Path = path,
                    ErrorHandlingType = ErrorHandlingTypes.Skip
                }
            };

            var output = Patch(input, patchDefinitions);

            input.ShouldBeJson(output);
        }

        public static IEnumerable<object[]> GetReplacePatchData_Success()
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
        [MemberData(nameof(GetReplacePatchData_Success))]
        public void Replace_Success(object input, string path, object value, string expectedJson)
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

        public static IEnumerable<object[]> GetReplacePatchData_Failure_Throw()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/foo", 3, "foo" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/foo", 7, "foo" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/10", 7, "10" };
            yield return new object[] { JToken.Parse("[]"), "/-", 7, "-" };
        }

        [Theory]
        [MemberData(nameof(GetReplacePatchData_Failure_Throw))]
        public void Replace_Failure_Throw(object input, string path, object value, params string[] expectedMessageParts)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchReplaceDefinition
                {
                    Path = path,
                    Value = value
                }
            };

            var exception = Assert.Throws<JsonPatchException>(() => { Patch(input, patchDefinitions); });

            foreach (var expectedMessagePart in expectedMessageParts)
            {
                exception.Message.ShouldContain(expectedMessagePart);
            }
        }

        public static IEnumerable<object[]> GetReplacePatchData_Failure_Skip()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/foo", 3 };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/foo", 7 };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/10", 7 };
            yield return new object[] { JToken.Parse("[]"), "/-", 7 };
        }

        [Theory]
        [MemberData(nameof(GetReplacePatchData_Failure_Skip))]
        public void Replace_Failure_Skip(object input, string path, object value)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchReplaceDefinition
                {
                    Path = path,
                    Value = value,
                    ErrorHandlingType = ErrorHandlingTypes.Skip
                }
            };

            var output = Patch(input, patchDefinitions);

            input.ShouldBeJson(output);
        }

        public static IEnumerable<object[]> GetMovePatchData_Success()
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
        [MemberData(nameof(GetMovePatchData_Success))]
        public void Move_Success(object input, string from, string path, string expectedJson)
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

        public static IEnumerable<object[]> GetMovePatchData_Failure_Throw()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/foo", "/boo", "foo" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "", "/boo", "parent", "child" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3]}"), "/arr/10", "/boo", "10" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3]}"), "/arr/foo", "/boo", "foo" };
            yield return new object[] { JToken.Parse("{ \"arr\": []}"), "/arr/-", "/boo", "-" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", "/boo/foo", "boo" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3]}"), "/arr/2", "/boo/foo", "boo" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3], \"tar\":[]}"), "/arr/2", "/tar/7", "7" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3], \"tar\":[]}"), "/arr/2", "/tar/boo", "boo" };
        }

        [Theory]
        [MemberData(nameof(GetMovePatchData_Failure_Throw))]
        public void Move_Failure_Throw(object input, string from, string path, params string[] expectedMessageParts)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchMoveDefinition
                {
                    Path = path,
                    From = from
                }
            };

            var exception = Assert.Throws<JsonPatchException>(() => { Patch(input, patchDefinitions); });

            foreach (var expectedMessagePart in expectedMessageParts)
            {
                exception.Message.ShouldContain(expectedMessagePart);
            }
        }

        public static IEnumerable<object[]> GetMovePatchData_Failure_Skip()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/foo", "/boo" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "", "/boo" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3]}"), "/arr/10", "/boo" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3]}"), "/arr/foo", "/boo" };
            yield return new object[] { JToken.Parse("{ \"arr\": []}"), "/arr/-", "/boo" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", "/boo/foo" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3]}"), "/arr/2", "/boo/foo" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3], \"tar\":[]}"), "/arr/2", "/tar/7" };
            yield return new object[] { JToken.Parse("{ \"arr\": [1,2,3], \"tar\":[]}"), "/arr/2", "/tar/boo" };
        }

        [Theory]
        [MemberData(nameof(GetMovePatchData_Failure_Skip))]
        public void Move_Failure_Skip(object input, string from, string path)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchMoveDefinition
                {
                    Path = path,
                    From = from,
                    ErrorHandlingType = ErrorHandlingTypes.Skip
                }
            };

            var output = Patch(input, patchDefinitions);

            input.ShouldBeJson(output);
        }

        public static IEnumerable<object[]> GetCopyPatchData_Success()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", "/boo", "{ \"var\": 5, \"boo\": 5 }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", "/boo", "{ \"var\": 5, \"boo\": 5 }" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", "", "5" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", "", "5" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "", "/bar", "{ \"var\": 5, \"bar\": { \"var\": 5 } }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "", "/bar", "{ \"var\": 5, \"bar\": { \"var\": 5 } }" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", "/-", "[1, 2, 3, 2]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/0", "/1", "[1, 1, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", "/-", "[1, 2, 3, 2]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/0", "/1", "[1, 1, 2, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", "", "2" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", "", "2" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "", "/2", "[1, 2, [1, 2, 3], 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "", "/2", "[1, 2, [1, 2, 3], 3]" };
            yield return new object[] { new { Name = "Andrey" }, "/Name", "/FirstName", "{\"Name\":\"Andrey\",\"FirstName\":\"Andrey\"}" };
        }

        [Theory]
        [MemberData(nameof(GetCopyPatchData_Success))]
        public void Copy_Success(object input, string from, string path, string expectedJson)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchCopyDefinition
                {
                    Path = path,
                    From = from
                }
            };

            var output = Patch(input, patchDefinitions);

            output.ShouldBeJson(expectedJson);
        }

        public static IEnumerable<object[]> GetCopyPatchData_Failure_Throw()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/foo", "/boo", "foo" };
            yield return new object[] { JToken.Parse("{\"arr\": [1,2,3]}"), "/arr/7", "/boo", "7" };
            yield return new object[] { JToken.Parse("{\"arr\": [1,2,3]}"), "/arr/boo", "/foo", "boo" };
            yield return new object[] { JToken.Parse("{\"arr\": []}"), "/arr/-", "/foo", "-" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", "/foo/bar", "foo" };
            yield return new object[] { JToken.Parse("{\"var\": 5,\"arr\": [1,2,3]}"), "/var", "/arr/10", "10" };
        }

        [Theory]
        [MemberData(nameof(GetCopyPatchData_Failure_Throw))]
        public void Copy_Failure_Throw(object input, string from, string path, params string[] expectedMessageParts)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchCopyDefinition
                {
                    Path = path,
                    From = from
                }
            };

            var exception = Assert.Throws<JsonPatchException>(() => { Patch(input, patchDefinitions); });

            foreach (var expectedMessagePart in expectedMessageParts)
            {
                exception.Message.ShouldContain(expectedMessagePart);
            }
        }

        public static IEnumerable<object[]> GetCopyPatchData_Failure_Skip()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/foo", "/boo" };
            yield return new object[] { JToken.Parse("{\"arr\": [1,2,3]}"), "/arr/7", "/boo" };
            yield return new object[] { JToken.Parse("{\"arr\": [1,2,3]}"), "/arr/boo", "/foo" };
            yield return new object[] { JToken.Parse("{\"arr\": []}"), "/arr/-", "/foo" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", "/foo/bar" };
            yield return new object[] { JToken.Parse("{\"var\": 5,\"arr\": [1,2,3]}"), "/var", "/arr/10" };
        }

        [Theory]
        [MemberData(nameof(GetCopyPatchData_Failure_Skip))]
        public void Copy_Failure_Skip(object input, string from, string path)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchCopyDefinition
                {
                    Path = path,
                    From = from,
                    ErrorHandlingType = ErrorHandlingTypes.Skip
                }
            };

            var output = Patch(input, patchDefinitions);

            input.ShouldBeJson(output);
        }

        public static IEnumerable<object[]> GetTestPatchData_Success()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", JToken.Parse("5"), "{ \"var\": 5 }" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", 5, "{ \"var\": 5 }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", JToken.Parse("5"), "{ \"var\": 5 }" };
            yield return new object[] { new Dictionary<string, object> { { "var", 5 } }, "/var", 5, "{ \"var\": 5 }" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", JToken.Parse("2"), "[1, 2, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/1", 2, "[1, 2, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", JToken.Parse("3"), "[1, 2, 3]" };
            yield return new object[] { JToken.Parse("[1, 2, 3]"), "/-", 3, "[1, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", JToken.Parse("2"), "[1, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/1", 2, "[1, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", JToken.Parse("3"), "[1, 2, 3]" };
            yield return new object[] { new[] { 1, 2, 3 }, "/-", 3, "[1, 2, 3]" };
            yield return new object[] { new { Name = "Andrey" }, "/Name", "Andrey", "{\"Name\":\"Andrey\"}" };
        }

        [Theory]
        [MemberData(nameof(GetTestPatchData_Success))]
        public void Test_Success(object input, string path, object value, string expectedJson)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchTestDefinition
                {
                    Path = path,
                    Value = value
                }
            };

            var output = Patch(input, patchDefinitions);

            output.ShouldBeJson(expectedJson);
        }

        public static IEnumerable<object[]> GetTestPatchData_Failure_Throw()
        {
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/var", JToken.Parse("7"), "failed" };
            yield return new object[] { JToken.Parse("{\"var\": 5}"), "/boo", JToken.Parse("7"), "boo" };
            yield return new object[] { JToken.Parse("[1,2,3]"), "/2", JToken.Parse("7"), "failed" };
            yield return new object[] { JToken.Parse("[]"), "/-", JToken.Parse("7"), "-" };
        }

        [Theory]
        [MemberData(nameof(GetTestPatchData_Failure_Throw))]
        public void Test_Failure_Throw(object input, string path, object value, params string[] expectedMessageParts)
        {
            var patchDefinitions = new JsonPatchDefinition[]
            {
                new JsonPatchTestDefinition
                {
                    Path = path,
                    Value = value
                }
            };

            var exception = Assert.Throws<JsonPatchException>(() => { Patch(input, patchDefinitions); });

            foreach (var expectedMessagePart in expectedMessageParts)
            {
                exception.Message.ShouldContain(expectedMessagePart);
            }
        }

    }
}