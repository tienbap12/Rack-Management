using System.ComponentModel;
using System.Runtime.Serialization;

namespace Rack.Domain.Enum;

public enum Role
{
    [EnumMember(Value = "Admin")]
    [Description("Administrator with full access")]
    Admin = 1,

    [EnumMember(Value = "User")]
    [Description("Standard user")]
    User = 2,

    [EnumMember(Value = "Technician")]
    [Description("Technical staff")]
    Technician = 3
}