using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Comments;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Comments;
using CusomMapOSM_Domain.Entities.Comments;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Comments;
using Optional;
using Optional.Unsafe;

namespace CusomMapOSM_Infrastructure.Features.Comments;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly ICurrentUserService _currentUserService;

    public CommentService(ICommentRepository commentRepository, ICurrentUserService currentUserService)
    {
        _commentRepository = commentRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Option<CommentDto, Error>> CreateComment(CreateCommentRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<CommentDto, Error>(Error.Unauthorized("User.NotAuthenticated", "User must be authenticated"));
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Option.None<CommentDto, Error>(Error.ValidationError("Comment.InvalidContent", "Comment content cannot be empty"));
            }

            if (!request.MapId.HasValue && !request.LayerId.HasValue)
            {
                return Option.None<CommentDto, Error>(Error.ValidationError("Comment.InvalidTarget", "Comment must be associated with either a map or a layer"));
            }

            var comment = new Comment
            {
                MapId = request.MapId,
                LayerId = request.LayerId,
                UserId = currentUserId.Value,
                Content = request.Content,
                Position = request.Position ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _commentRepository.CreateComment(comment);
            if (!created)
            {
                return Option.None<CommentDto, Error>(Error.Failure("Comment.CreateFailed", "Failed to create comment"));
            }

            var createdComment = await _commentRepository.GetCommentById(comment.CommentId);
            if (createdComment == null)
            {
                return Option.None<CommentDto, Error>(Error.Failure("Comment.NotFound", "Created comment not found"));
            }

            return Option.Some<CommentDto, Error>(MapToDto(createdComment));
        }
        catch (Exception ex)
        {
            return Option.None<CommentDto, Error>(Error.Failure("Comment.CreateFailed", $"Failed to create comment: {ex.Message}"));
        }
    }

    public async Task<Option<CommentDto, Error>> GetCommentById(int commentId)
    {
        try
        {
            var comment = await _commentRepository.GetCommentById(commentId);
            if (comment == null)
            {
                return Option.None<CommentDto, Error>(Error.NotFound("Comment.NotFound", "Comment not found"));
            }

            return Option.Some<CommentDto, Error>(MapToDto(comment));
        }
        catch (Exception ex)
        {
            return Option.None<CommentDto, Error>(Error.Failure("Comment.GetFailed", $"Failed to get comment: {ex.Message}"));
        }
    }

    public async Task<Option<List<CommentDto>, Error>> GetCommentsByMapId(Guid mapId)
    {
        try
        {
            var comments = await _commentRepository.GetCommentsByMapId(mapId);
            return Option.Some<List<CommentDto>, Error>(comments.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            return Option.None<List<CommentDto>, Error>(Error.Failure("Comment.GetFailed", $"Failed to get comments: {ex.Message}"));
        }
    }

    public async Task<Option<List<CommentDto>, Error>> GetCommentsByLayerId(Guid layerId)
    {
        try
        {
            var comments = await _commentRepository.GetCommentsByLayerId(layerId);
            return Option.Some<List<CommentDto>, Error>(comments.Select(MapToDto).ToList());
        }
        catch (Exception ex)
        {
            return Option.None<List<CommentDto>, Error>(Error.Failure("Comment.GetFailed", $"Failed to get comments: {ex.Message}"));
        }
    }

    public async Task<Option<CommentDto, Error>> UpdateComment(int commentId, UpdateCommentRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<CommentDto, Error>(Error.Unauthorized("User.NotAuthenticated", "User must be authenticated"));
            }

            var comment = await _commentRepository.GetCommentById(commentId);
            if (comment == null)
            {
                return Option.None<CommentDto, Error>(Error.NotFound("Comment.NotFound", "Comment not found"));
            }

            var belongsToUser = await _commentRepository.CheckCommentBelongsToUser(commentId, currentUserId.Value);
            if (!belongsToUser)
            {
                return Option.None<CommentDto, Error>(Error.Forbidden("Comment.NotAuthorized", "You can only update your own comments"));
            }

            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return Option.None<CommentDto, Error>(Error.ValidationError("Comment.InvalidContent", "Comment content cannot be empty"));
            }

            comment.Content = request.Content;
            comment.Position = request.Position ?? comment.Position;
            comment.UpdatedAt = DateTime.UtcNow;

            var updated = await _commentRepository.UpdateComment(comment);
            if (!updated)
            {
                return Option.None<CommentDto, Error>(Error.Failure("Comment.UpdateFailed", "Failed to update comment"));
            }

            var updatedComment = await _commentRepository.GetCommentById(commentId);
            if (updatedComment == null)
            {
                return Option.None<CommentDto, Error>(Error.Failure("Comment.NotFound", "Updated comment not found"));
            }

            return Option.Some<CommentDto, Error>(MapToDto(updatedComment));
        }
        catch (Exception ex)
        {
            return Option.None<CommentDto, Error>(Error.Failure("Comment.UpdateFailed", $"Failed to update comment: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> DeleteComment(int commentId)
    {
        try
        {
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<bool, Error>(Error.Unauthorized("User.NotAuthenticated", "User must be authenticated"));
            }

            var comment = await _commentRepository.GetCommentById(commentId);
            if (comment == null)
            {
                return Option.None<bool, Error>(Error.NotFound("Comment.NotFound", "Comment not found"));
            }

            var belongsToUser = await _commentRepository.CheckCommentBelongsToUser(commentId, currentUserId.Value);
            if (!belongsToUser)
            {
                return Option.None<bool, Error>(Error.Forbidden("Comment.NotAuthorized", "You can only delete your own comments"));
            }

            var deleted = await _commentRepository.DeleteComment(commentId);
            if (!deleted)
            {
                return Option.None<bool, Error>(Error.Failure("Comment.DeleteFailed", "Failed to delete comment"));
            }

            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("Comment.DeleteFailed", $"Failed to delete comment: {ex.Message}"));
        }
    }

    private static CommentDto MapToDto(Comment comment)
    {
        return new CommentDto
        {
            CommentId = comment.CommentId,
            MapId = comment.MapId,
            LayerId = comment.LayerId,
            UserId = comment.UserId,
            UserName = comment.User?.FullName,
            UserEmail = comment.User?.Email,
            Content = comment.Content,
            Position = comment.Position,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}

