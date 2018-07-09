using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IekOpcSamplerApp.Services
{
    public delegate void UpdateCollectionDelegate(List<KeyValuePair<int, double>> collection);
    public delegate void LapCompletedDelegate(bool fromHome);
    public delegate void TagUpdatedDelegate(Models.Tag tag);
    public delegate void CountChangedDelegate(double value);

    class OpcProcessor
    {
        public event UpdateCollectionDelegate StepsUpdated;
        public event LapCompletedDelegate LapCompleted;
        public event TagUpdatedDelegate TagValidated;
        public event TagUpdatedDelegate DelayUpdated;
        public event CountChangedDelegate CountChanged;

        private Queue<Models.OpcMessage> _MessageQ = new Queue<Models.OpcMessage>();
        private List<Models.Tag> _FestoTags = new List<Models.Tag>();

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
                        if (!string.IsNullOrEmpty(tag.Name))
                        {
                            _FestoTags.Add(tag);
                        }
                        return null;
                    }
                    else if (!_initialized)
                    {
                        _FestoTags.Where(x => x.Name.StartsWith("PLC1.Application.GVL.HMI_bShowAuto_")).ToList().ForEach(x =>
                        {
                            if (bool.Parse(x.Value))
                            {
                                int.TryParse(x.Name.Replace("PLC1.Application.GVL.HMI_bShowAuto_", ""), out var auto);
                                UpdateSteps(auto == 1 ? 8 : auto == 2 ? 16 : 32);
                            }
                        });

                        _initialized = true;
                    }
                    var t = _FestoTags.FirstOrDefault(x => x.Handle == tag.Handle);
                    if (t != null)
                    {
                        t.Value = tag.Value;
                    }
                    break;
                case "GaugeToolsXL OPC Server":
                    double.TryParse(tag.Value, out _gaugeLastVal);
                    CountChanged?.Invoke(_gaugeLastVal);
                    return null;
                default:
                    break;
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
                    TagValidated?.Invoke(new Models.Tag
                    {
                        Handle = int.Parse(tag.Value),
                        Name = ipos.Name,
                        Value = _gaugeLastVal.ToString()
                    });
                    break;
                case "PLC1.Application.GVL.HMI_bShowAuto_1":
                case "PLC1.Application.GVL.HMI_bShowAuto_2":
                case "PLC1.Application.GVL.HMI_bShowAuto_3":
                    if (bool.Parse(tag.Value))
                    {
                        int.TryParse(tag.Name.Replace("PLC1.Application.GVL.HMI_bShowAuto_", ""), out var auto);
                        UpdateSteps(auto == 1 ? 8 : auto == 2 ? 16 : 32);
                    }
                    break;
                case "PLC1.Application.GVL_1.HMI_rDelay":
                    DelayUpdated?.Invoke(tag);
                    break;
                default:
                    break;
            }

            if (!_FestoTags.Any(x => x.Name == "PLC1.Application.PLC_PRG.bHOME_OK" && bool.Parse(x.Value)))
            {
                return null;
            }

            return null;

        }

        private void UpdateSteps(int steps)
        {
            Steps = steps;
            var lSteps = new List<KeyValuePair<int, double>>();
            var pos = _FestoTags.Where(y => y.Name.Contains("PLC1.Application.GVL_1")).ToList();
            pos.Sort();
            pos.Take(steps).ToList().ForEach(y =>
            {
                lSteps.Add(new KeyValuePair<int, double>(int.Parse(y.Value), _gaugeLastVal));
            });
            StepsUpdated?.Invoke(lSteps);
        }

    }
}
