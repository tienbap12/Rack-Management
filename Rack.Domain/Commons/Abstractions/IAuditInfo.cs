using System;

namespace Rack.Domain.Commons.Abstractions
{
    public interface IAuditInfo
    {
        DateTime CreatedOn { get; set; }
        string CreatedBy { get; set; }
        DateTime? LastModifiedOn { get; set; }
        string LastModifiedBy { get; set; }
    }
}