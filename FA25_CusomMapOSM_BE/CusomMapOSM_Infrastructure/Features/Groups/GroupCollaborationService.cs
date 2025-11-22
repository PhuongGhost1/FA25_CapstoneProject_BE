using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Groups;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Groups.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Groups.Response;
using CusomMapOSM_Domain.Entities.Groups;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Groups;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Sessions;
using Optional;
using System.Text.Json;

namespace CusomMapOSM_Infrastructure.Features.Groups;

public class GroupCollaborationService : IGroupCollaborationService
{
    private readonly ISessionGroupRepository _groupRepository;
    private readonly IGroupSubmissionRepository _submissionRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionParticipantRepository _participantRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IGroupMemberRepository _groupMemberRepository;


    public GroupCollaborationService(
        ISessionGroupRepository groupRepository,
        IGroupSubmissionRepository submissionRepository,
        ISessionRepository sessionRepository,
        ISessionParticipantRepository participantRepository,
        IGroupMemberRepository groupMemberRepository,
        ICurrentUserService currentUserService)
    {
        _groupRepository = groupRepository;
        _submissionRepository = submissionRepository;
        _sessionRepository = sessionRepository;
        _participantRepository = participantRepository;
        _groupMemberRepository = groupMemberRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<GroupResponse, Error>> CreateGroup(CreateGroupRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<GroupResponse, Error>(
                Error.Unauthorized("Group.Unauthorized", "User not authenticated"));
        }

        // Verify user is session host
        var isHost = await _sessionRepository.CheckUserIsHost(request.SessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<GroupResponse, Error>(
                Error.Forbidden("Group.NotHost", "Only session host can create groups"));
        }

        // Validate members
        if (request.MemberParticipantIds == null || !request.MemberParticipantIds.Any())
        {
            return Option.None<GroupResponse, Error>(
                Error.ValidationError("Group.NoMembers", "At least one member is required"));
        }

        var group = new SessionGroup
        {
            GroupId = Guid.NewGuid(),
            SessionId = request.SessionId,
            GroupName = request.GroupName,
            Color = request.Color,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _groupRepository.CreateGroup(group);
        if (!created)
        {
            return Option.None<GroupResponse, Error>(
                Error.Failure("Group.CreateFailed", "Failed to create group"));
        }

        // Add members directly to database
        foreach (var participantId in request.MemberParticipantIds)
        {
            var member = new SessionGroupMember
            {
                GroupMemberId = Guid.NewGuid(),
                GroupId = group.GroupId,
                SessionParticipantId = participantId,
                IsLeader = request.LeaderParticipantId.HasValue && participantId == request.LeaderParticipantId.Value,
                JoinedAt = DateTime.UtcNow
            };
            _groupMemberRepository.CreateGroupMember(member);
        }

        // Get members for response
        var members = await _groupMemberRepository.GetGroupMembersByGroup(group.GroupId);

        return Option.Some<GroupResponse, Error>(MapToGroupResponse(group, members));
    }

    public async Task<Option<List<GroupResponse>, Error>> GetGroupsBySession(Guid sessionId)
    {
        var sessionExists = await _sessionRepository.CheckSessionExists(sessionId);
        if (!sessionExists)
        {
            return Option.None<List<GroupResponse>, Error>(
                Error.NotFound("Group.SessionNotFound", "Session not found"));
        }

        var groups = await _groupRepository.GetGroupsBySession(sessionId);
        
        // Get all members for these groups
        var groupIds = groups.Select(g => g.GroupId).ToList();
        var allMembers = await _groupMemberRepository.GetGroupMembersByGroup(groupIds);
        
        var response = groups.Select(g => 
        {
            var groupMembers = allMembers.Where(m => m.GroupId == g.GroupId).ToList();
            return MapToGroupResponse(g, groupMembers);
        }).ToList();

        return Option.Some<List<GroupResponse>, Error>(response);
    }

    public async Task<Option<GroupResponse, Error>> GetGroupById(Guid groupId)
    {
        var group = await _groupRepository.GetGroupById(groupId);
        if (group == null)
        {
            return Option.None<GroupResponse, Error>(
                Error.NotFound("Group.NotFound", "Group not found"));
        }

        // Get members for this group
        var members = await _groupMemberRepository.GetGroupMembersByGroup(groupId);

        return Option.Some<GroupResponse, Error>(MapToGroupResponse(group, members));
    }

    public async Task<Option<bool, Error>> DeleteGroup(Guid groupId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<bool, Error>(
                Error.Unauthorized("Group.Unauthorized", "User not authenticated"));
        }

        var group = await _groupRepository.GetGroupById(groupId);
        if (group == null)
        {
            return Option.None<bool, Error>(
                Error.NotFound("Group.NotFound", "Group not found"));
        }

        // Verify user is session host
        var isHost = await _sessionRepository.CheckUserIsHost(group.SessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<bool, Error>(
                Error.Forbidden("Group.NotHost", "Only session host can delete groups"));
        }

        var deleted = await _groupRepository.DeleteGroup(groupId);
        return deleted
            ? Option.Some<bool, Error>(true)
            : Option.None<bool, Error>(Error.Failure("Group.DeleteFailed", "Failed to delete group"));
    }

    public async Task<Option<GroupSubmissionResponse, Error>> SubmitGroupWork(SubmitGroupWorkRequest request)
    {
        var groupExists = await _groupRepository.CheckGroupExists(request.GroupId);
        if (!groupExists)
        {
            return Option.None<GroupSubmissionResponse, Error>(
                Error.NotFound("Group.NotFound", "Group not found"));
        }

        var submission = new GroupSubmission
        {
            SubmissionId = Guid.NewGuid(),
            GroupId = request.GroupId,
            Title = request.Title,
            Content = request.Content,
            AttachmentUrls = request.AttachmentUrls != null && request.AttachmentUrls.Any()
                ? JsonSerializer.Serialize(request.AttachmentUrls)
                : null,
            SubmittedAt = DateTime.UtcNow
        };

        var created = await _submissionRepository.CreateSubmission(submission);
        if (!created)
        {
            return Option.None<GroupSubmissionResponse, Error>(
                Error.Failure("Group.SubmissionFailed", "Failed to submit group work"));
        }

        var group = await _groupRepository.GetGroupById(request.GroupId);
        
        return Option.Some<GroupSubmissionResponse, Error>(new GroupSubmissionResponse
        {
            SubmissionId = submission.SubmissionId,
            GroupId = submission.GroupId,
            GroupName = group?.GroupName ?? "Unknown",
            Title = submission.Title,
            Content = submission.Content,
            AttachmentUrls = request.AttachmentUrls,
            Score = null,
            Feedback = null,
            SubmittedAt = submission.SubmittedAt,
            GradedAt = null
        });
    }

    public async Task<Option<List<GroupSubmissionResponse>, Error>> GetGroupSubmissions(Guid groupId)
    {
        var groupExists = await _groupRepository.CheckGroupExists(groupId);
        if (!groupExists)
        {
            return Option.None<List<GroupSubmissionResponse>, Error>(
                Error.NotFound("Group.NotFound", "Group not found"));
        }

        var submissions = await _submissionRepository.GetSubmissionsByGroup(groupId);
        var group = await _groupRepository.GetGroupById(groupId);
        
        var response = submissions.Select(s => MapToSubmissionResponse(s, group?.GroupName ?? "Unknown")).ToList();
        return Option.Some<List<GroupSubmissionResponse>, Error>(response);
    }

    public async Task<Option<List<GroupSubmissionResponse>, Error>> GetSessionSubmissions(Guid sessionId)
    {
        var sessionExists = await _sessionRepository.CheckSessionExists(sessionId);
        if (!sessionExists)
        {
            return Option.None<List<GroupSubmissionResponse>, Error>(
                Error.NotFound("Group.SessionNotFound", "Session not found"));
        }

        var submissions = await _submissionRepository.GetSubmissionsBySession(sessionId);
        var response = submissions.Select(s => MapToSubmissionResponse(s, s.Group?.GroupName ?? "Unknown")).ToList();
        
        return Option.Some<List<GroupSubmissionResponse>, Error>(response);
    }

    public async Task<Option<GroupSubmissionResponse, Error>> GradeSubmission(GradeSubmissionRequest request)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId == null)
        {
            return Option.None<GroupSubmissionResponse, Error>(
                Error.Unauthorized("Group.Unauthorized", "User not authenticated"));
        }

        var submission = await _submissionRepository.GetSubmissionById(request.SubmissionId);
        if (submission == null)
        {
            return Option.None<GroupSubmissionResponse, Error>(
                Error.NotFound("Group.SubmissionNotFound", "Submission not found"));
        }

        var group = await _groupRepository.GetGroupById(submission.GroupId);
        if (group == null)
        {
            return Option.None<GroupSubmissionResponse, Error>(
                Error.NotFound("Group.NotFound", "Group not found"));
        }

        // Verify user is session host
        var isHost = await _sessionRepository.CheckUserIsHost(group.SessionId, currentUserId.Value);
        if (!isHost)
        {
            return Option.None<GroupSubmissionResponse, Error>(
                Error.Forbidden("Group.NotHost", "Only session host can grade submissions"));
        }

        submission.Score = request.Score;
        submission.Feedback = request.Feedback;
        submission.GradedAt = DateTime.UtcNow;

        var updated = await _submissionRepository.UpdateSubmission(submission);
        if (!updated)
        {
            return Option.None<GroupSubmissionResponse, Error>(
                Error.Failure("Group.GradeFailed", "Failed to grade submission"));
        }

        return Option.Some<GroupSubmissionResponse, Error>(MapToSubmissionResponse(submission, group.GroupName));
    }

    private GroupResponse MapToGroupResponse(SessionGroup group, List<SessionGroupMember> members)
    {
        return new GroupResponse
        {
            GroupId = group.GroupId,
            SessionId = group.SessionId,
            GroupName = group.GroupName,
            Color = group.Color,
            Members = members.Select(m => new GroupMemberResponse
            {
                GroupMemberId = m.GroupMemberId,
                SessionParticipantId = m.SessionParticipantId,
                ParticipantName = m.SessionParticipant?.DisplayName ?? "Unknown",
                IsLeader = m.IsLeader,
                JoinedAt = m.JoinedAt
            }).ToList(),
            CreatedAt = group.CreatedAt
        };
    }

    private GroupSubmissionResponse MapToSubmissionResponse(GroupSubmission submission, string groupName)
    {
        List<string>? attachmentUrls = null;
        if (!string.IsNullOrEmpty(submission.AttachmentUrls))
        {
            attachmentUrls = JsonSerializer.Deserialize<List<string>>(submission.AttachmentUrls);
        }

        return new GroupSubmissionResponse
        {
            SubmissionId = submission.SubmissionId,
            GroupId = submission.GroupId,
            GroupName = groupName,
            Title = submission.Title,
            Content = submission.Content,
            AttachmentUrls = attachmentUrls,
            Score = submission.Score,
            Feedback = submission.Feedback,
            SubmittedAt = submission.SubmittedAt,
            GradedAt = submission.GradedAt
        };
    }
}
