﻿using Rack.Domain.Commons.Primitives;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rack.Domain.Entities
{
    public class RefreshToken : Entity
    {
        public Guid UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public Account Account { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpiryDate { get; set; }
    }
}