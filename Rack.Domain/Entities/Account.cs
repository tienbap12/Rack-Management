﻿using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rack.Domain.Entities
{
    public class Account : Entity, IAuditInfo, ISoftDelete
    {
        [StringLength(64)]
        public string Username { get; set; } = string.Empty;

        [StringLength(512)]
        public string Password { get; set; } = string.Empty;

        [StringLength(512)]
        public string Salt { get; set; } = string.Empty;

        [StringLength(128)]
        public string FullName { get; set; } = string.Empty;

        public DateTime? DoB { get; set; }

        [StringLength(11)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(128)]
        public string Email { get; set; } = string.Empty;

        [ForeignKey(nameof(Role))]
        public Guid RoleId { get; set; }

        public virtual Role Role { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime? LastModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}