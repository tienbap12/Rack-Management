using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;
using System.Collections.Generic;

namespace Rack.Domain.Entities;

public class DataCenter : Entity, IAuditInfo, ISoftDelete
{
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Quan hệ 1 - N với Rack
    public ICollection<DeviceRack> Racks { get; set; } = new List<DeviceRack>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public string LastModifiedBy { get; set; }
}