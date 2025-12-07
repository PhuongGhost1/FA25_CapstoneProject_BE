using System;
using CusomMapOSM_Domain.Entities.Maps.Enums;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class MapListItemDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public MapStatusEnum Status { get; set; }
        public bool IsStoryMap { get; set; }
        public string? PreviewImage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public bool IsOwner { get; set; }
        
        public string? WorkspaceName { get; set; }
    }
}

