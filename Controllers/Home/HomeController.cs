using CAE_CRM.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System;
using System.Threading.Tasks;

namespace CAE_CRM.Controllers
{
    [Authorize] // Exige que el usuario tenga sesión iniciada
    public class HomeController : Controller
    {
        private readonly string _connectionString;

        public HomeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CAE_CRM_DB");
        }

        public async Task<IActionResult> Index()
        {
            // Inicializamos en ceros. Así, si la BD falla, la pantalla se muestra con 0s en lugar de tronar
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

                    // 1. LEER LOS KPIs (Lo que ya tenías)
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
                    } // Se cierra el primer DataReader

                    // 2. LEER LOS ÚLTIMOS MOVIMIENTOS
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
                                    Desk = readerMov["Desk"] != DBNull.Value ? readerMov["Desk"].ToString() : string.Empty, // NUEVO CAMPO
                                    SerialNumber = readerMov["SerialNumber"].ToString(),
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
                // Registramos el error en la base de datos indicando que ocurrió en HomeController.Index
                await LogErrorAsync("Error", "HomeController.Index", ex);

                // Opcional: Mandar un mensaje a la vista para avisarle al usuario que los datos podrían no estar actualizados
                ViewBag.Warning = "No se pudieron cargar las métricas en tiempo real. Mostrando valores por defecto.";
            }

            // Enviamos el modelo dinámico a tu vista
            return View(kpis);
        }

        // ====================================================================
        // MÉTODO PRIVADO PARA REGISTRAR ERRORES EN LA BD
        // ====================================================================
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

                        // Rescatamos el nombre del usuario que está conectado viendo el Dashboard
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

        [AllowAnonymous] // Importante para que cualquiera pueda ver este mensaje
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
