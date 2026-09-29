using GeoMac.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GeoMac.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("{cropAnalysisId}")]
    // [Authorize] // Remova o comentário para exigir autenticação
    public async Task<IActionResult> GetDashboardData(Guid cropAnalysisId)
    {
        var data = await _dashboardService.GetDataAsync(cropAnalysisId);
        return Ok(data);
    }
}
