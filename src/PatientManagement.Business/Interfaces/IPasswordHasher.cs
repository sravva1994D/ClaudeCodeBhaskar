namespace PatientManagement.Business.Interfaces;

// The Business layer hashes and verifies passwords through this interface, so it holds no ASP.NET types
// (Documentation/04-Implementation/02-Developer-Guide.md §2). The implementation lives in the Web project.
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string hash, string password);
}
