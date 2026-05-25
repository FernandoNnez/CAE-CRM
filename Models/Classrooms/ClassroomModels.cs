using System.ComponentModel.DataAnnotations;

namespace CAE_CRM.Models.Classrooms
{
    public class ClassroomListModel
    {
        public int ClassroomID { get; set; }
        public string Name { get; set; }
        public string LocationDetails { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClassroomFormModel
    {
        public int ClassroomID { get; set; }

        [Required(ErrorMessage = "El nombre del salón es obligatorio.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El edificio es obligatorio")]
        public string LocationDetails { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
