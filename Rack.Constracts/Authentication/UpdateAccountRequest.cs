using System.ComponentModel.DataAnnotations;

namespace Rack.Contracts.Authentication;

public class UpdateAccountRequest
{
    [StringLength(128, ErrorMessage = "Họ tên không được vượt quá 128 ký tự")]
    public string FullName { get; set; }

    public DateTime? DoB { get; set; }

    [StringLength(11, ErrorMessage = "Số điện thoại không được vượt quá 11 ký tự")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(128, ErrorMessage = "Email không được vượt quá 128 ký tự")]
    public string Email { get; set; }

    public Guid? RoleId { get; set; }

    // Optional password update
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    public string NewPassword { get; set; }
}