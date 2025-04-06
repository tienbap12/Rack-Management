using Microsoft.EntityFrameworkCore;
using Rack.Domain.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Rack.Domain.Commons.Primitives
{
    public abstract class Entity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [NotMapped]
        [JsonIgnore]
        public virtual EntityState? CurrentStateTracker { get; set; }

        // Thêm các thành phần Domain Events
        private readonly List<IDomainEvent> _domainEvents = new();

        [NotMapped]
        [JsonIgnore]
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);

        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}