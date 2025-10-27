using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using WinFormsCrudDemo.Utils;

namespace WinFormsCrudDemo.Data
{
    public static class Db
    {
        private static readonly string _cs =
            ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString;

        // ==========================================
        // READ (GET ALL)
        // ==========================================
        public static DataTable GetAll()
        {
            try
            {
                using var con = new SqlConnection(_cs);
                using var da = new SqlDataAdapter("sp_Empleados_GetAll", con)
                { SelectCommand = { CommandType = CommandType.StoredProcedure } };
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Db.GetAll");
                throw; 
            }
        }

        // ==========================================
        // READ (SEARCH)
        // ==========================================
        public static DataTable SearchByName(string text)
        {
            try
            {
                using (var con = new SqlConnection(_cs))
                using (var da = new SqlDataAdapter("sp_Empleados_SearchByName", con))
                {
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@q", text ?? string.Empty);

                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Db.SearchByName");
                throw;
            }
        }

        // ==========================================
        // CREATE (INSERT)
        // ==========================================
        public static int Insert(string nombre, string puesto, decimal? salario, DateTime fechaAlta)
        {
            try
            {

                using (var con = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("sp_Empleados_Insert", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Puesto", puesto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Salario", salario.HasValue ? (object)salario.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaAlta", fechaAlta);

                    con.Open();
                    var newId = (int)cmd.ExecuteScalar(); // devuelve NewId del SP
                    return newId;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Db.Insert");
                throw; 
            }
        }

        // ==========================================
        // UPDATE
        // ==========================================
        public static void Update(int id, string nombre, string puesto, decimal? salario, DateTime fechaAlta)
        {
            try 
            {
                using (var con = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("sp_Empleados_Update", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdEmpleado", id);
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Puesto", puesto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Salario", salario.HasValue ? (object)salario.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FechaAlta", fechaAlta);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Db.Update");
                throw; 
            }
            
        }

        // ==========================================
        // DELETE
        // ==========================================
        public static void Delete(int id)
        {
            try
            {
                using (var con = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("sp_Empleados_Delete", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IdEmpleado", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Db.Delete");
                throw;
            }
            
        }
    }
}
