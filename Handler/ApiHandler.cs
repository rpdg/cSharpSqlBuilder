using System;
using System.Data;
using System.Web;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Web.SessionState;

namespace Lyu.Handler
{
	/// <summary>
	/// API handler.
	/*  <httpHandlers>
			<add path="/api/$star$/$star$/*" verb="*" type="Lyu.Handler.ApiHandler, Lyu" />
		</httpHandlers>
		note :  $star$ is *
		usage:  http://server:port/api/{dll-file-name}/{class-name}/{static-func}?param1=val1...
	 */
	/// </summary>
	public class ApiHandler : IHttpHandler, IRequiresSessionState
	{
		public void ProcessRequest(HttpContext context)
		{
			HttpRequest req = context.Request;
			HttpResponse resp = context.Response;
			string[] p = req.Path.Split('/');
			string packageName = p[2];
			string className = p[3];
			string methodName = p[4];

			Type type = Assembly.Load(packageName).GetType(packageName + "." + className);
			MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);

			object[] methodParam = ReturnMethodParams(req, methodInfo.GetParameters());
			var result = methodInfo.Invoke(methodInfo, methodParam);

			resp.WriteJson(result);
			
			
			/*resp.Write('\r');
			resp.Write("----------------->");
			resp.Write('\r');
			
			var pms = methodInfo.GetParameters();
			int l = pms.Length;
			var arr = new object[l];
			for (int i = 0; i < l; i++) {
				ParameterInfo parameterInfo = pms[i];
				arr[i] = new  {
					name = parameterInfo.Name,
					type = parameterInfo.ParameterType.ToString(),
					defaultValue = parameterInfo.RawDefaultValue,
					value = methodParam[i],
				};
			}
			resp.WriteJson(arr);*/
		}

		public bool IsReusable {
			get { return true; }
		}


		private static object [] ReturnMethodParams(HttpRequest request, ParameterInfo[] methodParm)
		{
			int l = methodParm.Length;
			object[] parmObject = new object [l];

			
			for (int i = 0; i < l; i++) {
				
				ParameterInfo parameterInfo = methodParm[i];
				var parType = parameterInfo.ParameterType.ToString();
				var parName = parameterInfo.Name;
				
				
				var param = request[parName];
				if (param != null) {
					switch (parType) {

						case "System.Int16":
							parmObject[i] = Lyu.Util.TryToShort(request[parName]);
							break;
						case "System.Decimal":
							parmObject[i] = Lyu.Util.TryToDecimal(request[parName]);
							break;
						case "System.Int32":
							parmObject[i] = Lyu.Util.TryToInt(request[parName]);
							break;
						case "System.Int64":
							parmObject[i] = Lyu.Util.TryToLong(request[parName]);
							break;
						default:
							parmObject[i] = request[parName];
							break;
					}
				}
				else {
					parmObject[i] = parameterInfo.RawDefaultValue;
				}
			}

			return parmObject;
		}
	}
}