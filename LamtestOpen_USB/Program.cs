using System;
using System.IO.Ports;
using System.Threading;

namespace Lamtest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            byte bytePowerOn = 0xCE;//206;
            byte bytePowerOff = 0xCF;//207;
            byte byteGoOnline = 0xEE;//238;
            byte byteOpenA = 0xAA;//170;
            byte byteCloseA = 0xAC;//172;


            //SerialPort lambdaRS232;
            //lambdaRS232 = new SerialPort("COM1", 9600, Parity.None, 8, StopBits.One);

            //lambdaRS232.Open();
            //lambdaRS232.Write(new byte[] { byteGoOnline }, 0, 1);

            //// Motor Power On, then Close
            //lambdaRS232.Write(new byte[] { bytePowerOn }, 0, 1);
            ////lambdaRS232.Write(new byte[] { byteCloseA }, 0, 1);

            //// Open, then Motor Power off
            //lambdaRS232.Write(new byte[] { byteOpenA }, 0, 1);
            //lambdaRS232.Write(new byte[] { bytePowerOff }, 0, 1);

            //lambdaRS232.Close();

            /////////////////////
            uint baudRate = 128000;

            FTDI_USB lambdaUSB;
            try { lambdaUSB = new FTDI_USB(); }
            catch { lambdaUSB = null; }

            lambdaUSB.OpenBySerialNum(lambdaUSB.GetSerialNum());//Need to use a list later
            lambdaUSB.SetBaudrate(baudRate);
            lambdaUSB.WriteByte(byteGoOnline);

            // Motor Power On, then Close
            //lambdaUSB.WriteByte(bytePowerOn);
            //lambdaUSB.WriteByte(byteCloseA);

            // Open, then Motor Power off
            lambdaUSB.WriteByte(byteOpenA);
            lambdaUSB.WriteByte(bytePowerOff);

            try { lambdaUSB.ClearBuffer(); }
            catch { lambdaUSB = null; }

        }
    }
}
