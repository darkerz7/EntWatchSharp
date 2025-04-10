using CounterStrikeSharp.API.Core;
using EntWatchSharp.Helpers;
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
				dbConfig ??= new DBConfig();
			}
            else dbConfig = new DBConfig();
            if (dbConfig.TypeDB == "mysql") db = new DB_Mysql(dbConfig.SQL_NameDatabase, $"{dbConfig.SQL_Server}:{dbConfig.SQL_Port}", dbConfig.SQL_User, dbConfig.SQL_Password);
            else if (dbConfig.TypeDB == "postgre") db = new DB_PosgreSQL(dbConfig.SQL_NameDatabase, $"{dbConfig.SQL_Server}:{dbConfig.SQL_Port}", dbConfig.SQL_User, dbConfig.SQL_Password);
			else
            {
				dbConfig.TypeDB = "sqlite";
				string sDBFile = Path.Join(ModuleDirectory, dbConfig.SQLite_File);
				db = new DB_SQLite(sDBFile);
            }
			if (db.bSuccess)
			{
				UI.EWSysInfo("Info.DB.Success", 6, dbConfig.TypeDB);
				#pragma warning disable CS8625
				Task.Run(() => {
					if (dbConfig.TypeDB == "sqlite")
					{
						db.AnyDB.QueryAsync("CREATE TABLE IF NOT EXISTS EntWatch_Current_Eban(	id INTEGER PRIMARY KEY AUTOINCREMENT, " +
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
																								"timestamp_unban INTEGER);", null, (_) =>
						{
							db.bDBReady = true;
						}, true);
					}
					else if (dbConfig.TypeDB == "mysql")
					{
						db.AnyDB.QueryAsync("CREATE TABLE IF NOT EXISTS EntWatch_Current_Eban(	id int(10) unsigned NOT NULL auto_increment, " +
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
																								"PRIMARY KEY(id));", null, (_) =>
						{
							db.bDBReady = true;
						}, true);
					}
					else if (dbConfig.TypeDB == "postgre")
					{
						db.AnyDB.QueryAsync("CREATE TABLE IF NOT EXISTS EntWatch_Current_Eban(	id serial, " +
																								"client_name varchar(32) NOT NULL, " +
																								"client_steamid varchar(64) NOT NULL, " +
																								"admin_name varchar(32) NOT NULL, " +
																								"admin_steamid varchar(64) NOT NULL, " +
																								"server varchar(64), " +
																								"duration integer NOT NULL, " +
																								"timestamp_issued integer NOT NULL, " +
																								"reason varchar(64), " +
																								"reason_unban varchar(64), " +
																								"admin_name_unban varchar(32), " +
																								"admin_steamid_unban varchar(64), " +
																								"timestamp_unban integer, " +
																								"PRIMARY KEY(id));" +
							"CREATE TABLE IF NOT EXISTS EntWatch_Old_Eban(	id serial, " +
																								"client_name varchar(32) NOT NULL, " +
																								"client_steamid varchar(64) NOT NULL, " +
																								"admin_name varchar(32) NOT NULL, " +
																								"admin_steamid varchar(64) NOT NULL, " +
																								"server varchar(64), " +
																								"duration integer NOT NULL, " +
																								"timestamp_issued integer NOT NULL, " +
																								"reason varchar(64), " +
																								"reason_unban varchar(64), " +
																								"admin_name_unban varchar(32), " +
																								"admin_steamid_unban varchar(64), " +
																								"timestamp_unban integer, " +
																								"PRIMARY KEY(id));", null, (_) =>
																								{
																									db.bDBReady = true;
																								}, true);
					}
				});
				#pragma warning restore CS8625
			}
			else UI.EWSysInfo("Info.DB.Failed", 15, dbConfig.TypeDB);
        }

		public static void BanClient(string sClientName, string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iDuration, long iTimeStamp, string sReason)
		{
			if (!string.IsNullOrEmpty(sClientName) && !string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminName) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
			{
				Task.Run(() =>
				{
					db.AnyDB.QueryAsync("INSERT INTO EntWatch_Current_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason) VALUES ('{ARG}', '{ARG}', '{ARG}', '{ARG}', '{ARG}', {ARG}, {ARG}, '{ARG}');", new List<string>([sClientName, sClientSteamID, sAdminName, sAdminSteamID, sServer, iDuration.ToString(), iTimeStamp.ToString(), sReason]), (_) => { }, true);
				});
			}
		}

		public static void UnBanClient(string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iTimeStamp, string sReason)
		{
			if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
			{
				Task.Run(() =>
				{
					if (Cvar.KeepExpiredBan)
						db.AnyDB.QueryAsync("UPDATE EntWatch_Current_Eban SET reason_unban = '{ARG}', admin_name_unban = '{ARG}', admin_steamid_unban = '{ARG}', timestamp_unban = {ARG} " +
												"WHERE client_steamid='{ARG}' and server='{ARG}' and admin_steamid_unban IS NULL;" +
											"INSERT INTO EntWatch_Old_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
												"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM EntWatch_Current_Eban " +
													"WHERE client_steamid='{ARG}' and server='{ARG}';" +
											"DELETE FROM EntWatch_Current_Eban " +
													"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sReason, sAdminName, sAdminSteamID, iTimeStamp.ToString(), sClientSteamID, sServer, sClientSteamID, sServer, sClientSteamID, sServer]), (_) => { }, true);
					else
						db.AnyDB.QueryAsync("DELETE FROM EntWatch_Current_Eban " +
							"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), (_) => { }, true);
				});
			}
		}
#nullable enable
		public delegate void GetBanCommFunc(string sClientSteamID, CCSPlayerController? admin, string reason, bool bConsole, List<List<string>> DBQuery_Result);
		public static void GetBan(string sClientSteamID, string sServer, CCSPlayerController? admin, string reason, bool bConsole, GetBanCommFunc getbanfunc)
#nullable disable
		{
			if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sServer) && db.bDBReady)
			{
				Task.Run(() =>
				{
					db.AnyDB.QueryAsync("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM EntWatch_Current_Eban " +
											"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), (res) =>
											{
												getbanfunc(sClientSteamID, admin, reason, bConsole, res);
											});
				});
			}
		}
#nullable enable
		public delegate void GetBanAPIFunc(string sClientSteamID, List<List<string>> DBQuery_Result);
		public static void GetBan(string sClientSteamID, string sServer, GetBanAPIFunc getbanfunc)
#nullable disable
		{
			if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sServer) && db.bDBReady)
			{
				Task.Run(() =>
				{
					db.AnyDB.QueryAsync("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason, client_name FROM EntWatch_Current_Eban " +
											"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), (res) =>
											{
												getbanfunc(sClientSteamID, res);
											});
				});
			}
		}

		public delegate void GetBanPlayerFunc(CCSPlayerController player, List<List<string>> DBQuery_Result, bool bShow);
		public static void GetBan(CCSPlayerController pl, string sServer, GetBanPlayerFunc getbanfunc, bool bShow)
		{
			if (pl.IsValid && !string.IsNullOrEmpty(sServer) && db.bDBReady)
			{
				Task.Run(() =>
				{
					db.AnyDB.QueryAsync("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM EntWatch_Current_Eban " +
											"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([EW.ConvertSteamID64ToSteamID(pl.SteamID.ToString()), sServer]), (res) =>
											{
												getbanfunc(pl, res, bShow);
											});
				});
			}
		}

		public static void OfflineUnban(string sServer)
		{
			if (db.bDBReady)
			{
				int iTime = Convert.ToInt32(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
				Task.Run(() =>
				{
				if (Cvar.KeepExpiredBan)
					{
						db.AnyDB.QueryAsync("SELECT id FROM EntWatch_Current_Eban " +
									"WHERE server='{ARG}' and duration>0 and timestamp_issued<{ARG};", new List<string>([sServer, iTime.ToString()]), (res) =>
						{
							if (res.Count > 0)
							{
								string sIDs = "";
								for (int i = 0; i < res.Count; i++)
								{
									sIDs = sIDs + ", " + res[i][0];
								}
								sIDs = sIDs[2..];
								db.AnyDB.QueryAsync("UPDATE EntWatch_Current_Eban SET reason_unban='Expired', admin_name_unban='Console', admin_steamid_unban='SERVER', timestamp_unban={ARG} WHERE id IN ({ARG});" +
									"INSERT INTO EntWatch_Old_Eban(client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
										"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM EntWatch_Current_Eban WHERE id IN ({ARG});" +
									"DELETE FROM EntWatch_Current_Eban WHERE id IN ({ARG});", new List<string>([iTime.ToString(), sIDs, sIDs, sIDs]), (_) => { }, true);
							}
						});
					}
					else
					{
						db.AnyDB.QueryAsync("DELETE FROM EntWatch_Current_Eban WHERE id IN (SELECT p.id FROM (" +
								"SELECT id FROM EntWatch_Current_Eban WHERE server='{ARG}' and duration>0 and timestamp_issued<{ARG}) AS p);", new List<string>([sServer, iTime.ToString()]), (_) => { }, true);
					}
				});
			}
		}
	}
}
