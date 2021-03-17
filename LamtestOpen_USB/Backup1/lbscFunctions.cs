using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Lamtest
{
    public class lbscFunctions : byteCommands

    {
               //byteCommands byteCom;
       public lambdaSerial com;//create comport object
       //private Mainform mf;
       public lbscFunctions(lambdaSerial com)//com object passed to VF5fuctions object
       {
           //byteCom = new byteCommands();
           this.com = com;//set reference of comport child to serial parent
       }
        public void cycleShutter(string shutter, int hertz)
        {
            com.stop.Checked = false;
            do
            {
                switch (shutter)
                {
                    case "A":
                        com.lbX.openA();
                        //txtCom.AppendText("W-Open A" + Environment.NewLine);
                        com.readPort();
                        Thread.Sleep(hertz);
                        com.lbX.closeShutterA();
                        //txtCom.AppendText("W-Open A" + Environment.NewLine);
                        com.readPort();
                        break;
                    case "B":
                        com.lbX.openB();
                        Thread.Sleep(hertz);
                        com.lbX.closeShutterB();
                        break;
                    case "C":
                        com.lbX.openC();
                        Thread.Sleep(hertz);
                        com.lbX.closeShutterC();
                        break;
                    case "A_&_B":
                        com.writeByte(189);
                        com.lbX.openAdefault();
                        com.lbX.openBdefault();
                        com.writeByte(190);
                        Thread.Sleep(hertz);
                        com.writeByte(189);
                        com.lbX.closeShutterA();
                        com.lbX.closeShutterB();
                        com.writeByte(190);
                        break;
                }
                Thread.Sleep(hertz);
                System.Windows.Forms.Application.DoEvents(); 
            } while (com.isOpen() && com.stop.Checked == false);
            com.stop.Checked = false;
            return;
        }
        public void setFreeRun(string mode, int runCycles)
        {
            //com.stopFreeRun();
            //string myMode = txtRunMode.Text;
            //int runCycles = (int)decCycles.Value;
            //Set free run cycles
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSetCycles);
            byte[] intBytes = BitConverter.GetBytes(runCycles);
            com.writeByte(intBytes[1]);
            com.writeByte(intBytes[0]);

            switch (mode)
            {
                case "Command":
                    com.writeByte(byteLBSC_Prefix);
                    com.writeByte(byteFreeRunCommand);
                    break;
                case "Power On":
                    com.writeByte(byteLBSC_Prefix);
                    com.writeByte(byteSetFreeRunPowerOn);
                    break;
                case "Trigger":
                    com.writeByte(byteLBSC_Prefix);
                    com.writeByte(byteSetFreeRunTTLIn);
                    break;
                default:
                    com.writeByte(byteLBSC_Prefix);
                    com.writeByte(byteFreeRunCommand);
                    break;
            }
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(193);//Save to controller
        }
        public void setTTLDisabled()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteDisableTTL);
            return;
        }
        public void setTTLHigh()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSetTTLHigh);
            return;
        }
        public void setTTLLow()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSetTTLLow);
            return;
        }
        public void setTTLToggleRisingEdge()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSetTTLToggleRising);
            return;
        }
        public void setTTLToggleFallingEdge()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSetTTLToggleFalling);
            return;
        }
        public void setSyncDisabled()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteDisableSync);
            return;
        }
        public void setSyncHighOpen()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSyncHighOpen);
            return;
        }
        public void setSyncLowOpen()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSyncLowOpen);
            return;
        }
        public void setNewDefault()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSetNewDefault);
            return;
        }
        public void setSingleShot()
        // The code for setting the exposure and dealy is almost identical!
        // The sole diference is in the second nibble of byte 4.
        {
            setTTLToggleFallingEdge();
            setFreeRunTTL(1);
        }
        public void setFreeRunTTL(int cycles)
        {
            byte byte1, byte2;
            byte2 = (byte)(cycles >> 8); // second byte
            byte1 = (byte)(cycles);
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSetCycles);
            com.writeByte(0);//com.writeByte(byte3);
            com.writeByte(1);//com.writeByte(byte1);
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteSetFreeRunTTLIn);//242 ttl
        }
        public void setExposureTimer(uint min, uint sec, uint ms, uint us)
        // The code for setting the exposure and dealy is almost identical!
        // The sole diference is in the second nibble of byte 4.
        {
            int msOnes, msTens, msHundreds;
            string msString, onesMs, tensMs, hundredsMs;
            byte byte1, byte2, byte3, byte4;
            if (us > 9) { us = 0; }
            if (ms > 999) { ms = 0; }
            if (sec > 59) { sec = 0; }
            if (min > 59) { min = 0; }
            //The time on the LB-SC has a unique encoding scheam
            //byte one
            msString = ms.ToString();
            if (ms >= 100) { msString = msString.Insert(0, "0"); }
            if (ms < 100 && ms > 9) { msString = msString.Insert(0, "00"); }
            if (ms < 10) { msString = msString.Insert(0, "000"); }
            onesMs = msString.Substring(3, 1);
            msOnes = int.Parse(onesMs);
            byte1 = (byte)((msOnes << 4) + us);
            //byte two
            try { tensMs = msString.Substring(2, 1); }
            catch (Exception) // catches without assigning to a variable
            {
                tensMs = "0";
            }
            msTens = int.Parse(tensMs);
            try { hundredsMs = msString.Substring(1, 1); }
            catch (Exception) // catches without assigning to a variable
            {
                hundredsMs = "0";
            }
            msHundreds = int.Parse(hundredsMs);
            //msHundreds = 20;
            byte2 = (byte)((msHundreds << 4) + msTens);
            //txtBoxDialog.AppendText("String: " + byte2 + " 100's " + msHundreds + " 10's " + msTens + " 1's" + msOnes);
            byte3 = (byte)(sec);
            byte4 = (byte)(min);
            // byte4 = (byte)(32 + byte4);//set exposure
            //Set timeer
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(32);//set exposure zero hours, 0-5
            com.writeByte(byte4);
            com.writeByte(byte3);
            com.writeByte(byte2);
            com.writeByte(byte1);
            return;
        }
        public void setDelayTimer(uint min, uint sec, uint ms, uint us)
        // The code for setting the exposure and dealy is almost identical!
        // The sole diference is in the second nibble of byte 4.
        {
            int msOnes, msTens, msHundreds;
            string msString, onesMs, tensMs, hundredsMs;
            byte byte1, byte2, byte3, byte4;
            if (us > 9) { us = 0; }
            if (ms > 999) { ms = 0; }
            if (sec > 59) { sec = 0; }
            if (min > 59) { min = 0; }
            //The time on the LB-SC has a unique encoding scheam
            //byte one
            msString = ms.ToString();
            if (ms >= 100) { msString = msString.Insert(0, "0"); }
            if (ms < 100 && ms > 9) { msString = msString.Insert(0, "00"); }
            if (ms < 10) { msString = msString.Insert(0, "000"); }
            onesMs = msString.Substring(3, 1);
            msOnes = int.Parse(onesMs);
            byte1 = (byte)((msOnes << 4) + us);
            //byte two
            try { tensMs = msString.Substring(2, 1); }
            catch (Exception) // catches without assigning to a variable
            {
                tensMs = "0";
            }
            msTens = int.Parse(tensMs);
            try { hundredsMs = msString.Substring(1, 1); }
            catch (Exception) // catches without assigning to a variable
            {
                hundredsMs = "0";
            }
            msHundreds = int.Parse(hundredsMs);
            byte2 = (byte)((msHundreds << 4) + msTens);
            byte3 = (byte)(sec);
            byte4 = (byte)(min);
            //byte4 = (byte)(16 + byte4);//set delay
            //Set timeer
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(16);//set exposure + zero hours, 0-5
            com.writeByte(byte4);
            com.writeByte(byte3);
            com.writeByte(byte2);
            com.writeByte(byte1);
            return;
        }
        public void restoreDefaults()
        {
            stopFreeRun();
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(byteResetDefault);
            return;
        }
        public void restoreLast()
        {
            stopFreeRun();
            com.writeByte(byteRestorLast);
            return;
        }
        public void stopFreeRun()
        {
            com.writeByte(byteStopFreeRun);
            return;
        }
        public void saveToLBSC()
        {
            com.writeByte(byteLBSC_Prefix);
            com.writeByte(193);//Save to controller
        }
        public byte getShutterHome()
        {
            byte home = 0;
            //byte mask = 240;//F0
            com.writeByte(byteGetHomePos);
            while (home != byteGetHomePos || home == 0)//Clear initial zeros. and read echo
            {
                home = com.readByte();
                Thread.Sleep(100);
            }
            Thread.Sleep(1500);
            //MessageBox.Show("echo" + home.ToString());//echo
            home = com.readByte();//read home
            Thread.Sleep(5);
            //home = lambdaCom.readByte();//channel?
            home = com.Util.getLowerNible(home);
            com.clearBuffer();//Good to clear the buffer
            return home;
        }
        public byte getShutterHome(int channel)
        {
            byte home = 0;
            //byte mask = 240;//F0
            com.writeByte(byteGetHomePos);
            while (home != byteGetHomePos  || home==0)//Clear initial zeros. and read echo
            {
                home = com.readByte();
                Thread.Sleep(100);
            }
            Thread.Sleep(1500);
            //MessageBox.Show("echo" + home.ToString() +"\n");//echo
            home = com.readByte();//read home
            //Thread.Sleep(1000);
            
            //home = lambdaCom.readByte();//channel?
            switch (channel)
            {
                case 1:
                    home = com.Util.getLowerNible(home);
                    break;
                case 2:
                    home = com.Util.getUpperNible(home);
                    break;
                case 3:
                    home = com.readByte();
                    home = com.Util.getLowerNible(home);
                    break;
                default:
                    home =  com.Util.getLowerNible(home);
                    break;
            }
            //MessageBox.Show("Home " + home.ToString());//echo
            com.clearBuffer();//Good to clear the buffer
            return home;
        }
    }
}
