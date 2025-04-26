using System.ComponentModel;
using System.Runtime.Serialization;

namespace Rack.Domain.Enum;

public enum CommonStatus
{
    [EnumMember(Value = "Active")]
    [Description("Entity is active")]
    Active = 1,

    [EnumMember(Value = "Inactive")]
    [Description("Entity is inactive")]
    Inactive = 2
}