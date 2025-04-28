using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using System;
using System.Collections.Generic;

namespace Rack.Domain.Entities;

public class Device : Entity, IAuditInfo, ISoftDelete
{
    public Guid? ParentDeviceID { get; set; }
    public Guid? RackID { get; set; }
    public int Size { get; set; }
    public string Name { get; set; } = null!;
    public string IpAddress { get; set; } = null!;
    public int? UPosition { get; set; }
    public string? LinkIdPage { get; set; }
    public string? SlotInParent { get; set; }
    public string DeviceType { get; set; } = null!;
    public string? Manufacturer { get; set; }
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Active;

    // Quan hệ tự tham chiếu
    public Device? ParentDevice { get; set; }

    public ICollection<Device> ChildDevices { get; set; } = new List<Device>();

    // Quan hệ với Rack
    public DeviceRack? Rack { get; set; }

    // Quan hệ 1-N với Card
    public ICollection<Card> Cards { get; set; } = new List<Card>();

    // Quan hệ 1-N với Port (cho các port trực tiếp thuộc về Device)
    public ICollection<Port> Ports { get; set; } = new List<Port>();

    // Quan hệ 1-N với ConfigurationItem
    public ICollection<ConfigurationItem> ConfigurationItems { get; set; } = new List<ConfigurationItem>();

    // Quan hệ 1-N với ServerRental
    public ICollection<ServerRental> ServerRentals { get; set; } = new List<ServerRental>();

    // Thuộc tính audit và soft delete
    public bool IsDeleted { get; set; }

    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime? LastModifiedOn { get; set; }
    public string? LastModifiedBy { get; set; }
}