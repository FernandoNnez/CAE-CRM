using System.ComponentModel.DataAnnotations;
using CAE_CRM.Models.Classrooms; // Necesario para la lista desplegable de salones

namespace CAE_CRM.Models.Computers
{
    // 1. Modelo para la lista (Panel principal de computadoras)
    public class ComputerListModel
    {
        public int ComputerID { get; set; }
        public string SerialNumber { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Processor { get; set; }
        public int? TotalMemoryGB { get; set; }
        public string Status { get; set; }
        public string Desk { get; set; }

        // El JOIN de SQL nos permite mostrar el nombre del salón en vez del ID
        public string ClassroomName { get; set; }
    }

    // 2. Modelo para el formulario de Creación y Edición
    public class ComputerFormModel
    {
        public int ComputerID { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un salón.")]
        public int ClassroomID { get; set; }

        [Required(ErrorMessage = "El Identificador interno es obligatorio.")]
        public string Desk { get; set; }

        // Propiedad auxiliar para llenar el <select> en la vista
        public IEnumerable<ClassroomListModel>? AvailableClassrooms { get; set; }

        [Required(ErrorMessage = "El número de serie es obligatorio.")]
        public string SerialNumber { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria.")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio.")]
        public string Model { get; set; }

        // --- ALMACENAMIENTO ---
        public string? StorageType { get; set; }
        public int? StorageCapacityGB { get; set; }
        public string? StorageDetails { get; set; }

        // --- MEMORIA RAM ---
        public string? MemoryType { get; set; }
        public string? RamCombination { get; set; }
        public int? TotalMemoryGB { get; set; }
        public string? MemoryDetails { get; set; }

        // --- PROCESADOR Y TARJETA MADRE ---
        public string? MotherboardType { get; set; }
        public string? ProcessorType { get; set; }
        public string? Processor { get; set; }
        public bool HasCooler { get; set; } = true;

        // --- ENERGÍA Y RED ---
        public string? PowerSupplyType { get; set; }
        public int? PowerSupplyWatts { get; set; }
        public string? WifiType { get; set; }
        public bool HasWifi { get; set; } = false;

        // --- PERIFÉRICOS INTEGRADOS ---
        public string? MonitorType { get; set; }
        public string? MonitorSerialNumber { get; set; }
        public bool HasKeyboard { get; set; } = true;
        public bool HasMouse { get; set; } = true;

        // --- ESTADO GENERAL ---
        public string Status { get; set; } = "Active"; // Active, Maintenance, Discarded
        public string? GeneralNotes { get; set; }
    }
}