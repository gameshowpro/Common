using System.Buffers;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using GameshowPro.Common.MessagePack;
using GameshowPro.Common.Model;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace GameshowPro.Common.Test;

[TestClass]
public class TestMessagePackResolvers
{
    [TestMethod]
    public void BclFormatterResolver_ShouldRoundTrip_IPAddressAsBinary()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        IPAddress value = IPAddress.Parse("2001:db8::1");
        byte[] bytes = MessagePackSerializer.Serialize(value, options);
        IPAddress? result = MessagePackSerializer.Deserialize<IPAddress>(bytes, options);

        Assert.AreEqual(value, result);
        Assert.IsTrue(bytes.Length >= 18);
        Assert.IsFalse(bytes.Any(static b => b is ((byte)'.') or ((byte)':')));
    }

    [TestMethod]
    public void BclFormatterResolver_ShouldRoundTrip_IPEndPoint()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        IPEndPoint value = new(IPAddress.Parse("127.0.0.1"), 5678);
        byte[] bytes = MessagePackSerializer.Serialize(value, options);
        IPEndPoint? result = MessagePackSerializer.Deserialize<IPEndPoint>(bytes, options);

        Assert.IsNotNull(result);
        Assert.AreEqual(value.Address, result.Address);
        Assert.AreEqual(value.Port, result.Port);
    }

    [TestMethod]
    public void ConfigurableFormatterResolver_ShouldAllowOverrides()
    {
        ConfigurableFormatterResolver resolver = new ConfigurableFormatterResolver()
            .RegisterFormatter(StringIPAddressFormatter.Instance)
            .AddResolver(BclFormatterResolver.Instance);

        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                resolver,
                StandardResolver.Instance));

        IPAddress value = IPAddress.Parse("10.20.30.40");
        byte[] bytes = MessagePackSerializer.Serialize(value, options);
        IPAddress? result = MessagePackSerializer.Deserialize<IPAddress>(bytes, options);

        Assert.AreEqual(value, result);
        Assert.IsTrue(bytes[0] is >= 0xA0 and <= 0xDB);
    }

    [TestMethod]
    public void BclFormatterResolver_ShouldRoundTrip_JsonBridgeTypes()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        JsonObject jsonObject = new()
        {
            ["name"] = "alpha",
            ["values"] = new JsonArray(1, 2, 3),
            ["enabled"] = true
        };

        byte[] nodeBytes = MessagePackSerializer.Serialize<JsonNode?>(jsonObject, options);
        JsonNode? nodeResult = MessagePackSerializer.Deserialize<JsonNode?>(nodeBytes, options);
        Assert.IsNotNull(nodeResult);
        Assert.AreEqual("alpha", nodeResult!["name"]!.GetValue<string>());

        byte[] objectBytes = MessagePackSerializer.Serialize(jsonObject, options);
        JsonObject? objectResult = MessagePackSerializer.Deserialize<JsonObject?>(objectBytes, options);
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(3, objectResult!["values"]!.AsArray().Count);

        JsonDocument document = JsonDocument.Parse("{\"kind\":\"doc\",\"n\":7}");
        byte[] documentBytes = MessagePackSerializer.Serialize(document, options);
        using JsonDocument? documentResult = MessagePackSerializer.Deserialize<JsonDocument?>(documentBytes, options);
        Assert.IsNotNull(documentResult);
        Assert.AreEqual("doc", documentResult!.RootElement.GetProperty("kind").GetString());

        JsonElement element = document.RootElement;
        byte[] elementBytes = MessagePackSerializer.Serialize<JsonElement?>(element, options);
        JsonElement? elementResult = MessagePackSerializer.Deserialize<JsonElement?>(elementBytes, options);
        Assert.IsNotNull(elementResult);
        Assert.AreEqual(7, elementResult!.Value.GetProperty("n").GetInt32());
    }

    [TestMethod]
    public void BclFormatterResolver_ShouldRoundTrip_AssemblyName_AndExceptionDto()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        AssemblyName assemblyName = typeof(TestMessagePackResolvers).Assembly.GetName();
        byte[] assemblyBytes = MessagePackSerializer.Serialize(assemblyName, options);
        AssemblyName? assemblyResult = MessagePackSerializer.Deserialize<AssemblyName?>(assemblyBytes, options);
        Assert.IsNotNull(assemblyResult);
        Assert.AreEqual(assemblyName.Name, assemblyResult.Name);

        Exception source = new InvalidOperationException("Top level", new ArgumentException("Inner error"));
        byte[] exceptionBytes = MessagePackSerializer.Serialize<Exception?>(source, options);
        Exception? exceptionResult = MessagePackSerializer.Deserialize<Exception?>(exceptionBytes, options);
        Assert.IsNotNull(exceptionResult);
        Assert.AreEqual("Top level", exceptionResult.Message);
        _ = Assert.IsInstanceOfType<ExceptionFormatter.FormattedException>(exceptionResult);
        Assert.IsNotNull(exceptionResult.InnerException);
        Assert.AreEqual("Inner error", exceptionResult.InnerException.Message);
    }

    [TestMethod]
    public void BclFormatterResolver_ShouldSupport_NullReferenceTypeRoundTrip()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        Assert.IsNull(MessagePackSerializer.Deserialize<IPAddress?>(MessagePackSerializer.Serialize<IPAddress?>(null, options), options));
        Assert.IsNull(MessagePackSerializer.Deserialize<IPEndPoint?>(MessagePackSerializer.Serialize<IPEndPoint?>(null, options), options));
        Assert.IsNull(MessagePackSerializer.Deserialize<JsonNode?>(MessagePackSerializer.Serialize<JsonNode?>(null, options), options));
        Assert.IsNull(MessagePackSerializer.Deserialize<JsonObject?>(MessagePackSerializer.Serialize<JsonObject?>(null, options), options));
        Assert.IsNull(MessagePackSerializer.Deserialize<JsonArray?>(MessagePackSerializer.Serialize<JsonArray?>(null, options), options));
        Assert.IsNull(MessagePackSerializer.Deserialize<JsonDocument?>(MessagePackSerializer.Serialize<JsonDocument?>(null, options), options));
        Assert.IsNull(MessagePackSerializer.Deserialize<AssemblyName?>(MessagePackSerializer.Serialize<AssemblyName?>(null, options), options));
        Assert.IsNull(MessagePackSerializer.Deserialize<Exception?>(MessagePackSerializer.Serialize<Exception?>(null, options), options));
    }

    [TestMethod]
    public void NullableFormatterExtensions_ShouldRead_Int32_Value_And_Null()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        ArrayBufferWriter<byte> buffer = new();
        MessagePackWriter writer = new(buffer);
        writer.Write(42);
        writer.WriteNil();
        writer.Flush();

        MessagePackReader reader = new(buffer.WrittenMemory);
        int? value = reader.ReadNullable<int>(options);
        int? nil = reader.ReadNullable<int>(options);

        Assert.AreEqual(42, value);
        Assert.IsNull(nil);
    }

    [TestMethod]
    public void NullableFormatterExtensions_ShouldRead_TimeSpan_Value_And_Null()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        byte[] valueBytes = MessagePackSerializer.Serialize(TimeSpan.FromSeconds(12), options);
        byte[] nilBytes = MessagePackSerializer.Serialize<TimeSpan?>(null, options);
        byte[] bytes = valueBytes.Concat(nilBytes).ToArray();

        MessagePackReader reader = new(bytes);
        TimeSpan? first = reader.ReadNullable<TimeSpan>(options);
        TimeSpan? second = reader.ReadNullable<TimeSpan>(options);

        Assert.AreEqual(TimeSpan.FromSeconds(12), first);
        Assert.IsNull(second);
    }

    [TestMethod]
    public void NullableFormatterExtensions_ShouldRoundTrip_IPAddress_ReferenceType()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        ArrayBufferWriter<byte> buffer = new();
        MessagePackWriter writer = new(buffer);
        IPAddress? firstValue = IPAddress.Parse("192.168.1.42");
        IPAddress? secondValue = null;
        writer.WriteNullable(firstValue, options);
        writer.WriteNullable(secondValue, options);
        writer.Flush();

        MessagePackReader reader = new(buffer.WrittenMemory);
        IPAddress? first = reader.ReadNullable<IPAddress>(options);
        IPAddress? second = reader.ReadNullable<IPAddress>(options);

        Assert.AreEqual(firstValue, first);
        Assert.IsNull(second);
    }

    [TestMethod]
    public void NullableFormatterExtensions_ShouldRead_String_Value_And_Null()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard.WithResolver(
            CompositeResolver.Create(
                BclFormatterResolver.Instance,
                StandardResolver.Instance));

        ArrayBufferWriter<byte> buffer = new();
        MessagePackWriter writer = new(buffer);
        writer.Write("alpha");
        writer.WriteNil();
        writer.Flush();

        MessagePackReader reader = new(buffer.WrittenMemory);
        string? first = reader.ReadNullable<string>(options);
        string? second = reader.ReadNullable<string>(options);

        Assert.AreEqual("alpha", first);
        Assert.IsNull(second);
    }

    [TestMethod]
    public void ModelFormatters_ShouldRoundTrip_UsingNullableStructHelpers()
    {
        MessagePackSerializerOptions options = MessagePackSerializerOptions.Standard;

        EdgeReport edge = new(1, 5, 3, TimeSpan.FromMilliseconds(250), true, false, TimeSpan.FromSeconds(2));
        EdgeReport? edgeRoundTrip = MessagePackSerializer.Deserialize<EdgeReport?>(MessagePackSerializer.Serialize(edge, options), options);
        Assert.IsNotNull(edgeRoundTrip);
        Assert.AreEqual(edge.Version, edgeRoundTrip.Version);
        Assert.AreEqual(edge.Ordinal, edgeRoundTrip.Ordinal);
        Assert.AreEqual(edge.TimeStamp, edgeRoundTrip.TimeStamp);
        Assert.AreEqual(edge.LockoutTimeRemaining, edgeRoundTrip.LockoutTimeRemaining);

        LockoutReport lockout = new(1, 7, TimeSpan.FromMilliseconds(30), true);
        LockoutReport? lockoutRoundTrip = MessagePackSerializer.Deserialize<LockoutReport?>(MessagePackSerializer.Serialize(lockout, options), options);
        Assert.IsNotNull(lockoutRoundTrip);
        Assert.AreEqual(lockout.Version, lockoutRoundTrip.Version);
        Assert.AreEqual(lockout.Index, lockoutRoundTrip.Index);

        ServiceState state = new("svc", "service-key", RemoteServiceStates.Connected, "ready", 0.5,
            new ObservableDictionary<string, ServiceState>
            {
                ["child"] = new ServiceState("child", "child", RemoteServiceStates.Connected, "ok", 1.0)
            });
        ServiceState? stateRoundTrip = MessagePackSerializer.Deserialize<ServiceState?>(MessagePackSerializer.Serialize(state, options), options);
        Assert.IsNotNull(stateRoundTrip);
        Assert.AreEqual(state.Progress, stateRoundTrip.Progress);
        Assert.AreEqual(state.Detail, stateRoundTrip.Detail);
    }

    internal sealed class StringIPAddressFormatter : IMessagePackFormatter<IPAddress?>
    {
        internal static readonly StringIPAddressFormatter Instance = new();

        public IPAddress? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            string? text = reader.ReadString();
            Assert.IsNotNull(text);
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

}