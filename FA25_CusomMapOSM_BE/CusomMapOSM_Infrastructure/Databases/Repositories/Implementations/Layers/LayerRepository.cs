using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Layers;
using CusomMapOSM_Infrastructure.Databases;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Layers;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Layers;

public class LayerRepository : ILayerRepository
{
    private readonly CustomMapOSMDbContext _dbContext;

    public LayerRepository(CustomMapOSMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<Layer>> GetAvailableLayersAsync(Guid userId, CancellationToken ct = default)
    {
        return _dbContext.Layers
            .Include(l => l.Map)
            .Where(l => l.UserId == userId || (l.Map != null && l.Map.IsPublic))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);
    }

    public Task<Layer?> GetLayerByIdAsync(Guid layerId, Guid userId, CancellationToken ct = default)
    {
        return _dbContext.Layers
            .Include(l => l.Map)
            .Where(l => l.LayerId == layerId && (l.UserId == userId || (l.Map != null && l.Map.IsPublic)))
            .FirstOrDefaultAsync(ct);
    }

    public Task<List<Layer>> GetLayersByMapAsync(Guid mapId, Guid userId, CancellationToken ct = default)
    {
        return _dbContext.Layers
            .Include(l => l.Map)
            .Where(l => l.MapId == mapId && (l.UserId == userId || (l.Map != null && l.Map.IsPublic)))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);
    }
}