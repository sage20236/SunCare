using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SunCare.Repository
{
    public class RegularFunction
    {
        public static String getCurrentEmployeeName()
        {
            String username = HttpContext.Current.User.Identity.Name;
            int index = 0;
            while (username[index] != '|')
            {
                index++;
            }
            return username.Substring(index + 1);
            
        }
        public static String getCurrentEmployeeID()
        {
            String username = HttpContext.Current.User.Identity.Name;
            int index = 0;
            while (username[index] != '|')
            {
                index++;
            }
            return username.Substring(0,index);
        }
        public static String encrypt(String msg)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(msg);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static String decrypt(String msg)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(msg);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string GetCurrentDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        public static string GetCurrentDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ".000";
        }
        public static string convertClassToJSON(Object myClass)
        {
            var list = JsonConvert.SerializeObject(myClass,
            Formatting.None,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            });

            return list;
        }
    }
}