using CusomMapOSM_API.Interfaces;
using CusomMapOSM_Domain.Constants;

namespace CusomMapOSM_API.Endpoints;

public class TestEndpoint : IEndpoint
{
    private const string API_PREFIX = "test";
    private readonly string solutionRoot;
    private readonly string exportMap;
    private readonly string mapExportPath;
    private readonly string outputDir;
    public TestEndpoint()
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        solutionRoot = Path.GetFullPath(Path.Combine(basePath, @"..\..\..\.."));
        exportMap = Path.GetFullPath(Path.Combine(solutionRoot, "export_map.py"));
        mapExportPath = Path.GetFullPath(Path.Combine(solutionRoot, @"..\..\..\Documents\Maps\vietnam_map_project.qgz"));
        outputDir = Path.GetFullPath(Path.Combine(solutionRoot, "tmp"));

        Console.WriteLine($"✅ Export map script path: {exportMap}");
        Console.WriteLine($"✅ QGIS project path: {mapExportPath}");
        Console.WriteLine($"✅ Output directory: {outputDir}");
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(API_PREFIX)
            .WithTags(Tags.Test)
            .WithDescription(Tags.Test);

        group.MapGet("/", () => Results.Ok("Test endpoint is working!"))
            .WithName("TestEndpoint")
            .WithTags("Test")
            .Produces<string>(StatusCodes.Status200OK)
            .WithMetadata(new HttpMethodMetadata(new[] { HttpMethods.Get }))
            .WithSummary("Test Endpoint")
            .WithDescription("This endpoint is used to test the API functionality.");

        group.MapGet("/test-map-export", async (HttpContext http) =>
        {
            string projectPath = mapExportPath;
            string fileName = $"map_export_{DateTime.UtcNow.Ticks}.png";
            string outputPath = Path.Combine(outputDir, fileName);
            string format = "PNG";
            string west = "105.75";
            string south = "20.95";
            string east = "105.85";
            string north = "21.05";

            string pythonPath = "C:\\OSGeo4W\\bin\\python-qgis.bat";
            string scriptPath = exportMap;

            string args = $"\"{scriptPath}\" \"{projectPath}\" \"{outputPath}\" {format} {west} {south} {east} {north}";

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{pythonPath} {args}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // ✅ Thiết lập biến môi trường QGIS
            psi.EnvironmentVariables["OSGEO4W_ROOT"] = "C:\\OSGeo4W";
            psi.EnvironmentVariables["QGIS_PREFIX_PATH"] = "C:\\OSGeo4W\\apps\\qgis";
            psi.EnvironmentVariables["PATH"] = string.Join(";", new[]
            {
    "C:\\OSGeo4W\\bin",
    "C:\\OSGeo4W\\apps\\qgis\\bin",
    "C:\\OSGeo4W\\apps\\Qt5\\bin",
    "C:\\OSGeo4W\\apps\\Python39\\Scripts",
    psi.EnvironmentVariables["PATH"]
});

            using var process = System.Diagnostics.Process.Start(psi);

            if (process == null)
            {
                return Results.Problem("❌ Failed to start the QGIS map export process.");
            }

            await process.WaitForExitAsync();

            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();

            if (process.ExitCode == 0 && System.IO.File.Exists(outputPath))
            {
                return Results.Ok(new { message = "✅ Map export successful", output = outputPath, stdout });
            }
            else
            {
                return Results.Problem($"❌ Map export failed.\n\nExitCode: {process.ExitCode}\nStdout:\n{stdout}\n\nStderr:\n{stderr}");
            }
        });
    }
}
