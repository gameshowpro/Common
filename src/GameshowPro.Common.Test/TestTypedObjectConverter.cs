using System.Collections;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameshowPro.Common.Test;

[TestClass]
public class TestTypedObjectConverter
{
    [TestMethod]
    public void TypedObjectConverter_ShouldRoundTrip_AllCueCoreSupportedTypes()
    {
        object?[] values =
        [
            42,
            (int?)42,
            (byte)4,
            (byte?)4,
            (short)7,
            (short?)7,
            1234567890123L,
            (long?)1234567890123L,
            1.25f,
            (float?)1.25f,
            2.5d,
            (double?)2.5d,
            3.75m,
            (decimal?)3.75m,
            true,
            (bool?)true,
            "hello",
            new DateTime(2026, 3, 26, 10, 11, 12, 333, DateTimeKind.Utc).AddTicks(7),
            (DateTime?)new DateTime(2025, 1, 2, 3, 4, 5, 222, DateTimeKind.Utc).AddTicks(8),
            TimeSpan.FromTicks(123456789),
            (TimeSpan?)TimeSpan.FromTicks(987654321),
            ImmutableArray.Create(1, 2, 3),
            (ImmutableArray<int>?)ImmutableArray.Create(10, 20),
            ImmutableArray.Create<byte>(1, 2, 3),
            (ImmutableArray<byte>?)ImmutableArray.Create<byte>(4, 5),
            ImmutableArray.Create("a", "b"),
            (ImmutableArray<string>?)ImmutableArray.Create("c", "d"),
            ImmutableArray.Create(true, false),
            (ImmutableArray<bool>?)ImmutableArray.Create(false, true),
            ImmutableArray.Create(1.1d, 2.2d),
            (ImmutableArray<double>?)ImmutableArray.Create(3.3d, 4.4d),
            ImmutableArray.Create(1.1f, 2.2f),
            (ImmutableArray<float>?)ImmutableArray.Create(3.3f, 4.4f),
            typeof(string),
            null
        ];

        foreach (object? value in values)
        {
            ObjectValueContainer input = new() { Value = value };
            string json = JsonSerializer.Serialize(input, SystemTextJsonUtils.DefaultJsonSerializerOptions);
            ObjectValueContainer? output = JsonSerializer.Deserialize<ObjectValueContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions);

            Assert.IsNotNull(output);
            AssertObjectEquivalent(value, output!.Value);
        }
    }

    [TestMethod]
    public void TypedObjectConverter_ShouldThrow_WhenTypeIsUnresolvable()
    {
        const string json = "{\"value\":{\"$type\":\"No.Such.Type, Bogus\",\"value\":123}}";
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<ObjectValueContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void TypedObjectConverter_ShouldThrow_WhenEnvelopeIsMissingValueProperty()
    {
        const string json = "{\"value\":{\"$type\":\"System.Int32, System.Private.CoreLib\"}}";
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<ObjectValueContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void TypedObjectConverter_ShouldThrow_WhenEnvelopeIsMissingTypeProperty()
    {
        const string json = "{\"value\":{\"value\":123}}";
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<ObjectValueContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void TypedObjectConverter_ShouldThrow_OnBarePrimitive()
    {
        const string json = "{\"value\":123}";
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<ObjectValueContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void TypedObjectConverter_ShouldThrow_WhenNonNullableObjectIsNull()
    {
        const string json = "{\"value\":null}";
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<NonNullableObjectContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void TypedObjectConverter_ShouldThrow_WhenTypePropertyIsNotString()
    {
        // Impossible from this serializer: $type is always emitted as a string.
        const string json = "{\"value\":{\"$type\":123,\"value\":1}}";
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<ObjectValueContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void TypedObjectConverter_ShouldThrow_WhenTypeValuePayloadIsNotString()
    {
        // Impossible from this serializer: when $type resolves to Type/RuntimeType, value is emitted as a string.
        const string json = "{\"value\":{\"$type\":\"System.RuntimeType, System.Private.CoreLib\",\"value\":123}}";
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<ObjectValueContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void TypedObjectConverter_ShouldThrow_WhenNonNullableValueTypePayloadIsNull()
    {
        // Impossible from this serializer: int value payload would never be null when type is System.Int32.
        const string json = "{\"value\":{\"$type\":\"System.Int32, System.Private.CoreLib\",\"value\":null}}";
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<ObjectValueContainer>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    private static void AssertObjectEquivalent(object? expected, object? actual)
    {
        if (expected is null)
        {
            Assert.IsNull(actual);
            return;
        }

        Assert.IsNotNull(actual);
        Assert.AreEqual(expected.GetType(), actual!.GetType());

        switch (expected)
        {
            case DateTime expectedDateTime:
                Assert.AreEqual(expectedDateTime.Ticks, ((DateTime)actual).Ticks);
                return;
            case TimeSpan expectedTimeSpan:
                Assert.AreEqual(expectedTimeSpan.Ticks, ((TimeSpan)actual).Ticks);
                return;
            case IEnumerable expectedEnumerable when expected.GetType().IsGenericType && expected.GetType().GetGenericTypeDefinition() == typeof(ImmutableArray<>):
            {
                IEnumerable actualEnumerable = (IEnumerable)actual!;
                CollectionAssert.AreEqual(expectedEnumerable.Cast<object?>().ToArray(), actualEnumerable.Cast<object?>().ToArray());
                return;
            }
            default:
                Assert.AreEqual(expected, actual);
                return;
        }
    }

    private sealed class ObjectValueContainer
    {
        [DataMember]
        public object? Value { get; set; }
    }

    private sealed class NonNullableObjectContainer
    {
        [DataMember]
        public object Value { get; }

        [JsonConstructor]
        private NonNullableObjectContainer(object value)
        {
            Value = value;
        }
    }
}
