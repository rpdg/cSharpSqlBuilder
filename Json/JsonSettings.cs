using System;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Lyu.Json
{
	
	/// <summary>
	/// Description of AppSettings.
	/// </summary>
	public class JsonSettings<T> where T : new()
    {
		//public string filePath {set; get;}
     
        virtual public void Save(string fileName)
        {
        	JsonSettings<T>.Save(this , fileName) ;
            //File.WriteAllText(MapPath(fileName), JsonConvert.SerializeObject(this , Formatting.Indented));
        }

 
		#region Static Functions
		
        public static string MapPath(string fileName)
        {
        	return HttpContext.Current.Server.MapPath(fileName);
        }
        
        public static void Save<T>(T pSettings, string fileName)
        {
            File.WriteAllText(MapPath(fileName), JsonConvert.SerializeObject(pSettings , Formatting.Indented));
        }

        public static T Load(string fileName)
        {
        	T t = new T();
        	
        	string mpPath = MapPath(fileName) ;
        	
        	if(File.Exists(mpPath))
        		t = JsonConvert.DeserializeObject<T>(File.ReadAllText(mpPath)) ;

            return t;
        }
        
        #endregion
    }
}
