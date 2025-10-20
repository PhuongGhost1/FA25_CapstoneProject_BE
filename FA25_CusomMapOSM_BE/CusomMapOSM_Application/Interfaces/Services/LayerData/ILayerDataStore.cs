using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Domain.Entities.Layers;

namespace CusomMapOSM_Application.Interfaces.Services.LayerData;

public interface ILayerDataStore
{
    Task<string?> GetDataAsync(Layer layer, CancellationToken cancellationToken = default);
    Task<object?> GetDataObjectAsync(Layer layer, CancellationToken cancellationToken = default);
    Task SetDataAsync(Layer layer, string data, CancellationToken cancellationToken = default);
    Task SetDataAsync(Layer layer, object data, CancellationToken cancellationToken = default);
    Task DeleteDataAsync(Layer layer, CancellationToken cancellationToken = default);
}
