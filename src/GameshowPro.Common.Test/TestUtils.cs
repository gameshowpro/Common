[assembly: Parallelize]
namespace GameshowPro.Common.Test;

#if WPF
// The Windows configurations target a Windows-only TFM, so every test class is platform-specific (MSTEST0084).
[OSCondition(OperatingSystems.Windows)]
#endif
[TestClass]
public class TestUtils
{
    private static readonly Random s_rnd = new();

    [TestMethod]
    [DataRow(1, "1st")]
    [DataRow(2, "2nd")]
    [DataRow(3, "3rd")]
    [DataRow(4, "4th")]
    [DataRow(10, "10th")]
    [DataRow(11, "11th")]
    [DataRow(12, "12th")]
    [DataRow(13, "13th")]
    [DataRow(21, "21st")]
    [DataRow(22, "22nd")]
    [DataRow(23, "23rd")]
    [DataRow(101, "101st")]
    [DataRow(111, "111th")]
    [DataRow(112, "112th")]
    [DataRow(113, "113th")]
    [DataRow(0, "0th")]
    public void ToOrdinal_UsesTheEnglishSuffixes(int value, string expected)
    {
        Assert.AreEqual(expected, value.ToOrdinal());
        Assert.AreEqual(expected, (value - 1).ToOrdinal(fromZeroBased: true));
        Assert.AreEqual(expected, ((int?)value).ToOrdinal());
    }

    [TestMethod]
    public void ToOrdinal_OfNull_IsEmpty() => Assert.AreEqual(string.Empty, ((int?)null).ToOrdinal());

    [TestMethod]
    public void RandomSequenceWithMinimumDistance_ShouldReturnSequenceWithMinimumDistance()
    {
        // Arrange
        int minimumRepeatDistance = 6;

        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int sourcePool = minimumRepeatDistance + 1; sourcePool < 100; sourcePool++)
        {
            for (int destinationLength = 10; destinationLength < 100; destinationLength++)
            {
                // Act
                ImmutableArray<int> result = Utils.RandomSequenceWithMinimumDistance(sourcePool, destinationLength, minimumRepeatDistance, true, s_rnd);
                // Assert
                Assert.HasCount(destinationLength, result);
                if (sourcePool > (minimumRepeatDistance * 2))
                {
                    ImmutableArray<int> resultRepeated = [.. result, .. result];
                    for (int i = 0; i < result.Length; i++)
                    {
                        int current = result[i];
                        for (int j = i + 1; j < (i + minimumRepeatDistance); j++)
                        {
                            Assert.AreNotEqual(current, resultRepeated[j]);
                        }
                    }
                }
            }
        }
        stopwatch.Stop();
        Debug.WriteLine($"Time: {stopwatch.Elapsed.TotalMilliseconds}ms");
    }

    [TestMethod]
    public void RandomSequenceWithMinimumDistance_ShouldThrowArgumentException_WhenMinimumRepeatDistanceIsNegative()
    {
        // Arrange
        int sourcePool = 9;
        int destinationLength = 5;
        int minimumRepeatDistance = -1;

        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() =>
        {
            _ = Utils.RandomSequenceWithMinimumDistance(sourcePool, destinationLength, minimumRepeatDistance, false, s_rnd);
        });
    }

    [TestMethod]
    public void RandomSequenceWithMinimumDistance_ShouldThrowArgumentException_WhenMinimumRepeatDistanceIsGreaterThanSourcePool()
    {
        // Arrange
        int sourcePool = 9;
        int destinationLength = 5;
        int minimumRepeatDistance = 11;

        // Act & Assert
        _ = Assert.Throws<ArgumentException>(() =>
        {
            _ = Utils.RandomSequenceWithMinimumDistance(sourcePool, destinationLength, minimumRepeatDistance, false, s_rnd);
        });
    }
}
