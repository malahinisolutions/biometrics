using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace apidemo.Services
{
    public class LogUtility
    {
        private string _logPath;
        public LogUtility(string logPath)
        {
            string name = string.Format("{0}_{1}_{2}.txt",DateTime.UtcNow.Day, DateTime.UtcNow.Month, DateTime.UtcNow.Year);
            _logPath = logPath+"/"+name;
        }
        public void Write(string target,string response,string method,string url,string status)
        {
            using (FileStream fs = new FileStream(_logPath, FileMode.Append, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.UtcNow.ToString()+"(UTC) | "+target+"\n");
                sb.Append("Method : "+method+"\n");
                sb.Append("URL : "+url+"\n");
                sb.Append("Status : "+status+"\n");
                sb.Append("Response : "+response);
                sb.Append("\n");
                sw.WriteLine(sb.ToString());
                sw.Flush();
            }
        }
    }
}