using System.Security.Claims;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Authentication;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Authentication;

public class AuthenticationEndpoint : IEndpoint
{
    private const string API_PREFIX = "auth";
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX);

        group.MapPost("login", async (
            [FromBody] LoginReqDto req,
            [FromServices] IAuthenticationService authenticationService) =>
        {
            var result = await authenticationService.Login(req);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("Login")
        .WithDescription("Login to the system")
        .ProducesValidationProblem();

        group.MapPost("verify-email", async (
            [FromBody] RegisterVerifyReqDto req,
            [FromServices] IAuthenticationService authenticationService) =>
        {
            var result = await authenticationService.VerifyEmail(req);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("VerifyEmail")
        .WithDescription("Verify email")
        .ProducesValidationProblem();

        group.MapPost("verify-otp", async (
            [FromBody] VerifyOtpReqDto req,
            [FromServices] IAuthenticationService authenticationService) =>
        {
            var result = await authenticationService.VerifyOtp(req);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("VerifyOtp")
        .WithDescription("Verify OTP")
        .ProducesValidationProblem();

        group.MapPost("logout", async (
            ClaimsPrincipal user,
            [FromServices] IAuthenticationService authenticationService) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Results.Unauthorized();

            var result = await authenticationService.LogOut(userId);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .RequireAuthorization()
        .WithName("Logout")
        .WithDescription("Logout from the system");

        group.MapPost("reset-password-verify", async (
            [FromBody] ResetPasswordVerifyReqDto req,
            [FromServices] IAuthenticationService authenticationService) =>
        {
            var result = await authenticationService.ResetPasswordVerify(req);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("ResetPasswordVerify")
        .WithDescription("Reset password verify")
        .ProducesValidationProblem();

        group.MapPost("reset-password", async (
            [FromBody] ResetPasswordReqDto req,
            [FromServices] IAuthenticationService authenticationService) =>
        {
            var result = await authenticationService.ResetPassword(req);
            return result.Match(
                success => Results.Ok(success),
                error => error.ToProblemDetailsResult()
            );
        })
        .WithName("ResetPassword")
        .WithDescription("Reset password")
        .ProducesValidationProblem();
    }
}
