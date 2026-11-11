using PatientManagement.Business.DTOs;
using PatientManagement.Business.Validators;

namespace PatientManagement.UnitTests.Validators;

// Unit tests for the patient rules (PDR-01, PDR-22, D-M2-3, D4). Synthetic values only.
// Test IDs follow CLAUDE.md §4.2 and §10.
public class PatientValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 11);

    private static PatientInput Valid() => new(
        FullName: "Test Anna Example",
        DateOfBirth: new DateOnly(1990, 5, 1),
        Gender: "Female",
        Phone: "5550101",
        Email: "anna@example.test",
        Address: "Test address");

    private static IReadOnlyList<string> CodesFor(PatientInput input) =>
        PatientValidator.Validate(input, Today).Select(message => message.Code).ToArray();

    [Fact]
    public void FR_PAT_001_accepts_a_complete_valid_patient()
    {
        Assert.Empty(PatientValidator.Validate(Valid(), Today));
    }

    [Fact]
    public void FR_PAT_001_missing_full_name_is_an_error()
    {
        Assert.Equal(["Patient.FullName.Required"], CodesFor(Valid() with { FullName = "   " }));
    }

    [Fact]
    public void FR_PAT_001_missing_date_of_birth_is_an_error()
    {
        Assert.Equal(["Patient.DateOfBirth.Required"], CodesFor(Valid() with { DateOfBirth = null }));
    }

    [Fact]
    public void FR_PAT_001_missing_gender_is_an_error()
    {
        Assert.Equal(["Patient.Gender.Required"], CodesFor(Valid() with { Gender = null }));
    }

    [Fact]
    public void FR_PAT_001_missing_phone_is_an_error()
    {
        Assert.Equal(["Patient.Phone.Required"], CodesFor(Valid() with { Phone = "" }));
    }

    [Fact]
    public void FR_PAT_001_missing_email_is_an_error()
    {
        Assert.Equal(["Patient.Email.Required"], CodesFor(Valid() with { Email = " " }));
    }

    [Fact]
    public void FR_PAT_001_missing_address_is_an_error()
    {
        Assert.Equal(["Patient.Address.Required"], CodesFor(Valid() with { Address = null }));
    }

    [Fact]
    public void FR_PAT_001_each_missing_required_field_gives_its_own_error()
    {
        var empty = new PatientInput(null, null, null, null, null, null);

        Assert.Equal(
            ["Patient.FullName.Required", "Patient.DateOfBirth.Required", "Patient.Gender.Required",
             "Patient.Phone.Required", "Patient.Email.Required", "Patient.Address.Required"],
            CodesFor(empty));
    }

    [Fact]
    public void FR_PAT_001_date_of_birth_in_the_future_is_an_error()
    {
        Assert.Equal(["Patient.DateOfBirth.Future"],
            CodesFor(Valid() with { DateOfBirth = Today.AddDays(1) }));
    }

    [Fact]
    public void FR_PAT_001_date_of_birth_today_is_accepted()
    {
        Assert.Empty(PatientValidator.Validate(Valid() with { DateOfBirth = Today }, Today));
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("male")]
    [InlineData("")]
    public void FR_PAT_001_gender_must_be_one_of_the_allowed_values(string gender)
    {
        var codes = CodesFor(Valid() with { Gender = gender });

        Assert.Contains(codes, code => code is "Patient.Gender.Invalid" or "Patient.Gender.Required");
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("Other")]
    public void FR_PAT_001_each_allowed_gender_is_accepted(string gender)
    {
        Assert.Empty(PatientValidator.Validate(Valid() with { Gender = gender }, Today));
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("1234567890123456")]
    public void FR_PAT_001_phone_with_six_or_sixteen_digits_is_refused(string phone)
    {
        Assert.Equal(["Patient.Phone.Invalid"], CodesFor(Valid() with { Phone = phone }));
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("123456789012345")]
    public void FR_PAT_001_phone_with_seven_or_fifteen_digits_is_accepted(string phone)
    {
        Assert.Empty(PatientValidator.Validate(Valid() with { Phone = phone }, Today));
    }

    [Theory]
    [InlineData("+91 98765-43210")]
    [InlineData("98765 43210")]
    [InlineData("+1-555-0101")]
    public void FR_PAT_001_spaces_hyphens_and_a_leading_plus_are_accepted(string phone)
    {
        Assert.Empty(PatientValidator.Validate(Valid() with { Phone = phone }, Today));
    }

    [Theory]
    [InlineData("98765abc10")]
    [InlineData("98+765432")]
    [InlineData("(555) 0101")]
    public void FR_PAT_001_other_characters_or_a_plus_not_at_the_start_are_refused(string phone)
    {
        Assert.Equal(["Patient.Phone.Invalid"], CodesFor(Valid() with { Phone = phone }));
    }

    [Theory]
    [InlineData("anna@example.test")]
    [InlineData("first.last@sub.example.test")]
    public void FR_PAT_001_standard_email_is_accepted(string email)
    {
        Assert.Empty(PatientValidator.Validate(Valid() with { Email = email }, Today));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("anna@")]
    [InlineData("Anna <anna@example.test>")]
    public void FR_PAT_001_invalid_email_is_refused(string email)
    {
        Assert.Equal(["Patient.Email.Invalid"], CodesFor(Valid() with { Email = email }));
    }

    [Fact]
    public void FR_PAT_001_full_name_at_the_limit_is_accepted_and_one_over_is_refused()
    {
        var atLimit = new string('a', PatientValidator.FullNameMaxLength);
        var overLimit = new string('a', PatientValidator.FullNameMaxLength + 1);

        Assert.Empty(PatientValidator.Validate(Valid() with { FullName = atLimit }, Today));
        Assert.Equal(["Patient.FullName.TooLong"], CodesFor(Valid() with { FullName = overLimit }));
    }

    [Fact]
    public void FR_PAT_001_address_at_the_limit_is_accepted_and_one_over_is_refused()
    {
        var atLimit = new string('a', PatientValidator.AddressMaxLength);
        var overLimit = new string('a', PatientValidator.AddressMaxLength + 1);

        Assert.Empty(PatientValidator.Validate(Valid() with { Address = atLimit }, Today));
        Assert.Equal(["Patient.Address.TooLong"], CodesFor(Valid() with { Address = overLimit }));
    }

    [Fact]
    public void FR_PAT_001_email_over_the_limit_is_refused()
    {
        var overLimit = new string('a', PatientValidator.EmailMaxLength - 11) + "@example.test";

        Assert.Contains("Patient.Email.TooLong", CodesFor(Valid() with { Email = overLimit }));
    }

    [Fact]
    public void FR_PAT_001_phone_over_the_column_limit_is_refused()
    {
        Assert.Equal(["Patient.Phone.TooLong"],
            CodesFor(Valid() with { Phone = "+" + new string('1', PatientValidator.PhoneMaxLength) }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void FR_PAT_004_empty_search_term_is_an_error(string? term)
    {
        var errors = PatientValidator.ValidateSearchTerm(term);

        Assert.Equal(["Search.Empty"], errors.Select(error => error.Code));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("555")]
    public void FR_PAT_004_any_non_empty_search_term_is_accepted(string term)
    {
        Assert.Empty(PatientValidator.ValidateSearchTerm(term));
    }
}
