
using System;
using System.Data;
using System.Data.Common;

using System.Data.SQLite;

namespace Lyu.Data.Client
{
	/// <summary>
	/// Description of SqlLiteClient.
	/// </summary>
	public class SQLiteClient
	{
		private DbProviderFactory factory = System.Data.SQLite.SQLiteFactory.Instance ;
		private string _connStr;
		private SQLiteConnection _conn;
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
		
		
		public SQLiteClient(string connStr, bool autoClose)
		{
			_connStr = connStr.Replace("{AppDir}", AppDomain.CurrentDomain.BaseDirectory);;
			_conn = new SQLiteConnection(connStr);
			_autoClose = autoClose;
		}
		
		
		public SQLiteClient(string connStr): this(connStr, true)
		{
			
		}
		
		public void Close()
		{
			if (_conn.State == System.Data.ConnectionState.Open)
				_conn.Close();
		}
		public void Dispose()
		{
			_conn.Close();
			_conn.Dispose();
		}
		
		SQLiteCommand PrepareCommand(string cmdText, DbParameter[] parameters, bool isStoredProcedure = false, bool isTransaction = false)
		{
			SQLiteCommand cmd = new SQLiteCommand();
			cmd.CommandText = cmdText;
			cmd.Connection = _conn;
			cmd.CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text;
			cmd.CommandTimeout = 600;
			
			if (_conn.State != System.Data.ConnectionState.Open){
				
				_conn.Open();
			}
				
			
			if (isTransaction)
				cmd.Transaction = _conn.BeginTransaction();


			if (parameters != null) {
				int len = parameters.Length;
				for (int i = 0; i < len; i++) {
					cmd.Parameters.Add( (SQLiteParameter) parameters[i] );
				}
			}
			
			//HttpContext.Current.Response.Write("<div style='border:1px solid green'>"+cmdText+"</div>") ;
			
			return cmd;
		}
		
		
		/// <summary>
		/// 执行查询，并返回查询所返回的结果集中第一行的第一列
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <returns></returns>
		public object ExecuteScalar(string sql, DbParameter[] parameters, bool isStoredProcedure = false)
		{
			object val;
			
			SQLiteCommand cmd = PrepareCommand(sql, parameters, isStoredProcedure);
			
			val = cmd.ExecuteScalar();
			//HttpContext.Current.Response.Write("【"+ val.GetType() +"】");
			
			cmd.Parameters.Clear();
			
			if (_autoClose)
				Close();
			
			if (val == DBNull.Value || val == null)
				val = string.Empty;
			return val;
		}
		
		/// <summary>
		/// 执行 sql 语句并返回受影响的行数
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <param name="isTransaction"></param>
		/// <returns></returns>
		public int ExecuteNonQuery(string sql, DbParameter[] parameters, bool isStoredProcedure = false, bool isTransaction = false)
		{
			int retVal;
			SQLiteCommand cmd = PrepareCommand(sql, parameters, isStoredProcedure, isTransaction);

			try {
				retVal = cmd.ExecuteNonQuery();
				if (isTransaction)
					cmd.Transaction.Commit();
				
			} catch (Exception e) {
				if(isTransaction)
					cmd.Transaction.Rollback();
				
				throw new Exception("数据库操作错误。错误信息" + e.Message);
			} finally {
				cmd.Parameters.Clear();
				if (_autoClose)
					Close();
			}
			
			return retVal;
		}
		
		
		public DataTable GetTable(string sql, DbParameter[] parameters, bool isStoredProcedure = false)
		{
			SQLiteCommand cmd = PrepareCommand(sql, parameters, isStoredProcedure);
			DataTable dt = new DataTable();
			SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
			da.Fill(dt);
			
			cmd.Parameters.Clear();
			
			if (_autoClose)
				Close();
			
			return dt;
			
		}
		/// <summary>
		/// 获得 DataSet
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <returns></returns>
		public DataSet GetDataSet(string sql, DbParameter[] parameters, bool isStoredProcedure = false)
		{
			SQLiteCommand cmd = PrepareCommand(sql, parameters, isStoredProcedure);
			DataSet ds = new DataSet();
			SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
			
			da.Fill(ds);
			
			
			cmd.Parameters.Clear();
			
			if (_autoClose)
				Close();
			
			return ds;
		}
		
		private SQLiteDataReader GetDataReaderByCmd(string sql, DbParameter[] parameters, bool isStoredProcedure = false)
		{
			SQLiteCommand cmd = PrepareCommand(sql, parameters, isStoredProcedure, false);

			SQLiteDataReader dr = cmd.ExecuteReader();

			//cmd.Cancel();
			cmd.Parameters.Clear();
			return dr;
		}
		/// <summary>
		/// 通过 DataReader 构建 DataTable 并返回
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <param name="isStoredProcedure"></param>
		/// <returns></returns>
		public DataTable GetTableByReader(string sql, DbParameter[] parameters, bool isStoredProcedure = false)
		{
			DataTable dt = new DataTable();
			
			SQLiteDataReader dr = GetDataReaderByCmd(sql, parameters, isStoredProcedure);
			
			dt.Load(dr);
			
			dr.Close();
			if (_autoClose)
				Close();

			return dt;
		}
		
		
		
		
		
		
		
		public DbParameter CreateParameter(string name , DbType dbType, ParameterDirection direction)
        {
            DbParameter parameter = factory.CreateParameter() ;
			parameter.ParameterName = name;
			parameter.Direction = direction ;
			parameter.DbType = dbType;
			return parameter;
        }
		
		public DbParameter CreateParameter(string name, object value , DbType dbType)
        {
            DbParameter parameter = factory.CreateParameter()  ;
			parameter.ParameterName = name;
			parameter.DbType = dbType;
			parameter.Direction = ParameterDirection.Input ;
			parameter.Value = (value == null) ? DBNull.Value : value;
			return parameter;
        }
		
		public DbParameter CreateParameter(string name, object value , DbType dbType , int size)
        {
			return CreateParameter(name, value , dbType , size , ParameterDirection.Input);
        }
		
		public DbParameter CreateParameter(string name, object value, DbType dbType, int size, ParameterDirection direction)
		{
			DbParameter parameter = factory.CreateParameter()  ;
			parameter.ParameterName = name;
			parameter.DbType = dbType;
			parameter.Size = size;
			parameter.Direction = direction;
			parameter.Value = (value == null) ? DBNull.Value : value;
			return parameter;
		}
		
	}
}
