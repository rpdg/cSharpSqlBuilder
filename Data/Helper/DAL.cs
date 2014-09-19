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
		public Table mainTable ;
		
		protected DAL()
		{			
		}
		
		
		protected BaseHelper initDB()
		{
			return initDB(mainTable);
		}
		
		protected BaseHelper initDB(Table tb)
		{
			var bh = DataBase.DB;
			return bh.Use(tb);
		}
		
		protected BaseHelper initDB(string tbName)
		{
			var bh = DataBase.DB;
			return bh.Use(DataBase.Tables[tbName]);
		}
		
		
		public int Add(Dictionary<string , object> vo)
		{
			var db = initDB();
			return Convert.ToInt16(db.Add(vo).Result);
		}
		
		
		public int Del(Dictionary<string , object> wheres)
		{
			var db = initDB();
			return Convert.ToInt16(db.Del(wheres).Result);
		}
		
		
		public int Set(Dictionary<string , object> updateTo, Dictionary<string , object> target)
		{
			var db = initDB();
			return Convert.ToInt16(db.Set(updateTo, target).Result);
		}
		
		
		public DataTable Get(Dictionary<string , object> wheres, QueryObj which)
		{
			var db = initDB();
			return db.Get(wheres, which).TableResult;
		}
		
		// 一个显式的分页查询调用
		public DataPage Get(Dictionary<string , object> wheres, QueryObj which, int pageSize = 0, int pageIndex = 0)
		{
			which.PageIndex = pageIndex;
			which.PageSize = pageSize;
			
			var db = initDB();
			return db.Get(wheres, which).PageResult;
		}
	}
}
