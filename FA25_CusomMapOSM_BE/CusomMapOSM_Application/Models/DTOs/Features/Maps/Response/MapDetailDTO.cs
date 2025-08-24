using System;
using System.Collections.Generic;

namespace CusomMapOSM_Application.Models.DTOs.Features.Maps.Response
{
    public class MapDetailDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public double InitialLatitude { get; set; }
        public double InitialLongitude { get; set; }
        public int InitialZoom { get; set; }
        public string BaseMapProvider { get; set; }
        
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public bool IsOwner { get; set; }
        public string UserRole { get; set; } // Owner, Editor, Viewer
        
        public List<LayerDTO> Layers { get; set; }
    }
}