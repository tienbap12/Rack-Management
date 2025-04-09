using Rack.Domain.Entities;

namespace Rack.Domain.Commons.Abstractions;

public interface IJwtProvider
{
    string Generate(Account req);
}