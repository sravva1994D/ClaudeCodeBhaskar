using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Web.ViewModels.Account;

public class LoginViewModel
{
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
