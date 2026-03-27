using System.Collections;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameshowPro.Common.Test;

[TestClass]
public class TestFlexibleJsonConstructorResolver
{
    [TestMethod]
    public void MixedModel_WithoutJsonPresentProperties_ShouldRoundTrip_ConstructorAndDirectSetMembers()
    {
        MixedWithoutPresent source = MixedWithoutPresent.CreateForTest(7, ImmutableArray.Create(1, 2, 3), "set-direct", true, 9.5d);

        string json = JsonSerializer.Serialize(source, SystemTextJsonUtils.DefaultJsonSerializerOptions);
        MixedWithoutPresent? output = JsonSerializer.Deserialize<MixedWithoutPresent>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions);

        Assert.IsNotNull(output);
        Assert.AreEqual(7, output.CtorNumber);
        Assert.AreEqual("set-direct", output.DirectString);
        Assert.IsTrue(output.DirectFlag);
        Assert.AreEqual(9.5d, output.DirectNumber);
        Assert.IsInstanceOfType<ImmutableArray<int>>(output.CtorObject);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ((ImmutableArray<int>)output.CtorObject).ToArray());
    }

    [TestMethod]
    public void MixedModel_WithJsonPresentProperties_ShouldTrackPresentProperties_AndSetDirectMembers()
    {
        const string json = """
{
  "ctorNumber": 5,
  "ctorObject": { "$type": "System.Int32, System.Private.CoreLib", "value": 11 },
  "directString": "from-json"
}
""";

        MixedWithPresent? output = JsonSerializer.Deserialize<MixedWithPresent>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions);

        Assert.IsNotNull(output);
        Assert.AreEqual(5, output.CtorNumber);
        Assert.AreEqual(11, output.CtorObject);
        Assert.AreEqual("from-json", output.DirectString);
        Assert.AreEqual(0.0d, output.DirectNumber);
        Assert.IsFalse(output.DirectFlag);

        Assert.IsTrue(output.JsonPresentProperties.Contains("ctorNumber"));
        Assert.IsTrue(output.JsonPresentProperties.Contains("ctorObject"));
        Assert.IsTrue(output.JsonPresentProperties.Contains("directString"));
        Assert.IsFalse(output.JsonPresentProperties.Contains("directFlag"));
        Assert.IsFalse(output.JsonPresentProperties.Contains("directNumber"));
    }

    [TestMethod]
    public void MixedModel_WithJsonPresentProperties_ShouldRoundTrip_AllSupportedObjectValues()
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
            MixedWithPresent source = MixedWithPresent.CreateForTest(1, value, value, "x", false, 3.2d);
            string json = JsonSerializer.Serialize(source, SystemTextJsonUtils.DefaultJsonSerializerOptions);
            MixedWithPresent? output = JsonSerializer.Deserialize<MixedWithPresent>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions);

            Assert.IsNotNull(output);
            AssertObjectEquivalent(value, output.CtorObject);
            AssertObjectEquivalent(value, output.DirectObject);
        }
    }

    [TestMethod]
    public void JsonMissingDefault_ShouldApply_WhenPropertyIsMissing()
    {
        const string json = "{}";
        MissingDefaultModel? output = JsonSerializer.Deserialize<MissingDefaultModel>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions);

        Assert.IsNotNull(output);
        Assert.AreEqual(-1, output.Number);
    }

    [TestMethod]
    public void FlexibleConstructor_ShouldThrow_WhenMultipleJsonConstructorsExist()
    {
        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<MultipleJsonConstructorsModel>("{}", SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void MixedModel_ShouldThrow_WhenObjectTypeCannotBeResolved()
    {
        const string json = """
{
  "ctorNumber": 5,
  "ctorObject": { "$type": "No.Such.Type, Bogus", "value": 1 }
}
""";

        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<MixedWithPresent>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void MixedModel_ShouldThrow_WhenObjectPayloadIsBarePrimitive()
    {
        // Impossible from this serializer: object? values are always emitted with {$type,value} envelope.
        const string json = """
{
  "ctorNumber": 5,
  "ctorObject": 123
}
""";

        Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<MixedWithPresent>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions));
    }

    [TestMethod]
    public void MixedModel_WithJsonPresentProperties_ShouldCaptureCamelCaseAndExtraKeys()
    {
        const string json = """
{
  "ipAddress": "10.1.2.3",
  "urlValue": 123,
  "extraKey": true
}
""";

        JsonPresentKeyCaseModel? output = JsonSerializer.Deserialize<JsonPresentKeyCaseModel>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions);

        Assert.IsNotNull(output);
        Assert.AreEqual("10.1.2.3", output.IPAddress);
        Assert.AreEqual(123, output.URLValue);
        Assert.IsTrue(output.JsonPresentProperties.SetEquals(["ipAddress", "urlValue", "extraKey"]));
        Assert.IsFalse(output.JsonPresentProperties.Contains("IPAddress"));
        Assert.IsFalse(output.JsonPresentProperties.Contains("URLValue"));
    }

    [TestMethod]
    public void MixedModel_WithJsonPresentProperties_ShouldDeserializeWrongCaseKeys_AndPreserveOriginalSpelling()
    {
        const string json = """
{
  "IPADDRESS": "10.2.3.4",
  "uRlVaLuE": 456,
  "ExTrA": false
}
""";

        JsonPresentKeyCaseModel? output = JsonSerializer.Deserialize<JsonPresentKeyCaseModel>(json, SystemTextJsonUtils.DefaultJsonSerializerOptions);

        Assert.IsNotNull(output);
        Assert.AreEqual("10.2.3.4", output.IPAddress);
        Assert.AreEqual(456, output.URLValue);

        // Binding is case-insensitive, but the captured presence set preserves raw payload spellings.
        Assert.IsTrue(output.JsonPresentProperties.SetEquals(["IPADDRESS", "uRlVaLuE", "ExTrA"]));
        Assert.IsFalse(output.JsonPresentProperties.Contains("ipAddress"));
        Assert.IsFalse(output.JsonPresentProperties.Contains("urlValue"));
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

    private sealed class MixedWithoutPresent
    {
        [DataMember]
        public int CtorNumber { get; }

        [DataMember]
        public object? CtorObject { get; }

        [DataMember]
        public string? DirectString { get; private set; }

        [DataMember]
        public bool DirectFlag { get; private set; }

        [DataMember]
        public double DirectNumber { get; private set; }

        [JsonConstructor]
        private MixedWithoutPresent(int? ctorNumber, object? ctorObject)
        {
            CtorNumber = ctorNumber ?? 0;
            CtorObject = ctorObject;
        }

        public static MixedWithoutPresent CreateForTest(int ctorNumber, object? ctorObject, string? directString, bool directFlag, double directNumber)
        {
            MixedWithoutPresent instance = new(ctorNumber, ctorObject)
            {
                DirectString = directString,
                DirectFlag = directFlag,
                DirectNumber = directNumber
            };
            return instance;
        }
    }

    private sealed class MixedWithPresent
    {
        [DataMember]
        public int CtorNumber { get; }

        [DataMember]
        public object? CtorObject { get; }

        [DataMember]
        public object? DirectObject { get; private set; }

        [DataMember]
        public string? DirectString { get; private set; }

        [DataMember]
        public bool DirectFlag { get; private set; }

        [DataMember]
        public double DirectNumber { get; private set; }

        public IReadOnlySet<string> JsonPresentProperties { get; }

        [JsonConstructor]
        private MixedWithPresent(int? ctorNumber, object? ctorObject, IReadOnlySet<string> jsonPresentProperties)
        {
            CtorNumber = ctorNumber ?? 0;
            CtorObject = ctorObject;
            JsonPresentProperties = jsonPresentProperties;
        }

        public static MixedWithPresent CreateForTest(int ctorNumber, object? ctorObject, object? directObject, string? directString, bool directFlag, double directNumber)
        {
            MixedWithPresent instance = new(ctorNumber, ctorObject, new HashSet<string>(StringComparer.OrdinalIgnoreCase))
            {
                DirectObject = directObject,
                DirectString = directString,
                DirectFlag = directFlag,
                DirectNumber = directNumber
            };
            return instance;
        }
    }

    private sealed class MissingDefaultModel
    {
        [DataMember]
        public int Number { get; }

        [JsonConstructor]
        private MissingDefaultModel([JsonMissingDefault(-1)] int number)
        {
            Number = number;
        }
    }

    private sealed class MultipleJsonConstructorsModel
    {
        [JsonConstructor]
        public MultipleJsonConstructorsModel()
        {
        }

        [JsonConstructor]
        public MultipleJsonConstructorsModel(int value)
        {
            Value = value;
        }

        [DataMember]
        public int Value { get; }
    }

    private sealed class JsonPresentKeyCaseModel
    {
        [DataMember]
        public string? IPAddress { get; private set; }

        [DataMember]
        public int URLValue { get; private set; }

        public IReadOnlySet<string> JsonPresentProperties { get; }

        [JsonConstructor]
        private JsonPresentKeyCaseModel(IReadOnlySet<string> jsonPresentProperties)
        {
            JsonPresentProperties = jsonPresentProperties;
        }
    }
}
