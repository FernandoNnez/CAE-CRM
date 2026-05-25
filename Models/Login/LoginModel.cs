using System.ComponentModel.DataAnnotations;

namespace CAE_CRM.Models.Login
{
    public class LoginModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string User { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
