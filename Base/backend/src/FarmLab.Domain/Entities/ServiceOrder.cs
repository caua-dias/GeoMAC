using System;

namespace FarmLab.Domain.Entities
{
    public class ServiceOrder
    {
        public int Id { get; set; }
        public int TalhaoId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
