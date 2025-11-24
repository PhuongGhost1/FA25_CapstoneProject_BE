using CusomMapOSM_Application.Interfaces.Features.Groups;
using CusomMapOSM_Application.Models.DTOs.Features.Groups.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace CusomMapOSM_Infrastructure.Hubs;

public class GroupCollaborationHub : Hub
{
    private readonly IGroupCollaborationService _groupService;

    public GroupCollaborationHub(IGroupCollaborationService groupService)
    {
        _groupService = groupService;
    }

    // Join a group's collaboration room
    public async Task JoinGroup(Guid groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"group_{groupId}");
    }

    // Leave a group's collaboration room
    public async Task LeaveGroup(Guid groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"group_{groupId}");
    }

    // Join session to receive group creation notifications
    public async Task JoinSession(Guid sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
    }

    // Leave session
    public async Task LeaveSession(Guid sessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
    }

    // Teacher creates groups (broadcast to session)
    public async Task CreateGroup(CreateGroupRequest request)
    {
        var result = await _groupService.CreateGroup(request);
        
        await result.Match(
            async group =>
            {
                // Broadcast new group to all session participants
                await Clients.Group($"session_{request.SessionId}")
                    .SendAsync("GroupCreated", group);
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            }
        );
    }

    // Group submits work
    public async Task SubmitGroupWork(SubmitGroupWorkRequest request)
    {
        var result = await _groupService.SubmitGroupWork(request);
        
        await result.Match(
            async submission =>
            {
                // Notify group members
                await Clients.Group($"group_{request.GroupId}")
                    .SendAsync("WorkSubmitted", submission);

                // Notify teacher
                await Clients.Caller.SendAsync("SubmissionConfirmed", submission);
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            }
        );
    }

    // Teacher grades submission
    public async Task GradeSubmission(GradeSubmissionRequest request)
    {
        var result = await _groupService.GradeSubmission(request);
        
        await result.Match(
            async gradedSubmission =>
            {
                // Notify group members about the grade
                await Clients.Group($"group_{gradedSubmission.GroupId}")
                    .SendAsync("SubmissionGraded", gradedSubmission);
            },
            async error =>
            {
                await Clients.Caller.SendAsync("Error", error.Description);
            }
        );
    }

    // Real-time collaboration: Send drawing/annotation data
    public async Task SendDrawing(Guid groupId, object drawingData)
    {
        // Broadcast drawing to other group members (exclude sender)
        await Clients.OthersInGroup($"group_{groupId}")
            .SendAsync("DrawingReceived", drawingData);
    }

    // Real-time collaboration: Send cursor position
    public async Task SendCursorPosition(Guid groupId, object cursorData)
    {
        // Broadcast cursor position to other group members
        await Clients.OthersInGroup($"group_{groupId}")
            .SendAsync("CursorMoved", cursorData);
    }

    // Real-time collaboration: Send chat message
    public async Task SendMessage(Guid groupId, string message)
    {
        var userName = Context.User?.Identity?.Name ?? "Unknown";
        
        // Broadcast message to all group members
        await Clients.Group($"group_{groupId}")
            .SendAsync("MessageReceived", new
            {
                userName,
                message,
                timestamp = DateTime.UtcNow
            });
    }

    // Notify when member is typing
    public async Task NotifyTyping(Guid groupId, bool isTyping)
    {
        var userName = Context.User?.Identity?.Name ?? "Unknown";
        
        // Notify other group members
        await Clients.OthersInGroup($"group_{groupId}")
            .SendAsync("MemberTyping", new { userName, isTyping });
    }
}
