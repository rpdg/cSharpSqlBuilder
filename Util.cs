/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2015/10/1
 * Time: 17:31
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Web;
using System.Data;
using Lyu.Json;
using Lyu.Data.Types ;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace Lyu
{
	/// <summary>
	/// Description of Util.
	/// </summary>
	public static class Util
	{
		public static T ParseJson<T>(this HttpRequest req, string paramName="vo")
		{
			string json = req[paramName] ;
			if(string.IsNullOrEmpty(json)){
				json = "{}" ;
			}
			
			return json.Populate<T>();
		}
		
		public static void WriteJson(this HttpResponse resp, object val)
		{
			resp.ContentType = "application/json";
			if (val == null)
				resp.Write("{\"data\":null}");
			else {
				Type type = val.GetType();
				
				switch (Type.GetTypeCode(type)) {
					case TypeCode.String:
						resp.Write(val);
						break;
									
					case TypeCode.Int16:
					case TypeCode.Int32:
						resp.Write(((int)val).Send());
						break;

					case TypeCode.Int64:
						resp.Write(((long)val).Send());
						break;

					case TypeCode.Boolean:
						resp.Write(((bool)val).Send());
						break;

					case TypeCode.Empty:
					case TypeCode.DBNull:
						resp.Write("{\"data\":null}");
						break;
								
					default:
						if (type == typeof(DataTable)) {
							resp.Write(((DataTable)val).Send());
						} 
						else if (type == typeof(DataSet)) {
							resp.Write(((DataSet)val).Send());
						} 
						else if(type == typeof(DataPage)){
							resp.Write(((DataPage)val).Send());
						}
						else if(type == typeof(LyuJson)){
							resp.Write(DataConverter.Serialize(val));
						}
						else {
							//response.Write(obj.ToString());
							
							var outer = new LyuJson {
								data = val
							};
							
							resp.Write(DataConverter.Serialize(outer));
						}
						break;
				}
			}
		}
		
		public static string TryToString(this object obj, string defaultVal = "")
		{
			if (obj == null || Convert.IsDBNull(obj)) {
				return defaultVal;
			}
			return obj.ToString().Trim();
		}
		
		public static decimal TryToDecimal(this object obj, decimal def = 0m)
		{
			if (obj == null || Convert.IsDBNull(obj)) {
				return def;
			}
			decimal val = def;
			var b = decimal.TryParse(obj.ToString(), out val);
			return val;
		}
        
        
		public static int TryToInt(this object obj, int def = 0)
		{
			decimal val = obj.TryToDecimal((decimal)def);
			return (int)val;
		}
        
		public static short TryToShort(this object obj, short def = 0)
		{
			decimal val = obj.TryToDecimal((decimal)def);
			return (short)val;
		}
		
		public static long TryToLong(this object obj, long def = 0L)
		{
			decimal val = obj.TryToDecimal((decimal)def);
			return (long)val;
		}

		public static DateTime TryToDateTime(this object obj)
		{
			DateTime d = DateTime.Now;

			if (obj == null || Convert.IsDBNull(obj)) {
				return d;
			}


			var b = DateTime.TryParse(obj.ToString(), out d);
			return d;
		}

		
		
		public static T Populate<T>(this string jsonString)
		{
			T t = JsonConvert.DeserializeObject<T>(jsonString) ;

            return t;
		}
		
		public static T Populate<T>(this DataRow row)
		{
			T t = JsonConvert.DeserializeObject<T>(DataConverter.Serialize(row)) ;

            return t;
		}
		
		public static List<T> Populate<T>(this DataTable tb)
		{
			var list = JsonConvert.DeserializeObject<List<T>>(DataConverter.Serialize(tb)) ;

            return list;
		}



		public static Dictionary<string , object> Obj2Dic(object vo)
		{
			var json = DataConverter.Serialize(vo);
			return JsonConvert.DeserializeObject<Dictionary<string , object>>(json);
		}
	}
}
