using System.ComponentModel.DataAnnotations;
using CAE_CRM.Models.Classrooms;

namespace CAE_CRM.Models.Computers
{
    public class ComputerListModel
    {
        public int ComputerID { get; set; }
        [Required(ErrorMessage = "El número de serie del fabricante es obligatorio.")]
        public string ManufacturerSerialNumber { get; set; }
        [Required(ErrorMessage = "El número de inventario de la universidad es obligatorio.")]
        public string UniversityInventoryNumber { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Processor { get; set; }
        public int? TotalMemoryGB { get; set; }
        public string Status { get; set; }
        public string Desk { get; set; }

        public string ClassroomName { get; set; }
    }

    public class ComputerFormModel
    {
        public int ComputerID { get; set; }

        [Required(ErrorMessage = "Debes seleccionar un salón.")]
        public int ClassroomID { get; set; }

        [Required(ErrorMessage = "El Identificador interno es obligatorio.")]
        public string Desk { get; set; }

        public IEnumerable<ClassroomListModel>? AvailableClassrooms { get; set; }

        [Required(ErrorMessage = "El número de serie del fabricante es obligatorio.")]
        public string ManufacturerSerialNumber { get; set; }
        [Required(ErrorMessage = "El número de inventario de la universidad es obligatorio.")]
        public string UniversityInventoryNumber { get; set; }

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