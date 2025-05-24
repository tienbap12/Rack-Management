using System.ComponentModel;
using System.Runtime.Serialization;

namespace Rack.Domain.Enum;

public enum CommonStatus
{
    [EnumMember(Value = "active")]
    active = 0,

    [EnumMember(Value = "inactive")]
    inactive = 1
}