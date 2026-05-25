using CAE_CRM.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System;

namespace CAE_CRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly string _connectionString;

        public UsersController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CAE_CRM_DB");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = new List<UserListModel>();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("cae_CRM_GetUsers", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                users.Add(new UserListModel
                                {
                                    ID = Convert.ToInt32(reader["ID"]),
                                    User = reader["User"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    Active = Convert.ToBoolean(reader["Active"]),
                                    LastLoginDate = reader["LastLoginDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginDate"]) : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "UsersController.Index", ex);
                TempData["Error"] = "Error al cargar la lista de usuarios.";
            }

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("cae_CRM_CreateUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@User", model.User);
                        command.Parameters.AddWithValue("@Password", passwordHash);
                        command.Parameters.AddWithValue("@Role", model.Role);

                        var outputIdParam = new SqlParameter("@NewID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputIdParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (SqlException ex) when (ex.Message.Contains("already exists"))
            {
                ViewBag.Error = "El nombre de usuario ya está registrado en el sistema.";
                return View(model);
            }
            catch (SqlException sqlEx)
            {
                await LogErrorAsync("Critical", "UsersController.Create", sqlEx);
                ViewBag.Error = "Ocurrió un error de conexión al crear el usuario. Por favor, intenta más tarde.";
                return View(model);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "UsersController.Create", ex);
                ViewBag.Error = "Ocurrió un error inesperado al crear el usuario.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = new EditUserModel();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("cae_CRM_GetUserById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID", id);
                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                model.ID = Convert.ToInt32(reader["ID"]);
                                model.User = reader["User"].ToString();
                                model.Role = reader["Role"].ToString();
                                model.Active = Convert.ToBoolean(reader["Active"]);
                            }
                            else
                            {
                                TempData["Error"] = "El usuario solicitado no existe.";
                                return RedirectToAction("Index");
                            }
                        }
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "UsersController.Edit_GET", ex);
                TempData["Error"] = "Error al intentar cargar la información del usuario.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                string? passwordHash = null;
                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    passwordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("cae_CRM_UpdateUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID", model.ID);
                        command.Parameters.AddWithValue("@User", model.User);
                        command.Parameters.AddWithValue("@Role", model.Role);
                        command.Parameters.AddWithValue("@Active", model.Active);

                        command.Parameters.AddWithValue("@Password", passwordHash ?? (object)DBNull.Value);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex) when (ex.Message.Contains("already exists") || ex.Number == 2627)
            {
                ViewBag.Error = "El nombre de usuario ingresado ya está en uso por otra cuenta.";
                return View(model);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "UsersController.Edit_POST", ex);
                ViewBag.Error = "Ocurrió un error al actualizar el usuario.";
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var currentUserName = User.Identity?.Name;

                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var checkCmd = new SqlCommand("SELECT [User] FROM Users WHERE ID = @ID", connection))
                    {
                        checkCmd.Parameters.AddWithValue("@ID", id);
                        await connection.OpenAsync();
                        var targetUser = (string)await checkCmd.ExecuteScalarAsync();

                        if (targetUser == currentUserName)
                        {
                            TempData["Error"] = "Por seguridad, no puedes darte de baja a ti mismo. Pide a otro Administrador que lo haga.";
                            return RedirectToAction("Index");
                        }
                    }

                    using (var command = new SqlCommand("cae_CRM_DeleteUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ID", id);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "UsersController.Delete", ex);
                TempData["Error"] = "Ocurrió un error al intentar dar de baja al usuario.";
            }

            return RedirectToAction("Index");
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

                        string userName = User.Identity?.Name ?? "Admin_Unknow";
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
            {}
        }
    }
}
