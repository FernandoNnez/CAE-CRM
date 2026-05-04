using CAE_CRM.Models.Classrooms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System;

namespace CAE_CRM.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClassroomsController : Controller
    {
        private readonly string _connectionString;

        public ClassroomsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("CAE_CRM_DB");
        }

        // ==========================================
        // INDEX: Tabla principal
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index()
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
                                list.Add(new ClassroomListModel
                                {
                                    ClassroomID = Convert.ToInt32(reader["ClassroomID"]),
                                    Name = reader["Name"].ToString(),
                                    LocationDetails = reader["LocationDetails"].ToString(),
                                    IsActive = Convert.ToBoolean(reader["IsActive"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ClassroomsController.Index", ex);
                TempData["Error"] = "Error al cargar la lista de salones.";
            }
            return View(list);
        }

        // ==========================================
        // CREATE: Crear nuevo salón
        // ==========================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ClassroomFormModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClassroomFormModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_CreateClassroom", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", model.Name);
                        cmd.Parameters.AddWithValue("@LocationDetails", model.LocationDetails ?? (object)DBNull.Value);

                        var outParam = new SqlParameter("@NewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(outParam);

                        await connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (SqlException ex) when (ex.Message.Contains("already exists"))
            {
                ViewBag.Error = "Ya existe un salón con ese nombre.";
                return View(model);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ClassroomsController.Create", ex);
                ViewBag.Error = "Ocurrió un error al guardar el salón.";
                return View(model);
            }
        }

        // ==========================================
        // EDIT: Editar salón existente
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = new ClassroomFormModel();
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_GetClassroomById", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClassroomID", id);
                        await connection.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                model.ClassroomID = Convert.ToInt32(reader["ClassroomID"]);
                                model.Name = reader["Name"].ToString();
                                model.LocationDetails = reader["LocationDetails"].ToString();
                                model.IsActive = Convert.ToBoolean(reader["IsActive"]);
                            }
                            else
                            {
                                TempData["Error"] = "Salón no encontrado.";
                                return RedirectToAction("Index");
                            }
                        }
                    }
                }
                return View(model);
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ClassroomsController.Edit_GET", ex);
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ClassroomFormModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_UpdateClassroom", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClassroomID", model.ClassroomID);
                        cmd.Parameters.AddWithValue("@Name", model.Name);
                        cmd.Parameters.AddWithValue("@LocationDetails", model.LocationDetails ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                        await connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ClassroomsController.Edit_POST", ex);
                ViewBag.Error = "Error al actualizar el salón.";
                return View(model);
            }
        }

        // ==========================================
        // DELETE: Baja de salón
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("cae_CRM_DeleteClassroom", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ClassroomID", id);
                        await connection.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await LogErrorAsync("Error", "ClassroomsController.Delete", ex);
                TempData["Error"] = "Ocurrió un error al dar de baja el salón.";
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
