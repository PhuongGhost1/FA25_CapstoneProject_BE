using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Community;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Community;

public interface ICommunityService
{
    Task<List<CommunityPostSummaryResponse>> GetPublishedPostsAsync(string? topic, CancellationToken ct = default);
    Task<Option<CommunityPostDetailResponse, Error>> GetPostBySlugAsync(string slug, CancellationToken ct = default);

    Task<List<CommunityPostSummaryResponse>> AdminGetPostsAsync(CancellationToken ct = default);
    Task<Option<CommunityPostDetailResponse, Error>> AdminGetPostByIdAsync(string id, CancellationToken ct = default);
    Task<Option<CommunityPostDetailResponse, Error>> AdminCreatePostAsync(CommunityPostCreateRequest request, CancellationToken ct = default);
    Task<Option<CommunityPostDetailResponse, Error>> AdminUpdatePostAsync(string id, CommunityPostUpdateRequest request, CancellationToken ct = default);
    Task<Option<bool, Error>> AdminDeletePostAsync(string id, CancellationToken ct = default);
}


