using System.ComponentModel.DataAnnotations;

namespace PKIBSEP.Dtos.Keys
{
    public class SavePublicKeyDto
    {
        [Required(ErrorMessage = "Javni ključ je obavezan")]
        public string PublicKeyPem { get; set; } = string.Empty;
    }
}
