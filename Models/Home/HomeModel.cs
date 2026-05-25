namespace CAE_CRM.Models.Home
{
    public class DashboardModel
    {
        public int TotalComputers { get; set; }
        public int TotalPeripherals { get; set; }
        public int TotalClassrooms { get; set; }
        public int TotalInMaintenance { get; set; }
        public List<RecentMovementModel> RecentMovements { get; set; } = new List<RecentMovementModel>();
    }
    public class RecentMovementModel
    {
        public string ClassroomName { get; set; }
        public string Desk {  get; set; }
        public string ManufacturerSerialNumber { get; set; }
        public string UniversityInventoryNumber { get; set; }
        public string MovementType { get; set; }
        public DateTime MovementDate { get; set; }
    }
}
