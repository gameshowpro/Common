# Common System.Text.Json Functionality in GameshowPro.Common

This document describes the System.Text.Json functionality that is implemented and available now in GameshowPro.Common.

## Scope

These behaviors are active by default when callers use:

- `SystemTextJsonUtils.DefaultJsonSerializerOptions`
- `SystemTextJsonUtils.Persist(...)`
- `SystemTextJsonUtils.Depersist(...)`
- `SystemTextJsonUtils.Persistence` (an `IPersistence`)

No extra setup is required for standard usage.

## Default Serialization Pipeline

`SystemTextJsonUtils.DefaultJsonSerializerOptions` is configured with:

- `PropertyNameCaseInsensitive = true`
- `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
- `DefaultIgnoreCondition = JsonIgnoreCondition.Never`
- `WriteIndented = true`
- `TypeInfoResolver = new OptInResolver<DataMemberAttribute>()`

Converters registered by default:

1. `FlexibleJsonConstructorConverterFactory`
2. `TypedObjectConverter`
3. `TypeConverter`
4. `JsonStringEnumConverter`
5. `IpAddressConverter`

Because `Persist` and `Depersist` use these options by default, all custom handling below applies automatically.

## Implemented Features

### 1. Opt-in member model with DataMember

Implemented by `OptInResolver<TDataMemberAttribute>`.

Behavior:

- Only members marked with `[DataMember]` participate in JSON serialization/deserialization.
- Supports public and non-public members.
- Includes private/internal fields and properties when annotated.
- Member names are mapped using the active naming policy (camelCase by default).

### 2. Typed object round-trip (`object?`)

Implemented by `TypedObjectConverter : JsonConverter<object?>`.

Wire format:

```json
{
  "$type": "System.Int32, System.Private.CoreLib",
  "value": 42
}
```

Behavior:

- `null` writes as JSON `null`.
- Non-null values write as `{ "$type", "value" }`.
- Runtime type is preserved, including value types and immutable collections.
- `Type` and runtime type values are serialized as stripped assembly-qualified names.

Error behavior (throws `JsonException`):

- Missing `$type` or `value` in object envelope.
- Unresolvable `$type` name.
- Bare primitive where object envelope is expected.
- Non-nullable value-type target with `null` value payload.

### 3. Flexible JsonConstructor support

Implemented by `FlexibleJsonConstructorConverterFactory`.

Behavior:

- Finds `[JsonConstructor]` on public or non-public constructors.
- Throws if multiple constructors are marked with `[JsonConstructor]`.
- Maps constructor parameters using JSON naming policy and case-insensitive binding.
- Supports mixed construction flow:
  - some members set by constructor parameters
  - remaining `[DataMember]` members set directly after construction
- Supports `IReadOnlySet<string> jsonPresentProperties` constructor parameter injection.
  - injected set contains exactly the JSON property names that were present in the payload
  - original payload spelling is preserved

### 4. Missing-property defaults for constructor parameters

Implemented by `JsonMissingDefaultAttribute`.

Usage:

- Apply `[JsonMissingDefault(value)]` to a constructor parameter.
- If that JSON property is absent, the annotated value is injected.
- If the property is present (including explicit `null`), normal deserialization rules apply.

### 5. Type serialization

Implemented by `TypeConverter`.

Behavior:

- Serializes `System.Type` as stripped assembly-qualified name.
- Deserializes with `Type.GetType(...)`.
- Uses common helper behavior for type-name normalization.

## How to Use

### Standard persistence API

```csharp
IPersistence persistence = SystemTextJsonUtils.Persistence;
await persistence.Persist(settings, path, logger: null, cancellationToken: null);
MySettings? loaded = await persistence.Depersist<MySettings>(path, false, true, null, null);
```

### Direct serializer options usage

```csharp
string json = JsonSerializer.Serialize(model, SystemTextJsonUtils.DefaultJsonSerializerOptions);
MyModel? roundTrip = JsonSerializer.Deserialize<MyModel>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions);
```

### Recommended model annotations

- Use `[DataMember]` on fields/properties that should be serialized.
- Use `[JsonConstructor]` on the intended construction path.
- For `object?` members, no extra converter attribute is required when default options are used.
- Use `[JsonMissingDefault(...)]` on constructor parameters where missing JSON should map to a non-standard default.

## Notes for Consumers

### JSON compatibility

`object?` values now require the typed envelope format. Legacy JSON that stores `object?` as bare primitives or untyped objects will fail deserialization under this pipeline.

### Constructor patterns supported

The library supports constructor-driven models commonly used in GameshowPro projects:

- private/internal constructors with `[JsonConstructor]`
- delegating constructors
- nullable constructor parameters with defaulting logic in constructor body
- post-construction member population for additional `[DataMember]` members

## Test Coverage Summary

Implemented tests cover:

- round-trip for all CueCore-supported `object?` types, including nullable and non-nullable variants
- mixed constructor/direct-set model deserialization
- `jsonPresentProperties` injection and exact key capture behavior
- malformed literal JSON inputs that this serializer would not emit, with expected exceptions
- unresolvable type names and nullability violations

All tests pass across net8.0, net9.0, and net10.0 targets.
