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
		$star$ is *
	 */
	/// </summary>
	public class ApiHandler : IHttpHandler, IRequiresSessionState
	{
		public void ProcessRequest (HttpContext context)
		{
			HttpRequest req = context.Request;
			HttpResponse resp = context.Response;
			string [] p = req.Path.Split ('/');
			string packageName = p [2];
			string className = p [3];
			string methodName = p [4];

			Type type = Assembly.Load(packageName).GetType (packageName + "." + className);
			MethodInfo methodInfo = type.GetMethod (methodName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);

			object [] methodParam = ReturnMethodParams (req, methodInfo.GetParameters ());
			var result = methodInfo.Invoke (methodInfo, methodParam);

			resp.WriteJson (result);
		}

		public bool IsReusable {
			get { return true; }
		}


		private static object [] ReturnMethodParams (HttpRequest request, ParameterInfo [] methodParm)
		{
			int l = methodParm.Length;
			object [] parmObject = new object [l];

			for (int i = 0; i < l; i++) {
				
				ParameterInfo parameterInfo = methodParm [i];
				var parType = parameterInfo.ParameterType.ToString ();
				var parName = parameterInfo.Name;
				switch (parType) {

				case "System.Int16":
					parmObject [i] = Lyu.Util.TryToShort (request [parName]);
					break;
				case "System.Decimal":
					parmObject [i] = Lyu.Util.TryToDecimal (request [parName]);
					break;
				case "System.Int32":
					parmObject [i] = Lyu.Util.TryToInt (request [parName]);
					break;
				case "System.Int64":
					parmObject [i] = Lyu.Util.TryToLong (request [parName]);
					break;
				default:
					parmObject [i] = request [parName];
					break;
				}
			}

			return parmObject;
		}
	}
}