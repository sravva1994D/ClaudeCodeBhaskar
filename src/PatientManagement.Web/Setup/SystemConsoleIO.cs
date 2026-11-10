using System.Text;

namespace PatientManagement.Web.Setup;

// Reads from the real console. The secret is read key by key and never echoed or written anywhere.
public sealed class SystemConsoleIO : IConsoleIO
{
    public string? ReadLine(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    public string ReadSecret(string prompt)
    {
        Console.Write(prompt);
        var secret = new StringBuilder();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return secret.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (secret.Length > 0)
                {
                    secret.Length--;
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                secret.Append(key.KeyChar);
            }
        }
    }

    public void WriteLine(string text) => Console.WriteLine(text);
}
