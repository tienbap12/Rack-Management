using System.ComponentModel;
using System.Runtime.Serialization;

namespace Rack.Domain.Enum;

public enum DeviceStatus
{
    [EnumMember(Value = "Pending")] Pending = 1,
    [EnumMember(Value = "Active")] Active = 2,
    [EnumMember(Value = "Suspended")] Suspended = 3,
    [EnumMember(Value = "Terminated")] Terminated = 4,
    [EnumMember(Value = "Inventory")] Inventory = 5,
    [EnumMember(Value = "Maintenance")] Maintenance = 6,
    [EnumMember(Value = "Faulty")] Faulty = 7,
    [EnumMember(Value = "Retired")] Retired = 8
}