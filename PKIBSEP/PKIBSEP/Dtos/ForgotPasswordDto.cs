using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Dtos
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email je obavezan")]
        [EmailAddress(ErrorMessage = "Neispravna email adresa")]
        public string Email { get; set; } = string.Empty;
    }
}
