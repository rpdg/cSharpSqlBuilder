/*
 * Created by SharpDevelop.
 * Author: Lyu Pengfei
 * Date: 2014/8/14
 * Time: 15:13
*/
using System;
using System.Text;

using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using Lyu.Data;
using Lyu.Data.Helper;
using Lyu.Data.Types;
using Lyu.Data.Client;
using Lyu.Json;

namespace Lyu.Data.Helper
{
	/// <summary>
	/// Description of DAL.
	/// </summary>
	public class DAL
	{
		public Table mainTable;
		
		private BaseHelper bh;
		
		protected bool needLog = false;
		
		public List<string> SqlLogs {
			get {
				return bh.SqlLogs;
			}
		}
		
		protected DAL()
		{

		}
		
		
		public void Log(bool b = true)
		{ 
			needLog = b;
		}
		
		protected BaseHelper initDB()
		{
			return initDB(mainTable);
		}
		
		protected BaseHelper initDB(Table tb)
		{
			bh = DataBase.DB;
			
			if (needLog)
				bh.LogSql = needLog;
			
			return bh.Use(tb);
		}
		
		protected BaseHelper initDB(string tbName)
		{
			bh = DataBase.DB;
			if (needLog)
				bh.LogSql = needLog;
			
			return bh.Use(DataBase.Tables[tbName]);
		}
		

		public long Add<T>(T vo)
		{
			var dic = Util.Obj2Dic(vo);

			return Add(dic);
		}

		public long AddToTable<T>(T vo, Table tb)
		{
			var dic = Util.Obj2Dic(vo);

			return AddToTable(dic, tb);
		}

		public long Add(Dictionary<string , object> vo)
		{
			var db = initDB();
			return Convert.ToInt64(db.Add(vo).Result);
		}

		public long AddToTable(Dictionary<string , object> vo, Table tb)
		{
			var db = initDB(tb);
			return Convert.ToInt64(db.Add(vo).Result);
		}
		
		
		public int Del(Dictionary<string , object> wheres)
		{
			var db = initDB();
			return Convert.ToInt16(db.Del(wheres).Result);
		}

		public int Del(Dictionary<string , object> wheres, Table tb)
		{
			var db = initDB(tb);
			return Convert.ToInt16(db.Del(wheres).Result);
		}

		public int DelByKey(object primaryKeyValue, Table tb)
		{
			Dictionary<string , object> wheres = new Dictionary<string , object> { 
				{ this.mainTable.PrimaryKey , primaryKeyValue }
			};
			return Del(wheres, tb);
		}

		public int DelByKey(object primaryKeyValue)
		{
			Dictionary<string , object> wheres = new Dictionary<string , object> { 
				{ this.mainTable.PrimaryKey , primaryKeyValue }
			};
			return Del(wheres);
		}
		
		public int Set(Dictionary<string , object> updateTo, Dictionary<string , object> target)
		{
			var db = initDB();
			return Convert.ToInt32(db.Set(updateTo, target).Result);
		}
		public int SetToTable(Dictionary<string , object> updateTo, Dictionary<string , object> target, Table tb)
		{
			var db = initDB(tb);
			return Convert.ToInt32(db.Set(updateTo, target).Result);
		}

		public int Set<T>(T vo)
		{
			var keyName = this.mainTable.PrimaryKey;
			long keyValue = vo.GetType().GetProperty(keyName).GetValue(vo, null).TryToLong();
			if (keyValue > 0) {
				var upTo = Util.Obj2Dic(vo);
				upTo.Remove(keyName);

				var target = new Dictionary<string,object>() { 
					{ keyName , keyValue }
				};

				return Set(upTo, target);
			}

			return -1;
		}

		public int Set<T>(T vo, Table tb)
		{
			var keyName = this.mainTable.PrimaryKey;
			long keyValue = vo.GetType().GetProperty(keyName).GetValue(vo, null).TryToLong();
			if (keyValue > 0) {
				var upTo = Util.Obj2Dic(vo);
				upTo.Remove(keyName);

				var target = new Dictionary<string,object>() { 
					{ keyName , keyValue }
				};

				return SetToTable(upTo, target, tb);
			}

			return -1;
		}
		
		public DataTable Get(Dictionary<string , object> wheres, QueryObj which)
		{
			var db = initDB();
			return db.Get(wheres, which).TableResult;
		}


		public List<T> Get<T>(Dictionary<string , object> wheres, QueryObj which)
		{
			var tb = Get(wheres, which);
			return tb.Populate<T>();
		}

		
		// 一个显式的分页查询调用
		public DataPage Get(Dictionary<string , object> wheres, QueryObj which, int pageSize = 0, int pageIndex = 0)
		{
			which.PageIndex = pageIndex;
			which.PageSize = pageSize;
			
			var db = initDB();
			return db.Get(wheres, which).PageResult;
		}

		/// <summary>
		/// Gets the one.
		/// </summary>
		/// <returns>The one.</returns>
		/// <param name="wheres">Wheres.</param>
		/// <param name="which">Which.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetOne<T>(Dictionary<string , object> wheres, QueryObj which)
		{
			var db = initDB();
			var rv = db.GetRow(wheres, which);
			var row = rv.RowResult;
			if (row != null)
				return row.Populate<T>();
			
			return default(T);
		}
	}
}
