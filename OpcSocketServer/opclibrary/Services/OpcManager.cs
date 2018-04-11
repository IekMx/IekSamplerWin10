using OPCAutomation;
using System;

namespace opclibrary.Services
{
    public delegate void OpcDataChanged(object sender, Mappings.OpcEventArgs e);
    public delegate void StatusMessageDelegate(Exception e, string message);

    public class OpcManager
    {
        public event OpcDataChanged DataChanged;
        public static OpcManager Instance = new OpcManager();
        OPCServer _OPCServer;
        public event StatusMessageDelegate StatusMessageChanged;

        public enum CanonicalDataTypes
        {
            CanonDtByte = 17,
            CanonDtChar = 16,
            CanonDtWord = 18,
            CanonDtShort = 2,
            CanonDtDWord = 19,
            CanonDtLong = 3,
            CanonDtFloat = 4,
            CanonDtDouble = 5,
            CanonDtBool = 11,
            CanonDtString = 8,
        }

        private OpcManager()
        {
            try
            {
                _OPCServer = new OPCAutomation.OPCServer();
                _OPCServer.Connect("GaugeToolsXL OPC Server", "");
                _OPCServer.OPCGroups.DefaultGroupIsActive = true;
                _OPCServer.OPCGroups.DefaultGroupDeadband = 0;
                Module1._OPCGroup = _OPCServer.OPCGroups.Add("Base group");
                Module1._OPCGroup.UpdateRate = 250;
                Module1._OPCGroup.IsSubscribed = true;

                Module1._OPCGroup.DataChange += OPCGroup_DataChanged;
                Module1._OPCGroup.OPCItems.DefaultIsActive = true;
                Module1._OPCGroup.OPCItems.AddItems(Module1.TagCount, Module1.TagNameArray, Module1.HandleArray, out Module1.ItemServerHandles, out Module1.ItemServerErrors);

                bool itemgood = false;

                for (int i = 1; i <= Module1.TagCount; i++)
                {
                    int ab = (Int32)Module1.ItemServerErrors.GetValue(i);
                    if (ab == 0)
                    {
                        itemgood = true;
                    }
                }
                if (!itemgood)
                {
                }
            }
            catch (Exception ex)
            {
                Instance.StatusMessageChanged?.Invoke(ex, "OpcManager desconectado");
                _OPCServer = null;
            }
        }

        public void Write(int index)
        {
            try
            {
                // Write only 1 item
                short ItemCount = 1;

                // Create some local scope variables to hold the value to be sent.
                // These arrays could just as easily contain all of the item we have added.
                int[] SyncItemServerHandles = new int[2];
                object[] SyncItemValues = new object[2];
                System.Array SyncItemServerErrors = null;
                OPCAutomation.OPCItem AnOpcItem = default(OPCAutomation.OPCItem);

                // Get the Servers handle for the desired item.  The server handles
                // were returned in add item subroutine.
                SyncItemServerHandles[1] = (int)Module1.ItemServerHandles.GetValue(index);
                AnOpcItem = Module1._OPCGroup.OPCItems.GetOPCItem((int)Module1.ItemServerHandles.GetValue(index));

                // Load the value to be written using Item's Canonical Data Type to
                // convert to correct type. 
                // See Kepware Application note on Canonical Data Types
                Array ItsAnArray = null;
                short CanonDT = 0;
                short vbArray = 8192;

                CanonDT = AnOpcItem.CanonicalDataType;


                // If it is an array, figure out the base type
                if (CanonDT > vbArray)
                {
                    CanonDT -= vbArray;
                }

                switch (CanonDT)
                {
                    case (short)CanonicalDataTypes.CanonDtByte:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(byte), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToByte((string)Module1.TagList[index].Value);
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtChar:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(SByte), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToSByte((string)Module1.TagList[index].Value);
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtWord:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(UInt16), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToUInt16(Module1.TagList[index].Value);
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtShort:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(Int16), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToInt16((string)Module1.TagList[index].Value);
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtDWord:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(UInt32), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToUInt32((string)Module1.TagList[index].Value);
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtLong:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(Int32), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToInt32((string)Module1.TagList[index].Value);
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtFloat:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(float), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToSingle((string)Module1.TagList[index].Value);
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtDouble:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(double), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Module1.TagList[index].Value;
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtBool:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(bool), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToBoolean(Module1.TagList[index].Value);
                        }
                        break;
                    // End case

                    case (short)CanonicalDataTypes.CanonDtString:
                        if ((int)Module1.OPCItemIsArray.GetValue(index) > 0)
                        {
                            ItsAnArray = Array.CreateInstance(typeof(string), (int)Module1.OPCItemIsArray.GetValue(index));
                            if (!LoadArray(ref ItsAnArray, CanonDT, (string)Module1.TagList[index].Name))
                            {
                            }
                            SyncItemValues[1] = (object)ItsAnArray;
                        }
                        else
                        {
                            SyncItemValues[1] = Convert.ToString((string)Module1.TagList[index].Value);
                        }
                        break;
                        // End case

                }

                // Invoke the SyncWrite operation.  Remember this call will wait until completion
                Module1._OPCGroup.SyncWrite(ItemCount, SyncItemServerHandles, SyncItemValues, out SyncItemServerErrors);

                if ((int)SyncItemServerErrors.GetValue(1) != 0)
                {
                    //MessageBox.Show("SyncItemServerError: " + SyncItemServerErrors.GetValue(1));
                }
            }
            catch (Exception ex)
            {
                // Error handling
                //MessageBox.Show("OPC server write item failed with exception: " + ex.Message + " INDEX = " + index.ToString() + " c=" + opcItemValueToWritte[index].Text + "/", "SimpleOPCInterface Exception", MessageBoxButtons.OK);
            }


        }


        public static bool LoadArray(ref System.Array AnArray, int CanonDT, string wrTxt)
        {
            int ii = 0;
            int loc = 0;
            int Wlen = 0;
            int start = 0;

            try
            {
                start = 1;
                Wlen = wrTxt.Length;
                for (ii = AnArray.GetLowerBound(0); ii <= AnArray.GetUpperBound(0); ii++)
                {
                    loc = wrTxt.IndexOf(",", 0);
                    if (ii < AnArray.GetUpperBound(0))
                    {
                        if (loc == 0)
                        {
                            //MessageBox.Show("Valor escrito: Numero incorrecto de digitos para el tamaño del arreglo?", "Error de argumento", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return false;
                        }
                    }
                    else
                    {
                        loc = Wlen + 1;
                    }

                    switch (CanonDT)
                    {
                        case (int)CanonicalDataTypes.CanonDtByte:
                            AnArray.SetValue(Convert.ToByte((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        case (int)CanonicalDataTypes.CanonDtChar:
                            AnArray.SetValue(Convert.ToSByte((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case


                        case (int)CanonicalDataTypes.CanonDtWord:
                            AnArray.SetValue(Convert.ToUInt16((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        case (int)CanonicalDataTypes.CanonDtShort:
                            AnArray.SetValue(Convert.ToInt16((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        case (int)CanonicalDataTypes.CanonDtDWord:
                            AnArray.SetValue(Convert.ToInt32((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        case (int)CanonicalDataTypes.CanonDtLong:
                            AnArray.SetValue(Convert.ToInt32((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        case (int)CanonicalDataTypes.CanonDtFloat:
                            AnArray.SetValue(Convert.ToSingle((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        case (int)CanonicalDataTypes.CanonDtDouble:
                            AnArray.SetValue(Convert.ToDouble((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        case (int)CanonicalDataTypes.CanonDtBool:
                            AnArray.SetValue(Convert.ToBoolean((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        case (int)CanonicalDataTypes.CanonDtString:
                            AnArray.SetValue(Convert.ToString((wrTxt.Substring(start, loc - start))), ii);
                            break;
                        // End case

                        default:
                            //MessageBox.Show("El tipo de valor que se intenta escribir es desconocido", "Error de argumento", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return false;
                    }

                    start = loc + 1;
                }

                return true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Al intentar escribir el vaor se genero la siguiente excepción: " + ex.Message, "Excepción de OPC", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }


        private void OPCGroup_DataChanged(int TransactionID, int NumItems, ref Array ClientHandles, ref Array ItemValues, ref Array Qualities, ref Array TimeStamps)
        {
            Mappings.OpcEventArgs e = new Mappings.OpcEventArgs();
            for (int i = 1; i <= ClientHandles.Length; i++)
            {
                if (ItemValues.GetValue(i) == null) continue;
                e.ItemHandle = (int)ClientHandles.GetValue(i);
                if (typeof(double) == ItemValues.GetValue(i).GetType())
                {
                    e.ItemValue = ItemValues.GetValue(i) != null ? (double)ItemValues.GetValue(i) : 0.0;
                }
                else if (typeof(bool) == ItemValues.GetValue(i).GetType())
                {
                    e.ItemValue = ItemValues.GetValue(i) != null ? (bool)ItemValues.GetValue(i) : false;
                    e.IsFault = (bool)e.ItemValue && ((e.ItemHandle <= 18 && e.ItemHandle >= 15) || (e.ItemHandle <= 98 && e.ItemHandle >= 93));
                }
                else if (typeof(byte) == ItemValues.GetValue(i).GetType())
                {
                    e.ItemValue = ItemValues.GetValue(i) != null ? (byte)ItemValues.GetValue(i) : new Byte();
                }
                else if (typeof(string) == ItemValues.GetValue(i).GetType())
                {
                    e.ItemValue = ItemValues.GetValue(i) != null ? (string)ItemValues.GetValue(i) : "";
                }
                else if (typeof(int) == ItemValues.GetValue(i).GetType())
                {
                    e.ItemValue = ItemValues.GetValue(i) != null ? (int)ItemValues.GetValue(i) : -1;
                }
                else if (typeof(Single) == ItemValues.GetValue(i).GetType())
                {
                    e.ItemValue = ItemValues.GetValue(i) != null ? (Single)ItemValues.GetValue(i) : -1;
                }

                DataChanged?.Invoke(this, e);
            }
        }
    }
}
