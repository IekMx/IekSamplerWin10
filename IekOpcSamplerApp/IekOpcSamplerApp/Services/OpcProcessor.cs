using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IekOpcSamplerApp.Services
{
    public delegate void UpdateCollectionDelegate(List<KeyValuePair<int, double>> collection);
    class OpcProcessor
    {
        private Queue<Models.OpcMessage> _MessageQ = new Queue<Models.OpcMessage>();
        private List<Models.Tag> _FestoTags = new List<Models.Tag>();
        private double _GaugeLastVal = 0.0;
        public event UpdateCollectionDelegate HomeCompleted;
        private List<KeyValuePair<int, double>> _Pos = new List<KeyValuePair<int, double>>();


        void AddIncomingMessage(string msgString)
        {

        }

        public Models.Tag ProcessTag(Models.Tag tag)
        {
            switch (tag.Server)
            {
                case "CoDeSys.OPC.DA":
                    if (_FestoTags.Count < 37 && !_FestoTags.Any(x => x.Name == tag.Name))
                    {
                        _FestoTags.Add(tag);
                    }
                    break;
                case "GaugeToolsXL OPC Server":
                    double.TryParse(tag.Value, out _GaugeLastVal);
                    return null;
                default:
                    break;
            }

            if (!_FestoTags.Any(x => x.Name == "PLC1.Application.PLC_PRG.bHOME_OK" && bool.Parse(x.Value)))
            {
                return null;
            }

            switch (tag.Name)
            {
                case "PLC1.Application.GVL.Record1.lrTarget":
                    var ipos = _FestoTags.FirstOrDefault(x => x.Value == tag.Value);
                    return new Models.Tag
                    {
                        Handle = int.Parse(tag.Value),
                        Name = ipos.Name,
                        Value = _GaugeLastVal.ToString()
                    };
                case "PLC1.Application.GVL.HMI_bShowAuto_1":
                case "PLC1.Application.GVL.HMI_bShowAuto_2":
                case "PLC1.Application.GVL.HMI_bShowAuto_3":
                    if (bool.Parse(tag.Value))
                    {
                        _Pos.Clear();
                        int.TryParse(tag.Name.Replace("PLC1.Application.GVL.HMI_bShowAuto_", ""), out var auto);
                        var pos = _FestoTags.Where(x => x.Name.Contains("PLC1.Application.GVL_1")).ToList();
                        pos.Sort();
                        pos.Take(auto == 1 ? 8 : auto == 2 ? 16 : 32).ToList().ForEach(x =>
                        {
                            _Pos.Add(new KeyValuePair<int, double>(int.Parse(x.Value), _GaugeLastVal));
                        });
                        HomeCompleted?.Invoke(_Pos);
                    }
                    break;
                default:
                    break;
            }

            return null;

        }



    }
}
