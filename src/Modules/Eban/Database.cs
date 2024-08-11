using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using EntWatchSharp.Helpers;
using MySqlConnector;

namespace EntWatchSharp.Modules.Eban
{
    public abstract class Database
    {
        public string TypeDB = "sqlite";
        public string ConnStr;
        public bool bSuccess = false;
        public bool bDBReady = false;
        public Database(string connstr) { ConnStr = connstr; }
        public abstract Task<string> TestConnection();
    }

    public class DB_Mysql : Database
    {
        public DB_Mysql(string connstr) : base(connstr)
        {
            TypeDB = "mysql";
        }
        public async override Task<string> TestConnection()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnStr))
                {
                    await conn.OpenAsync();
                    bSuccess = true;
                }
            }
            catch (Exception e) { return e.Message; }
            return null;
        }
        public async Task<DataTableReader> Query(MySqlCommand command)
        {
            if (bSuccess)
            {
                try
                {
                    DataTable dt = new DataTable();
                    using (MySqlConnection conn = new MySqlConnection(ConnStr))
                    {
                        await conn.OpenAsync();
                        command.Connection = conn;
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                            return dt.CreateDataReader();
                        }
                    }
                }
                catch (Exception ex) { UI.EWSysInfo("Info.Error", 15, ex.Message); }
            }
            return null;
        }
        public async Task<int> Execute(MySqlCommand command)
        {
            if (bSuccess)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(ConnStr))
                    {
                        await conn.OpenAsync();
                        command.Connection = conn;
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception) { return -1; }
                return 1;
            }
            return -1;
        }
    }

    public class DB_SQLite : Database
    {
        public DB_SQLite(string connstr) : base(connstr)
        {
            TypeDB = "sqlite";
        }
        public async override Task<string> TestConnection()
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                {
                    await conn.OpenAsync();
                    bSuccess = true;
                }
            }
            catch (Exception e) { return e.Message; }
            return null;
        }
        public async Task<DataTableReader> Query(SQLiteCommand command)
        {
            if (bSuccess)
            {
                try
                {
                    DataTable dt = new DataTable();
                    using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                    {
                        await conn.OpenAsync();
                        command.Connection = conn;
                        using (DbDataReader reader = await command.ExecuteReaderAsync())
                        {
                            dt.Load(reader);
                            return dt.CreateDataReader();
                        }
                    }
                }
                catch (Exception ex) { UI.EWSysInfo("Info.Error", 15, ex.Message); }
            }
            return null;
        }
        public async Task<int> Execute(SQLiteCommand command)
        {
            if (bSuccess)
            {
                try
                {
                    using (SQLiteConnection conn = new SQLiteConnection(ConnStr))
                    {
                        await conn.OpenAsync();
                        command.Connection = conn;
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception) { return -1; }
                return 1;
            }
            return -1;
        }
    }

    public class DBConfig
    {
        public string TypeDB { get; set; }
        public string SQLite_File { get; set; }
        public string Mysql_Server { get; set; }
        public string Mysql_Port { get; set; }
        public string Mysql_User { get; set; }
        public string Mysql_Password { get; set; }
        public string Mysql_NameDatabase { get; set; }
        public DBConfig()
        {
            TypeDB = "sqlite";
            SQLite_File = "database.db";
        }
    }
}
