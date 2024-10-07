using CounterStrikeSharp.API.Core;
using EntWatchSharp.Helpers;
using MySqlConnector;
using System.Data;
using System.Data.SQLite;
using System.Text.Json;

namespace EntWatchSharp.Modules.Eban
{
    static class EbanDB
    {
        public static Database db;
        private static DBConfig dbConfig;

        public static void Init_DB(string ModuleDirectory)
        {
            string sConfig = $"{Path.Join(ModuleDirectory, "db_config.json")}";
            string sData;
            if (File.Exists(sConfig))
            {
                sData = File.ReadAllText(sConfig);
                dbConfig = JsonSerializer.Deserialize<DBConfig>(sData);
            }
            else dbConfig = new DBConfig();
            if (dbConfig.TypeDB == "mysql") db = new DB_Mysql($"server={dbConfig.Mysql_Server};port={dbConfig.Mysql_Port};user={dbConfig.Mysql_User};database={dbConfig.Mysql_NameDatabase};password={dbConfig.Mysql_Password};");
            else
            {
                string sDBFile = Path.Join(ModuleDirectory, dbConfig.SQLite_File);
                if (!File.Exists(sDBFile)) File.WriteAllBytes(sDBFile, Array.Empty<byte>());
                db = new DB_SQLite($"Data Source={sDBFile}");
            }
            Task.Run(async () =>
            {
                string sExceptionMessage = await db.TestConnection();
                if (db.bSuccess)
                {
                    UI.EWSysInfo("Info.DB.Success", 6, db.TypeDB);

                    if (db.TypeDB == "sqlite")
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand())
                        {
                            cmd.CommandText = "CREATE TABLE IF NOT EXISTS EntWatch_Current_Eban(	id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                                                                                "client_name varchar(32) NOT NULL, " +
                                                                                                "client_steamid varchar(64) NOT NULL, " +
                                                                                                "admin_name varchar(32) NOT NULL, " +
                                                                                                "admin_steamid varchar(64) NOT NULL, " +
                                                                                                "server varchar(64), " +
                                                                                                "duration INTEGER NOT NULL, " +
                                                                                                "timestamp_issued INTEGER NOT NULL, " +
                                                                                                "reason varchar(64), " +
                                                                                                "reason_unban varchar(64), " +
                                                                                                "admin_name_unban varchar(32), " +
                                                                                                "admin_steamid_unban varchar(64), " +
                                                                                                "timestamp_unban INTEGER);" +
                        "CREATE TABLE IF NOT EXISTS EntWatch_Old_Eban(	id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                                                                                "client_name varchar(32) NOT NULL, " +
                                                                                                "client_steamid varchar(64) NOT NULL, " +
                                                                                                "admin_name varchar(32) NOT NULL, " +
                                                                                                "admin_steamid varchar(64) NOT NULL, " +
                                                                                                "server varchar(64), " +
                                                                                                "duration INTEGER NOT NULL, " +
                                                                                                "timestamp_issued INTEGER NOT NULL, " +
                                                                                                "reason varchar(64), " +
                                                                                                "reason_unban varchar(64), " +
                                                                                                "admin_name_unban varchar(32), " +
                                                                                                "admin_steamid_unban varchar(64), " +
                                                                                                "timestamp_unban INTEGER);";
                            if (await ((DB_SQLite)db).Execute(cmd) > -1) db.bDBReady = true;
                        }
                    }
                    else if (db.TypeDB == "mysql")
                    {
                        using (MySqlCommand cmd = new MySqlCommand())
                        {
                            cmd.CommandText = "CREATE TABLE IF NOT EXISTS EntWatch_Current_Eban(	id int(10) unsigned NOT NULL auto_increment, " +
                                                                                                "client_name varchar(32) NOT NULL, " +
                                                                                                "client_steamid varchar(64) NOT NULL, " +
                                                                                                "admin_name varchar(32) NOT NULL, " +
                                                                                                "admin_steamid varchar(64) NOT NULL, " +
                                                                                                "server varchar(64), " +
                                                                                                "duration int unsigned NOT NULL, " +
                                                                                                "timestamp_issued int NOT NULL, " +
                                                                                                "reason varchar(64), " +
                                                                                                "reason_unban varchar(64), " +
                                                                                                "admin_name_unban varchar(32), " +
                                                                                                "admin_steamid_unban varchar(64), " +
                                                                                                "timestamp_unban int, " +
                                                                                                "PRIMARY KEY(id));" +
                            "CREATE TABLE IF NOT EXISTS EntWatch_Old_Eban(	id int(10) unsigned NOT NULL auto_increment, " +
                                                                                                "client_name varchar(32) NOT NULL, " +
                                                                                                "client_steamid varchar(64) NOT NULL, " +
                                                                                                "admin_name varchar(32) NOT NULL, " +
                                                                                                "admin_steamid varchar(64) NOT NULL, " +
                                                                                                "server varchar(64), " +
                                                                                                "duration int unsigned NOT NULL, " +
                                                                                                "timestamp_issued int NOT NULL, " +
                                                                                                "reason varchar(64), " +
                                                                                                "reason_unban varchar(64), " +
                                                                                                "admin_name_unban varchar(32), " +
                                                                                                "admin_steamid_unban varchar(64), " +
                                                                                                "timestamp_unban int, " +
                                                                                                "PRIMARY KEY(id));";
                            if (await ((DB_Mysql)db).Execute(cmd) > -1) db.bDBReady = true;
                        }
                    }
                }
                else UI.EWSysInfo("Info.DB.Failed", 15, db.TypeDB, sExceptionMessage);
            });
        }

        public static async Task<bool> BanClient(string sClientName, string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iDuration, long iTimeStamp, string sReason)
        {
            if (!string.IsNullOrEmpty(sClientName) && !string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminName) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
            {
                if (db.TypeDB == "sqlite")
                {
                    using (SQLiteCommand cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "INSERT INTO EntWatch_Current_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason) VALUES (@clientname, @clientsteamid, @adminname, @adminsteamid, @server, @duration, @timeissued, @reason);";
                        cmd.Parameters.Add(new SQLiteParameter("@clientname", sClientName));
                        cmd.Parameters.Add(new SQLiteParameter("@clientsteamid", sClientSteamID));
                        cmd.Parameters.Add(new SQLiteParameter("@adminname", sAdminName));
                        cmd.Parameters.Add(new SQLiteParameter("@adminsteamid", sAdminSteamID));
                        cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
                        cmd.Parameters.Add(new SQLiteParameter("@duration", iDuration));
                        cmd.Parameters.Add(new SQLiteParameter("@timeissued", iTimeStamp));
                        cmd.Parameters.Add(new SQLiteParameter("@reason", sReason));
                        await ((DB_SQLite)db).Execute(cmd);
                    }
                }
                else if (db.TypeDB == "mysql")
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.CommandText = "INSERT INTO EntWatch_Current_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason) VALUES (@clientname, @clientsteamid, @adminname, @adminsteamid, @server, @duration, @timeissued, @reason);";
                        cmd.Parameters.Add(new MySqlParameter("@clientname", sClientName));
                        cmd.Parameters.Add(new MySqlParameter("@clientsteamid", sClientSteamID));
                        cmd.Parameters.Add(new MySqlParameter("@adminname", sAdminName));
                        cmd.Parameters.Add(new MySqlParameter("@adminsteamid", sAdminSteamID));
                        cmd.Parameters.Add(new MySqlParameter("@server", sServer));
                        cmd.Parameters.Add(new MySqlParameter("@duration", iDuration));
                        cmd.Parameters.Add(new MySqlParameter("@timeissued", iTimeStamp));
                        cmd.Parameters.Add(new MySqlParameter("@reason", sReason));
                        await ((DB_Mysql)db).Execute(cmd);
                    }
                }
                return true;
            }
            return false;
        }

        public static async Task<bool> UnBanClient(string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iTimeStamp, string sReason)
        {
            if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
            {
                if (db.TypeDB == "sqlite")
                {
                    SQLiteCommand cmd = new SQLiteCommand();
                    if (Cvar.KeepExpiredBan)
                        cmd.CommandText = "UPDATE EntWatch_Current_Eban SET reason_unban=@reason, admin_name_unban=@adminname, admin_steamid_unban=@adminsteamid, timestamp_unban=@timeunban " +
                                            "WHERE client_steamid=@clientsteamid and server=@server and admin_steamid_unban IS NULL; " +

                                        "INSERT INTO EntWatch_Old_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
                                            "SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM EntWatch_Current_Eban " +
                                                "WHERE client_steamid=@clientsteamid and server=@server; ";

                    cmd.CommandText += "DELETE FROM EntWatch_Current_Eban " +
                                            "WHERE client_steamid=@clientsteamid and server=@server;";
                    cmd.Parameters.Add(new SQLiteParameter("@clientsteamid", sClientSteamID));
                    cmd.Parameters.Add(new SQLiteParameter("@adminname", sAdminName));
                    cmd.Parameters.Add(new SQLiteParameter("@adminsteamid", sAdminSteamID));
                    cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
                    cmd.Parameters.Add(new SQLiteParameter("@timeunban", iTimeStamp));
                    cmd.Parameters.Add(new SQLiteParameter("@reason", sReason));
                    await ((DB_SQLite)db).Execute(cmd);
                }
                else if (db.TypeDB == "mysql")
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        if (Cvar.KeepExpiredBan)
                            cmd.CommandText = "UPDATE EntWatch_Current_Eban SET reason_unban=@reason, admin_name_unban=@adminname, admin_steamid_unban=@adminsteamid, timestamp_unban=@timeunban " +
                                                "WHERE client_steamid=@clientsteamid and server=@server and admin_steamid_unban IS NULL; " +

                                            "INSERT INTO EntWatch_Old_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
                                                "SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM EntWatch_Current_Eban " +
                                                    "WHERE client_steamid=@clientsteamid and server=@server; ";

                        cmd.CommandText += "DELETE FROM EntWatch_Current_Eban " +
                                            "WHERE client_steamid=@clientsteamid and server=@server;";
                        cmd.Parameters.Add(new MySqlParameter("@clientsteamid", sClientSteamID));
                        cmd.Parameters.Add(new MySqlParameter("@adminname", sAdminName));
                        cmd.Parameters.Add(new MySqlParameter("@adminsteamid", sAdminSteamID));
                        cmd.Parameters.Add(new MySqlParameter("@server", sServer));
                        cmd.Parameters.Add(new MySqlParameter("@timeunban", iTimeStamp));
                        cmd.Parameters.Add(new MySqlParameter("@reason", sReason));
                        await ((DB_Mysql)db).Execute(cmd);
                    }
                }
                return true;
            }
            return false;
        }

        public static async Task<bool> GetBan(CCSPlayerController player, string sServer)
        {
            try
            {
                if (player.IsValid && !string.IsNullOrEmpty(sServer) && db.bDBReady)
                {
                    if (db.TypeDB == "sqlite")
                    {
                        using (SQLiteCommand cmd = new SQLiteCommand())
                        {
                            cmd.CommandText = "SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM EntWatch_Current_Eban " +
                                                "WHERE client_steamid=@steam and server=@server;";
                            cmd.Parameters.Add(new SQLiteParameter("@steam", EW.ConvertSteamID64ToSteamID(player.SteamID.ToString())));
                            cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
                            using (DataTableReader reader = await ((DB_SQLite)db).Query(cmd))
                            {
                                if (reader != null && reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    if (EW.g_BannedPlayer.ContainsKey(player))
                                    {
                                        EW.g_BannedPlayer[player].bBanned = true;
                                        EW.g_BannedPlayer[player].sAdminName = await reader.GetFieldValueAsync<string>(0);
                                        EW.g_BannedPlayer[player].sAdminSteamID = await reader.GetFieldValueAsync<string>(1);
                                        EW.g_BannedPlayer[player].iDuration = Convert.ToInt32(await reader.GetFieldValueAsync<uint>(2));
                                        EW.g_BannedPlayer[player].iTimeStamp_Issued = await reader.GetFieldValueAsync<int>(3);
                                        EW.g_BannedPlayer[player].sReason = await reader.GetFieldValueAsync<string>(4);

                                        return true;
                                    }
                                    else
                                    {
                                        EW.g_BannedPlayer.TryAdd(player, new EbanPlayer());
                                        return await GetBan(player, sServer);
                                    }
                                }
                            }
                        }
                    }
                    else if (db.TypeDB == "mysql")
                    {
                        using (MySqlCommand cmd = new MySqlCommand())
                        {
                            cmd.CommandText = "SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM EntWatch_Current_Eban " +
                                                "WHERE client_steamid=@steam and server=@server;";
                            cmd.Parameters.Add(new MySqlParameter("@steam", EW.ConvertSteamID64ToSteamID(player.SteamID.ToString())));
                            cmd.Parameters.Add(new MySqlParameter("@server", sServer));
                            using (DataTableReader reader = await ((DB_Mysql)db).Query(cmd))
                            {
                                if (reader != null && reader.HasRows)
                                {
                                    if (EW.g_BannedPlayer.ContainsKey(player))
                                    {
                                        await reader.ReadAsync();
                                        EW.g_BannedPlayer[player].bBanned = true;
                                        EW.g_BannedPlayer[player].sAdminName = await reader.GetFieldValueAsync<string>(0);
                                        EW.g_BannedPlayer[player].sAdminSteamID = await reader.GetFieldValueAsync<string>(1);
                                        EW.g_BannedPlayer[player].iDuration = Convert.ToInt32(await reader.GetFieldValueAsync<uint>(2));
                                        EW.g_BannedPlayer[player].iTimeStamp_Issued = await reader.GetFieldValueAsync<int>(3);
                                        EW.g_BannedPlayer[player].sReason = await reader.GetFieldValueAsync<string>(4);
                                        return true;
                                    }
                                    else
                                    {
                                        EW.g_BannedPlayer.TryAdd(player, new EbanPlayer());
                                        return await GetBan(player, sServer);
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }
            } catch (Exception ex) { Console.WriteLine(ex); }
            return false;
        }

		public static async Task<EbanPlayer> GetBan(string SteamID, string sServer)
		{
			try
			{
				if (!string.IsNullOrEmpty(sServer) && db.bDBReady)
				{
					if (db.TypeDB == "sqlite")
					{
						using (SQLiteCommand cmd = new SQLiteCommand())
						{
							cmd.CommandText = "SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM EntWatch_Current_Eban " +
												"WHERE client_steamid=@steam and server=@server;";
							cmd.Parameters.Add(new SQLiteParameter("@steam", SteamID));
							cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
							using (DataTableReader reader = await ((DB_SQLite)db).Query(cmd))
							{
								if (reader != null && reader.HasRows)
								{
									await reader.ReadAsync();
									EbanPlayer player = new EbanPlayer();
									player.bBanned = true;
									player.sAdminName = await reader.GetFieldValueAsync<string>(0);
									player.sAdminSteamID = await reader.GetFieldValueAsync<string>(1);
									player.iDuration = Convert.ToInt32(await reader.GetFieldValueAsync<uint>(2));
									player.iTimeStamp_Issued = await reader.GetFieldValueAsync<int>(3);
									player.sReason = await reader.GetFieldValueAsync<string>(4);
									player.sClientName = await reader.GetFieldValueAsync<string>(5);
                                    player.sClientSteamID = SteamID;
									return player;
								}
							}
						}
					}
					else if (db.TypeDB == "mysql")
					{
						using (MySqlCommand cmd = new MySqlCommand())
						{
							cmd.CommandText = "SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM EntWatch_Current_Eban " +
												"WHERE client_steamid=@steam and server=@server;";
							cmd.Parameters.Add(new MySqlParameter("@steam", SteamID));
							cmd.Parameters.Add(new MySqlParameter("@server", sServer));
							using (DataTableReader reader = await ((DB_Mysql)db).Query(cmd))
							{
								if (reader != null && reader.HasRows)
								{
									await reader.ReadAsync();
									EbanPlayer player = new EbanPlayer();
									player.bBanned = true;
									player.sAdminName = await reader.GetFieldValueAsync<string>(0);
									player.sAdminSteamID = await reader.GetFieldValueAsync<string>(1);
									player.iDuration = Convert.ToInt32(await reader.GetFieldValueAsync<uint>(2));
									player.iTimeStamp_Issued = await reader.GetFieldValueAsync<int>(3);
									player.sReason = await reader.GetFieldValueAsync<string>(4);
									player.sClientName = await reader.GetFieldValueAsync<string>(5);
									player.sClientSteamID = SteamID;
									return player;
								}
							}
						}
					}
					return null;
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			return null;
		}

		public static async Task<bool> OfflineUnban(string sServer, int iTime)
        {
            if (db.bDBReady)
            {
                if (db.TypeDB == "sqlite")
                {
                    using (SQLiteCommand cmd = new SQLiteCommand())
                    {
                        cmd.CommandText = "SELECT id FROM EntWatch_Current_Eban " +
                                            "WHERE server=@server and duration>0 and timestamp_issued<@time;";
                        cmd.Parameters.Add(new SQLiteParameter("@server", sServer));
                        cmd.Parameters.Add(new SQLiteParameter("@time", iTime));
                        using (DataTableReader reader = await ((DB_SQLite)db).Query(cmd))
                        {
                            if (reader != null && reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    int iID = await reader.GetFieldValueAsync<int>(0);

                                    SQLiteCommand cmd1 = new SQLiteCommand();
                                    if (Cvar.KeepExpiredBan)
                                        cmd1.CommandText = "UPDATE EntWatch_Current_Eban SET reason_unban='Expired', admin_name_unban='Console', admin_steamid_unban='SERVER', timestamp_unban=@time WHERE id=@id; " +

                                                        "INSERT INTO EntWatch_Old_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
                                                            "SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM EntWatch_Current_Eban " +
                                                                "WHERE id=@id; ";

                                    cmd1.CommandText += "DELETE FROM EntWatch_Current_Eban WHERE id=@id;";
                                    cmd1.Parameters.Add(new SQLiteParameter("@time", iTime));
                                    cmd1.Parameters.Add(new SQLiteParameter("@id", iID));
                                    await ((DB_SQLite)db).Execute(cmd1);
                                }
                            }
                        }
                    }
                }
                else if (db.TypeDB == "mysql")
                {
                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.CommandText = "SELECT id FROM EntWatch_Current_Eban " +
                                            "WHERE server=@server and duration>0 and timestamp_issued<@time;";
                        cmd.Parameters.Add(new MySqlParameter("@server", sServer));
                        cmd.Parameters.Add(new MySqlParameter("@time", iTime));
                        using (DataTableReader reader = await ((DB_Mysql)db).Query(cmd))
                        {
                            if (reader != null && reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    int iID = await reader.GetFieldValueAsync<int>(0);

                                    MySqlCommand cmd1 = new MySqlCommand();
                                    if (Cvar.KeepExpiredBan)
                                        cmd1.CommandText = "UPDATE EntWatch_Current_Eban SET reason_unban='Expired', admin_name_unban='Console', admin_steamid_unban='SERVER', timestamp_unban=@time WHERE id=@id; " +

                                                            "INSERT INTO EntWatch_Old_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
                                                                "SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM EntWatch_Current_Eban " +
                                                                    "WHERE id=@id; ";

                                    cmd1.CommandText += "DELETE FROM EntWatch_Current_Eban WHERE id=@id;";
                                    cmd1.Parameters.Add(new MySqlParameter("@time", iTime));
                                    cmd1.Parameters.Add(new MySqlParameter("@id", iID));
                                    await ((DB_Mysql)db).Execute(cmd1);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            return false;
        }
    }
}
