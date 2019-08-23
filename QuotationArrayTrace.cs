using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Windows.Forms;
using AmiBroker.Data;

namespace AmiBroker.DataSources.IB
{
    static class QuotationArrayTrace
    {
        public static void WriteToFile(string ticker, ref QuotationArray quotes)
        {
            string fileName = ticker + "_" + DateTime.Now.ToLongTimeString();

            // create a filename from ticker

            char[] invalidChars = Path.GetInvalidFileNameChars();

            for (int i = 0; i < invalidChars.Length; i++)
                fileName = fileName.Replace(invalidChars[i], '_');

            fileName = fileName + ".csv";

            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            StreamWriter sw = new StreamWriter(filePath, false, Encoding.ASCII);

            for (int i = 0; i < quotes.Count; i++)
            {
                Quotation q = quotes[i];
                string line = ((DateTime)q.DateTime).ToShortDateString() + " " +
                              ((DateTime)q.DateTime).ToLongTimeString() + " " +
                              (q.DateTime.IsEod ? "EOD" : "INTRA") + "," +
                                    q.Open + "," +
                                    q.High + "," +
                                    q.Low + "," +
                                    q.Price + "," +
                                    q.Volume;
                sw.WriteLine(line);
            }

            sw.Close();

            return;
        }


        [Flags]
        private enum KeyStates
        {
            None = 0,
            Down = 1,
            Toggled = 2
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        private static KeyStates GetKeyState(Keys key)
        {
            KeyStates state = KeyStates.None;

            short retVal = GetKeyState((int)key);

            //If the high-order bit is 1, the key is down 
            //otherwise, it is up. 
            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            //If the low-order bit is 1, the key is toggled. 
            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }

        public static bool IsKeyDown(Keys key)
        {
            return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
        }

        public static bool IsKeyToggled(Keys key)
        {
            return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
        }
    }
}

