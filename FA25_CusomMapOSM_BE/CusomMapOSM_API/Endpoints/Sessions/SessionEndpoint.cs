using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Sessions;
using CusomMapOSM_Application.Models.DTOs.Features.Sessions.Request;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Sessions;

public class SessionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sessions")
            .WithTags("Sessions")
            .WithDescription("Session management endpoints for interactive learning");

        // Create Session
        group.MapPost("/", async (
                [FromBody] CreateSessionRequest req,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.CreateSession(req);
                return result.Match(
                    success => Results.Created($"/api/sessions/{success.SessionId}", success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("CreateSession")
            .WithDescription("Create a new interactive session with unique PIN code")
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .Produces(401);

        // Get Session by ID
        group.MapGet("/{sessionId:guid}", async (
                [FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.GetSessionById(sessionId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetSessionById")
            .WithDescription("Get session details by ID")
            .Produces(200)
            .Produces(404);

        // Get Session by Code (PIN)
        group.MapGet("/code/{sessionCode}", async (
                [FromRoute] string sessionCode,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.GetSessionByCode(sessionCode);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetSessionByCode")
            .WithDescription("Get session details by PIN code (for students joining)")
            .Produces(200)
            .Produces(404);

        // Get My Sessions (as Host/Teacher)
        group.MapGet("/my", async (
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.GetMySessionsAsHost();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMySessions")
            .WithDescription("Get all sessions where current user is the host")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401);

        // Start Session
        group.MapPost("/{sessionId:guid}/start", async (
                [FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.StartSession(sessionId);
                return result.Match(
                    success => Results.Ok(new { message = "Session started successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("StartSession")
            .WithDescription("Start a session (only host can do this)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Pause Session
        group.MapPost("/{sessionId:guid}/pause", async (
                [FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.PauseSession(sessionId);
                return result.Match(
                    success => Results.Ok(new { message = "Session paused successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("PauseSession")
            .WithDescription("Pause a running session")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403);

        // Resume Session
        group.MapPost("/{sessionId:guid}/resume", async (
                [FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.ResumeSession(sessionId);
                return result.Match(
                    success => Results.Ok(new { message = "Session resumed successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("ResumeSession")
            .WithDescription("Resume a paused session")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403);

        // End Session
        group.MapPost("/{sessionId:guid}/end", async (
                [FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.EndSession(sessionId);
                return result.Match(
                    success => Results.Ok(new { message = "Session ended successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("EndSession")
            .WithDescription("End a session (marks all participants as left)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403);

        // Delete Session
        group.MapDelete("/{sessionId:guid}", async (
                [FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.DeleteSession(sessionId);
                return result.Match(
                    success => Results.Ok(new { message = "Session deleted successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DeleteSession")
            .WithDescription("Delete a session (only host can do this)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Join Session (Student)
        group.MapPost("/join", async (
                [FromBody] JoinSessionRequest req,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.JoinSession(req);
                return result.Match(
                    success => Results.Created($"/api/sessions/{success.SessionId}/participants/{success.SessionParticipantId}", success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("JoinSession")
            .WithDescription("Join a session using PIN code (students can join as guest)")
            .Produces(201)
            .Produces(400)
            .Produces(404)
            .Produces(409);

        // Leave Session
        group.MapPost("/participants/{participantId:guid}/leave", async (
                [FromRoute] Guid participantId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.LeaveSession(participantId);
                return result.Match(
                    success => Results.Ok(new { message = "Left session successfully" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("LeaveSession")
            .WithDescription("Leave a session")
            .Produces(200)
            .Produces(404);

        // Get Leaderboard
        group.MapGet("/{sessionId:guid}/leaderboard", async ([FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService,
                [FromQuery] int limit = 10) =>
            {
                var result = await sessionService.GetLeaderboard(sessionId, limit);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetLeaderboard")
            .WithDescription("Get session leaderboard (top N participants)")
            .Produces(200)
            .Produces(404);

        // Activate Next Question (Teacher Control)
        group.MapPost("/{sessionId:guid}/questions/next", async (
                [FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.ActivateNextQuestion(sessionId);
                return result.Match(
                    success => Results.Ok(new { message = "Next question activated" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("ActivateNextQuestion")
            .WithDescription("Activate the next question in queue (teacher only)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Skip Current Question (Teacher Control)
        group.MapPost("/{sessionId:guid}/questions/skip", async (
                [FromRoute] Guid sessionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.SkipCurrentQuestion(sessionId);
                return result.Match(
                    success => Results.Ok(new { message = "Question skipped" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("SkipCurrentQuestion")
            .WithDescription("Skip the current active question (teacher only)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Extend Time (Teacher Control)
        group.MapPost("/questions/{sessionQuestionId:guid}/extend", async (
                [FromRoute] Guid sessionQuestionId,
                [FromQuery] int additionalSeconds,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.ExtendTime(sessionQuestionId, additionalSeconds);
                return result.Match(
                    success => Results.Ok(new { message = $"Time extended by {additionalSeconds} seconds" }),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("ExtendTime")
            .WithDescription("Extend time for current question (teacher only, max 120 seconds)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        // Submit Response (Student)
        group.MapPost("/participants/{participantId:guid}/responses", async (
                [FromRoute] Guid participantId,
                [FromBody] SubmitResponseRequest req,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.SubmitResponse(participantId, req);
                return result.Match(
                    success => Results.Created($"/api/sessions/responses/{success.StudentResponseId}", success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("SubmitResponse")
            .WithDescription("Submit an answer to a question (supports all 5 question types)")
            .Produces(201)
            .Produces(400)
            .Produces(404)
            .Produces(409);

        // Get Word Cloud Data (for Word Cloud questions)
        group.MapGet("/questions/{sessionQuestionId:guid}/word-cloud", async (
                [FromRoute] Guid sessionQuestionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.GetWordCloudData(sessionQuestionId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetWordCloudData")
            .WithDescription("Get word cloud analytics for a Word Cloud question")
            .Produces(200)
            .Produces(404);

        // Get Map Pins Data (for Pin on Map questions)
        group.MapGet("/questions/{sessionQuestionId:guid}/map-pins", async (
                [FromRoute] Guid sessionQuestionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.GetMapPinsData(sessionQuestionId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMapPinsData")
            .WithDescription("Get map pins analytics for a Pin on Map question")
            .Produces(200)
            .Produces(404);

        group.MapGet("/questions/{sessionQuestionId:guid}/responses", async (
                [FromRoute] Guid sessionQuestionId,
                [FromServices] ISessionService sessionService) =>
            {
                var result = await sessionService.GetQuestionResponses(sessionQuestionId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetQuestionResponses")
            .WithDescription("Get all student responses for a question with detailed answer content (who answered what)")
            .RequireAuthorization()
            .Produces(200)
            .Produces(404);
    }
}