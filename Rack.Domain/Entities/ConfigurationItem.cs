using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;

namespace Rack.Domain.Entities
{
    public class ConfigurationItem : Entity, IAuditInfo, ISoftDelete
    {
        public Guid DeviceID { get; set; }
        public string ConfigType { get; set; } = null!;
        public string? SerialNumber { get; set; }
        public string ConfigValue { get; set; } = null!;
        public int Count { get; set; } = 1;

        // Quan hệ với Device
        public Device Device { get; set; } = null!;

        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }
    }
}