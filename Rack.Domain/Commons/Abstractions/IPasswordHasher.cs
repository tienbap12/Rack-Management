namespace Rack.Application.Commons.Abstractions
{
    public interface IPasswordHasher
    {
        (string, string) HashPassword(string password);
    }
}