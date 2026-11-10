using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Web.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Display(Name = "Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "New password")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Display(Name = "Confirm new password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
