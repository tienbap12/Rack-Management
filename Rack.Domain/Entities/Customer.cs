using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;
using System.Collections.Generic;

namespace Rack.Domain.Entities;

public class Customer : Entity, IAuditInfo, ISoftDelete
{
    public string Name { get; set; } = null!;
    public string? ContactInfo { get; set; } = string.Empty;

    // Quan hệ 1 - N với ServerRental
    public ICollection<ServerRental> ServerRentals { get; set; } = new List<ServerRental>();

    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public string LastModifiedBy { get; set; }
}