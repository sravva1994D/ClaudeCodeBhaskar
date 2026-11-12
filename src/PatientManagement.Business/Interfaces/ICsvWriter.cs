namespace PatientManagement.Business.Interfaces;

// Writes rows as CSV bytes (PDR-17, DEC-020): a header row, UTF-8 with a byte-order mark, CRLF line ends, and values
// with a comma, a quote or a line break in quotes. The layout is set here; the content is set by the export service.
public interface ICsvWriter
{
    byte[] Write(IReadOnlyList<string> header, IReadOnlyList<IReadOnlyList<string>> rows);
}
