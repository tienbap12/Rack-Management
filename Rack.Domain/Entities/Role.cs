using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using System;
using System.Collections.Generic;

namespace Rack.Domain.Entities
{
    public class Role : Entity, IAuditInfo, ISoftDelete
    {
        public string Name { get; set; }
        public CommonStatus Status { get; set; } = CommonStatus.Active;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string LastModifiedBy { get; set; }
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}