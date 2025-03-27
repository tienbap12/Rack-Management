using System;

namespace Rack.Domain.Commons.Abstractions
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        string DeletedBy { get; set; }
        DateTime? DeletedOn { get; set; }
    }
}
