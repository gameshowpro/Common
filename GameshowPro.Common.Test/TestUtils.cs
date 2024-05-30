using System.Diagnostics;

namespace GameshowPro.Common.Test;

[TestClass]
public class TestUtils
{
    private static readonly Random s_rnd = new ();

    [TestMethod]
    public void RandomSequenceWithMinimumDistance_ShouldReturnSequenceWithMinimumDistance()
    {
        // Arrange
        var sourcePool = Enumerable.Range(0, 10).ToList();
        var destinationLength = 54;
        var minimumRepeatDistance = 6;

        // Act
        Stopwatch stopwatch = Stopwatch.StartNew();
        ImmutableArray<int> result = Utils.RandomSequenceWithMinimumDistance(sourcePool, destinationLength, minimumRepeatDistance, true, s_rnd);
        stopwatch.Stop();
        Debug.WriteLine($"RandomSequenceWithMinimumDistance_ShouldReturnSequenceWithMinimumDistance time: {stopwatch.Elapsed.TotalMilliseconds}ms");
        // Assert
        Assert.AreEqual(destinationLength, result.Length);

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

    [TestMethod]
    public void RandomSequenceWithMinimumDistance_ShouldThrowArgumentException_WhenMinimumRepeatDistanceIsNegative()
    {
        // Arrange
        var sourcePool = Enumerable.Range(1, 10).ToList();
        var destinationLength = 5;
        var minimumRepeatDistance = -1;

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            Utils.RandomSequenceWithMinimumDistance(sourcePool, destinationLength, minimumRepeatDistance, false, s_rnd);
        });
    }

    [TestMethod]
    public void RandomSequenceWithMinimumDistance_ShouldThrowArgumentException_WhenMinimumRepeatDistanceIsGreaterThanSourcePool()
    {
        // Arrange
        var sourcePool = Enumerable.Range(1, 10).ToList();
        var destinationLength = 5;
        var minimumRepeatDistance = 11;

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
        {
            Utils.RandomSequenceWithMinimumDistance(sourcePool, destinationLength, minimumRepeatDistance, false, s_rnd);
        });
    }
}
