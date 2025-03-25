using Rack.Domain.Entities;

namespace Rack.Domain.Commons.Abstractions
{
    public interface IPasswordHashChecker
    {
        bool HashesMatch(string providerPassword, Account user);
    }
}