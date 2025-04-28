using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rack.Domain.Entities
{
    public class PortConnection : Entity, ISoftDelete, IAuditInfo
    {
        public Guid SourcePortID { get; set; }
        public Guid DestinationPortID { get; set; }

        [ForeignKey(nameof(SourcePortID))]
        public Port SourcePort { get; set; } = null!;

        [ForeignKey(nameof(DestinationPortID))]
        public Port DestinationPort { get; set; } = null!;

        public string? CableType { get; set; } // Ví dụ: Cat6, Fiber OM4, Twinax
        public string? Comment { get; set; } // Thêm ghi chú nếu cần
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }
    }
}