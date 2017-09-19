
using System;
using System.Data;
using System.Data.SQLite;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Lyu.Data;
using Lyu.Text;
using Lyu.Data.Types;
using Lyu.Data.Client;

namespace Lyu.Data.Helper
{
	/// <summary>
	/// Description of SQLite.
	/// </summary>
	public class SQLite : BaseHelper
	{
		bool inTrans;
		StringBuilder TransactionSql;
		List<SQLiteParameter> TransactionParamList;
		
		int ParamCount;
		SQLiteClient dac;
		
		//使用当前数据库
		public SQLite()
			: this(DataBase.ConnectionString)
		{
		}
		
		//使用当前数据库某个表
		public SQLite(Table tb)
			: this(DataBase.ConnectionString, tb)
		{
		}
		
		//使用某个数据库
		public SQLite(string connStr)
		{
			dac = new SQLiteClient(connStr);
		}
		
		
		//使用某个数据库某个表
		public SQLite(string connStr, Table tb)
			: this(connStr)
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
			TransactionParamList = new List<SQLiteParameter>();
			
			return this;
		}
		
		private BaseHelper SetTrans(string sql, SQLiteParameter[] param)
		{
			TransactionSql.Append(sql);
			if (param != null)
				TransactionParamList.AddRange(param);
			
			return this;
		}
		
		private BaseHelper CommitTrans()
		{
			string sql = TransactionSql.ToString();

			SQLiteParameter[] param = TransactionParamList.ToArray();

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
			string sortType = string.Compare(query.SortType, "asc", StringComparison.CurrentCultureIgnoreCase) == 0 ? " ASC" : " DESC";
			
			int pz;
			StringBuffer sql = "SELECT ";
			WhereCondition which = MakeWhere(wheres);
			string tbWhere = Schema + CurrentTable.Name + which.WhereSql;
			
			//paginated data
			if (query.PageSize > 0) {
				
				pz = query.PageSize;
				
				int offset = pz * query.PageIndex;
			
			
				sql.AppendFormat("{0} FROM {1} ORDER BY {2} {3} LIMIT {4} OFFSET {5};", showCols, tbWhere, sortOn, sortType, pz, offset);
				
				sql.AppendFormat("SELECT Count({0}) AS rowCount FROM {1};", sortOn, tbWhere);
			
				//HttpContext.Current.Response.Write("<hr>" + sql + which.Params.Length + "<hr><hr>");
				//return this;
			
				DataSet ds = dac.GetDataSet(sql, which.Params);
				
				int count = ds.Tables[1].Rows[0][0].TryToInt();
					
				DataPage dp = new DataPage { 
					Table = ds.Tables[0],
					Page = MakePageInfo(count, pz, query.PageIndex) 
				};
			
				return End(dp, sql);
			} else {
				//sql += DefaultPagesize + " " + showCols + " From " + tbWhere + " Order By " + sortOn + sortType ;
				sql.AppendFormat("{0} FROM {1} ORDER BY {2} {3} LIMIT {4}", showCols, tbWhere, sortOn, sortType, DefaultPagesize);
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
			
			StringBuffer orderBySql = string.IsNullOrEmpty(query.SortOn) ? string.Empty : (" Order By " + query.SortOn);
			
			WhereCondition which = MakeWhere(wheres);
			
			var sql = new StringBuffer();//"Select " + showCols + " FROM " + CurrentTable.Name + which.WhereSql + orderBySql + " ;";
			sql.AppendFormat("SELECT {0} FROM {1} {2} {3};", showCols, CurrentTable.Name, which.WhereSql, orderBySql);
			//HttpContext.Current.Response.Write("<hr>"+sql +  "<hr><hr>");
			//return this;
			
			object objVal = dac.ExecuteScalar(sql, which.Params);
			
			return End(objVal, sql);
		}
		
		/// <summary>
		/// fetch one row
		/// </summary>
		/// <param name="wheres"></param>
		/// <param name="query"></param>
		/// <returns></returns>
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
			List<SQLiteParameter> _parameters = new List<SQLiteParameter>();
			
			foreach (KeyValuePair<string, Column> kv in cols) {
				string fieldName = kv.Key;
				Column col = kv.Value;
				
				//在传入obj中存在数据
				if (obj.ContainsKey(fieldName)) {
					
					_colStrs.Append(fieldName + ",");
					
					SQLiteParameter param = makeParam(col, obj[fieldName]);
					
					_parameters.Add(param);
					_valueNames.Append(param.ParameterName + ",");
				}
				
				//obj中不存在数据，但在映射中存在默认值
				else if (col.Default != null) {
					_colStrs.Append(fieldName + ",");
					
					//当 字段默认值为 函数
					if (col.Default.IndexOf("(", StringComparison.Ordinal) > -1) {
						_valueNames.Append(col.Default + ",");
					} else {
						SQLiteParameter param = makeParam(col, col.Default);
						_parameters.Add(param);
						_valueNames.Append(param.ParameterName + ",");
					}
				}
				//不存在值，且不允许空，且不会自动生成
				else if (!col.AllowNull && !col.AutoGenerate) {
					throw new Exception("Error: " + CurrentTable.Name + ".[" + fieldName + "] can't be null.");
				}
			}
			
			_colStrs.Length = _colStrs.Length - 1;
			_valueNames.Length = _valueNames.Length - 1;
			
			StringBuffer sql = "";
			sql.AppendFormat("Insert Into {0}{1} ({2}) Values ({3}) ; ", Schema, CurrentTable.Name, _colStrs, _valueNames);
			sql += "SELECT last_insert_rowid() ;";
			
			
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
			if (updated.Count == 0) {
				return End(0, string.Empty);
			}
			
			Dictionary<string , Column> cols = CurrentTable.Columns;
			
			StringBuffer _valueNames = string.Empty;
			List<SQLiteParameter> _parameters = new List<SQLiteParameter>();
			
			foreach (KeyValuePair<string, Column> kv in cols) {
				string colName = kv.Key;
				Column col = kv.Value;
				
				//在传入obj中存在数据
				if (updated.ContainsKey(colName)) {
					
					SQLiteParameter param = makeParam(col, updated[colName]);
					_parameters.Add(param);
					
					_valueNames += colName + "=" + param.ParameterName + ",";
				}
			}
			
			
			WhereCondition whereObj = MakeWhere(wheres);
			
			//合并2个params
			int n = _parameters.Count;
			int m = whereObj.Params.Length;
			SQLiteParameter[] paramx = new SQLiteParameter[n + m];
			_parameters.CopyTo(paramx);
			whereObj.Params.CopyTo(paramx, n);
			
			StringBuffer sql = new StringBuffer();//"Update " + Schema + CurrentTable.Name + " Set " + _valueNames.ToString(0, _valueNames.Length - 1) + " " + whereObj.WhereSql + " ; ";
			sql.AppendFormat("UPDATE {0}{1} SET {2} {3} ;", Schema, CurrentTable.Name, _valueNames.ToString(0, _valueNames.Length - 1), whereObj.WhereSql);
			
			
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
				throw new InvalidExpressionException("Error: 'WHERE' condition can't be null."); 
			
			WhereCondition whereObj = MakeWhere(wheres);
			var sql = new StringBuffer();
			sql.AppendFormat("Delete From {0}{1} {2};", Schema, CurrentTable.Name, whereObj.WhereSql);
			//SELECT ROW_COUNT();
			
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
			
			List<SQLiteParameter> parameters = new List<SQLiteParameter>();
			
			StringBuffer sb = " Where 1=1 ";
			
			foreach (KeyValuePair<string, object> kv in wheres) {
				string key = kv.Key;
				object val = kv.Value;
				if (key.Contains("!")) {
					key = key.Replace("!", string.Empty);
				}

				string prc;
				SQLiteParameter param;
				
				if (hasColumn(key)) {
					
					
						
					if (val is KeyValuePair<string, object>) {
						KeyValuePair<string, object> bv = (KeyValuePair<string, object>)val;
						prc = bv.Key;
						param = makeParam(CurrentTable.Columns[key], bv.Value);
					} else {
						prc = "=";
						param = makeParam(CurrentTable.Columns[key], val);
					}
					parameters.Add(param);
						
					sb.AppendFormat(" And ({0} {1} {2}) ", key, prc, param.ParameterName);
						
				} else if (key.StartsWith("$", StringComparison.InvariantCultureIgnoreCase)) {
					//
					if (key.Equals("$or", StringComparison.InvariantCultureIgnoreCase)) {
						// { "$or" : {"colName" : colValue} }
						if (val is KeyValuePair<string, object>) {
							var rv = (KeyValuePair<string, object>)val;
							param = makeParam(CurrentTable.Columns[rv.Key], rv.Value);
							
							parameters.Add(param);
							sb.AppendFormat(" Or {0}={1} ", rv.Key, param.ParameterName);
						} 
						// { "$or" : [{"colName1" : colValue1} , {"colName2" : colValue2}] }
						else if (val is List<KeyValuePair<string, object>>) {
							var rvs = (List<KeyValuePair<string, object>>)val;
							if (rvs.Count > 0) {
								sb += " And ( ";
								foreach (var rv in rvs) {
									param = makeParam(CurrentTable.Columns[rv.Key], rv.Value);
									parameters.Add(param);
									sb.AppendFormat(" {0}={1} Or ", rv.Key, param.ParameterName);
								}
								sb.Length -= 3;
								sb += ") ";
							}
						}
						//sb.AppendFormat(" And ({0} Or {1})");
					} else if (key.Equals("$and", StringComparison.InvariantCultureIgnoreCase)) {
						// { "$and" : {"colName" : colValue} }
						if (val is KeyValuePair<string, object>) {
							var rv = (KeyValuePair<string, object>)val;
							param = makeParam(CurrentTable.Columns[rv.Key], rv.Value);

							parameters.Add(param);
							sb.AppendFormat(" And {0}={1} ", rv.Key, param.ParameterName);
						} 
					}
				}
			}
			
			return new WhereCondition {
				WhereSql = sb.ToString(),
				Params = parameters.ToArray()
			};
		}
		
		private SQLiteParameter makeParam(Column col, object val)
		{
			string pName = "$" + col.Name + (++ParamCount);	
			DbType type = (DbType)Enum.Parse(typeof(DbType), col.Type, true);
			
			SQLiteParameter param = new SQLiteParameter(pName, type);
			param.Value = val;
			
			return param;
		}
		
		private bool hasColumn(string colName)
		{
			return (allColumnNames.IndexOf("`" + colName + "`", StringComparison.OrdinalIgnoreCase) > -1);
		}
		
		
		public struct WhereCondition
		{
			public string WhereSql;
			public SQLiteParameter[] Params;
		}
		
	}
}
