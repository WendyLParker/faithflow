using System.Security.Claims;

namespace FaithFlow.Backend.Common;

public static class ClaimsHelper
{
    public static string GetUserId(ClaimsPrincipal user) =>
        user.FindFirst("sub")?.Value
        ?? user.FindFirst("cognito:username")?.Value
        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? "unknown-user";

    /// <summary>
    /// Cognito puts <c>email</c> on the ID token, not the access token.
    /// The access token may expose the sign-in email via <c>username</c> when email is the username.
    /// </summary>
    public static string? GetEmail(ClaimsPrincipal user)
    {
        foreach (var claim in new[] { "email", ClaimTypes.Email, "preferred_username" })
        {
            var value = user.FindFirst(claim)?.Value;
            if (!string.IsNullOrWhiteSpace(value) && value.Contains('@'))
                return value;
        }

        foreach (var claim in new[] { "username", "cognito:username" })
        {
            var value = user.FindFirst(claim)?.Value;
            if (!string.IsNullOrWhiteSpace(value) && value.Contains('@'))
                return value;
        }

        return null;
    }

    public static string? DefaultDisplayNameFromEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        var at = email.IndexOf('@');
        if (at <= 0) return null;

        return email[..at].Trim();
    }
}
