using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Npgsql;

namespace BarCocktails
{
    public class MyORM:MyDataContext
    {
        private static string connectionString { get; set; } = "Server=127.0.0.1;User Id=user1;Password=qwerty654321;Port=5432;Database=itis1;";
        private NpgsqlConnection connection;
        public MyORM() 
        {
            connection = new NpgsqlConnection(connectionString);
        }
        public MyORM(string connectionString)
        {
            MyORM.connectionString = connectionString;
            connection = new NpgsqlConnection(connectionString);
        }

        public bool Add<T>(T elem)
        {
            /*
                INSERT INTO public.products (
                product_id, name, price, count) VALUES(
                '1'::integer, 'apple'::character varying, '12.12'::double precision, '1'::integer)
                returning product_id;
            */
            connection.Open();
            var type = elem.GetType();
            var tableName = type.Name; // Имя таблицы предполагается как имя класса
            var properties = type.GetProperties();
            var columnNames = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ", properties.Select(p => $"'{p.GetValue(elem).ToString().Replace(',', '.')}'"));
            var query = $"INSERT INTO public.{tableName} ({columnNames}) VALUES ({values}) returning id;";
            Console.WriteLine(query);
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            int result = cmd.ExecuteNonQuery();
            connection.Close();
            return result > 0;
        }
        public bool Update<T>(T elem)
        {
            connection.Open();
            var type = elem.GetType();
            var tableName = type.Name; // Имя таблицы предполагается как имя класса
            var properties = type.GetProperties();
            int id = (int)type.GetProperty($"{tableName.Substring(0, tableName.Length - 1)}_id").GetValue(elem);
            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = '{p.GetValue(elem).ToString().Replace(',', '.')}'"));
            var query = $"UPDATE {tableName} SET {setClause} WHERE {tableName.Substring(0, tableName.Length - 1)}_id = {id};";
            Console.WriteLine(query);
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            int result = cmd.ExecuteNonQuery();
            connection.Close();
            return result > 0;
        }

        public bool Delete<T>(int id)
        {
            connection.Open();
            var type = typeof(T);
            var tableName = type.Name; // Имя таблицы предполагается как имя класса
            var query = $"DELETE FROM {tableName} WHERE id = {id};";
            Console.WriteLine(query);
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            int result = cmd.ExecuteNonQuery();
            connection.Close();
            return result > 0;
        }
        public List<T> Select<T>(T elem)
        {
            var type = elem.GetType();
            var tableName = type.Name; // Имя таблицы предполагается как имя класса
            var properties = type.GetProperties();
            var whereClause = string.Join(" AND ", properties.Select(p => $"{p.Name} = '{p.GetValue(elem).ToString().Replace(',', '.')}'"));
            var query = $"SELECT * FROM {tableName} WHERE {whereClause};";
            Console.WriteLine(query);
            connection.Open();
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            List<T> results = new List<T>();
            while (reader.Read())
            {
                T obj = Activator.CreateInstance<T>();
                foreach (var prop in properties)
                {
                    prop.SetValue(obj, reader[prop.Name]);
                }
                results.Add(obj);
            }
            connection.Close();
            return results;
        }
        public T SelectById<T>(int id)
        {
            var type = typeof(T);
            var tableName = type.Name; // Имя таблицы предполагается как имя класса
            var properties = type.GetProperties();
            var query = $"SELECT * FROM {tableName} WHERE  \"id\" = {id};";
            Console.WriteLine(query);
            connection.Open();
            NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            NpgsqlDataReader reader = cmd.ExecuteReader();
            T result = Activator.CreateInstance<T>(); ;
            while (reader.Read())
            {
                foreach (var prop in properties)
                {
                    prop.SetValue(result, reader[prop.Name]);
                }
                break;
            }
            connection.Close();
            return result;
        }
    }
}
