using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rack.Domain.Entities
{
    public class Device : Entity, IAuditInfo, ISoftDelete
    {
        public Guid? ParentDeviceID { get; set; }  // NULL cho thiết bị cấp cao nhất
        public Guid? RackID { get; set; }          // NULL cho blade server trong chassis
        public int Size { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }

        public int? UPosition { get; set; }

        public string? SlotInParent { get; set; }    // Ví dụ: 'Slot1' cho blade server
        public string DeviceType { get; set; } = null!;   // Ví dụ: 'Server', 'Switch', 'SAN', 'BladeChassis'
        public string? Manufacturer { get; set; }
        public string? SerialNumber { get; set; }
        public string? Model { get; set; }
        public DeviceStatus Status { get; set; } = DeviceStatus.Active;

        // Quan hệ tự tham chiếu: 1 Device có thể có nhiềuAction state Device con
        public Device? ParentDevice { get; set; }

        public ICollection<Device> ChildDevices { get; set; } = new List<Device>();

        // Quan hệ với Rack
        public DeviceRack? Rack { get; set; }

        // Quan hệ 1 - N với ConfigurationItem
        public ICollection<ConfigurationItem> ConfigurationItems { get; set; } = new List<ConfigurationItem>();

        // Quan hệ 1 - N với ServerRental
        public ICollection<ServerRental> ServerRentals { get; set; } = new List<ServerRental>();

        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }

        [NotMapped]
        public string? CalculatedPositionString // Thuộc tính để hiển thị dạng "U10-U11"
        {
            get
            {
                if (UPosition.HasValue && Size > 0)
                {
                    if (Size == 1) return $"U{UPosition.Value}";
                    return $"U{UPosition.Value}-U{UPosition.Value + Size - 1}";
                }
                return null;
            }
        }
    }
}