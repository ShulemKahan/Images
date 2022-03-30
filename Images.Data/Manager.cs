using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Images.Data
{
   public class Manager
    {
        private readonly string _connectionString;

        public Manager(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(Image image)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Images VALUES (@name, @password, @views) SELECT SCOPE_IDENTITY()";
            cmd.Parameters.AddWithValue("@name", image.Name);
            cmd.Parameters.AddWithValue("@password", image.Password);
            cmd.Parameters.AddWithValue("@views", image.Views);
            conn.Open();
            image.Id = (int)(decimal)cmd.ExecuteScalar();
            
        }

        public Image GetImage(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Images WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            Image image = GetFromReader(reader);
            return image;
        }
        private Image GetFromReader(SqlDataReader reader)
        {
            var image = new Image();
            image.Id = (int)reader["Id"];
            image.Name = (string)reader["Name"];
            image.Password = (string)reader["Password"];
            image.Views = (int)reader["Views"];
         
            return image;
        }

        public void IncrementViewCount(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "UPDATE Images SET Views = Views + 1 WHERE Id = @id";
            cmd.Parameters.AddWithValue("@id", id);
            connection.Open();
            cmd.ExecuteNonQuery();
        }

    }
}
