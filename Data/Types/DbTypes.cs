/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2014/7/23
 * Time: 13:02
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Data;
using Lyu.Json;
using System.Collections.Generic;

namespace Lyu.Data.Types
{
	public class DBS : JsonSettings<DBS>
	{
		public string Provider { get; set; }
		public string Version { get ; set ; }
		public string Schema { get; set; }
		public Dictionary<string , Table> Tables  { get; set; }
	}
		
	public struct Table
	{
		public string Name { get; set; }
		public string PrimaryKey { get; set; }
		public Dictionary<string , Column> Columns { get; set; }
	}
		
	public struct Column
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Default { get; set; }
		public bool AllowNull { get; set; }
		public bool AutoGenerate { get; set; }
	}
	
	
	public struct QueryObj
	{
		public string Select;
		public int PageSize;
		public int PageIndex;
		public string SortOn;
		public string SortType;
	}

	
	public struct PageInfo
	{
		public int pageSize;
		public int pageIndex;
		public int rowCount;
		public int pageCount;
	}
			
	public struct DataPage
	{
		public DataTable Table ;
		public PageInfo Page;
	}
}
