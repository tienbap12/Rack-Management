using System.ComponentModel;
using System.Runtime.Serialization;

namespace Rack.Domain.Enum;

public enum ComponentStatus
{
    [EnumMember(Value = "Available")]
    [Description("Component is available in inventory")]
    Available = 1,

    [EnumMember(Value = "InUse")]
    [Description("Component is currently in use")]
    InUse = 2,

    [EnumMember(Value = "Faulty")]
    [Description("Component is faulty")]
    Faulty = 3,

    [EnumMember(Value = "Retired")]
    [Description("Component is retired")]
    Retired = 4,

    [EnumMember(Value = "UnderMaintenance")]
    [Description("Component is under maintenance")]
    UnderMaintenance = 5,

    [EnumMember(Value = "Allocated")]
    [Description("Component is allocated, awaiting installation")]
    Allocated = 6
}