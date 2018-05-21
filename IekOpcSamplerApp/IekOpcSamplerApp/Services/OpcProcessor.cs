using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IekOpcSamplerApp.Services
{
    public delegate void UpdateCollectionDelegate(List<KeyValuePair<int, double>> collection);
    public delegate void LapCompletedDelegate(bool fromHome);
    public delegate void TagValidatedDelegate(Models.Tag tag);

    class OpcProcessor
    {
        public event UpdateCollectionDelegate HomeCompleted;
        public event LapCompletedDelegate LapCompleted;
        public event TagValidatedDelegate TagValidated;

        private Queue<Models.OpcMessage> _MessageQ = new Queue<Models.OpcMessage>();
        private List<Models.Tag> _FestoTags = new List<Models.Tag>();
        private List<KeyValuePair<int, double>> _Pos = new List<KeyValuePair<int, double>>();

        private double _gaugeLastVal = 0.0;
        private bool _initialized = false;

        public int Steps { get; set; }

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
                        return null;
                    }
                    else if(!_initialized)
                    {
                        _FestoTags.Where(x => x.Name.StartsWith("PLC1.Application.GVL.HMI_bShowAuto_")).ToList().ForEach(x =>
                        {
                            if (bool.Parse(x.Value))
                            {
                                _Pos.Clear();
                                int.TryParse(x.Name.Replace("PLC1.Application.GVL.HMI_bShowAuto_", ""), out var auto);
                                Steps = auto == 1 ? 8 : auto == 2 ? 16 : 32;
                                var pos = _FestoTags.Where(y => y.Name.Contains("PLC1.Application.GVL_1")).ToList();
                                pos.Sort();
                                pos.Take(Steps).ToList().ForEach(y =>
                                {
                                    _Pos.Add(new KeyValuePair<int, double>(int.Parse(y.Value), _gaugeLastVal));
                                });
                                HomeCompleted?.Invoke(_Pos);
                            }
                        });
                        _initialized = true;
                    }
                    break;
                case "GaugeToolsXL OPC Server":
                    double.TryParse(tag.Value, out _gaugeLastVal);
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
                    var miniPos = _FestoTags.Where(x => x.Name.StartsWith("PLC1.Application.GVL_1.HMI_iPos") && x.Value != "0").Min(x => int.Parse(x.Value)).ToString();
                    var maxiPos = _FestoTags.Where(x => x.Name.StartsWith("PLC1.Application.GVL_1.HMI_iPos") && x.Value != "0").Max(x => int.Parse(x.Value)).ToString();
                    if (ipos.Value == maxiPos)
                    {
                        LapCompleted?.Invoke(true);
                    }
                    if (ipos.Value == miniPos)
                    {
                        LapCompleted?.Invoke(false);
                    }
                    TagValidated.Invoke(new Models.Tag
                    {
                        Handle = int.Parse(tag.Value),
                        Name = ipos.Name,
                        Value = _gaugeLastVal.ToString()
                    });
                    break;
                default:
                    break;
            }

            return null;

        }



    }
}
