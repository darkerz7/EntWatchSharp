using AnyBaseLibNext;

namespace EntWatchSharp.Modules.Eban
{
    public abstract class Database
    {
		public IAnyBaseNext AnyDB;
		public bool bSuccess = false;
        public bool bDBReady = false;
        public Database(string sDBName, string sDBHost = "", string sDBUser = "", string sDBPassword = "") { }
    }

	public class DB_Mysql : Database
	{
		public DB_Mysql(string sDBName, string sDBHost = "", string sDBUser = "", string sDBPassword = "") : base(sDBName, sDBHost, sDBUser, sDBPassword)
		{
			AnyDB = CAnyBaseNext.Base("mysql");
			AnyDB.Set(sDBName, sDBHost, sDBUser, sDBPassword);
		}
	}

	public class DB_PosgreSQL : Database
	{
		public DB_PosgreSQL(string sDBName, string sDBHost = "", string sDBUser = "", string sDBPassword = "") : base(sDBName, sDBHost, sDBUser, sDBPassword)
		{
			AnyDB = CAnyBaseNext.Base("postgre");
			AnyDB.Set(sDBName, sDBHost, sDBUser, sDBPassword);
		}
	}

	public class DB_SQLite : Database
	{
		public DB_SQLite(string sDBName) : base(sDBName)
		{
			AnyDB = CAnyBaseNext.Base("sqlite");
			AnyDB.Set(sDBName);
		}
	}

	public class DBConfig
    {
        public string TypeDB { get; set; }
        public string SQLite_File { get; set; }
        public string SQL_Server { get; set; }
        public string SQL_Port { get; set; }
        public string SQL_User { get; set; }
        public string SQL_Password { get; set; }
        public string SQL_NameDatabase { get; set; }
        public DBConfig()
        {
            TypeDB = "sqlite";
            SQLite_File = "database";
        }
    }
}
