using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace PatientManagement.IntegrationTests.Security;

// Every POST action must check the anti-forgery token (CLAUDE.md §9). The check reads the code, so a new
// POST action without the attribute fails here even before anyone tests it in a browser (NFR-SEC-001).
public class AntiForgeryCoverageTests
{
    [Fact]
    public void NFR_SEC_001_every_post_action_requires_an_anti_forgery_token()
    {
        var controllers = typeof(Program).Assembly.GetTypes()
            .Where(type => typeof(Controller).IsAssignableFrom(type) && !type.IsAbstract)
            .ToList();

        var postActions = controllers
            .SelectMany(controller => controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttribute<HttpPostAttribute>() is not null)
                .Select(method => (Controller: controller, Method: method)))
            .ToList();

        // Guard: if the scan finds no POST actions, the assertion below would pass for the wrong reason.
        Assert.NotEmpty(postActions);

        var unprotected = postActions
            .Where(action => action.Method.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() is null
                             && action.Controller.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() is null)
            .Select(action => $"{action.Controller.Name}.{action.Method.Name}")
            .ToList();

        Assert.Empty(unprotected);
    }
}
