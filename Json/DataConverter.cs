/*
 * Created by SharpDevelop.
 * Author: Lyu Pengfei
 * Date: 2014/7/23
 * Time: 13:37
*/
using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Lyu.Data.Types;
using System.Text;
namespace Lyu.Json
{
	
	public struct LyuJson
	{
		public dynamic data	{
			get;
			set;
		}
		public string error {
			get;
			set;
		}
	}
	
	/// <summary>
	/// Json处理类
	/// </summary>
	public static class DataConverter
	{
		/// <summary>
		/// 序列化数据为Json数据格式.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static string Serialize(object value)
		{
			string type = value.GetType().ToString();
 
			JsonSerializer json = new JsonSerializer();
 
			/**/
			json.NullValueHandling = NullValueHandling.Ignore;
			json.ObjectCreationHandling = ObjectCreationHandling.Replace;
			json.MissingMemberHandling = MissingMemberHandling.Ignore;
			json.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
 
			switch (type) {
				case "System.Data.DataRow":
					{
						json.Converters.Add(new DataRowConverter());
						break;
					}
				case "System.Data.DataTable":
					{
						json.Converters.Add(new DataTableConverter());
						break;
					}
				case "System.Data.DataSet":
					{
						json.Converters.Add(new DataSetConverter());
						break;
					}
				case "System.DateTime":
					{
						var dateConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd hh:mm:ss" };
						return JsonConvert.SerializeObject(value, dateConverter);
					}
				default:
					{
						//return JsonConvert.SerializeObject(value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore , DateFormatString="yyyy-MM-dd HH:mm:ss"});
						return JsonConvert.SerializeObject(value, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
					}
			}
			/*if (type == typeof(DataRow))
				json.Converters.Add(new DataRowConverter());
			else if (type == typeof(DataTable))
				json.Converters.Add(new DataTableConverter());
			else if (type == typeof(DataSet))
				json.Converters.Add(new DataSetConverter());
			else if (type == typeof(DateTime)) {
				var dateConverter = new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd hh:mm:ss" };
				return JsonConvert.SerializeObject(value, dateConverter);
			} 
			else
				return JsonConvert.SerializeObject(value);*/
			
			using (StringWriter sw = new StringWriter()) 
			{
				Newtonsoft.Json.JsonTextWriter writer = new JsonTextWriter(sw);
				//writer.Formatting = Formatting.Indented;
				writer.Formatting = Formatting.None;
				writer.QuoteChar = '"';
				json.Serialize(writer, value);
 
				string output = sw.ToString();
				writer.Close();
				sw.Close();
 
				return output;
			}
			
		}
 
		/// <summary>
		/// 反序列化Json数据格式.
		/// </summary>
		/// <param name="jsonText">The json text.</param>
		/// <param name="valueType">Type of the value.</param>
		/// <returns></returns>
		public static object Deserialize(string jsonText, Type valueType)
		{
			Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();
 
			json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
			json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
			json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
			json.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
 
			StringReader sr = new StringReader(jsonText);
			Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(sr);
			object result = json.Deserialize(reader, valueType);
			reader.Close();
 
			return result;
		}
		
		
		
		
				
		
		
		public const string nullJsonStr = "{\"data\":null}";
		public static JsonSerializerSettings ignoreNull = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

		
		public static string SendOne(this DataTable tb, int index = 0)
		{
			if (index < tb.Rows.Count)
				return "{\"data\":" + DataConverter.Serialize(tb.Rows[index]) + "}";
			
			return nullJsonStr;
		}
		
		public static string Send(this DataRow row)
		{
			if (row == null)
				return nullJsonStr;
			
			var sb = new StringBuilder("{\"data\":");
			sb.Append(DataConverter.Serialize(row));
			sb.Append('}');
			return sb.ToString();
		}
		public static string Send(this DataRow[] rows)
		{
			var sb = new StringBuilder("{\"data\":[");
			foreach (DataRow row in rows) {
				sb.Append(DataConverter.Serialize(row));
				sb.Append(',');
			}
			sb.Length = sb.Length - 1;
			sb.Append("]}");
			return sb.ToString();
		}
		
		public static string Send(this DataTable tb)
		{
			var sb = new StringBuilder("{\"data\":");
			//sb.Append(tb.ObjecttoJson()) ;
			sb.Append(DataConverter.Serialize(tb));
			sb.Append('}');
			return sb.ToString();
		}
		
		public static string Send(this DataTable tb, string pageSize, string pageIndex, int rowCount)
		{
			return tb.Send(Convert.ToInt32(pageSize), Convert.ToInt32(pageIndex), rowCount);
		}
		public static string Send(this DataTable tb, int pageSize, int pageIndex, int rowCount)
		{
			var sb = new StringBuilder("{\"data\":");
			sb.Append(DataConverter.Serialize(tb));
			sb.Append(MakePageInfo(pageSize, pageIndex, rowCount));
				
			return sb.ToString();
		}
		
		public static string Send(this DataPage dp)
		{				
			return dp.Table.Send(dp.Page.pageSize, dp.Page.pageIndex, dp.Page.rowCount);
		}

		
		public static string Send(this DataSet ds)
		{
			var sb = new StringBuilder("{\"data\":");
			sb.Append(DataConverter.Serialize(ds));
			sb.Append('}');
			return sb.ToString();
		}

		public static string SendOne<T>(this IEnumerable<T> b, int i = 0)
		{
			if (b == null)
				return nullJsonStr;
			
			
			int c = 0;
			foreach (var p in b) {
				if (c == i)
					return "{\"data\":" + JsonConvert.SerializeObject(p, ignoreNull) + "}";
				else
					c++;
			}
			
			return nullJsonStr;
		}
		
		public static string Send<T>(this IEnumerable<T> t)
		{
			var sb = new StringBuilder("{\"data\":");
			sb.Append(JsonConvert.SerializeObject(t, ignoreNull));
			sb.Append('}');
			return sb.ToString();
		}
		
		public static string Send<T>(this List<T> t)
		{
			var sb = new StringBuilder("{\"data\":");
			sb.Append(JsonConvert.SerializeObject(t, ignoreNull));
			sb.Append('}');
			
			return sb.ToString();
		}

		public static string Send<T>(this List<T> t, int pageSize, int pageIndex, int rowCount)
		{
			var sb = new StringBuilder("{\"data\":");
			sb.Append(JsonConvert.SerializeObject(t, ignoreNull));
			sb.Append(MakePageInfo(pageSize, pageIndex, rowCount));

			return sb.ToString();
		}

		
//		public static string Send(this bool t, string prefix)
//		{
//			return t ? (prefix + "成功").JsonStrToStrJson() : string.Empty.JsonStrToStrJson(prefix + "失败");
//		}
		
		public static string Send(this bool t)
		{
			return "{\"data\":" + (t ? "true" : "false") + "}";
		}
		
		
		public static string Send(this string str)
		{
			var obj = new LyuJson {
				data = str
			};
			return JsonConvert.SerializeObject(obj);
		}
		
		public static string Send(this long t)
		{
			return "{\"data\":" + t + "}";
		}
		public static string Send(this long t, string propertyName)
		{
			return "{\"" + propertyName + "\":" + t + "}";
		}
		
		public static string Send(this int t)
		{
			return "{\"data\":" + t + "}";
		}
		public static string Send(this int t, string propertyName)
		{
			return "{\"" + propertyName + "\":" + t + "}";
		}
		
		
		
		/// <summary>
		/// 成功操作应当传入空字串或null，失败的传入原因
		/// </summary>
		/// <param name="t"></param>
		/// <param name="prefix"></param>
		/// <returns></returns>
//		public static string Send(this string t, string prefix)
//		{
//			return string.IsNullOrEmpty(t) ? 
//				(prefix + "成功").JsonStrToStrJson() : 
//				string.Empty.JsonStrToStrJson("由于" + t + ", " + prefix + "失败");
//		}
		
		
		/// <summary>
		/// 生成前面带“，”，结尾2个“｝”
		/// </summary>
		/// <param name="pageSize"></param>
		/// <param name="pageIndex"></param>
		/// <param name="rowCount"></param>
		/// <returns></returns>
		public static string MakePageInfo(int pageSize, int pageIndex, int rowCount)
		{
			var sb = new StringBuilder(",\"page\":{\"rowCount\":");
			sb.Append(rowCount);
			sb.Append(",\"pageCount\":");
			if (rowCount > 0)
				sb.Append((int)Math.Ceiling(rowCount / (double)pageSize));
			else
				sb.Append('0');
			sb.Append(",\"pageSize\":");
			sb.Append(pageSize);
			sb.Append(",\"pageIndex\":");
			sb.Append(pageIndex);
			sb.Append("}}");
			return sb.ToString();
		}
		
		public static string MakePageInfo(string pageSize, string pageIndex, int rowCount)
		{
			return MakePageInfo(Convert.ToInt32(pageSize), Convert.ToInt32(pageIndex), rowCount);
		}

		
	}
 
	/// <summary>
	/// Converts a <see cref="DataRow"/> object to and from JSON.
	/// </summary>
	public class DataRowConverter : JsonConverter
	{
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
		/// <param name="dataRow">The data row.</param>
		/// <param name="ser">The JsonSerializer 对象.</param>
		public override void WriteJson(JsonWriter writer, object dataRow, JsonSerializer ser)
		{
			DataRow row = dataRow as DataRow;
 
			writer.WriteStartObject();
			foreach (DataColumn column in row.Table.Columns) {
				writer.WritePropertyName(column.ColumnName);
				ser.Serialize(writer, row[column]);
			}
			writer.WriteEndObject();
		}
 
		/// <summary>
		/// Determines whether this instance can convert the specified value type.
		/// </summary>
		/// <param name="valueType">Type of the value.</param>
		/// <returns>
		///     <c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanConvert(Type valueType)
		{
			return typeof(DataRow).IsAssignableFrom(valueType);
		}
 
		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue"></param>
		/// <param name="ser"></param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer ser)
		{
			throw new NotImplementedException();
		}
	}
 
	/// <summary>
	/// Converts a DataTable to JSON. Note no support for deserialization
	/// </summary>
	public class DataTableConverter : JsonConverter
	{
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
		/// <param name="dataTable">The data table.</param>
		/// <param name="ser">The JsonSerializer Object.</param>
		public override void WriteJson(JsonWriter writer, object dataTable, JsonSerializer ser)
		{
			DataTable table = dataTable as DataTable;
			DataRowConverter converter = new DataRowConverter();
 
			//writer.WriteStartObject();
			//writer.WritePropertyName("data");
 			
			writer.WriteStartArray();
 
			foreach (DataRow row in table.Rows) {
				converter.WriteJson(writer, row, ser);
			}
 
			writer.WriteEndArray();
			
			//writer.WriteEndObject();
		}
 
		/// <summary>
		/// Determines whether this instance can convert the specified value type.
		/// </summary>
		/// <param name="valueType">Type of the value.</param>
		/// <returns>
		///     <c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanConvert(Type valueType)
		{
			return typeof(DataTable).IsAssignableFrom(valueType);
		}
 
		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue"></param>
		/// <param name="ser"></param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer ser)
		{
			throw new NotImplementedException();
		}
	}
 
	/// <summary>
	/// Converts a <see cref="DataSet"/> object to JSON. No support for reading.
	/// </summary>
	public class DataSetConverter : JsonConverter
	{
		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
		/// <param name="dataset">The dataset.</param>
		/// <param name="ser">The JsonSerializer Object.</param>
		public override void WriteJson(JsonWriter writer, object dataset, JsonSerializer ser)
		{
			DataSet dataSet = dataset as DataSet;
 
			DataTableConverter converter = new DataTableConverter();
 
			//writer.WriteStartObject();
			//writer.WritePropertyName("data");
 			
			writer.WriteStartArray();
 
			foreach (DataTable table in dataSet.Tables) {
				converter.WriteJson(writer, table, ser);
			}
			writer.WriteEndArray();
			
			//writer.WriteEndObject();
		}
 
		/// <summary>
		/// Determines whether this instance can convert the specified value type.
		/// </summary>
		/// <param name="valueType">Type of the value.</param>
		/// <returns>
		///     <c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
		/// </returns>
		public override bool CanConvert(Type valueType)
		{
			return typeof(DataSet).IsAssignableFrom(valueType);
		}
 
		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue"></param>
		/// <param name="ser"></param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer ser)
		{
			throw new NotImplementedException();
		}
		
		

		
	}

}
