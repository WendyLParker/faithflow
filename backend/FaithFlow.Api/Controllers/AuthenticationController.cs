using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using FaithFlow.Backend.Models.Auth;

namespace FaithFlow.Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAmazonCognitoIdentityProvider _cognito;
    private readonly CognitoSettings _settings;

    public AuthController(
        IAmazonCognitoIdentityProvider cognito,
        IOptions<CognitoSettings> settings)
    {
        _cognito = cognito;
        _settings = settings.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var signUpRequest = new SignUpRequest
        {
            ClientId = _settings.ClientId,
            Username = request.Email,
            Password = request.Password,
            UserAttributes = new List<AttributeType>
            {
                new() { Name = "email", Value = request.Email }
            }
        };

        try
        {
            var response = await _cognito.SignUpAsync(signUpRequest);

            return Ok(new
            {
                Message = "Registration successful! Please check your email for the confirmation code.",
                UserSub = response.UserSub
            });
        }
        catch (UsernameExistsException)
        {
            return BadRequest(new { Error = "An account with this email already exists." });
        }
        catch (InvalidPasswordException ex)
        {
            return BadRequest(new { Error = "Password does not meet requirements." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var confirmRequest = new ConfirmSignUpRequest
        {
            ClientId = _settings.ClientId,
            Username = request.Username,
            ConfirmationCode = request.ConfirmationCode
        };

        try
        {
            await _cognito.ConfirmSignUpAsync(confirmRequest);
            return Ok(new { Message = "Account confirmed successfully. You can now log in." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var authRequest = new AdminInitiateAuthRequest
        {
            UserPoolId = _settings.UserPoolId,
            ClientId = _settings.ClientId,
            AuthFlow = AuthFlowType.ADMIN_USER_PASSWORD_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                { "USERNAME", request.Username },
                { "PASSWORD", request.Password }
            }
        };

        try
        {
            var response = await _cognito.AdminInitiateAuthAsync(authRequest);

            if (response.AuthenticationResult != null)
            {
                return Ok(new AuthResponse
                {
                    IdToken = response.AuthenticationResult.IdToken,
                    AccessToken = response.AuthenticationResult.AccessToken,
                    RefreshToken = response.AuthenticationResult.RefreshToken,
                    ExpiresIn = response.AuthenticationResult.ExpiresIn.Value
                });
            }

            return BadRequest(new { Error = "Authentication failed", Challenge = response.ChallengeName?.ToString() });
        }
        catch (NotAuthorizedException)
        {
            return Unauthorized(new { Error = "Invalid username or password." });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Error = ex.Message });
        }
    }
}