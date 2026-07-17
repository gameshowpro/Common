# MessagePack

`GameshowPro.Common` includes a MessagePack integration layer intended for consistent cross-application behavior.

The implementation lives in the `GameshowPro.Common.MessagePack` namespace and includes:

- Standard BCL formatters for a small set of types not covered by default MessagePack behavior.
- A read-only default resolver.
- A configurable resolver that supports runtime overrides and resolver chaining.
- Nullable helper extensions that delegate to the active MessagePack resolver.

## Components

### Resolvers

- `BclFormatterResolver`
  - Read-only resolver for the library defaults.
  - Intended to be chained into `CompositeResolver.Create(...)`.

- `ConfigurableFormatterResolver`
  - Supports `RegisterFormatter<T>(...)` for explicit overrides.
  - Supports `AddResolver(...)` for ordered fallback chaining.
  - Caches resolved formatters for performance.

### BCL and platform formatters

Default `BclFormatterResolver` registrations currently include:

- `IPAddress?` as binary payload (4 bytes IPv4 / 16 bytes IPv6).
- `IPEndPoint?` as `[Address, Port]`.
- `JsonNode?`, `JsonObject?`, `JsonArray?` as UTF-8 JSON binary payloads.
- `JsonDocument?`, `JsonElement?` as UTF-8 JSON binary payloads.
- `AssemblyName?` as display name string (`FullName` fallback to `Name`).
- `Exception?` as a safe DTO-like shape containing class name, message, stack trace, and recursive inner exception.

### Nullable helper extensions

The library provides thin nullable helpers that resolve the formatter from `MessagePackSerializerOptions`, so you can write code like `reader.ReadNullable<int>(options)`, `reader.ReadNullable<TimeSpan>(options)`, or `reader.ReadNullable<string>(options)` without manually repeating nil checks. This does not replace MessagePack-CSharp's built-ins; it simply wraps the resolver lookup and nil handling.

- `NullableFormatterExtensions`
    - `WriteNullable<T>(..., MessagePackSerializerOptions options)`
    - `ReadNullable<T>(..., MessagePackSerializerOptions options)`

- `NullableReferenceFormatterExtensions`
    - `WriteNullable<T>(..., MessagePackSerializerOptions options)`
    - `ReadNullable<T>(..., MessagePackSerializerOptions options)`

Because C# does not allow overloads that differ only by generic constraints in a single class, these methods are split across two extension classes (`struct` and `class` constraints). Concrete call sites resolve naturally. In unconstrained generic code (`T` without `where T : struct` or `where T : class`), add a constraint at the caller so overload resolution remains unambiguous.

These are used in model formatters such as `ServiceState.MsgPackResolver`, `EdgeReportFormatter`, and `LockoutReportFormatter`.

### The `ServiceState` wire format is a frozen cross-language contract

`MsgPackResolver` does not just serve C#-to-C# hops: TriggerGate's ESP32 firmware emits the format from C (`cmp`), and Rust engines emit it via `gsp-engine-ipc`'s `service_state` module for the EngineIpc intrinsic status notification. Treat it exactly like an int-keyed record: **additive evolution only** — append fields (readers already skip extras beyond the current field count), never reorder, remove, or retype. Each node is:

```text
[ key: str, name: str|nil, state: uint8, detail: str|nil, progress: f64|nil, children: node[] ]
```

Key is field 0, matching the leading `key` parameter of every `ServiceState` constructor. History note (2026-07): `Serialize` wrote `Name` first while `Deserialize` bound field 0 to `key`, silently swapping the two across any hop where they differed — masked because most trees set `Key == Name`. Fixed in this repo; external emitters were already effectively key-first (ESP32 writes identical key/name strings). Cross-language byte-compatibility is regression-guarded by the golden tests in the EngineIpc repo.

## Consuming app usage

### Use library defaults + standard fallback

```csharp
using GameshowPro.Common.MessagePack;
using MessagePack;
using MessagePack.Resolvers;

MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard
    .WithResolver(CompositeResolver.Create(
        BclFormatterResolver.Instance,
        StandardResolver.Instance));

MessagePackSerializer.DefaultOptions = options;
```

### Override a library formatter and add app-specific formatters

```csharp
using GameshowPro.Common.MessagePack;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System.Net;

public sealed class StringIPAddressFormatter : IMessagePackFormatter<IPAddress?>
{
    public IPAddress? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        string text = reader.ReadString()!;
        return IPAddress.Parse(text);
    }

    public void Serialize(ref MessagePackWriter writer, IPAddress? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.Write(value.ToString());
    }
}

ConfigurableFormatterResolver resolver = new ConfigurableFormatterResolver()
    .RegisterFormatter<IPAddress?>(new StringIPAddressFormatter())
    .AddResolver(BclFormatterResolver.Instance);

MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard
    .WithResolver(CompositeResolver.Create(
        resolver,
        StandardResolver.Instance));
```

### Use nullable helper extensions inside custom formatters

```csharp
using GameshowPro.Common.MessagePack;
using MessagePack;
using MessagePack.Formatters;

public sealed class MyTypeFormatter : IMessagePackFormatter<MyType?>
{
    public MyType? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadNil())
        {
            return null;
        }

        int? version = reader.ReadNullable<int>(options);
        TimeSpan? elapsed = reader.ReadNullable<TimeSpan>(options);
        string? name = reader.ReadNullable<string>(options);
        return new MyType(version, elapsed, name);
    }

    public void Serialize(ref MessagePackWriter writer, MyType? value, MessagePackSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(3);
        writer.WriteNullable(value.Version, options);
        writer.WriteNullable(value.Elapsed, options);

        writer.WriteNullable(value.Name, options);
    }
}
```