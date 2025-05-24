using System.ComponentModel.DataAnnotations;

namespace Rack.Contracts.Authentication;

public class CreateAccountRequest
{
    [Required(ErrorMessage = "Username là bắt buộc")]
    [StringLength(64, ErrorMessage = "Username không được vượt quá 64 ký tự")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    public string Password { get; set; }

    [StringLength(128, ErrorMessage = "Họ tên không được vượt quá 128 ký tự")]
    public string FullName { get; set; }

    public DateTime? DoB { get; set; }

    [StringLength(11, ErrorMessage = "Số điện thoại không được vượt quá 11 ký tự")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(128, ErrorMessage = "Email không được vượt quá 128 ký tự")]
    public string Email { get; set; }

    [Required(ErrorMessage = "RoleId là bắt buộc")]
    public Guid RoleId { get; set; }
}