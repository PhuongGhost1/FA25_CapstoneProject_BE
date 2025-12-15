using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Home;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Home;

public interface IHomeService
{
   Task<Option<HomeStatsResponse, Error>> GetHomeStats();
}
