namespace FarmLab.Domain.Entities
{
    public class Talhao
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double AreaHectares { get; set; }
        // Geometry pode ser armazenado como GeoJSON string ou WKT
        public string Geometry { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
