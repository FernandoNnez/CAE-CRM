using CAE_CRM.Models.Login;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Data;
using BCrypt.Net;
using System;
using Microsoft.AspNetCore.Authorization;

namespace CAE_CRM.Controllers.Login
{
    public class LoginController : Controller
    {
        private readonly string _connectionString;

        public LoginController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CAE_CRM_DB");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View("LoginView");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid) return View("LoginView", model);

            string hashDB = null;
            string userRol = "Standard";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var command = new SqlCommand("cae_CRM_GetUserIdentity", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@User", model.User);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                hashDB = reader["Password"].ToString();
                                userRol = reader["Role"].ToString();
                            }
                        }
                    }
                }

                if (hashDB != null && BCrypt.Net.BCrypt.Verify(model.Password, hashDB))
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.User),
                new Claim(ClaimTypes.Role, userRol)
            };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    using (var connectionUpdate = new SqlConnection(_connectionString))
                    {
                        using (var cmdUpdate = new SqlCommand("cae_CRM_UpdateLastLogin", connectionUpdate))
                        {
                            cmdUpdate.CommandType = CommandType.StoredProcedure;
                            cmdUpdate.Parameters.AddWithValue("@User", model.User);
                            await connectionUpdate.OpenAsync();
                            await cmdUpdate.ExecuteNonQueryAsync();
                        }
                    }

                    return RedirectToAction("Index", "Home");
                }

                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View("LoginView", model);
            }
            catch (SqlException sqlEx)
            {
                await LogErrorAsync("Critical", model.User, sqlEx);
                ViewBag.Error = "Error de conexión con el servidor. Por favor, intenta más tarde.";
                return View("LoginView", model);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", model.User, ex);
                ViewBag.Error = "Ocurrió un error inesperado al procesar tu solicitud.";
                return View("LoginView", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }

        private async Task LogErrorAsync(string severity, string userName, Exception ex)
        {
            try
            {
                using (var connectionLog = new SqlConnection(_connectionString))
                {
                    using (var cmdLog = new SqlCommand("cae_CRM_InsertErrorLog", connectionLog))
                    {
                        cmdLog.CommandType = CommandType.StoredProcedure;

                        cmdLog.Parameters.AddWithValue("@Severity", severity);
                        cmdLog.Parameters.AddWithValue("@User", string.IsNullOrEmpty(userName) ? (object)DBNull.Value : userName);
                        cmdLog.Parameters.AddWithValue("@Source", "LoginController.Login");
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
        [HttpGet("Login/GenerarHash")]
        public IActionResult GenerarHash(string txt)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return Content("Por favor, pon tu contraseña en la URL así: /Login/GenerarHash?txt=Wiskas");
            }

            string hashSeguro = BCrypt.Net.BCrypt.HashPassword(txt);

            return Content($"Tu contraseña: {txt}\nTu Hash para SQL: {hashSeguro}");
        }
        
    }
}
