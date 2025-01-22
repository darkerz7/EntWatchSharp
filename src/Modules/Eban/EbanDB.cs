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
				#pragma warning restore CS8625
			}
			else UI.EWSysInfo("Info.DB.Failed", 15, dbConfig.TypeDB);
        }

        public static bool BanClient(string sClientName, string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iDuration, long iTimeStamp, string sReason)
        {
            if (!string.IsNullOrEmpty(sClientName) && !string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminName) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
            {
				#pragma warning disable CS8625
				db.AnyDB.QueryAsync("INSERT INTO EntWatch_Current_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason) VALUES ('{ARG}', '{ARG}', '{ARG}', '{ARG}', '{ARG}', {ARG}, {ARG}, '{ARG}');", new List<string>([sClientName, sClientSteamID, sAdminName, sAdminSteamID, sServer, iDuration.ToString(), iTimeStamp.ToString(), sReason]), null, true);
				#pragma warning restore CS8625
				return true;
            }
            return false;
        }

        public static bool UnBanClient(string sClientSteamID, string sAdminName, string sAdminSteamID, string sServer, long iTimeStamp, string sReason)
        {
            if (!string.IsNullOrEmpty(sClientSteamID) && !string.IsNullOrEmpty(sAdminSteamID) && db.bDBReady)
            {
				#pragma warning disable CS8625
				if (Cvar.KeepExpiredBan)
					db.AnyDB.QueryAsync("UPDATE EntWatch_Current_Eban SET reason_unban = '{ARG}', admin_name_unban = '{ARG}', admin_steamid_unban = '{ARG}', timestamp_unban = {ARG} " +
											"WHERE client_steamid='{ARG}' and server='{ARG}' and admin_steamid_unban IS NULL;", new List<string>([sReason, sAdminName, sAdminSteamID, iTimeStamp.ToString(), sClientSteamID, sServer]), (_) =>
					{
						db.AnyDB.QueryAsync("INSERT INTO EntWatch_Old_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
												"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM EntWatch_Current_Eban " +
													"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), (_) =>
						{
							db.AnyDB.QueryAsync("DELETE FROM EntWatch_Current_Eban " +
								"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), null, true);
						}, true);
					}, true);
				else db.AnyDB.QueryAsync("DELETE FROM EntWatch_Current_Eban " +
											"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([sClientSteamID, sServer]), null, true);
				#pragma warning restore CS8625
				return true;
            }
            return false;
        }

        public static bool GetBan(CCSPlayerController player, string sServer)
        {
            try
            {
                if (player.IsValid && !string.IsNullOrEmpty(sServer) && db.bDBReady)
                {
					var res = db.AnyDB.Query("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM EntWatch_Current_Eban " +
												"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([EW.ConvertSteamID64ToSteamID(player.SteamID.ToString()), sServer]));
					if (res.Count > 0)
                    {
						if (EW.CheckDictionary(player))
                        {
							EW.g_EWPlayer[player].BannedPlayer.bBanned = true;
                            EW.g_EWPlayer[player].BannedPlayer.sAdminName = res[0][0];
							EW.g_EWPlayer[player].BannedPlayer.sAdminSteamID = res[0][1];
							EW.g_EWPlayer[player].BannedPlayer.iDuration = Convert.ToInt32(res[0][2]);
							EW.g_EWPlayer[player].BannedPlayer.iTimeStamp_Issued = Convert.ToInt32(res[0][3]);
							EW.g_EWPlayer[player].BannedPlayer.sReason = res[0][4];

							return true;
                        }
						else
						{
							return GetBan(player, sServer);
						}
					}
                    return false;
                }
            } catch (Exception ex) { Console.WriteLine(ex); }
            return false;
        }

		public static EbanPlayer GetBan(string SteamID, string sServer)
		{
			try
			{
				if (!string.IsNullOrEmpty(sServer) && db.bDBReady)
				{
					var res = db.AnyDB.Query("SELECT admin_name, admin_steamid, duration, timestamp_issued, reason FROM EntWatch_Current_Eban " +
												"WHERE client_steamid='{ARG}' and server='{ARG}';", new List<string>([SteamID, sServer]));
					if (res.Count > 0)
					{
						EbanPlayer player = new()
						{
							bBanned = true,
							sAdminName = res[0][0],
							sAdminSteamID = res[0][1],
							iDuration = Convert.ToInt32(res[0][2]),
							iTimeStamp_Issued = Convert.ToInt32(res[0][3]),
							sReason = res[0][4],
							sClientName = res[0][5],
							sClientSteamID = SteamID
						};
						return player;
					}
					return null;
				}
			}
			catch (Exception ex) { Console.WriteLine(ex); }
			return null;
		}

		public static bool OfflineUnban(string sServer, int iTime)
        {
            if (db.bDBReady)
            {
				var res = db.AnyDB.Query("SELECT id FROM EntWatch_Current_Eban " +
									"WHERE server='{ARG}' and duration>0 and timestamp_issued<{ARG};", new List<string>([sServer, iTime.ToString()]));

				if(res.Count > 0)
                {
					#pragma warning disable CS8625
					for (int i = 0; i < res.Count; i++) 
                    {
						string sID = res[i][0];
						if (Cvar.KeepExpiredBan)
							db.AnyDB.QueryAsync("UPDATE EntWatch_Current_Eban SET reason_unban='Expired', admin_name_unban='Console', admin_steamid_unban='SERVER', timestamp_unban={ARG} WHERE id={ARG};", new List<string>([iTime.ToString(), sID]), (_) =>
							{
								db.AnyDB.QueryAsync("INSERT INTO EntWatch_Old_Eban (client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban) " +
														"SELECT client_name, client_steamid, admin_name, admin_steamid, server, duration, timestamp_issued, reason, reason_unban, admin_name_unban, admin_steamid_unban, timestamp_unban FROM EntWatch_Current_Eban " +
															"WHERE id={ARG};", new List<string>([sID]), (_) =>
									{
										db.AnyDB.QueryAsync("DELETE FROM EntWatch_Current_Eban WHERE id={ARG};", new List<string>([sID]), null, true);
									}, true);
							}, true);	
						else db.AnyDB.QueryAsync("DELETE FROM EntWatch_Current_Eban WHERE id={ARG};", new List<string>([sID]), null, true);
					}
					#pragma warning restore CS8625
				}
				return true;
            }
            return false;
        }
    }
}
