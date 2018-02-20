﻿using opclibrary.Mappings;
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
        public static string OPCServerName = "KEPware.KEPServerEx.V5";
        public static List<OpcTag> TagList = new List<OpcTag>();
        public static int TagCount = 2;
        public static Array ItemServerHandles;
        public static OPCGroup _OPCGroup;
        public static Array ItemServerErrors;
        public static Array TagNameArray = new string[3] { "", "Channel1.Device1.B_BERTHA", "Channel1.Device1.CTU_ACC" };
        public static Array HandleArray = new int[] { 0, 1, 2 };
        public static int[] OPCItemIsArray = new int[3];

        static Module1()
        {
            //List<dynamic> dt = DbManager.GetDataTable("SELECT handle,name,formid,corr FROM tags ORDER BY handle");
            //foreach (var i in dt)
            //{
            //    TagList.Add(new OpcTag
            //    {
            //        Handle = int.Parse(i.ItemArray[0].ToString()),
            //        Name = i.ItemArray[1].ToString(),
            //        Corr = int.Parse(i.ItemArray[3].ToString())
            //    });
            //}
            //TagNameArray = TagList.Select(x => x.Name).ToArray();
            //HandleArray = TagList.Select(x => x.Handle).ToArray();
        }

    }
}
