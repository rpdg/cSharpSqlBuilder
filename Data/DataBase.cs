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
		private static readonly DBS _dbSets = DBS.Load("App_Code/sql.json");
		
		private static readonly string _typeName = "Lyu.Data.Helper." + _dbSets.Provider;
		
		//private static readonly Assembly _dll = Assembly.Load(_typeName);
		//private static readonly BaseHelper _db = _dll.CreateInstance(_typeName) as BaseHelper;
		
		private static readonly Type _dbType = Type.GetType(_typeName);

		public static Dictionary<string , Table> Tables = _dbSets.Tables;
		
		public static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["constr"].ConnectionString;
		
		public static BaseHelper DB {
			get { 
				BaseHelper bh = Activator.CreateInstance(_dbType) as BaseHelper;
				bh.Schema = string.IsNullOrEmpty(_dbSets.Schema) ? "" : _dbSets.Schema+"." ;
				return bh;
			}
		}
		
		
		public static string Info {
			get {
				//_dbSets.Save("config/sql.json.config");				
				return _dbSets.Provider + ", " + _dbSets.Version;
			}
		}
        
	}
	
	
}
