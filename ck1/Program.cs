using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace ck1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Введіть імя:");
            string name = Console.ReadLine();
            string apiUrl = $"https://api.genderize.io/?name={name}";

            SQLiteConnection sqlite_conn;
            sqlite_conn = CreateConnection();
            CreateTable(sqlite_conn);
            DataEntry (apiUrl ,sqlite_conn);
            ReadLastData(sqlite_conn);
            Console.ReadLine();

        }



        static SQLiteConnection CreateConnection()
        {

            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection("Data Source=database.db; Version = 3; New = True; Compress = True; ");
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return sqlite_conn;
        }
        static void CreateTable(SQLiteConnection conn)
        {

            SQLiteCommand sqlite_cmd;
            string createTableQuery = @"
               CREATE TABLE IF NOT EXISTS Name (
                   Id INTEGER PRIMARY KEY AUTOINCREMENT,
                   count INTEGER,
                   Name TEXT,
                   Gender TEXT,
                   probability REAL 
               );";
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = createTableQuery;
            sqlite_cmd.ExecuteNonQuery();
        }
        static async Task DataEntry(string apiUrl, SQLiteConnection conn)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);
                string content = await response.Content.ReadAsStringAsync();

                // Тут ви можете вивести вміст або виконати інші дії з рядком content
                Console.WriteLine(content);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    User user = JsonConvert.DeserializeObject<User>(responseData);
                    EntryUser(conn, user);
                }
                else
                {
                    Console.WriteLine($"Помилка отримання даних з API: {response.StatusCode}");
                }

            }
        }
        static void EntryUser(SQLiteConnection conn, User user)
        {
            string insertDataQuery = @"
            INSERT INTO Name (
            count, Name, Gender, probability) 
            VALUES (
            @count, @Name, @Gender, @probability);";
            using (SQLiteCommand command = new SQLiteCommand(insertDataQuery, conn))
            {
                command.Parameters.AddWithValue("@count", user.count);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Gender", user.gender);
                command.Parameters.AddWithValue("@probability", user.probability);
                command.ExecuteNonQuery();
            }

        }


        static void ReadLastData(SQLiteConnection conn)
        {
            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = "SELECT * FROM Name ORDER BY ID DESC LIMIT 1";
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            while (sqlite_datareader.Read())
            {
                for (int i = 0; i < sqlite_datareader.FieldCount; i++)
                {
                    string columnName = sqlite_datareader.GetName(i);
                    string columnValue = sqlite_datareader.GetValue(i).ToString();
                    Console.WriteLine($"{columnName}: {columnValue}");
                }
                Console.WriteLine();
            }
        }
    }
}
