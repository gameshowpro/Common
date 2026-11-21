namespace GameshowPro.Common.Test;

#if WPF
// The Windows configurations target a Windows-only TFM, so every test class is platform-specific (MSTEST0084).
[OSCondition(OperatingSystems.Windows)]
#endif
[TestClass]
public class TestUtilsBugFixes
{
    private sealed record SampleRecord(int Value);

    private sealed class SampleClass
    {
    }

    [TestMethod]
    public void MakeValidFilename_ReplacesEachInvalidCharWithHash()
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        string input = $"a{invalid[0]}b{invalid[^1]}c";
        Assert.AreEqual("a#b#c", input.MakeValidFilename());
        Assert.AreEqual("a#b#c", "a/b*c".MakeValidFilename());
    }

    [TestMethod]
    public void MakeValidFilename_WithNoInvalidChars_ReturnsInput()
    {
        const string input = "Valid file name.txt";
        Assert.AreEqual(input, input.MakeValidFilename());
        Assert.AreEqual(string.Empty, string.Empty.MakeValidFilename());
    }

    [TestMethod]
    public void MakeValidFilename_NeverReturnsTypeName()
    {
        Assert.AreNotEqual("System.Char[]", "a\0b".MakeValidFilename());
        Assert.AreEqual("a#b", "a\0b".MakeValidFilename());
    }

    [TestMethod]
    public void TimeSpanToString_WithHours_DoesNotThrowAndRollsDaysIntoHours()
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            TimeSpan value = new(1, 2, 3, 4, 500);
            Assert.AreEqual("26:03:04.5", value.ToString(true, 2, 1));
            Assert.AreEqual("1563:04.5", value.ToString(false, 2, 1));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [TestMethod]
    public void EnsureStringLength_TruncatesPadsAndKeeps()
    {
        Assert.AreEqual("abc", GameshowPro.Common.Utils.EnsureStringLength("abcdefgh", 3, '_'));
        Assert.AreEqual("ab___", GameshowPro.Common.Utils.EnsureStringLength("ab", 5, '_'));
        Assert.AreEqual("abcd", GameshowPro.Common.Utils.EnsureStringLength("abcd", 4, '_'));
        Assert.AreEqual(string.Empty, GameshowPro.Common.Utils.EnsureStringLength("abcd", 0, '_'));
    }

    [TestMethod]
    [DataRow("HELLO WORLD", true)]
    [DataRow("McDONALD'S RESTAURANT", true)]
    [DataRow("Hello world from here", false)]
    [DataRow("hello world", false)]
    [DataRow("AbCd", null)]
    [DataRow("", false)]
    public void IsUpperCase_ReportsMostlyUpper(string input, bool? expected)
    {
        Assert.AreEqual(expected, input.IsUpperCase());
    }

    [TestMethod]
    public void IsUpperCase_OfNull_IsNull()
    {
        string? input = null;
        Assert.IsNull(GameshowPro.Common.Utils.IsUpperCase(input!));
    }

    [TestMethod]
    public void IsUpperCase_RespectsAllowedLowerPercent()
    {
        // 8 upper, 2 lower: 20% lower.
        const string input = "ABCDEFGHij";
        Assert.IsTrue(input.IsUpperCase(.5));
        Assert.IsTrue(input.IsUpperCase(.25));
        Assert.IsFalse(input.IsUpperCase(.15));
    }

    [TestMethod]
    [DataRow(null, null)]
    [DataRow("Hello world from here", "HELLO WORLD FROM HERE")]
    [DataRow("McDONALD'S RESTAURANT", "McDONALD'S RESTAURANT")]
    [DataRow("hello world", "hello world")]
    public void UpperCaseIfRequired_PinsBehavior(string? input, string? expected)
    {
        Assert.AreEqual(expected, GameshowPro.Common.Utils.UpperCaseIfRequired(input));
    }

    [TestMethod]
    public void UpperCaseIfRequired_WhenTooCloseToCall_FollowsUpperIfUnknown()
    {
        Assert.AreEqual("ABCD", GameshowPro.Common.Utils.UpperCaseIfRequired("AbCd", .5, true));
        Assert.AreEqual("ABCD", GameshowPro.Common.Utils.UpperCaseIfRequired("AbCd"));
        Assert.AreEqual("AbCd", GameshowPro.Common.Utils.UpperCaseIfRequired("AbCd", .5, false));
    }

    private static void AssertAllPermutations(ImmutableArray<int[]> permutations, int length, int expectedCount)
    {
        Assert.HasCount(expectedCount, permutations);
        HashSet<string> seen = [];
        int[] sorted = [.. Enumerable.Range(0, length)];
        foreach (int[] permutation in permutations)
        {
            Assert.AreSequenceEqual(sorted, permutation.Order());
            Assert.IsTrue(seen.Add(string.Join(',', permutation)), "Duplicate permutation.");
        }
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(2, 2)]
    [DataRow(3, 6)]
    [DataRow(4, 24)]
    [DataRow(5, 120)]
    public void ShuffledNumbersPermutations_ReturnsEveryPermutationEndingInOrder(int length, int factorial)
    {
        ImmutableArray<int[]> result = GameshowPro.Common.Utils.ShuffledNumbersPermutations(length);
        AssertAllPermutations(result, length, factorial);
        Assert.AreSequenceEqual(Enumerable.Range(0, length), result[^1]);
    }

    [TestMethod]
    public void ShuffledNumbersPermutations_LengthTwo_HasExactOrder()
    {
        ImmutableArray<int[]> result = GameshowPro.Common.Utils.ShuffledNumbersPermutations(2);
        Assert.HasCount(2, result);
        int[] first = [1, 0];
        int[] second = [0, 1];
        Assert.AreSequenceEqual(first, result[0]);
        Assert.AreSequenceEqual(second, result[1]);
    }

    [TestMethod]
    public void ShuffledNumbersPermutations_LengthZero_IsEmpty()
    {
        Assert.IsEmpty(GameshowPro.Common.Utils.ShuffledNumbersPermutations(0));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    public void ShuffledNumbersPermutations_MaxCountAboveFactorial_ReturnsAllDistinct(int length)
    {
        int factorial = Enumerable.Range(1, length).Aggregate(1, (a, b) => a * b);
        ImmutableArray<int[]> result = GameshowPro.Common.Utils.ShuffledNumbersPermutations(length, factorial + 10, new Random(1));
        AssertAllPermutations(result, length, factorial);
    }

    [TestMethod]
    public void ShuffledNumbersPermutations_SmallMaxCount_ReturnsThatManyDistinct()
    {
        ImmutableArray<int[]> result = GameshowPro.Common.Utils.ShuffledNumbersPermutations(4, 4, new Random(1));
        AssertAllPermutations(result, 4, 4);
    }

    [TestMethod]
    public void IsRecordType_DistinguishesRecordsFromClassesAndNull()
    {
        Assert.IsTrue(typeof(SampleRecord).IsRecordType());
        Assert.IsFalse(typeof(SampleClass).IsRecordType());
        Assert.IsFalse(((Type?)null).IsRecordType());
    }

    [TestMethod]
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Test recomputes the result by reflection.")]
    public void IsRecordType_IsSafeUnderConcurrency()
    {
        Type[] types = [.. typeof(object).Assembly.GetTypes().Take(1500)];
        bool[] expected = [.. types.Select(static t => t.GetMethods().Any(static m => m.Name == "<Clone>$"))];
        bool[] actual = new bool[types.Length];
        _ = Parallel.For(0, types.Length * 4, i =>
        {
            int index = i % types.Length;
            actual[index] = types[index].IsRecordType();
        });
        Assert.AreSequenceEqual(expected, actual);
    }

    [TestMethod]
    public void PingClient_Buffer_Is32BytesOfAsciiA()
    {
        FieldInfo? field = typeof(PingClient).GetField("s_buffer", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(field);
        byte[]? buffer = field.GetValue(null) as byte[];
        Assert.IsNotNull(buffer);
        Assert.AreSequenceEqual(Enumerable.Repeat((byte)'a', 32), buffer);
    }

    private sealed class DepersistSample
    {
        public int Value { get; set; }
    }

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task SystemTextJsonDepersist_WithInvalidFileAndRename_RenamesBrokenFile()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"GspDepersist_{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(dir);
        try
        {
            string path = Path.Combine(dir, "settings.json");
            await File.WriteAllTextAsync(path, "{ this is not valid json", TestContext.CancellationToken);
            DepersistSample? result = await SystemTextJsonUtils.Depersist<DepersistSample>(path, false, true, null, null, TestContext.CancellationToken);
            Assert.IsNull(result);
            Assert.IsFalse(File.Exists(path));
            Assert.IsTrue(File.Exists(Path.Combine(dir, "settings.broken.1.json")));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [TestMethod]
    public void ShuffledNumbersPermutations_AllOverloadWithMoreThan12Items_Throws()
    {
        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(static () => GameshowPro.Common.Utils.ShuffledNumbersPermutations(13));
        Assert.AreEqual("sequenceLength", ex.ParamName);
    }

    [TestMethod]
    public void ShuffledNumbersPermutations_WithMaxCountAndLongSequence_ReturnsDistinctValidPermutations()
    {
        ImmutableArray<int[]> result = GameshowPro.Common.Utils.ShuffledNumbersPermutations(13, 5, new Random(1));
        Assert.HasCount(5, result);
        Assert.HasCount(5, result.Select(static p => string.Join(",", p)).Distinct().ToList());
        int[] expected = [.. Enumerable.Range(0, 13)];
        foreach (int[] permutation in result)
        {
            Assert.AreSequenceEqual(expected, permutation.Order());
        }
    }
}
