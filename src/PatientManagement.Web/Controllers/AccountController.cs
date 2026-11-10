using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Business.Common;
using PatientManagement.Business.DTOs;
using PatientManagement.Business.Interfaces;
using PatientManagement.Web.ViewModels.Account;

namespace PatientManagement.Web.Controllers;

// Sign-in, forgot-password and sign-out (SCR-01, SCR-02; Documentation/02-Planning/09-Controller-Design.md rows 2 to 6).
// The controller holds no rules. Every rule is in AuthService.
[AllowAnonymous]
public class AccountController(IAuthService auth) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        var result = await auth.SignInAsync(model.Username, model.Password, cancellationToken);

        if (!result.IsSuccess)
        {
            AddErrors(result);
            model.Password = string.Empty;
            return View(model);
        }

        await SignInCookieAsync(result.Value!);

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken cancellationToken)
    {
        var result = await auth.ResetPasswordAsync(
            model.Email, model.NewPassword, model.ConfirmPassword, cancellationToken);

        if (!result.IsSuccess)
        {
            AddErrors(result);
            model.NewPassword = string.Empty;
            model.ConfirmPassword = string.Empty;
            return View(model);
        }

        // The BRD signs the user in after a reset (Security; DEC-009).
        await SignInCookieAsync(result.Value!);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(nameof(Login));
    }

    private async Task SignInCookieAsync(SignedInUser user)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            ],
            CookieAuthenticationDefaults.AuthenticationScheme);

        // Persistent, so the 24-hour idle timeout (sliding) applies across browser restarts (PDR-20).
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, AllowRefresh = true });
    }

    private void AddErrors(Result<SignedInUser> result)
    {
        foreach (var error in result.Errors)
        {
            // Errors with a field name go next to that field; the rest appear in the summary.
            var key = error.Field ?? string.Empty;
            ModelState.AddModelError(key, error.Message);
        }
    }

    // Only a local return address is followed, so the login page cannot redirect to another site.
    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home");
}
