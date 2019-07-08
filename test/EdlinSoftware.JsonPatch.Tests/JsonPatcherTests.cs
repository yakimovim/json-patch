using System.ComponentModel;
using EdlinSoftware.JsonPatch.Pointers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using Xunit;

namespace EdlinSoftware.JsonPatch.Tests
{
    public class JsonPatcherTests
    {
        [Fact]
        public void PatchTokenCopy_DoesNotModifyArgument()
        {
            var input = JToken.Parse("{}");

            JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/var",
                        Value = JToken.Parse("3")
                    }
                });

            input.ShouldBeJson("{}");
        }

        [Fact]
        public void AddModification_ObjectProperty_JTokenValue()
        {
            var input = JToken.Parse("{}");

            var output = JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/var",
                        Value = JToken.Parse("3")
                    }
                });

            output.ShouldBeJson("{ \"var\": 3 }");
        }

        [Fact]
        public void AddModification_ObjectProperty_POCValue()
        {
            var input = JToken.Parse("{}");

            var output = JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/var",
                        Value = 3
                    }
                });

            output.ShouldBeJson("{ \"var\": 3 }");
        }

        [Fact]
        public void AddModification_ArrayItem_JTokenValue()
        {
            var input = JToken.Parse("[1, 2, 3]");

            var output = JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/1",
                        Value = JToken.Parse("7")
                    }
                });

            output.ShouldBeJson("[1, 7, 2, 3]");
        }

        [Fact]
        public void AddModification_ArrayItem_POCValue()
        {
            var input = JToken.Parse("[1, 2, 3]");

            var output = JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/1",
                        Value = 7
                    }
                });

            output.ShouldBeJson("[1, 7, 2, 3]");
        }

        [Fact]
        public void AddModification_LastArrayItem()
        {
            var input = JToken.Parse("[1, 2, 3]");

            var output = JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/-",
                        Value = 7
                    }
                });

            output.ShouldBeJson("[1, 2, 3, 7]");
        }

        [Fact]
        public void AddModification_ReplaceRoot()
        {
            var input = JToken.Parse("[1, 2, 3]");

            var output = JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "",
                        Value = new { skip = 7 }
                    }
                });

            output.ShouldBeJson("{\"skip\":7}");
        }

        [Fact]
        public void AddModification_POCObject()
        {
            var input = new Person();

            var output = JsonPatcher.PatchObjectCopy(input, new[]
            {
                new JsonPatchAddDefinition
                {
                    Path = "/name",
                    Value = "Ivan"
                }
            });

            output.Name.ShouldBe("Ivan");
        }
    }


    internal class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public static class JsonTestUtilities
    {
        public static void ShouldBeJson(this JToken actualToken, string expectedJson)
        {
            var actualJson = actualToken.ToString();
            expectedJson = JToken.Parse(expectedJson).ToString();

            actualJson.ShouldBe(expectedJson);
        }

        public static void ShouldBe(this JsonPointer jsonPointer, string expectedValue)
        {
            jsonPointer.ToString().ShouldBe(expectedValue);
        }
    }
}