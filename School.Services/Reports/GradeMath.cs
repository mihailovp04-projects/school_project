namespace School.Services.Reports;

public static class GradeMath
{
    public static int ToFinalGrade(double average) => (int)Math.Round(average, MidpointRounding.AwayFromZero);
}
