using GeoMac.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace GeoMac.Application.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetDataAsync(Guid cropAnalysisId);
}
