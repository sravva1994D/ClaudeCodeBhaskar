namespace PatientManagement.Web.Setup;

// Console input and output for the seed command. An interface so the command can be tested without a real console.
public interface IConsoleIO
{
    string? ReadLine(string prompt);

    // Reads a secret. Nothing is echoed on screen.
    string ReadSecret(string prompt);

    void WriteLine(string text);
}
