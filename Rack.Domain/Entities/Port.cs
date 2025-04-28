using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;

namespace Rack.Domain.Entities;

public class Port : Entity, IAuditInfo, ISoftDelete
{
    public Guid DeviceID { get; set; }
    public Guid? CardID { get; set; }
    public string PortName { get; set; } = null!;
    public string PortType { get; set; } = null!;
    public Device Device { get; set; } = null!;
    public Card? Card { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public string LastModifiedBy { get; set; }
}