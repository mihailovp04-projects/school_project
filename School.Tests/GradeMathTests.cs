using School.Services.Reports;

namespace School.Tests;

public class GradeMathTests
{
    [Theory]
    [InlineData(8.5, 9)]
    [InlineData(7.5, 8)]
    [InlineData(6.5, 7)]
    [InlineData(8.49, 8)]
    [InlineData(9.33, 9)]
    [InlineData(1.0, 1)]
    [InlineData(10.0, 10)]
    public void ToFinalGrade_RoundsHalfAwayFromZero(double average, int expected)
    {
        Assert.Equal(expected, GradeMath.ToFinalGrade(average));
    }
}
