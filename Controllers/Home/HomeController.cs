using CAE_CRM.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Threading.Tasks;

namespace CAE_CRM.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CAE_CRM_DB");
        }

        public async Task<IActionResult> Index()
        {
            var kpis = new DashboardModel
            {
                TotalComputers = 0,
                TotalPeripherals = 0,
                TotalClassrooms = 0,
                TotalInMaintenance = 0
            };

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmdKpi = new SqlCommand("cae_CRM_GetDashboardMetrics", connection))
                    {
                        cmdKpi.CommandType = CommandType.StoredProcedure;
                        using (var reader = await cmdKpi.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                kpis.TotalComputers = Convert.ToInt32(reader["TotalComputers"]);
                                kpis.TotalPeripherals = Convert.ToInt32(reader["TotalPeripherals"]);
                                kpis.TotalClassrooms = Convert.ToInt32(reader["TotalClassrooms"]);
                                kpis.TotalInMaintenance = Convert.ToInt32(reader["TotalInMaintenance"]);
                            }
                        }
                    }

                    using (var cmdMov = new SqlCommand("cae_CRM_GetRecentMovements", connection))
                    {
                        cmdMov.CommandType = CommandType.StoredProcedure;
                        using (var readerMov = await cmdMov.ExecuteReaderAsync())
                        {
                            while (await readerMov.ReadAsync())
                            {
                                kpis.RecentMovements.Add(new RecentMovementModel
                                {
                                    ClassroomName = readerMov["ClassroomName"]?.ToString() ?? "Sin Asignar",
                                    Desk = readerMov["Desk"] != DBNull.Value ? readerMov["Desk"].ToString() : string.Empty,
                                    ManufacturerSerialNumber = readerMov["@ManufacturerSerialNumber"].ToString(),
                                    UniversityInventoryNumber = readerMov["UniversityInventoryNumber"].ToString(),
                                    MovementType = readerMov["MovementType"].ToString(),
                                    MovementDate = Convert.ToDateTime(readerMov["MovementDate"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "HomeController.Index", ex);

                ViewBag.Warning = "No se pudieron cargar las métricas en tiempo real. Mostrando valores por defecto.";
            }

            return View(kpis);
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

                        string userName = User.Identity?.Name ?? "Usuario_Desconocido";
                        cmdLog.Parameters.AddWithValue("@User", userName);

                        cmdLog.Parameters.AddWithValue("@Source", source);
                        cmdLog.Parameters.AddWithValue("@ErrorMessage", ex.Message);
                        cmdLog.Parameters.AddWithValue("@StackTrace", ex.StackTrace ?? (object)DBNull.Value);

                        string requestPath = HttpContext?.Request?.Path.Value ?? "Ruta Desconocida";
                        cmdLog.Parameters.AddWithValue("@RequestPath", requestPath);

                        await connectionLog.OpenAsync();
                        await cmdLog.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
            }
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
