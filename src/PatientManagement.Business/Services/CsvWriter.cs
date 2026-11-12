using System.Text;
using PatientManagement.Business.Interfaces;

namespace PatientManagement.Business.Services;

// RFC 4180 layout with a byte-order mark so spreadsheet tools read names correctly (PDR-17).
public sealed class CsvWriter : ICsvWriter
{
    private const string LineEnd = "\r\n";

    public byte[] Write(IReadOnlyList<string> header, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var text = new StringBuilder();
        AppendRow(text, header);
        foreach (var row in rows)
        {
            AppendRow(text, row);
        }

        var preamble = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetPreamble();
        var body = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(text.ToString());

        return [.. preamble, .. body];
    }

    private static void AppendRow(StringBuilder text, IReadOnlyList<string> values)
    {
        text.Append(string.Join(',', values.Select(Quote)));
        text.Append(LineEnd);
    }

    // A value is quoted when it holds a comma, a quote or a line break. Inner quotes are doubled.
    private static string Quote(string value) =>
        value.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
}
