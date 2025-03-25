using System;

namespace Rack.Domain.Commons.Abstractions
{
    public interface IAuditInfo
    {
        DateTime CreatedOn { get; set; }
        DateTime? LastModifiedOn { get; set; }
    }
}