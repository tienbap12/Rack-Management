using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;
using System.ComponentModel.DataAnnotations;

namespace Rack.Domain.Entities
{
    public class ServerRental : Entity, IAuditInfo, ISoftDelete
    {
        public Guid CustomerID { get; set; }
        public Guid DeviceID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }    // NULL nếu vẫn đang thuê
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }
        // Quan hệ với Customer
        public Customer Customer { get; set; } = null!;
        // Quan hệ với Device
        public Device Device { get; set; } = null!;
    }
}
