using System;
using System.Reflection;
using System.Collections.Generic;
using Lyu.Data.Types;
using Lyu.Data.Helper;
using System.Configuration;

namespace Lyu.Data
{
	/// <summary>
	/// Description of DbSettings.
	/// </summary>
	public class DataBase
	{
		private static DBS _dbSets;
		
		private static string _typeName;
		
		//private static readonly Assembly _dll = Assembly.Load(_typeName);
		//private static readonly BaseHelper _db = _dll.CreateInstance(_typeName) as BaseHelper;
		
		private static Type _dbType;

		public static Dictionary<string , Table> Tables;
		
		public static string ConnectionString;
		
		public static BaseHelper DB {
			get { 
				BaseHelper bh = Activator.CreateInstance(_dbType) as BaseHelper;
				bh.Schema = string.IsNullOrEmpty(_dbSets.Schema) ? "" : _dbSets.Schema + ".";
				return bh;
			}
		}
		
		public static void Init(string connStr, string cfgFileName)
		{
			ConnectionString = ConfigurationManager.ConnectionStrings[connStr].ConnectionString;
			_dbSets = DBS.Load("App_GlobalResources/" + cfgFileName);
			_typeName = "Lyu.Data.Helper." + _dbSets.Provider;
			_dbType = Type.GetType(_typeName);
			Tables = _dbSets.Tables;
			
		}
		
		public static string Info {
			get {
				//_dbSets.Save("config/sql.json.config");				
				return _dbSets.Provider + ", " + _dbSets.Version;
			}
		}
        
	}
	
	/// <summary>
	/// 数据库类型枚举
	/// </summary>
	public enum DbProviderType : byte
	{
		SqlServer,
		MySql,
		SQLite,
		Oracle,
		ODBC,
		OleDb,
		Firebird,
		PostgreSql,
		DB2,
		Informix,
		SqlServerCe
	}
}
