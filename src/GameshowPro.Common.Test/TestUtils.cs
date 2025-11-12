using System.Diagnostics;
[assembly: Parallelize]
namespace GameshowPro.Common.Test;

[TestClass]
public class TestUtils
{
    private static readonly Random s_rnd = new ();

    [TestMethod]
    public void RandomSequenceWithMinimumDistance_ShouldReturnSequenceWithMinimumDistance()
    {
        // Arrange
        var minimumRepeatDistance = 6;

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
        var sourcePool = 9;
        var destinationLength = 5;
        var minimumRepeatDistance = -1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Utils.RandomSequenceWithMinimumDistance(sourcePool, destinationLength, minimumRepeatDistance, false, s_rnd);
        });
    }

    [TestMethod]
    public void RandomSequenceWithMinimumDistance_ShouldThrowArgumentException_WhenMinimumRepeatDistanceIsGreaterThanSourcePool()
    {
        // Arrange
        var sourcePool = 9;
        var destinationLength = 5;
        var minimumRepeatDistance = 11;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Utils.RandomSequenceWithMinimumDistance(sourcePool, destinationLength, minimumRepeatDistance, false, s_rnd);
        });
    }
}
