using CusomMapOSM_API.Constants;
using CusomMapOSM_API.Extensions;
using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Application.Interfaces.Features.Organization;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;
using Microsoft.AspNetCore.Mvc;

namespace CusomMapOSM_API.Endpoints.Organization;

public class OrganizationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(Routes.Prefix.Organization)
            .WithTags(Tags.Organization)
            .WithDescription(Tags.Organization);

        group.MapPost(Routes.OrganizationsEndpoints.Create, async (
                [FromBody] OrganizationReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.Create(req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("CreateOrganization")
            .WithDescription("Create a new organization")
            .RequireAuthorization()
            .Produces<OrganizationResDto>(200)
            .ProducesValidationProblem();

        group.MapGet(Routes.OrganizationsEndpoints.GetAll, async (
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.GetAll();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetAllOrganizations")
            .WithDescription("Get all organizations")
            .RequireAuthorization()
            .Produces<GetAllOrganizationsResDto>(200);

        group.MapGet(Routes.OrganizationsEndpoints.GetById, async (
                [FromRoute] Guid id,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.GetById(id);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("GetOrganizationById")
            .WithDescription("Get organization by ID")
            .RequireAuthorization()
            .Produces<GetOrganizationByIdResDto>(200);

        group.MapPut(Routes.OrganizationsEndpoints.Update, async (
                [FromRoute] Guid id,
                [FromBody] OrganizationReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.Update(id, req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("UpdateOrganization")
            .WithDescription("Update organization")
            .RequireAuthorization()
            .Produces<UpdateOrganizationResDto>(200)
            .ProducesValidationProblem();

        group.MapDelete(Routes.OrganizationsEndpoints.Delete, async (
                [FromRoute] Guid id,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.Delete(id);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName("DeleteOrganization")
            .WithDescription("Delete organization")
            .RequireAuthorization()
            .Produces<DeleteOrganizationResDto>(200);

        group.MapPost(Routes.OrganizationsEndpoints.InviteMember, async (
                [FromBody] InviteMemberOrganizationReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.InviteMember(req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName(Routes.OrganizationsEndpoints.InviteMember)
            .WithDescription("Invite a member to an organization")
            .RequireAuthorization()
            .Produces<InviteMemberOrganizationResDto>(200)
            .ProducesValidationProblem();

        group.MapPost(Routes.OrganizationsEndpoints.AcceptInvite, async (
                [FromBody] AcceptInviteOrganizationReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.AcceptInvite(req);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName(Routes.OrganizationsEndpoints.AcceptInvite)
            .WithDescription("Accept an invitation to an organization")
            .RequireAuthorization()
            .Produces<AcceptInviteOrganizationResDto>(200)
            .ProducesValidationProblem();

        group.MapGet(Routes.OrganizationsEndpoints.GetMyInvitations, async (
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.GetMyInvitations();
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName(Routes.OrganizationsEndpoints.GetMyInvitations)
            .WithDescription("Get my invitations")
            .RequireAuthorization()
            .Produces<GetInvitationsResDto>(200)
            .ProducesValidationProblem();

        group.MapGet(Routes.OrganizationsEndpoints.GetOrganizationMembers, async (
                [FromRoute] Guid orgId,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.GetMembers(orgId);
                return result.Match(Results.Ok, e => e.ToProblemDetailsResult());
            }).WithName(Routes.OrganizationsEndpoints.GetOrganizationMembers)
            .WithDescription("Get organization members")
            .RequireAuthorization()
            .Produces<GetOrganizationMembersResDto>(200);

        group.MapPut(Routes.OrganizationsEndpoints.UpdateMemberRole, async (
                [FromBody] UpdateMemberRoleReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.UpdateMemberRole(req);
                return result.Match(Results.Ok, e => e.ToProblemDetailsResult());
            }).WithName(Routes.OrganizationsEndpoints.UpdateMemberRole)
            .WithDescription("Update member role in organization")
            .RequireAuthorization()
            .Produces<UpdateMemberRoleResDto>(200)
            .ProducesValidationProblem();

        group.MapDelete(Routes.OrganizationsEndpoints.RemoveMember, async (
                [FromBody] RemoveMemberReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.RemoveMember(req);
                return result.Match(Results.Ok, e => e.ToProblemDetailsResult());
            }).WithName(Routes.OrganizationsEndpoints.RemoveMember)
            .WithDescription("Remove member from organization")
            .RequireAuthorization()
            .Produces<RemoveMemberResDto>(200)
            .ProducesValidationProblem();

        group.MapPost(Routes.OrganizationsEndpoints.RejectInvite, async (
                [FromBody] RejectInviteOrganizationReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.RejectInvite(req);
                return result.Match(Results.Ok, e => e.ToProblemDetailsResult());
            }).WithName(Routes.OrganizationsEndpoints.RejectInvite)
            .WithDescription("Reject an organization invitation")
            .RequireAuthorization()
            .Produces<RejectInviteOrganizationResDto>(200)
            .ProducesValidationProblem();
        group.MapPost(Routes.OrganizationsEndpoints.CancelInvite, async (
                [FromBody] CancelInviteOrganizationReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.CancelInvite(req);
                return result.Match(Results.Ok, e => e.ToProblemDetailsResult());
            }).WithName(Routes.OrganizationsEndpoints.CancelInvite)
            .WithDescription("Cancel a pending organization invitation")
            .RequireAuthorization()
            .Produces<CancelInviteOrganizationResDto>(200)
            .ProducesValidationProblem();

        group.MapGet(Routes.OrganizationsEndpoints.GetMyOrganizations, async (
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.GetMyOrganizations();
                return result.Match(Results.Ok, e => e.ToProblemDetailsResult());
            }).WithName(Routes.OrganizationsEndpoints.GetMyOrganizations)
            .WithDescription("Get organizations I belong to")
            .RequireAuthorization()
            .Produces<GetMyOrganizationsResDto>(200);

        group.MapPost(Routes.OrganizationsEndpoints.TransferOwnership, async (
                [FromBody] TransferOwnershipReqDto req,
                [FromServices] IOrganizationService organizationService) =>
            {
                var result = await organizationService.TransferOwnership(req);
                return result.Match(Results.Ok, e => e.ToProblemDetailsResult());
            }).WithName(Routes.OrganizationsEndpoints.TransferOwnership)
            .WithDescription("Transfer organization ownership")
            .RequireAuthorization()
            .Produces<TransferOwnershipResDto>(200)
            .ProducesValidationProblem();

        group.MapPost(Routes.OrganizationsEndpoints.BulkCreateStudents, async (
                IFormFile excelFile,
                [FromForm] Guid organizationId,
                [FromForm] string domain,
                [FromServices] IOrganizationService organizationService) =>
            {
                var request = new BulkCreateStudentsRequest
                {
                    OrganizationId = organizationId,
                    Domain = domain
                };
                var result = await organizationService.BulkCreateStudents(excelFile, request);
                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToProblemDetailsResult()
                );
            }).WithName(Routes.OrganizationsEndpoints.BulkCreateStudents)
            .WithDescription("Bulk create student accounts from Excel file")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<BulkCreateStudentsResponse>(200)
            .ProducesValidationProblem();

    }
}