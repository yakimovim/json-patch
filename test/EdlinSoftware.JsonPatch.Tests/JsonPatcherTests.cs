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

        [Fact]
        public void PatchTokenCopy_UsesSerializerForPatchValues()
        {
            // Arrange
            var input = JToken.Parse("[]");

            var serializer = JsonSerializer.Create(GetSerializerSettingsForPersonData());

            // Act
            var output = JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/-",
                        Value = new PersonData("Ivan", 40)
                    }
                },
                serializer);

            // Assert
            JToken.DeepEquals(
                JToken.Parse("[{\"name\":\"Ivan\",\"age\":40}]"),
                output
                ).ShouldBeTrue();
        }

        [Fact]
        public void PatchTokenCopy_UsesSerializationSettingsForPatchValues()
        {
            // Arrange
            var input = JToken.Parse("[]");

            var serializationSettings = GetSerializerSettingsForPersonData();

            // Act
            var output = JsonPatcher.PatchTokenCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/-",
                        Value = new PersonData("Ivan", 40)
                    }
                },
                serializationSettings);

            // Assert
            JToken.DeepEquals(
                JToken.Parse("[{\"name\":\"Ivan\",\"age\":40}]"),
                output
                ).ShouldBeTrue();
        }

        [Fact]
        public void PatchObjectCopy_UsesSerializerForPatchValues()
        {
            // Arrange
            var input = new PersonData[0];

            var serializer = JsonSerializer.Create(GetSerializerSettingsForPersonData());

            // Act
            var output = JsonPatcher.PatchObjectCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/-",
                        Value = new PersonData("Ivan", 40)
                    }
                },
                serializer);

            // Assert
            output.ShouldNotBeNull();
            output.Length.ShouldBe(1);
            output[0].ShouldNotBeNull();
            output[0].ToString().ShouldBe(PersonData.GetStringPresentation("Ivan", 40));
        }

        [Fact]
        public void PatchObjectCopy_UsesSerializationSettingsForPatchValues()
        {
            // Arrange
            var input = new PersonData[0];

            var serializationSettings = GetSerializerSettingsForPersonData();

            // Act
            var output = JsonPatcher.PatchObjectCopy(
                input,
                new[]
                {
                    new JsonPatchAddDefinition
                    {
                        Path = "/-",
                        Value = new PersonData("Ivan", 40)
                    }
                },
                serializationSettings);

            // Assert
            output.ShouldNotBeNull();
            output.Length.ShouldBe(1);
            output[0].ShouldNotBeNull();
            output[0].ToString().ShouldBe(PersonData.GetStringPresentation("Ivan", 40));
        }

        [Fact]
        public void PatchObjectCopy_UsesSerializerForInput()
        {
            // Arrange
            var input = new PersonData("Ivan", 40);

            var serializer = JsonSerializer.Create(GetSerializerSettingsForPersonData());

            // Act
            var output = JsonPatcher.PatchObjectCopy(
                input,
                new[]
                {
                    new JsonPatchReplaceDefinition
                    {
                        Path = "/age",
                        Value = 41
                    }
                },
                serializer);

            // Assert
            output.ShouldNotBeNull();
            output.ToString().ShouldBe(PersonData.GetStringPresentation("Ivan", 41));
        }

        [Fact]
        public void PatchObjectCopy_UsesSerializationSettingsForInput()
        {
            // Arrange
            var input = new PersonData("Ivan", 40);

            var serializationSettings = GetSerializerSettingsForPersonData();

            // Act
            var output = JsonPatcher.PatchObjectCopy(
                input,
                new[]
                {
                    new JsonPatchReplaceDefinition
                    {
                        Path = "/age",
                        Value = 41
                    }
                },
                serializationSettings);

            // Assert
            output.ShouldNotBeNull();
            output.ToString().ShouldBe(PersonData.GetStringPresentation("Ivan", 41));
        }

        private static JsonSerializerSettings GetSerializerSettingsForPersonData()
        {
            return new JsonSerializerSettings
            {
                Converters =
                    {
                        new PersonData.PersonDataConverter()
                    }
            };
        }
    }
}