namespace Rack.Contracts.Authentication;

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public DateTime? DoB { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}
