using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.QuestionBanks;
using CusomMapOSM_Application.Models.DTOs.Features.QuestionBanks.Request;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.QuestionBanks;

public class QuestionBankEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/question-banks")
            .WithTags("Question Banks")
            .WithDescription("Question bank and question management endpoints");

        // Create Question Bank
        group.MapPost("/", async (
                [FromBody] CreateQuestionBankRequest req,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.CreateQuestionBank(req);
                return result.Match(
                    success => Results.Created($"/api/question-banks/{success.QuestionBankId}", success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("CreateQuestionBank")
            .WithDescription("Create a new question bank")
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .Produces(401);

        // Get Question Bank by ID
        group.MapGet("/{questionBankId:guid}", async (
                [FromRoute] Guid questionBankId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.GetQuestionBankById(questionBankId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetQuestionBankById")
            .WithDescription("Get question bank details by ID")
            .Produces(200)
            .Produces(404);

        // Get My Question Banks
        group.MapGet("/my", async (
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.GetMyQuestionBanks();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMyQuestionBanks")
            .WithDescription("Get all question banks owned by current user")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401);

        // Get Public Question Banks
        group.MapGet("/public", async (
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.GetPublicQuestionBanks();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetPublicQuestionBanks")
            .WithDescription("Get all public question banks")
            .Produces(200);

        // Get My Question Banks by Organization
        group.MapGet("/organization/{orgId:guid}/my", async (
                [FromRoute] Guid orgId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.GetMyQuestionBanksByOrganization(orgId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMyQuestionBanksByOrganization")
            .WithDescription("Get question banks owned by current user within a specific organization")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401);

        // Get Public Question Banks by Organization
        group.MapGet("/organization/{orgId:guid}/public", async (
                [FromRoute] Guid orgId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.GetPublicQuestionBanksByOrganization(orgId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetPublicQuestionBanksByOrganization")
            .WithDescription("Get public question banks within a specific organization")
            .Produces(200);

        // Duplicate Question Bank
        group.MapPost("/{questionBankId:guid}/duplicate", async (
                [FromRoute] Guid questionBankId,
                [FromQuery] Guid targetWorkspaceId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.DuplicateQuestionBank(questionBankId, targetWorkspaceId);
                return result.Match(
                    success => Results.Created($"/api/question-banks/{success.QuestionBankId}", success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DuplicateQuestionBank")
            .WithDescription("Create a copy of a question bank with all its questions into a target workspace")
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(404);

        group.MapPut("/{questionBankId:guid}", async (
                [FromRoute] Guid questionBankId,
                [FromBody] UpdateQuestionBankRequest req,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.UpdateQuestionBank(questionBankId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UpdateQuestionBank")
            .WithDescription("Update a question bank")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Delete Question Bank
        group.MapDelete("/{questionBankId:guid}", async (
                [FromRoute] Guid questionBankId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.DeleteQuestionBank(questionBankId);
                return result.Match(
                    success => Results.Ok(new { message = "Question bank deleted successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DeleteQuestionBank")
            .WithDescription("Delete a question bank (only owner can do this)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Add tags to question bank
        group.MapPost("/{questionBankId:guid}/tags", async (
                [FromRoute] Guid questionBankId,
                [FromBody] UpdateQuestionBankTagsRequest req,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.AddQuestionBankTags(questionBankId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("AddQuestionBankTags")
            .WithDescription("Append tags to an existing question bank")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Replace question bank tags
        group.MapPut("/{questionBankId:guid}/tags", async (
                [FromRoute] Guid questionBankId,
                [FromBody] UpdateQuestionBankTagsRequest req,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.ReplaceQuestionBankTags(questionBankId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("ReplaceQuestionBankTags")
            .WithDescription("Overwrite the entire tag list of a question bank")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Create Question
        group.MapPost("/{questionBankId:guid}/questions", async (
                [FromRoute] Guid questionBankId,
                [FromBody] CreateQuestionRequest req,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                // Override questionBankId from route
                req.QuestionBankId = questionBankId;

                var result = await questionBankService.CreateQuestion(req);
                return result.Match(
                    success => Results.Created($"/api/questions/{success}", new { questionId = success }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("CreateQuestion")
            .WithDescription("Create a new question in a question bank (supports all 5 question types)")
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .Produces(401)
            .Produces(403);

        // Get Questions by Question Bank ID
        group.MapGet("/{questionBankId:guid}/questions", async (
                [FromRoute] Guid questionBankId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.GetQuestionsByQuestionBankId(questionBankId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetQuestionsByQuestionBankId")
            .WithDescription("Get all questions in a question bank")
            .Produces(200)
            .Produces(404);

        // Update Question
        group.MapPut("/questions/{questionId:guid}", async (
                [FromRoute] Guid questionId,
                [FromBody] UpdateQuestionRequest req,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                req.QuestionId = questionId;
                var result = await questionBankService.UpdateQuestion(req);
                return result.Match(
                    success => Results.Ok(new { questionId = success }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UpdateQuestion")
            .WithDescription("Update an existing question (supports all 5 question types)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Delete Question
        group.MapDelete("/questions/{questionId:guid}", async (
                [FromRoute] Guid questionId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.DeleteQuestion(questionId);
                return result.Match(
                    success => Results.Ok(new { message = "Question deleted successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DeleteQuestion")
            .WithDescription("Delete a question (only owner can do this)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);
        
        // Attach Question Bank to Session
        group.MapPost("/{questionBankId:guid}/sessions", async (
                [FromRoute] Guid questionBankId,
                [FromBody] AttachQuestionBankToSessionRequest req,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.AttachQuestionBankToSession(questionBankId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("AttachQuestionBankToSession")
            .WithDescription("Attach a question bank to a session")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Detach Question Bank from Session
        group.MapDelete("/{questionBankId:guid}/sessions/{sessionId:guid}", async (
                [FromRoute] Guid questionBankId,
                [FromRoute] Guid sessionId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.DetachQuestionBankFromSession(sessionId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DetachQuestionBankFromSession")
            .WithDescription("Detach a question bank from a session")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Get Question Banks by Session ID
        group.MapGet("/sessions/{sessionId:guid}/question-banks", async (
                [FromRoute] Guid sessionId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.GetQuestionBanksBySessionId(sessionId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetQuestionBanksBySessionId")
            .WithDescription("Get all question banks attached to a session")
            .Produces(200)
            .Produces(404);

        // Get Sessions by Question Bank ID
        group.MapGet("/{questionBankId:guid}/sessions", async (
                [FromRoute] Guid questionBankId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.GetSessionsByQuestionBankId(questionBankId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetSessionsByQuestionBankId")
            .WithDescription("Get all sessions attached to a question bank")
            .Produces(200)
            .Produces(404);

        // Upload Question Image
        group.MapPost("/questions/upload-image", async (
                IFormFile file,
                [FromQuery] Guid? questionBankId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.UploadQuestionImage(file, questionBankId);
                return result.Match(
                    url => Results.Ok(new { imageUrl = url }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UploadQuestionImage")
            .WithDescription("Upload an image for question")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200)
            .Produces(400)
            .Produces(500);

        // Upload Question Audio
        group.MapPost("/questions/upload-audio", async (
                IFormFile file,
                [FromQuery] Guid? questionBankId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.UploadQuestionAudio(file, questionBankId);
                return result.Match(
                    url => Results.Ok(new { audioUrl = url }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UploadQuestionAudio")
            .WithDescription("Upload an audio file for question")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200)
            .Produces(400)
            .Produces(500);

        // Upload Option Image
        group.MapPost("/questions/options/upload-image", async (
                IFormFile file,
                [FromQuery] Guid? questionBankId,
                [FromServices] IQuestionBankService questionBankService) =>
            {
                var result = await questionBankService.UploadOptionImage(file, questionBankId);
                return result.Match(
                    url => Results.Ok(new { imageUrl = url }),
                    error => error.ToProblemDetailsResult()
                );
            })
            .WithName("UploadOptionImage")
            .WithDescription("Upload an image for question option")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces(200)
            .Produces(400)
            .Produces(500);
    }
}