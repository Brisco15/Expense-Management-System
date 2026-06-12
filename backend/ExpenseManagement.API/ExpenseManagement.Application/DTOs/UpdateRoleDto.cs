
using System.ComponentModel.DataAnnotations;
using ExpenseManagement.Domain.Enums;

namespace ExpenseManagement.Application.DTOs
{
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "Role is required.")]
        
        public Role Role { get; set; } = Role.Employee;
    }
}
