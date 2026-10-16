using System.Text;
using School.Domain;
using School.Services.Reports;

namespace School.Tests;

public class TermReportCsvTests
{
    private const int MathId = 10;
    private const int HistoryId = 20;

    private static TermReport MakeReport(string studentName = "Иванов Иван") => new()
    {
        Term = new AcademicTerm { Id = 1, Name = "I четверть", StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2026, 10, 31) },
        SchoolClass = new SchoolClass { Id = 5, Name = "5-А" },
        Subjects = new List<Subject>
        {
            new() { Id = HistoryId, Name = "История" },
            new() { Id = MathId, Name = "Математика" }
        },
        Rows = new List<TermReportRow>
        {
            new()
            {
                StudentName = studentName,
                SubjectAverages = new Dictionary<int, double?> { [HistoryId] = null, [MathId] = 8.5 },
                Average = 8.5,
                FinalGrade = 9
            }
        },
        SubjectAverages = new Dictionary<int, double?> { [HistoryId] = null, [MathId] = 22.0 / 3 },
        HasGrades = true
    };

    private static string[] ReadLines(byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        return text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
    }

    [Fact]
    public void Build_StartsWithUtf8Bom()
    {
        var bytes = TermReportCsv.Build(MakeReport());

        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());
    }

    [Fact]
    public void Build_UsesSemicolonAndDecimalComma()
    {
        var lines = ReadLines(TermReportCsv.Build(MakeReport()));

        Assert.Equal(3, lines.Length);
        Assert.Equal("Ученик;История;Математика;Средний;Итоговая", lines[0]);
        Assert.Equal("Иванов Иван;;8,50;8,50;9", lines[1]);
        Assert.Equal("Средний по классу;;7,33;;", lines[2]);
    }

    [Fact]
    public void Build_TextWithSeparatorOrQuotes_IsQuoted()
    {
        var lines = ReadLines(TermReportCsv.Build(MakeReport("Иванов; \"Ваня\"")));

        Assert.StartsWith("\"Иванов; \"\"Ваня\"\"\";", lines[1]);
    }

    [Fact]
    public void Build_TextLookingLikeFormula_IsPrefixed()
    {
        var lines = ReadLines(TermReportCsv.Build(MakeReport("=HYPERLINK(1)")));

        Assert.StartsWith("'=HYPERLINK(1);", lines[1]);
    }

    [Fact]
    public void BuildFileName_TransliteratesClassAndTerm()
    {
        var fileName = TermReportCsv.BuildFileName(MakeReport());

        Assert.Equal("tabel-5-A-I-chetvert.csv", fileName);
    }
}
