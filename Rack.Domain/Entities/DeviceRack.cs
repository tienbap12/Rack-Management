using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;
using System.Collections.Generic;

namespace Rack.Domain.Entities
{
    public class DeviceRack : Entity, IAuditInfo, ISoftDelete
    {
        public Guid DataCenterID { get; set; }
        public string RackNumber { get; set; } = null!;
        public int Size { get; set; }    // Ví dụ: '42U'
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }

        // Quan hệ với DataCenter
        public DataCenter DataCenter { get; set; } = null!;

        // Quan hệ 1 - N với Device
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}