using Rack.Domain.Entities;

namespace Rack.Application.Commons.Abstractions;

public interface IJwtProvider{
    string Generate(Account req);
}