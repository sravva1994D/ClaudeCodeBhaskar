using PatientManagement.Web.Security;

namespace PatientManagement.IntegrationTests.Security;

// Test ID follows CLAUDE.md §4.2 and §10. Synthetic password only.
public class AspNetPasswordHasherTests
{
    private const string Password = "Abcdef1!";

    private readonly AspNetPasswordHasher hasher = new();

    [Fact]
    public void NFR_SEC_002_stored_value_is_a_hash_not_the_password()
    {
        var hash = hasher.Hash(Password);

        Assert.NotEqual(Password, hash);
        Assert.DoesNotContain(Password, hash, StringComparison.Ordinal);
    }

    [Fact]
    public void NFR_SEC_002_hash_verifies_with_the_original_password()
    {
        var hash = hasher.Hash(Password);

        Assert.True(hasher.Verify(hash, Password));
    }

    [Fact]
    public void NFR_SEC_002_hash_does_not_verify_with_another_password()
    {
        var hash = hasher.Hash(Password);

        Assert.False(hasher.Verify(hash, "Other-Pass1"));
    }

    [Fact]
    public void NFR_SEC_002_the_same_password_gives_different_hashes()
    {
        Assert.NotEqual(hasher.Hash(Password), hasher.Hash(Password));
    }
}
