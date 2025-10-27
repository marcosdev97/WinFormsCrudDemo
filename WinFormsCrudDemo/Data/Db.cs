using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WinFormsCrudDemo.Data
{
    public static class Db
    {
        private static readonly string _cs =
            ConfigurationManager.ConnectionStrings["DemoDb"].ConnectionString;

        public static DataTable GetAll()
        {       
            using (var con = new SqlConnection(_cs))
            using (var da = new SqlDataAdapter(
                "SELECT IdEmpleado,Nombre,Puesto,Salario,FechaAlta FROM dbo.Empleados ORDER BY IdEmpleado ASC", con))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public static DataTable SearchByName(string text)
        {
            using (var con = new SqlConnection(_cs))
            using (var da = new SqlDataAdapter(
                @"SELECT IdEmpleado,Nombre,Puesto,Salario,FechaAlta
                  FROM dbo.Empleados
                  WHERE Nombre LIKE @q
                  ORDER BY IdEmpleado DESC;", con))
            {
                da.SelectCommand.Parameters.AddWithValue("@q", $"%{text}%");
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        public static int Insert(string nombre, string puesto, decimal? salario, DateTime fechaAlta)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.Empleados (Nombre,Puesto,Salario,FechaAlta)
          VALUES (@n,@p,@s,@f);
          SELECT CAST(SCOPE_IDENTITY() AS INT);", con))
            {
                cmd.Parameters.AddWithValue("@n", nombre);
                cmd.Parameters.AddWithValue("@p", puesto ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@s", salario.HasValue ? (object)salario.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@f", fechaAlta);
                con.Open();
                var newId = (int)cmd.ExecuteScalar();
                return newId;
            }
        }



        public static void Update(int id, string nombre, string puesto, decimal? salario, DateTime fechaAlta)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(
                @"UPDATE dbo.Empleados
          SET Nombre=@n, Puesto=@p, Salario=@s, FechaAlta=@f
          WHERE IdEmpleado=@id;", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@n", nombre);
                cmd.Parameters.AddWithValue("@p", puesto ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@s", salario.HasValue ? (object)salario.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@f", fechaAlta);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }


        public static void Delete(int id)
        {
            using (var con = new SqlConnection(_cs))
            using (var cmd = new SqlCommand(
                "DELETE FROM dbo.Empleados WHERE IdEmpleado=@id;", con))
            {
                cmd.Parameters.AddWithValue("@id", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}
