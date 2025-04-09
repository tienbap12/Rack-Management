using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rack.Domain.Entities
{
    public class Role : Entity, IAuditInfo
    {
        public string Name { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? LastModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public ICollection<Account> Accounts { get; set; } = new List<Account>();
    }
}