
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
		public SQLite(): this(DataBase.ConnectionString)
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
		
		
		
		#endregion CRUD
		
	}
}
