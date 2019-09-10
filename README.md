[![Build status](https://ci.appveyor.com/api/projects/status/948byb9nflk2e7fa/branch/master?svg=true)](https://ci.appveyor.com/project/IvanIakimov/json-patch/branch/master)

# Json Patch implementation

This library contains [Json Patch](http://jsonpatch.com) implementation using [Newtonsoft.Json](https://www.newtonsoft.com/json).

## Usage

The library supports two modes. In the first one you can patch `JToken` objects using `PatchTokenCopy` method:

```cs
var input = JToken.Parse("{ \"age\": 40 }");

var output = JsonPatcher.PatchTokenCopy(
    input,
    new[]
    {
        new JsonPatchReplaceOperation
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
        new JsonPatchAddOperation
        {
            Path = "/Name",
            Value = "Ivan"
        }
    });

output.Name.ShouldBe("Ivan");
```

Be aware, that the library never modifies input object.

## Serialization settings

In `JsonPatch` library, you can use POCO objects in two places. First of all, you can patch them, as you saw in the previous example. But also you can use them as values of patch operations:

```cs
var patchOperation = new JsonPatchAddOperation
    {
        Path = "/-",
        Value = new PersonData("Ivan", 40)
    };
```

In both cases, it is quite possible that your objects can contain custom serializations settings. You can respect these settings by passing either `JsonSerializer` or `JsonSerializerSettings` object into `PatchTokenCopy` and `PatchObjectCopy` methods:

```cs
var input = JToken.Parse("[]");

var serializer = JsonSerializer.Create(GetSerializerSettingsForPersonData());

var output = JsonPatcher.PatchTokenCopy(
    input,
    new[]
    {
        new JsonPatchAddOperation
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

But you can customize this behavior. Almost every class of patch operation contains `ErrorHandlingType` property.

```cs
var patch = new JsonPatchMoveOperation
{
    Path = "/bar",
    From = "/foo",
    ErrorHandlingType = ErrorHandlingTypes.Skip
};
```

This property can have one of the following values:

* `null`. Default. In this case, if the patch can't be applied default error handling behavior will be applied. Read about it later.
* `Skip`. In this case, if the patch can't be applied, nothing happens and the next patch will be processed.
* `Throw`. In this case, if the patch can't be applied a `JsonPatchException` exception will be thrown.

Be aware, that `JsonPatchTestOperation` does not have this property. If the test is failed, an exception will always be thrown.

You can set default error handling behavior by setting `globalErrorHandling` parameter in the `PatchTokenCopy` and `PatchObjectCopy` methods of `JsonPatcher` class:

```cs
var output = JsonPatcher.PatchObjectCopy(
    input,
    new[]
    {
        new JsonPatchReplaceOperation
        {
            Path = "/xxx",
            Value = 41
        }
    },
    serializationSettings,
    globalErrorHandling: ErrorHandlingTypes.Skip);
```

## Patch operations

[Json Patch](http://jsonpatch.com) specification provides 6 patch operations:

* `Add`
* `Move`
* `Replace`
* `Remove`
* `Test`
* `Copy`

All of them are supported in the library. But the library provides one more additional operation: `AddMany`. For this operation 'Path' property must point to a place in an array. `Value` property may be anything. But if it is an array, then each value of this array will be inserted into the `Path` array.

Here is the difference between `Add` and `AddMany` operations. Let's say we want to patch JSON `[1, 2, 3]`. In the case of `Add` operation:

```json
{
    "op": "add",
    "path": "/-",
    "value": [4, 5, 6]
}
```

result will be `[1, 2, 3, [4, 5, 6]]`. But in the case of 'AddMany' operation:

```json
{
    "op": "addmany",
    "path": "/-",
    "value": [4, 5, 6]
}
```

result will be `[1, 2, 3, 4, 5, 6]`.