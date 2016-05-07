/*
 * Created by SharpDevelop.
 * User: Lyu Pengfei
 * Date: 2014/7/23
 * Time: 11:31
 */
using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using Lyu.Data.Types;
using Lyu.Json;

namespace Lyu.Data.Helper
{
	/// <summary>
	/// Description of Base.
	/// </summary>
	public abstract class BaseHelper
	{
		public string Schema;
		public Table CurrentTable;
		public string allColumnNames;
		public List<object> Results = new List<object>();
		public List<string> SqlLogs = new List<string>();
		public bool LogSql;
		public int DefaultPagesize = 100;
		
		//返回数据库连接状态
		public abstract string ConnectState{ get; }
		
		public BaseHelper Log(bool isLog)
		{
			LogSql = isLog;
			return this;
		}
		
		public string LastSql {
			get {
				if (SqlLogs.Count > 0)
					return SqlLogs[SqlLogs.Count - 1];
				return null;
			}
		}
		
		public BaseHelper PageSize(int p)
		{
			DefaultPagesize = p;
			return this;
		}
		
		
		/// <summary>
		/// 使用某个 Table对象
		/// </summary>
		/// <param name="tb"></param>
		/// <returns></returns>
		public BaseHelper Use(Table tb)
		{
			CurrentTable = tb;
			
			StringBuilder sb = new StringBuilder();
			foreach (KeyValuePair<string, Column> kv in tb.Columns) {
				sb.Append("`" + kv.Key + "`,");
			}
			allColumnNames = sb.ToString(0, sb.Length - 1);

			return this;
		}
		
		/// <summary>
		/// 最后一次取得的操作结果
		/// </summary>
		public object Result {
			get {
				if (Results.Count > 0)
					return Results[Results.Count - 1];
				
				return null;
			}
		}

		/// <summary>
		/// Gets the row result.
		/// </summary>
		/// <value>The row result.</value>
		public DataRow RowResult{
			get{
				var rs = this.Result;
				if (rs != null) {
					var type = rs.GetType();
					if (type == typeof(DataRow))
						return (DataRow)rs;

					return null;
				}
				return null;
			}
		}

		/// <summary>
		/// 将最后一次的操作结果Table
		/// </summary>
		public DataTable TableResult {
			get {
				var rs = this.Result;
				var type = rs.GetType();
				
				if (type == typeof(DataSet))
					return ((DataSet)rs).Tables[0];
				
				if (type == typeof(DataTable))
					return (DataTable)rs;
				
				if (type == typeof(DataPage))
					return ((DataPage)rs).Table;
				
				return null;
			}
		}
		
		/// <summary>
		/// 返回最后一次查询的结果集的总数，非DataTable中的结果数
		/// 需DataSet配合执行
		/// </summary>
		public int Count {
			get {
				var rs = this.Result;
				var type = rs.GetType();
				
				if (type == typeof(DataSet)) {
					DataRow rw = ((DataSet)rs).Tables[1].Rows[0];
					return (int)rw[0];
				}
				
				return 0;
			}
		}
		
		
		
		/// <summary>
		/// 返回最后一次的操作结果DataPage
		/// </summary>
		public DataPage PageResult {
			get {
				var rs = this.Result;
				var type = rs.GetType();
				
				if (type == typeof(DataPage))
					return (DataPage)rs;
				else {
					DataTable tb = this.TableResult;
					return new DataPage { 
						Table = tb,
						Page = new PageInfo { 
							pageSize = this.DefaultPagesize,
							pageIndex = 0,
							pageCount = 1,
							rowCount = tb.Rows.Count
						}
					};
				}
			}
		}
		
		/// <summary>
		/// 将最后一次的操作结果ToJson
		/// </summary>
		public string JsonResult {
			get {
				var rs = this.Result;
				if (rs == null)
					return "null";
				
				return DataConverter.Serialize(rs);
			}
		}
		
		
		/// <summary>
		/// 生成一个 PageInfo 对象
		/// </summary>
		/// <param name="rowCount"></param>
		/// <param name="pageSize"></param>
		/// <param name="pageIndex"></param>
		/// <returns></returns>
		public static PageInfo MakePageInfo(int rowCount, int pageSize, int pageIndex)
		{
			return new PageInfo {
				rowCount = rowCount,
				pageCount = (int)Math.Ceiling(rowCount / (double)pageSize),
				pageSize = pageSize,
				pageIndex = pageIndex
			};
		}
		
		
		public abstract BaseHelper Batch(bool beginTransaction = false);
		public abstract BaseHelper EndBatch();

		#region CRUD
		public abstract BaseHelper Get(Dictionary<string , object> wheres);
		public abstract BaseHelper Get(Dictionary<string , object> wheres, QueryObj query);
		public abstract BaseHelper GetField(Dictionary<string , object> wheres, QueryObj query, string whichColumn = null);
		public abstract BaseHelper GetRow(Dictionary<string , object> wheres, QueryObj query);

		public abstract BaseHelper Add(Dictionary<string , object> obj) ;
		public abstract BaseHelper Set(Dictionary<string , object> updated, Dictionary<string , object> wheres) ;
		public abstract BaseHelper Del(Dictionary<string , object> wheres);
		
		//public abstract BaseHelper ExecSql(string sql, DbParameter[] sqlParameters, bool isStoredProcedure = false);
		#endregion
	}
}
