using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using PatientManagement.Business.Validators;

namespace PatientManagement.Web.ViewModels.Patients;

// The register and edit form (SCR-05). Fields match the service's field names so errors land next to the right input.
// Text fields are nullable on purpose: a non-nullable string makes MVC add its own "required" message before the
// service runs, which would replace the service's messages (Documentation/02-Planning/13-Validation-Strategy.md §5).
public class PatientFormViewModel
{
    public int? Id { get; set; }

    public string? PatientCode { get; set; }

    [Display(Name = "Full name")]
    public string? FullName { get; set; }

    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    // Set only by the "Confirm and save" button, after a duplicate warning (Documentation/02-Planning/13-Validation-Strategy.md §4).
    public bool ConfirmWarnings { get; set; }

    // Duplicate warnings shown before the user confirms.
    public List<string> Warnings { get; set; } = [];

    public bool IsEdit => Id.HasValue;

    public IEnumerable<SelectListItem> GenderItems =>
        new[] { new SelectListItem("Select...", string.Empty) }
            .Concat(PatientValidator.AllowedGenders.Select(gender => new SelectListItem(gender, gender)));
}
