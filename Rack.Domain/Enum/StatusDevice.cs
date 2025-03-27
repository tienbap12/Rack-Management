using System;

namespace Rack.Domain.Enum
{
    public readonly struct StatusDevice(string statusState)
    {
        public const string PENDING = "PENDING";
        public const string ACTIVE = "ACTIVE";
        public const string COMPLETED = "COMPLETED";
        public const string SUSPENDED = "SUSPENDED";
        public const string TERMINATED = "TERMINATED";
        public string Current
        {
            get
            {
                return statusState switch
                {
                    PENDING => PENDING,
                    ACTIVE => ACTIVE,
                    COMPLETED => COMPLETED,
                    SUSPENDED => SUSPENDED,
                    TERMINATED => TERMINATED,
                    _ => throw new InvalidOperationException($"Status Device not supported")
                };

            }
        }
    }
}
