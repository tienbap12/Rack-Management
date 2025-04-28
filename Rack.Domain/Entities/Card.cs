using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;
using System.Collections.Generic;

namespace Rack.Domain.Entities;

public class Card : Entity, IAuditInfo, ISoftDelete
{
    public Guid DeviceID { get; set; }
    public string CardType { get; set; } = null!;
    public string CardName { get; set; } = null!;
    public Device Device { get; set; } = null!;
    public string? SerialNumber { get; set; }
    public ICollection<Port> Ports { get; set; } = new List<Port>();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public string LastModifiedBy { get; set; }
}