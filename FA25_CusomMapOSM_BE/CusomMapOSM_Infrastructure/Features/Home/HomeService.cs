using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Home;
using CusomMapOSM_Application.Models.DTOs.Features.Home;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Maps;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Home;

public class HomeService : IHomeService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IMapRepository _mapRepository;

    public HomeService(
        IOrganizationRepository organizationRepository,
        IMapRepository mapRepository)
    {
        _organizationRepository = organizationRepository;
        _mapRepository = mapRepository;
    }

    public async Task<Option<HomeStatsResponse, Error>> GetHomeStats()
    {
        try
        {
            var organizationCount = await _organizationRepository.GetTotalOrganizationCount();
            
            var templates = await _mapRepository.GetMapTemplates();
            var templateCount = templates.Count;
            
            var totalMaps = await _mapRepository.GetTotalMapsCount();
            
            var monthlyExports = await _mapRepository.GetMonthlyExportsCount();

            var response = new HomeStatsResponse
            {
                OrganizationCount = organizationCount,
                TemplateCount = templateCount,
                TotalMaps = totalMaps,
                MonthlyExports = monthlyExports
            };

            return Option.Some<HomeStatsResponse, Error>(response);
        }
        catch (Exception ex)
        {
            return Option.None<HomeStatsResponse, Error>(
                Error.Failure("Home.StatsError", $"Failed to retrieve home statistics: {ex.Message}"));
        }
    }
}
