using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Services.Blog;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace CusomMapOSM_API.Endpoints;

public class TestEndpoint : IEndpoint
{
    private const string API_PREFIX = "test";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX);

        // Public endpoint - no authorization required
        group.MapGet("/public", () =>
        {
            return Results.Ok(new { message = "This is a public endpoint - no authentication required" });
        })
        .WithName("PublicTest")
        .WithDescription("Public test endpoint - no authentication required")
        .WithTags("Test");

        // Protected endpoint - authorization required
        group.MapGet("/protected", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            return Results.Ok(new
            {
                message = "This is a protected endpoint - authentication required",
                userId = userId,
                email = email,
                authenticated = user.Identity?.IsAuthenticated ?? false
            });
        })
        .RequireAuthorization()
        .WithName("ProtectedTest")
        .WithDescription("Protected test endpoint - authentication required")
        .WithTags("Test");

        // Admin endpoint - specific role required
        group.MapGet("/admin", (ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            return Results.Ok(new
            {
                message = "This is an admin endpoint - admin role required",
                userId = userId,
                email = email,
                authenticated = user.Identity?.IsAuthenticated ?? false
            });
        })
        .RequireAuthorization(policy => policy.RequireRole("Admin"))
        .WithName("AdminTest")
        .WithDescription("Admin test endpoint - admin role required")
        .WithTags("Test");

        // Azure Blob Storage Test Endpoints
        // Upload file test
        group.MapPost("/blob/upload", async (
                IFormFile file,
                [FromServices] IBlogStorageService blogStorageService) =>
            {
                if (file == null || file.Length == 0)
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        error = "No file provided",
                        message = "Please provide a valid file to upload"
                    });
                }

                try
                {
                    using var stream = file.OpenReadStream();
                    var blobUrl = await blogStorageService.UploadFileAsync(file.FileName, stream);

                    return Results.Ok(new
                    {
                        success = true,
                        message = "File uploaded successfully to Azure Blob Storage",
                        fileName = file.FileName,
                        fileSize = file.Length,
                        contentType = file.ContentType,
                        blobUrl = blobUrl
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Upload failed",
                        detail: ex.Message,
                        statusCode: 500);
                }
            })
            .WithName("TestBlobUpload")
            .WithDescription("Test endpoint for uploading file to Azure Blob Storage")
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200)
            .Produces(400)
            .Produces(500)
            .WithTags("Test");

        // Download file test (returns URL)
        group.MapGet("/blob/download/{fileName}", async (
                string fileName,
                [FromServices] IBlogStorageService blogStorageService) =>
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        error = "Invalid file name",
                        message = "Please provide a valid file name"
                    });
                }

                try
                {
                    var blobUrl = await blogStorageService.DownloadFileAsync(fileName);

                    return Results.Ok(new
                    {
                        success = true,
                        message = "File download URL generated successfully",
                        fileName = fileName,
                        blobUrl = blobUrl,
                        note = "Use this URL to download the file"
                    });
                }
                catch (FileNotFoundException ex)
                {
                    return Results.NotFound(new
                    {
                        success = false,
                        error = "File not found",
                        message = ex.Message,
                        fileName = fileName
                    });
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Download failed",
                        detail: ex.Message,
                        statusCode: 500);
                }
            })
            .WithName("TestBlobDownload")
            .WithDescription("Test endpoint for getting download URL from Azure Blob Storage")
            .Produces(200)
            .Produces(404)
            .Produces(500)
            .WithTags("Test");

        // Delete file test
        group.MapDelete("/blob/delete/{fileName}", async (
                string fileName,
                [FromServices] IBlogStorageService blogStorageService) =>
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        error = "Invalid file name",
                        message = "Please provide a valid file name"
                    });
                }

                try
                {
                    var deleted = await blogStorageService.DeleteFileAsync(fileName);

                    if (deleted)
                    {
                        return Results.Ok(new
                        {
                            success = true,
                            message = "File deleted successfully from Azure Blob Storage",
                            fileName = fileName
                        });
                    }
                    else
                    {
                        return Results.NotFound(new
                        {
                            success = false,
                            error = "File not found",
                            message = $"File '{fileName}' was not found in Azure Blob Storage",
                            fileName = fileName
                        });
                    }
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "Delete failed",
                        detail: ex.Message,
                        statusCode: 500);
                }
            })
            .WithName("TestBlobDelete")
            .WithDescription("Test endpoint for deleting file from Azure Blob Storage")
            .Produces(200)
            .Produces(404)
            .Produces(500)
            .WithTags("Test");
    }
}
