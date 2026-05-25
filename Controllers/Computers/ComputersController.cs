using CAE_CRM.Models.Classrooms;
using CAE_CRM.Models.Computers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using System.Data;
using System;

namespace CAE_CRM.Controllers
{
    [Authorize(Roles = "Admin, Standard")]
    public class ComputersController : Controller
    {
        private readonly string _connectionString;

        public ComputersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CAE_CRM_DB");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = new List<ComputerListModel>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_GetComputers", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        await connection.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                list.Add(new ComputerListModel
                                {
                                    ComputerID = Convert.ToInt32(reader["ComputerID"]),
                                    ManufacturerSerialNumber = reader["ManufacturerSerialNumber"].ToString(),
                                    UniversityInventoryNumber = reader["UniversityInventoryNumber"].ToString(),
                                    Brand = reader["Brand"].ToString(),
                                    Model = reader["Model"].ToString(),
                                    Processor = reader["Processor"].ToString(),
                                    TotalMemoryGB = reader["TotalMemoryGB"] != DBNull.Value ? Convert.ToInt32(reader["TotalMemoryGB"]) : null,
                                    Status = reader["Status"].ToString(),
                                    ClassroomName = reader["ClassroomName"]?.ToString() ?? "Sin Asignar",
                                    Desk = reader["Desk"] != DBNull.Value ? reader["Desk"].ToString() : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ComputersController.Index", ex);
                TempData["Error"] = "Error al cargar la lista de computadoras.";
            }
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ComputerFormModel
            {
                AvailableClassrooms = await GetAvailableClassroomsAsync()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ComputerFormModel formData)
        {
            if (!ModelState.IsValid)
            {
                formData.AvailableClassrooms = await GetAvailableClassroomsAsync();
                return View(formData);
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_CreateComputer", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ClassroomID", formData.ClassroomID);
                        cmd.Parameters.AddWithValue("@Desk", formData.Desk ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ManufacturerSerialNumber", formData.ManufacturerSerialNumber);
                        cmd.Parameters.AddWithValue("@UniversityInventoryNumber", formData.UniversityInventoryNumber); cmd.Parameters.AddWithValue("@Brand", formData.Brand);
                        cmd.Parameters.AddWithValue("@Model", formData.Model);

                        cmd.Parameters.AddWithValue("@StorageType", formData.StorageType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@StorageCapacityGB", formData.StorageCapacityGB ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@StorageDetails", formData.StorageDetails ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@MemoryType", formData.MemoryType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalMemoryGB", formData.TotalMemoryGB ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MemoryDetails", formData.MemoryDetails ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RamCombination", formData.RamCombination ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@MotherboardType", formData.MotherboardType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ProcessorType", formData.ProcessorType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Processor", formData.Processor ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HasCooler", formData.HasCooler);

                        cmd.Parameters.AddWithValue("@PowerSupplyType", formData.PowerSupplyType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PowerSupplyWatts", formData.PowerSupplyWatts ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HasWifi", formData.HasWifi);
                        cmd.Parameters.AddWithValue("@WifiType", formData.WifiType ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@MonitorType", formData.MonitorType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MonitorSerialNumber", formData.MonitorSerialNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HasKeyboard", formData.HasKeyboard);
                        cmd.Parameters.AddWithValue("@HasMouse", formData.HasMouse);

                        cmd.Parameters.AddWithValue("@Status", formData.Status);
                        cmd.Parameters.AddWithValue("@GeneralNotes", formData.GeneralNotes ?? (object)DBNull.Value);

                        await connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ComputersController.Create", ex);
                ViewBag.Error = $"Error del Sistema: {ex.Message}";
                formData.AvailableClassrooms = await GetAvailableClassroomsAsync();
                return View(formData);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = new ComputerFormModel();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_GetComputerById", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ComputerID", id);

                        await connection.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                model.ComputerID = Convert.ToInt32(reader["ComputerID"]);
                                model.ClassroomID = Convert.ToInt32(reader["ClassroomID"]);
                                model.Desk = reader["Desk"] != DBNull.Value ? reader["Desk"].ToString() : null;
                                model.ManufacturerSerialNumber = reader["ManufacturerSerialNumber"].ToString();
                                model.UniversityInventoryNumber = reader["UniversityInventoryNumber"].ToString();
                                model.Brand = reader["Brand"].ToString();
                                model.Model = reader["Model"].ToString();

                                model.StorageType = reader["StorageType"] != DBNull.Value ? reader["StorageType"].ToString() : null;
                                model.StorageCapacityGB = reader["StorageCapacityGB"] != DBNull.Value ? Convert.ToInt32(reader["StorageCapacityGB"]) : null;
                                model.StorageDetails = reader["StorageDetails"] != DBNull.Value ? reader["StorageDetails"].ToString() : null;

                                model.MemoryType = reader["MemoryType"] != DBNull.Value ? reader["MemoryType"].ToString() : null;
                                model.TotalMemoryGB = reader["TotalMemoryGB"] != DBNull.Value ? Convert.ToInt32(reader["TotalMemoryGB"]) : null;
                                model.MemoryDetails = reader["MemoryDetails"] != DBNull.Value ? reader["MemoryDetails"].ToString() : null;
                                model.RamCombination = reader["RamCombination"] != DBNull.Value ? reader["RamCombination"].ToString() : null;

                                model.MotherboardType = reader["MotherboardType"] != DBNull.Value ? reader["MotherboardType"].ToString() : null;
                                model.ProcessorType = reader["ProcessorType"] != DBNull.Value ? reader["ProcessorType"].ToString() : null;
                                model.Processor = reader["Processor"] != DBNull.Value ? reader["Processor"].ToString() : null;
                                model.HasCooler = Convert.ToBoolean(reader["HasCooler"]);

                                model.PowerSupplyType = reader["PowerSupplyType"] != DBNull.Value ? reader["PowerSupplyType"].ToString() : null;
                                model.PowerSupplyWatts = reader["PowerSupplyWatts"] != DBNull.Value ? Convert.ToInt32(reader["PowerSupplyWatts"]) : null;
                                model.HasWifi = Convert.ToBoolean(reader["HasWifi"]);
                                model.WifiType = reader["WifiType"] != DBNull.Value ? reader["WifiType"].ToString() : null;

                                model.MonitorType = reader["MonitorType"] != DBNull.Value ? reader["MonitorType"].ToString() : null;
                                model.MonitorSerialNumber = reader["MonitorSerialNumber"] != DBNull.Value ? reader["MonitorSerialNumber"].ToString() : null;
                                model.HasKeyboard = Convert.ToBoolean(reader["HasKeyboard"]);
                                model.HasMouse = Convert.ToBoolean(reader["HasMouse"]);

                                model.Status = reader["Status"].ToString();
                                model.GeneralNotes = reader["GeneralNotes"] != DBNull.Value ? reader["GeneralNotes"].ToString() : null;
                            }
                            else
                            {
                                TempData["Error"] = "Equipo no encontrado.";
                                return RedirectToAction("Index");
                            }
                        }
                    }
                }

                model.AvailableClassrooms = await GetAvailableClassroomsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ComputersController.Edit_GET", ex);
                TempData["Error"] = "Error al intentar cargar la información del equipo.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] ComputerFormModel formData)
        {
            if (!ModelState.IsValid)
            {
                formData.AvailableClassrooms = await GetAvailableClassroomsAsync();
                return View(formData);
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_UpdateComputer", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@ComputerID", formData.ComputerID);
                        cmd.Parameters.AddWithValue("@ClassroomID", formData.ClassroomID);
                        cmd.Parameters.AddWithValue("@Desk", formData.Desk ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ManufacturerSerialNumber", formData.ManufacturerSerialNumber);
                        cmd.Parameters.AddWithValue("@UniversityInventoryNumber", formData.UniversityInventoryNumber);
                        cmd.Parameters.AddWithValue("@Brand", formData.Brand);
                        cmd.Parameters.AddWithValue("@Model", formData.Model);

                        cmd.Parameters.AddWithValue("@StorageType", formData.StorageType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@StorageCapacityGB", formData.StorageCapacityGB ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@StorageDetails", formData.StorageDetails ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@MemoryType", formData.MemoryType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TotalMemoryGB", formData.TotalMemoryGB ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MemoryDetails", formData.MemoryDetails ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@RamCombination", formData.RamCombination ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@MotherboardType", formData.MotherboardType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ProcessorType", formData.ProcessorType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Processor", formData.Processor ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HasCooler", formData.HasCooler);

                        cmd.Parameters.AddWithValue("@PowerSupplyType", formData.PowerSupplyType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@PowerSupplyWatts", formData.PowerSupplyWatts ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HasWifi", formData.HasWifi);
                        cmd.Parameters.AddWithValue("@WifiType", formData.WifiType ?? (object)DBNull.Value);

                        cmd.Parameters.AddWithValue("@MonitorType", formData.MonitorType ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@MonitorSerialNumber", formData.MonitorSerialNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@HasKeyboard", formData.HasKeyboard);
                        cmd.Parameters.AddWithValue("@HasMouse", formData.HasMouse);

                        cmd.Parameters.AddWithValue("@Status", formData.Status);
                        cmd.Parameters.AddWithValue("@GeneralNotes", formData.GeneralNotes ?? (object)DBNull.Value);

                        await connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ComputersController.Edit_POST", ex);
                ViewBag.Error = $"Error del Sistema: {ex.Message}";
                formData.AvailableClassrooms = await GetAvailableClassroomsAsync();
                return View(formData);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_DeleteComputer", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ComputerID", id);
                        await connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ComputersController.Delete", ex);
                TempData["Error"] = "Ocurrió un error al intentar dar de baja el equipo.";
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_RestoreComputer", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ComputerID", id);
                        await connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ComputersController.Restore", ex);
                TempData["Error"] = "Ocurrió un error al intentar rehabilitar el equipo.";
            }

            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<ClassroomListModel>> GetAvailableClassroomsAsync()
        {
            var list = new List<ClassroomListModel>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_GetClassrooms", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        await connection.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                if (Convert.ToBoolean(reader["IsActive"]))
                                {
                                    list.Add(new ClassroomListModel
                                    {
                                        ClassroomID = Convert.ToInt32(reader["ClassroomID"]),
                                        Name = reader["Name"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch {}
            return list;
        }
        private async Task LogErrorAsync(string severity, string source, Exception ex)
        {
            try
            {
                using (var connectionLog = new SqlConnection(_connectionString))
                {
                    using (var cmdLog = new SqlCommand("cae_CRM_InsertErrorLog", connectionLog))
                    {
                        cmdLog.CommandType = CommandType.StoredProcedure;
                        cmdLog.Parameters.AddWithValue("@Severity", severity);
                        cmdLog.Parameters.AddWithValue("@User", User.Identity?.Name ?? "Desconocido");
                        cmdLog.Parameters.AddWithValue("@Source", source);
                        cmdLog.Parameters.AddWithValue("@ErrorMessage", ex.Message);
                        cmdLog.Parameters.AddWithValue("@StackTrace", ex.StackTrace ?? (object)DBNull.Value);
                        cmdLog.Parameters.AddWithValue("@RequestPath", HttpContext?.Request?.Path.Value ?? "Desconocido");

                        await connectionLog.OpenAsync();
                        await cmdLog.ExecuteNonQueryAsync();
                    }
                }
            }
            catch { }
        }
    }
}
