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
        public void PatchObjectCopy_DoesNotModifyArgument()
        {
            var input = new Person
            {
                Name = "Andrey"
            };

            JsonPatcher.PatchObjectCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/Name",
                        Value = "Ivan"
                    }
                });

            input.Name.ShouldBe("Andrey");
        }
    }
}