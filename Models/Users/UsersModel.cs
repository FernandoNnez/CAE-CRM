using System.ComponentModel.DataAnnotations;

namespace CAE_CRM.Models.Users
{
    public class UserModel
    {
        public int ID { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }
    public class UserListModel
    {
        public int ID { get; set; }
        public string User { get; set; }
        public string Role { get; set; }
        public bool Active { get; set; }
        public DateTime? LastLoginDate { get; set; }
    }

    public class CreateUserModel
    {
        [Required(ErrorMessage = "El Usuario es requerido.")]
        public string User { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; }

        public string Role { get; set; } = "Standard";
    }

    public class EditUserModel
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        public string User { get; set; }

        public string Role { get; set; }

        public bool Active { get; set; }

        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string? NewPassword { get; set; }
    }
}
