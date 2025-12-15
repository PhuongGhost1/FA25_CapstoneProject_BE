using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Layers;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Layers;

public interface ILayerRepository
{
    Task<List<Layer>> GetAvailableLayersAsync(Guid userId, CancellationToken ct = default);
    Task<Layer?> GetLayerByIdAsync(Guid layerId, Guid userId, CancellationToken ct = default);
    Task<List<Layer>> GetLayersByMapAsync(Guid mapId, Guid userId, CancellationToken ct = default);
    Task<Layer?> GetLayerByIdAsync(Guid layerId, CancellationToken ct = default);
}