using opclibrary.Mappings;
using OPCAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opclibrary.Services
{
    public static class Module1
    {
        public static string OPCServerName = "Gauge ToolsXL OPC Server";
        public static List<OpcTag> TagList = new List<OpcTag>();
        public static int TagCount = 1;
        public static Array ItemServerHandles;
        public static OPCGroup _OPCGroup;
        public static Array ItemServerErrors;
        public static Array TagNameArray = new string[2] { "", "Output"};
        public static Array HandleArray = new int[] { 0, 1};
        public static int[] OPCItemIsArray = new int[2];

        static Module1()
        {
            //List<dynamic> dt = DbManager.GetDataTable("SELECT handle,name,formid,corr FROM tags ORDER BY handle");
            //foreach (var i in dt)
            //{
            TagList.Add(new OpcTag());
            TagList.Add(new OpcTag
            {
                Handle = 1,
                Name = "Output",
                Corr = 0
            });
            //}
            //TagNameArray = TagList.Select(x => x.Name).ToArray();
            //HandleArray = TagList.Select(x => x.Handle).ToArray();
        }

    }
}
