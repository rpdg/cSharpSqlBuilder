/*
 * Created by SharpDevelop.
 * Author: Lyu Pengfei
 * Date: 2014/7/24
 * Time: 12:51
*/
using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web;

using Lyu.Data.Types;

namespace Lyu.Data.Client
{
	/// <summary>
	/// Description of SqlServer.
	/// </summary>
	public class SqlClient
	{
		private string _connStr;
		private SqlConnection _conn;
		private bool _autoClose;
		
		public bool autoClose {
			get {
				return _autoClose;
			}
			set {
				_autoClose = value;
			}
		}
		
		public string state {
			get {
				return _conn.State.ToString();
			}
		}
		
		
		public SqlClient(string connStr)
			: this(connStr, true)
		{
			//Add further instructions here.
		}
		
		public SqlClient(string connStr, bool autoClose)
		{
			_connStr = connStr;
			_conn = new SqlConnection(connStr);
			_autoClose = autoClose;
		}
		
		public void Close()
		{
			if (_conn.State == ConnectionState.Open)
				_conn.Close();
		}
		public void Dispose()
		{
			_conn.Dispose();
		}
		
		SqlCommand PrepareCommand(string cmdText, DbParameter[] cmdParms, bool isStoredProcedure = false, bool isTransaction = false)
		{
			SqlCommand cmd = new SqlCommand();
			cmd.CommandText = cmdText;
			cmd.Connection = _conn;
			cmd.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
			
			if (_conn.State != ConnectionState.Open)
				_conn.Open();
			
			if (isTransaction)
				cmd.Transaction = _conn.BeginTransaction();


			if (cmdParms != null) {
				int len = cmdParms.Length;
				for (int i = 0; i < len; i++) {
					cmd.Parameters.Add((SqlParameter)cmdParms[i]);
				}
			}
			
			//HttpContext.Current.Response.Write("<div style='border:1px solid green'>"+cmdText+"</div>") ;
			
			return cmd;
		}
		
		
		/// <summary>
		/// 执行查询，并返回查询所返回的结果集中第一行的第一列
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="sqlParameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <returns></returns>
		public object ExecuteScalar(string sql, DbParameter[] sqlParameters, bool isStoredProcedure = false)
		{
			object val;
			
			SqlCommand cmd = PrepareCommand(sql, sqlParameters, isStoredProcedure);
			
			val = cmd.ExecuteScalar();
			//HttpContext.Current.Response.Write("【"+ val.GetType() +"】");
			
			cmd.Parameters.Clear();
			
			if (_autoClose)
				Close();
			
			if (val == DBNull.Value || val == null)
				val = String.Empty;
			return val;
		}
		
		/// <summary>
		/// 执行 sql 语句并返回受影响的行数
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="sqlParameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <param name="isTransaction"></param>
		/// <returns></returns>
		public int ExecuteNonQuery(string sql, DbParameter[] sqlParameters, bool isStoredProcedure = false, bool isTransaction = false)
		{
			int retVal;
			SqlCommand cmd = PrepareCommand(sql, sqlParameters, isStoredProcedure, isTransaction);

			try {
				retVal = cmd.ExecuteNonQuery();
				if (isTransaction)
					cmd.Transaction.Commit();
				
			} catch (Exception e) {
				cmd.Transaction.Rollback();
				throw new Exception("数据库操作错误。错误信息" + e.Message);
			} finally {
				cmd.Parameters.Clear();
				if (_autoClose)
					Close();
			}
			
			return retVal;
		}
		
		/// <summary>
		/// 直接返回Dr，不能自动关闭，必须手动调用 dr.Close(); dataIsland.Close();
		/// 外部调用建议使用以下两个ByDr的方法
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="sqlParameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <returns></returns>
		private SqlDataReader GetDataReaderByCmd(string sql, DbParameter[] sqlParameters, bool isStoredProcedure = false)
		{
			SqlCommand cmd = PrepareCommand(sql, sqlParameters, isStoredProcedure, false);

			SqlDataReader dr = cmd.ExecuteReader();

			//cmd.Cancel();
			cmd.Parameters.Clear();
			return dr;
		}
		
		public DataTable GetTable(string sql, DbParameter[] sqlParameters, bool isStoredProcedure = false)
		{
			SqlCommand cmd = PrepareCommand(sql, sqlParameters, isStoredProcedure);
			DataTable dt = new DataTable();
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(dt);
			
			cmd.Parameters.Clear();
			
			if (_autoClose)
				Close();
			
			return dt;
			
		}
		
		/// <summary>
		/// 通过 DataReader 构建 DataTable 并返回
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="sqlParameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <returns></returns>
		public DataTable GetTableByReader(string sql, DbParameter[] sqlParameters, bool isStoredProcedure = false)
		{
			DataTable dt = new DataTable();
			
			SqlDataReader dr = GetDataReaderByCmd(sql, sqlParameters, isStoredProcedure);
			
			dt.Load(dr);
			
			dr.Close();
			if (_autoClose)
				Close();

			return dt;
		}
		
		/// <summary>
		/// 获得 DataSet
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="sqlParameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <returns></returns>
		public DataSet GetDataSet(string sql, DbParameter[] sqlParameters, bool isStoredProcedure = false)
		{
			SqlCommand cmd = PrepareCommand(sql, sqlParameters, isStoredProcedure);
			DataSet ds = new DataSet();
			SqlDataAdapter da = new SqlDataAdapter(cmd);
			da.Fill(ds);
			
			cmd.Parameters.Clear();
			
			if (_autoClose)
				Close();
			
			return ds;
		}
	}
}
