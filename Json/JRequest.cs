/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2014/8/21
 * Time: 13:40
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Web;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Lyu.Json
{
	/// <summary>
	/// Description of RequestForm.
	/// </summary>
	public class JRequest
	{
		public static object ParseForm()
		{
			//HttpContext.Current.Request.Form ;
			return new {} ;
		}
		
		public static Dictionary<string, object> ParseVO(string vo)
		{
			//return JsonConvert.DeserializeObject<List<KeyValuePair<string , object>>>(vo);
			return JsonConvert.DeserializeObject<Dictionary<string, object>>(vo);
		}
	}
}
