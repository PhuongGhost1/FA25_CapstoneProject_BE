using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CusomMapOSM_Application.Interfaces.Services.LayerData;
using CusomMapOSM_Domain.Entities.Layers;

namespace CusomMapOSM_Infrastructure.Services.LayerData.Relational;

public class RelationalLayerDataStore : ILayerDataStore
{
    public Task<string?> GetDataAsync(Layer layer, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(layer.LayerData);
    }

    public Task<object?> GetDataObjectAsync(Layer layer, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(layer.LayerData))
            return Task.FromResult<object?>(null);

        try
        {
            // Try to parse as JSON
            using var document = JsonDocument.Parse(layer.LayerData);
            return Task.FromResult<object?>(document.RootElement);
        }
        catch (JsonException)
        {
            // If parsing fails, return as string
            return Task.FromResult<object?>(layer.LayerData);
        }
    }

    public Task SetDataAsync(Layer layer, string data, CancellationToken cancellationToken = default)
    {
        layer.LayerData = data;
        return Task.CompletedTask;
    }

    public Task SetDataAsync(Layer layer, object data, CancellationToken cancellationToken = default)
    {
        if (data is JsonElement jsonElement)
        {
            layer.LayerData = jsonElement.GetRawText();
        }
        else
        {
            layer.LayerData = JsonSerializer.Serialize(data);
        }
        return Task.CompletedTask;
    }

    public Task DeleteDataAsync(Layer layer, CancellationToken cancellationToken = default)
    {
        layer.LayerData = null;
        return Task.CompletedTask;
    }
}

