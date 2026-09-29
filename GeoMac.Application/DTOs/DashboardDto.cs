using System.Collections.Generic;

namespace GeoMac.Application.DTOs;

public class DashboardDto
{
    public IEnumerable<double> NdviValues { get; set; } = new List<double>();
    public IEnumerable<double> Precipitations { get; set; } = new List<double>();
    public IEnumerable<double> Temperatures { get; set; } = new List<double>();
    public IEnumerable<string> Labels { get; set; } = new List<string>();
}
