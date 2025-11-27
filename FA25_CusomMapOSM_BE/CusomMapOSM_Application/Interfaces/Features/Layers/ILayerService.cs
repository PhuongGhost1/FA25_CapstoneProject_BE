using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Layers;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Layers;

public interface ILayerService
{
    Task<Option<List<LayerSummaryDto>, Error>> GetAvailableLayersAsync(CancellationToken ct = default);
    Task<Option<LayerDetailDto, Error>> GetLayerByIdAsync(Guid layerId, CancellationToken ct = default);
    Task<Option<List<LayerDetailDto>, Error>> GetLayersByMapAsync(Guid mapId, CancellationToken ct = default);
}

