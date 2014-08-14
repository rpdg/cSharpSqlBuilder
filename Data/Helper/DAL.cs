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
		public BaseHelper db;
		
		public DAL()
		{			
		}
		
		public DAL(string tbName)
		{
			db = initDB(tbName);
		}
		
		protected BaseHelper initDB(string tbName)
		{
			var bh = Lyu.Data.DataBase.DB;
			return bh.Use(Lyu.Data.DataBase.Tables[tbName]);
		}
		
		
		public int Add(Dictionary<string , object> vo)
		{
			return Convert.ToInt16(db.Add(vo).Result);
		}
		
		public int Del(Dictionary<string , object> wheres)
		{
			return Convert.ToInt16(db.Del(wheres).Result);
		}
		
		public int Set(Dictionary<string , object> updateTo, Dictionary<string , object> target)
		{
			return Convert.ToInt16(db.Set(updateTo, target).Result);
		}
		
		public DataTable Get(Dictionary<string , object> wheres, QueryObj which)
		{
			return db.Get(wheres, which).TableResult;
		}
		
		/// <summary>
		/// 一个显式的分页查询
		/// </summary>
		/// <param name="wheres"></param>
		/// <param name="which"></param>
		/// <param name="pageSize"></param>
		/// <param name="pageIndex"></param>
		/// <returns>DataPage</returns>
		public DataPage Get(Dictionary<string , object> wheres, QueryObj which, int pageSize = 0, int pageIndex = 0)
		{
			which.PageIndex = pageIndex;
			which.PageSize = pageSize;
			return db.Get(wheres, which).PageResult;
		}
	}
}
