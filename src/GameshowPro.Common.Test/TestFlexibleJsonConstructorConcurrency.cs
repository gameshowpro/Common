using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameshowPro.Common.Test;

[TestClass]
public class TestFlexibleJsonConstructorConcurrency
{
    private const int Rounds = 25;
    private const int ThreadsPerRound = 16;

    // NullabilityInfoContext is not thread-safe. The converter used to share one context from its
    // factory and query it on every read, so concurrent first reads through a fresh factory could
    // throw "An item with the same key has already been added".
    [TestMethod]
    public void ConcurrentFirstReads_ThroughFreshFactory_ShouldNotThrow()
    {
        const string json = """{ "name": null, "value": 3 }""";
        for (int round = 0; round < Rounds; round++)
        {
            JsonSerializerOptions options = new() { Converters = { new FlexibleJsonConstructorConverterFactory() } };
            using Barrier start = new(ThreadsPerRound);
            List<Exception> failures = [];
            Thread[] threads = [.. Enumerable.Range(0, ThreadsPerRound).Select(_ => new Thread(() =>
            {
                start.SignalAndWait();
                try
                {
                    NullableCtorModel? model = JsonSerializer.Deserialize<NullableCtorModel>(json, options);
                    Assert.IsNotNull(model);
                    Assert.IsNull(model.Name);
                    Assert.AreEqual(3, model.Value);
                }
                catch (Exception ex)
                {
                    lock (failures)
                    {
                        failures.Add(ex);
                    }
                }
            }))];
            foreach (Thread thread in threads)
            {
                thread.Start();
            }
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
            Assert.IsEmpty(failures, $"Round {round}: {string.Join(" | ", failures.Select(static f => f.Message))}");
        }
    }

    [TestMethod]
    public void NonNullableParameter_ShouldStillRejectNull()
    {
        JsonSerializerOptions options = new() { Converters = { new FlexibleJsonConstructorConverterFactory() } };
        _ = Assert.ThrowsExactly<JsonException>(() => JsonSerializer.Deserialize<NonNullableCtorModel>("""{ "name": null }""", options));
    }

    private sealed class NullableCtorModel
    {
        [JsonConstructor]
        internal NullableCtorModel(string? name, int value)
        {
            Name = name;
            Value = value;
        }

        public string? Name { get; }
        public int Value { get; }
    }

    private sealed class NonNullableCtorModel
    {
        [JsonConstructor]
        internal NonNullableCtorModel(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
