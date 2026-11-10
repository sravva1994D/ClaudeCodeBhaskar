using PatientManagement.Business.Interfaces;

namespace PatientManagement.Web.Setup;

// The console command that creates the single account (DEC-018; plan Module 1, step 8).
// Run once per machine:  dotnet run --project src/PatientManagement.Web -- seed-user
// Nothing is read from code, configuration or the repository, and the password is never written out.
public sealed class SeedUserRunner(IAuthService auth, IConsoleIO console)
{
    public const string CommandName = "seed-user";

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        console.WriteLine("Create the single account. The password is typed without being shown and is not stored.");

        var username = console.ReadLine("Username: ")?.Trim() ?? string.Empty;
        var email = console.ReadLine("Email: ")?.Trim() ?? string.Empty;
        var password = console.ReadSecret("Password: ");
        var confirmation = console.ReadSecret("Confirm password: ");

        if (password != confirmation)
        {
            console.WriteLine("The passwords do not match. Nothing was changed.");
            return 1;
        }

        var result = await auth.CreateInitialAccountAsync(username, email, password, cancellationToken);

        if (!result.IsSuccess)
        {
            foreach (var error in result.Errors)
            {
                console.WriteLine(error.Message);
            }

            return 1;
        }

        console.WriteLine($"Account '{result.Value!.Username}' created. You can now sign in.");
        return 0;
    }
}
