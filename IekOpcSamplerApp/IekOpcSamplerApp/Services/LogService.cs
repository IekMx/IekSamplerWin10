using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IekOpcSamplerApp.Services
{
    public class LogService
    {
        private static StringBuilder _logContent = new StringBuilder();

        public static string GetFullLog()
        {
            return _logContent.ToString();
        }

        public static void AddEntry(string where, Exception exception)
        {
            //_logContent.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm ") + where);
            //_logContent.AppendLine(exception.Message);
            //_logContent.AppendLine(exception.StackTrace);
        }

    }
}
