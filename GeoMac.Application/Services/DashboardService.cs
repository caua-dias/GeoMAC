using GeoMac.Domain.Entities;
using GeoMac.Domain.Interfaces;
using GeoMac.Application.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GeoMac.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IRepository<CropAnalysis> _cropRepo;

    public DashboardService(IRepository<CropAnalysis> cropRepo)
    {
        _cropRepo = cropRepo;
    }

    public async Task<DashboardDto> GetDataAsync(Guid cropAnalysisId)
    {
        // Mock data logic for now, in a real scenario this would join tables or use a specific query
        var analysis = await _cropRepo.GetByIdAsync(cropAnalysisId);
        
        // Simulating some data based on what frontend Chart.js expects
        return new DashboardDto
        {
            Labels = new[] { "Jan", "Fev", "Mar", "Abr", "Mai" },
            NdviValues = new[] { 0.2, 0.4, 0.6, 0.8, 0.7 },
            Precipitations = new[] { 120.5, 90.0, 150.2, 80.5, 60.0 },
            Temperatures = new[] { 22.5, 24.0, 23.5, 20.0, 18.5 }
        };
    }
}
