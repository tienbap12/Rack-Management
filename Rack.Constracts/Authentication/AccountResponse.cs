namespace Rack.Contracts.Authentication;

public class AccountResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public DateTime? DoB { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string RoleName { get; set; }
    public Guid RoleId { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool IsDeleted { get; set; }
}