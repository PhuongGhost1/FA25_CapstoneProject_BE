using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Workspaces;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Workspace.Response;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Workspace;

public class WorkspaceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Workspace)
            .WithTags(Tags.Workspace)
            .WithDescription(Tags.Workspace);

        // Create Workspace
        group.MapPost(Routes.WorkspaceEndpoints.Create, async (
                [FromBody] WorkspaceReqDto req,
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.Create(req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("CreateWorkspace")
            .WithDescription("Create a new workspace")
            .RequireAuthorization()
            .Produces<WorkspaceResDto>(200)
            .ProducesValidationProblem();

        // Get All Workspaces
        group.MapGet(Routes.WorkspaceEndpoints.GetAll, async (
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.GetAll();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetAllWorkspaces")
            .WithDescription("Get all workspaces")
            .RequireAuthorization()
            .Produces<GetAllWorkspacesResDto>(200);

        // Get Workspace by ID
        group.MapGet(Routes.WorkspaceEndpoints.GetById, async (
                [FromRoute] Guid id,
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.GetById(id);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetWorkspaceById")
            .WithDescription("Get workspace by ID")
            .RequireAuthorization()
            .Produces<GetWorkspaceByIdResDto>(200);

        // Update Workspace
        group.MapPut(Routes.WorkspaceEndpoints.Update, async (
                [FromRoute] Guid id,
                [FromBody] WorkspaceReqDto req,
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.Update(id, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UpdateWorkspace")
            .WithDescription("Update workspace")
            .RequireAuthorization()
            .Produces<UpdateWorkspaceResDto>(200)
            .ProducesValidationProblem();

        // Delete Workspace
        group.MapDelete(Routes.WorkspaceEndpoints.Delete, async (
                [FromRoute] Guid id,
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.Delete(id);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DeleteWorkspace")
            .WithDescription("Delete workspace")
            .RequireAuthorization()
            .Produces<DeleteWorkspaceResDto>(200);

        // Get Workspaces by Organization
        group.MapGet(Routes.WorkspaceEndpoints.GetByOrganization, async (
                [FromRoute] Guid orgId,
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.GetByOrganization(orgId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetWorkspacesByOrganization")
            .WithDescription("Get workspaces by organization ID")
            .RequireAuthorization()
            .Produces<GetWorkspacesByOrganizationResDto>(200);

        // Get My Workspaces
        group.MapGet(Routes.WorkspaceEndpoints.GetMyWorkspaces, async (
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.GetMyWorkspaces();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetMyWorkspaces")
            .WithDescription("Get workspaces I have access to")
            .RequireAuthorization()
            .Produces<GetMyWorkspacesResDto>(200);

        // Get Workspace Maps
        group.MapGet(Routes.WorkspaceEndpoints.GetWorkspaceMaps, async (
                [FromRoute] Guid workspaceId,
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.GetWorkspaceMaps(workspaceId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetWorkspaceMaps")
            .WithDescription("Get maps in a workspace")
            .RequireAuthorization()
            .Produces<GetWorkspaceMapsResDto>(200);

        // Add Map to Workspace
        group.MapPost(Routes.WorkspaceEndpoints.AddMapToWorkspace, async (
                [FromRoute] Guid workspaceId,
                [FromBody] AddMapToWorkspaceReqDto req,
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.AddMapToWorkspace(workspaceId, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("AddMapToWorkspace")
            .WithDescription("Add a map to a workspace")
            .RequireAuthorization()
            .Produces<AddMapToWorkspaceResDto>(200)
            .ProducesValidationProblem();

        // Remove Map from Workspace
        group.MapDelete(Routes.WorkspaceEndpoints.RemoveMapFromWorkspace, async (
                [FromRoute] Guid workspaceId,
                [FromRoute] Guid mapId,
                [FromServices] IWorkspaceService workspaceService) =>
            {
                var result = await workspaceService.RemoveMapFromWorkspace(workspaceId, mapId);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("RemoveMapFromWorkspace")
            .WithDescription("Remove a map from a workspace")
            .RequireAuthorization()
            .Produces<RemoveMapFromWorkspaceResDto>(200);
    }
}
