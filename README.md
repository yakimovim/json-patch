# Json Patch implementation

This library contains [Json Patch](http://jsonpatch.com) implementation using [Newtonsoft.Json](https://www.newtonsoft.com/json).

## Usage

The library contains two modes. In the first one you can patch `JToken` objects using `PatchTokenCopy` method:

```cs
var input = JToken.Parse("{ \"age\": 40 }");

var output = JsonPatcher.PatchTokenCopy(
    input,
    new[]
    {
        new JsonPatchReplaceDefinition
        {
            Path = "/age",
            Value = 41
        }
    });

output.ShouldBeJson("{ \"age\": 41 }");
```

In the second mode you can patch POCO objects using `PatchObjectCopy` method:

```cs
var input = new Person
{
    Name = "Andrey"
};

var output = JsonPatcher.PatchObjectCopy(
    input,
    new[]
    {
        new JsonPatchAddDefinition
        {
            Path = "/Name",
            Value = "Ivan"
        }
    });

output.Name.ShouldBe("Ivan");
```

Be aware, that the library never modifies input object.

## Serialization settings

In `JsonPatch` library you can use POCO objects in two places. First of all, you can patch them, as you saw in the previous example. But also you can  use them as values of patch definitions:

```cs
var patchDefinition = new JsonPatchAddDefinition
    {
        Path = "/-",
        Value = new PersonData("Ivan", 40)
    };
```

In both cases it is quite possible that your objects can contain custom serializations settings. You can respect these settings by passing either `JsonSerializer` or `JsonSerializerSettings` object into `PatchTokenCopy` and `PatchObjectCopy` methods:

```cs
var input = JToken.Parse("[]");

var serializer = JsonSerializer.Create(GetSerializerSettingsForPersonData());

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

JToken.DeepEquals(
    JToken.Parse("[{\"name\":\"Ivan\",\"age\":40}]"),
    output
    ).ShouldBeTrue();
```

## Error handling

 [Json Patch](http://jsonpatch.com) specification says that patching should stop on any error. And this is a default behaviour of the library. In case any operation can't be fulfilled, `JsonPatchException` exception will be thrown.

 