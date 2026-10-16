using System.Globalization;
using System.Text;

namespace School.Services.Reports;

public static class TermReportCsv
{
    private const char Separator = ';';
    private const string LineBreak = "\r\n";

    private static readonly CultureInfo NumberCulture = CultureInfo.GetCultureInfo("ru-RU");
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    private static readonly Dictionary<char, string> Transliteration = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['е'] = "e", ['ё'] = "e",
        ['ж'] = "zh", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k", ['л'] = "l", ['м'] = "m",
        ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u",
        ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts", ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "shch", ['ъ'] = "",
        ['ы'] = "y", ['ь'] = "", ['э'] = "e", ['ю'] = "yu", ['я'] = "ya",
        ['ă'] = "a", ['â'] = "a", ['î'] = "i", ['ș'] = "s", ['ş'] = "s", ['ț'] = "t", ['ţ'] = "t"
    };

    public static byte[] Build(TermReport report)
    {
        var text = new StringBuilder();

        AppendLine(text, new[] { "Ученик" }
            .Concat(report.Subjects.Select(s => s.Name))
            .Concat(new[] { "Средний", "Итоговая" })
            .Select(EscapeText));

        foreach (var row in report.Rows)
        {
            AppendLine(text, new[] { EscapeText(row.StudentName) }
                .Concat(report.Subjects.Select(s => FormatAverage(row.SubjectAverages[s.Id])))
                .Concat(new[] { FormatAverage(row.Average), row.FinalGrade?.ToString(NumberCulture) ?? string.Empty }));
        }

        AppendLine(text, new[] { EscapeText("Средний по классу") }
            .Concat(report.Subjects.Select(s => FormatAverage(report.SubjectAverages[s.Id])))
            .Concat(new[] { string.Empty, string.Empty }));

        var preamble = Utf8WithBom.GetPreamble();
        var body = Utf8WithBom.GetBytes(text.ToString());
        var result = new byte[preamble.Length + body.Length];
        preamble.CopyTo(result, 0);
        body.CopyTo(result, preamble.Length);
        return result;
    }

    public static string BuildFileName(TermReport report)
    {
        return $"tabel-{ToSlug(report.SchoolClass.Name)}-{ToSlug(report.Term.Name)}.csv";
    }

    private static void AppendLine(StringBuilder text, IEnumerable<string> fields)
    {
        text.Append(string.Join(Separator, fields));
        text.Append(LineBreak);
    }

    private static string FormatAverage(double? value) =>
        value.HasValue ? value.Value.ToString("F2", NumberCulture) : string.Empty;

    private static string EscapeText(string value)
    {
        if (value.Length > 0 && value[0] is '=' or '+' or '-' or '@')
        {
            value = "'" + value;
        }

        if (value.IndexOfAny(new[] { Separator, '"', '\r', '\n' }) >= 0)
        {
            value = "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        return value;
    }

    private static string ToSlug(string value)
    {
        var slug = new StringBuilder();
        foreach (var symbol in value)
        {
            var lower = char.ToLowerInvariant(symbol);
            string part;
            if (Transliteration.TryGetValue(lower, out var latin))
            {
                part = char.IsUpper(symbol) && latin.Length > 0
                    ? char.ToUpperInvariant(latin[0]) + latin[1..]
                    : latin;
            }
            else if (char.IsAsciiLetterOrDigit(symbol))
            {
                part = symbol.ToString();
            }
            else
            {
                part = "-";
            }

            slug.Append(part);
        }

        var collapsed = string.Join('-', slug.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries));
        return collapsed.Length == 0 ? "report" : collapsed;
    }
}
