﻿using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Collections.Generic;

using Lyu.Data;
using Lyu.Data.Types;
using Lyu.Data.Client;

namespace Lyu.Data.Helper
{
	/// <summary>
	/// Description of SqlServer.
	/// </summary>
	public class SqlServer : BaseHelper
	{
		bool inTrans;
		StringBuilder TransactionSql;
		List<SqlParameter> TransactionParamList;
		
		int ParamCount;
		SqlClient dac;
		
		//使用当前数据库
		public SqlServer() : this(DataBase.ConnectionString)
		{
		}
		
		//使用当前数据库某个表
		public SqlServer(Table tb) : this(DataBase.ConnectionString, tb)
		{
		}
		
		//使用某个数据库
		public SqlServer(string connStr)
		{
			dac = new SqlClient(connStr);
		}
		
		//使用某个数据库某个表
		public SqlServer(string connStr, Table tb) : this(connStr)
		{
			this.Use(tb);
		}
		
		
		private BaseHelper End(object result, string sql)
		{
			Results.Add(result);
			
			if (LogSql) {
				SqlLogs.Add(sql);
			}
			return this;
		}
		
		//返回数据库连接状态
		public override string ConnectState { 
			get {
				return dac.state;
			}
		}
		
		//启用批次模式
		public override BaseHelper Batch(bool beginTransaction = false)
		{
			dac.autoClose = false;
			if (beginTransaction)
				return BeginTrans();
			return this;
		}
		
		
		//结束批次模式
		public override BaseHelper EndBatch()
		{
			dac.autoClose = true;
			dac.Close();

			if (inTrans)
				return CommitTrans();

			return this;
		}
		
		private BaseHelper BeginTrans()
		{
			inTrans = true;
			TransactionSql = new StringBuilder(256);
			TransactionParamList = new List<SqlParameter>();
			
			return this;
		}
		
		private BaseHelper SetTrans(string sql, SqlParameter[] param)
		{
			TransactionSql.Append(sql);
			if (param != null)
				TransactionParamList.AddRange(param);
			
			return this;
		}
		
		private BaseHelper CommitTrans()
		{
			string sql = TransactionSql.ToString();

			SqlParameter[] param = TransactionParamList.ToArray();

			int i = dac.ExecuteNonQuery(sql, param, false, true);

			inTrans = false;
			TransactionSql = null;
			TransactionParamList = null;

			return End(i, sql);
		}
		
		
		

		#region CRUD
		public override BaseHelper Get(Dictionary<string , object> wheres)
		{
			QueryObj query = new QueryObj();
			
			return Get(wheres, query);
		}
		public override BaseHelper Get(Dictionary<string , object> wheres, QueryObj query)
		{
			string showCols;
			if (string.IsNullOrEmpty(query.Select) || query.Select == "*") {
				showCols = allColumnNames;
			} else {
				showCols = query.Select;
			}
			
			string sortOn = string.IsNullOrEmpty(query.SortOn) ? CurrentTable.PrimaryKey : query.SortOn;
			string sortType = string.IsNullOrEmpty(query.SortType) ? "ASC" : query.SortType.ToLower() == "asc" ? "ASC" : "DESC";
			
			int pz;
			string sql;
			WhereCondition which = MakeWhere(wheres);
			
			//paginated data
			if (query.PageSize > 0) {
				
				pz = query.PageSize;
				
				int offset = pz * query.PageIndex;
			
				sql = "Select top " + pz + " * FROM (Select " + showCols + ", ROW_NUMBER() OVER (ORDER BY " + sortOn + " " + sortType + ") AS TabRowNumber FROM " +
				Schema + CurrentTable.Name + which.WhereSql +
				") as TA WHERE TA.TabRowNumber>" + offset + ";";
			
				sql += "Select @rowCount=Count(" + sortOn + ") FROM " + Schema + CurrentTable.Name + which.WhereSql;
			
				SqlParameter rowCount = new SqlParameter("@rowCount", SqlDbType.Int) { 
					Direction = ParameterDirection.Output 
				};
				
				SqlParameter[] pms = new SqlParameter[which.Params.Length + 1];
				which.Params.CopyTo(pms, 0);
				pms[which.Params.Length] = rowCount;
				
				
				//HttpContext.Current.Response.Write("<hr>" + sql + which.Params.Length + "<hr><hr>");
				//return this;
			
				DataTable dt = dac.GetTable(sql, pms);
				
				int count = (int)rowCount.Value;
					
				DataPage dp = new DataPage { 
					Table = dt,
					Page = MakePageInfo(count, pz, query.PageIndex) 
				};
			
				return End(dp, sql);
			} else {
				pz = DefaultPagesize;
				
				sql = "Select Top " + pz + " " + showCols + " From " + Schema + CurrentTable.Name
				+ which.WhereSql + " Order By " + sortOn + " " + sortType + ";";
				
				//HttpContext.Current.Response.Write("<hr>" + sql + "<hr><hr>");
				//return this;
				
				DataTable dt = dac.GetTableByReader(sql, which.Params);
			
				return End(dt, sql);
			}
			
		}
		
		
		
		
		public override BaseHelper GetField(Dictionary<string , object> wheres, QueryObj query, string whichColumn = null)
		{
			string showCols = whichColumn ?? CurrentTable.PrimaryKey;
			if (!CurrentTable.Columns.ContainsKey(whichColumn))
				return End(string.Empty, string.Empty);
			
			string orderBySql = string.IsNullOrEmpty(query.SortOn) ? String.Empty : (" Order By " + query.SortOn);
			
			WhereCondition which = MakeWhere(wheres);
			
			string sql = "Select " + showCols + " FROM " + CurrentTable.Name + which.WhereSql + orderBySql + " ;";
			
			//HttpContext.Current.Response.Write("<hr>"+sql +  "<hr><hr>");
			//return this;
			
			object objVal = dac.ExecuteScalar(sql, which.Params);
			
			return End(objVal, sql);
		}
		
		public override BaseHelper GetRow(Dictionary<string , object> wheres, QueryObj query)
		{
			query.PageSize = 1;
			
			DataTable tb = Get(wheres, query).TableResult;
			
			if (tb.Rows.Count > 0)
				return End(tb.Rows[0], string.Empty);
			
			return End(null, string.Empty);
		}
		

		/// <summary>
		/// Insert
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override BaseHelper Add(Dictionary<string , object> obj)
		{
			Dictionary<string , Column> cols = CurrentTable.Columns;
			
			StringBuilder _colStrs = new StringBuilder();
			StringBuilder _valueNames = new StringBuilder();
			List<SqlParameter> _parameters = new List<SqlParameter>();
			
			foreach (KeyValuePair<string, Column> kv in cols) {
				string fieldName = kv.Key;
				Column col = kv.Value;
				
				//在传入obj中存在数据
				if (obj.ContainsKey(fieldName)) {
					
					_colStrs.Append(fieldName + ",");
					
					SqlParameter param = makeParam(col, obj[fieldName]);
					
					_parameters.Add(param);
					_valueNames.Append(param.ParameterName + ",");
				}
				
				//obj中不存在数据，但在映射中存在默认值
				else if (col.Default != null) {
					_colStrs.Append(fieldName + ",");
					
					//当 字段默认值为 函数
					if (col.Default.IndexOf("()") > -1) {
						_valueNames.Append(col.Default + ",");
					} else {
						SqlParameter param = makeParam(col, col.Default);
						_parameters.Add(param);
						_valueNames.Append(param.ParameterName + ",");
					}
				}
				//不存在值，且不允许空，且不会自动生成
				else if (!col.AllowNull && !col.AutoGenerate) {
					throw new Exception("Error: " + CurrentTable.Name + ".[" + fieldName + "] can't be null.");
				}
			}
			
			string sql = "Set Nocount On; Insert Into " + Schema + CurrentTable.Name
			             + " (" + _colStrs.ToString(0, _colStrs.Length - 1) + ") Values ("
			             + _valueNames.ToString(0, _valueNames.Length - 1)
			             + ") ; SELECT @@IDENTITY ;Set Nocount Off;";
			
			//HttpContext.Current.Response.Write("<hr>"+sql +  "<hr><hr>");
			//return this;
			if (inTrans)
				return SetTrans(sql, _parameters.ToArray());
			else {
				object objVal = dac.ExecuteScalar(sql, _parameters.ToArray());
				return End(objVal, sql);
			}
		}
		
		/// <summary>
		/// Update
		/// </summary>
		/// <param name="updated"></param>
		/// <param name="wheres"></param>
		/// <returns></returns>
		public override BaseHelper Set(Dictionary<string , object> updated, Dictionary<string , object> wheres)
		{
			Dictionary<string , Column> cols = CurrentTable.Columns;
			
			StringBuilder _valueNames = new StringBuilder();
			List<SqlParameter> _parameters = new List<SqlParameter>();
			
			foreach (KeyValuePair<string, Column> kv in cols) {
				string colName = kv.Key;
				Column col = kv.Value;
				
				//在传入obj中存在数据
				if (updated.ContainsKey(colName)) {
					
					SqlParameter param = makeParam(col, updated[colName]);
					_parameters.Add(param);
					
					_valueNames.Append(colName + "=" + param.ParameterName + ",");
				}
			}
			
			WhereCondition whereObj = MakeWhere(wheres);
			
			//合并2个params
			int n = _parameters.Count;
			int m = whereObj.Params.Length;
			SqlParameter[] paramx = new SqlParameter[n + m];
			_parameters.CopyTo(paramx);
			whereObj.Params.CopyTo(paramx, n);
			
			string sql = "Update " + Schema + CurrentTable.Name + " Set " + _valueNames.ToString(0, _valueNames.Length - 1) + " " + whereObj.WhereSql + " ; ";
			
//			HttpContext.Current.Response.Write("<hr>"+sql +  "<hr>"+paramx.Length+"<hr>");
//			for (int i = 0; i < paramx.Length ; i++){
//				HttpContext.Current.Response.Write("<hr>"+paramx[i].ParameterName +  "<hr>"+paramx[i].Value+"<hr>");
//			}
//			return this;
			
			if (inTrans)
				return SetTrans(sql, paramx);
			else {
				int v = dac.ExecuteNonQuery(sql, paramx);
				return End(v, sql);
			}
		}
		
		/// <summary>
		/// Delete
		/// </summary>
		/// <param name="wheres"></param>
		/// <returns></returns>
		public override BaseHelper Del(Dictionary<string , object> wheres)
		{
			if (wheres == null)
				throw new Exception("Error: 'WHERE' condition can't be null."); 
			
			WhereCondition whereObj = MakeWhere(wheres);
			string sql = "Delete From " + Schema + CurrentTable.Name + whereObj.WhereSql + " ; ";
			
			if (inTrans)
				return SetTrans(sql, whereObj.Params);
			else {
				int v = dac.ExecuteNonQuery(sql, whereObj.Params);
				return End(v, sql);
			}
		}
		
		
		#endregion CRUD
		
		
		
		private WhereCondition MakeWhere(Dictionary<string , object> wheres)
		{
			
			List<SqlParameter> parameters = new List<SqlParameter>();
			
			StringBuilder sb = new StringBuilder(" Where 1=1 ", 255);
			
			
			foreach (KeyValuePair<string, object> kv in wheres) {
				string key = kv.Key;
				object val = kv.Value;
				
				if (hasColumn(key)) {
					
					string prc;
					SqlParameter param ;
					
					if (val is KeyValuePair<string, object>) {
						KeyValuePair<string, object> bv = (KeyValuePair<string, object>) val;
						prc = bv.Key;
						param = makeParam(CurrentTable.Columns[key], bv.Value);
					}
					else{
						prc = "=";
						param = makeParam(CurrentTable.Columns[key], val);
					}
					parameters.Add(param);
					sb.Append(" And (" + key + prc + param.ParameterName + ") ");
				}
			}
			
			return new WhereCondition {
				WhereSql = sb.ToString(),
				Params = parameters.ToArray()
			};
		}
		
		private SqlParameter makeParam(Column col, object val)
		{
			string pName = "@" + col.Name + (++ParamCount);	
			SqlDbType type = (SqlDbType)Enum.Parse(typeof(SqlDbType), col.Type, true);
			
			SqlParameter param = new SqlParameter(pName, type);
			param.Value = val;
			
			return param;
		}
		private bool hasColumn(string colName)
		{
			return (allColumnNames.IndexOf("[" + colName + "]") > -1);
		}
		
		
		public struct WhereCondition
		{
			public string WhereSql;
			public SqlParameter[] Params;
		}
		
	}
	
	
}
