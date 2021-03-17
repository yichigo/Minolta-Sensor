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
        public lambdaSerial lambdaCom;//create comport object
       //private Mainform userInterface;
       public lbscFunctions(lambdaSerial com)//com object passed to VF5fuctions object
       {
           //byteCom = new byteCommands();
           this.lambdaCom = com;//set reference of comport child to serial parent
       }
        public void cycleShutter(string shutter, int hertz)
        {
            lambdaCom.stop.Checked = false;
            do
            {
                switch (shutter)
                {
                    case "A":
                        lambdaCom.writeByte(189);
                        lambdaCom.lbX.openA();
                        lambdaCom.writeByte(190);
                        //txtCom.AppendText("W-Open A" + Environment.NewLine);
                        //lambdaCom.readPort();
                        Thread.Sleep(hertz);
                        lambdaCom.writeByte(189);
                        lambdaCom.lbX.closeShutterA();
                        lambdaCom.writeByte(190);
                        //txtCom.AppendText("W-Open A" + Environment.NewLine);
                        //lambdaCom.readPort();
                        break;
                    case "B":
                        lambdaCom.writeByte(189);
                        lambdaCom.lbX.openB();
                        lambdaCom.writeByte(190);
                        Thread.Sleep(hertz);
                        lambdaCom.writeByte(189);
                        lambdaCom.lbX.closeShutterB();
                        lambdaCom.writeByte(190);
                        break;
                    case "C":
                        lambdaCom.writeByte(189);
                        lambdaCom.lbX.openC();
                        lambdaCom.writeByte(190);
                        Thread.Sleep(hertz);
                        lambdaCom.writeByte(189);
                        lambdaCom.lbX.closeShutterC();
                        lambdaCom.writeByte(190);
                        break;
                    case "A_&_B":
                        lambdaCom.writeByte(189);
                        lambdaCom.lbX.openAdefault();
                        lambdaCom.lbX.openBdefault();
                        lambdaCom.writeByte(190);
                        Thread.Sleep(hertz);
                        lambdaCom.writeByte(189);
                        lambdaCom.lbX.closeShutterA();
                        lambdaCom.lbX.closeShutterB();
                        lambdaCom.writeByte(190);
                        break;
                }
                Thread.Sleep(hertz);
                System.Windows.Forms.Application.DoEvents(); 
            } while (lambdaCom.isOpen() && lambdaCom.stop.Checked == false);
            lambdaCom.stop.Checked = false;
            return;
        }
        public void setFreeRun(string mode, int runCycles)
        {
            //lambdaCom.stopFreeRun();
            //string myMode = txtRunMode.Text;
            //int runCycles = (int)decCycles.Value;
            //Set free run cycles
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSetCycles);
            byte[] intBytes = BitConverter.GetBytes(runCycles);
            lambdaCom.writeByte(intBytes[1]);
            lambdaCom.writeByte(intBytes[0]);

            switch (mode)
            {
                case "Command":
                    lambdaCom.writeByte(byteLBSC_Prefix);
                    lambdaCom.writeByte(byteFreeRunCommand);
                    break;
                case "Power On":
                    lambdaCom.writeByte(byteLBSC_Prefix);
                    lambdaCom.writeByte(byteSetFreeRunPowerOn);
                    break;
                case "Trigger":
                    lambdaCom.writeByte(byteLBSC_Prefix);
                    lambdaCom.writeByte(byteSetFreeRunTTLIn);
                    break;
                default:
                    lambdaCom.writeByte(byteLBSC_Prefix);
                    lambdaCom.writeByte(byteFreeRunCommand);
                    break;
            }
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(193);//Save to controller
        }
        public void setTTLDisabled()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteDisableTTL);
            return;
        }
        public void setTTLHigh()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSetTTLHigh);
            return;
        }
        public void setTTLLow()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSetTTLLow);
            return;
        }
        public void setTTLToggleRisingEdge()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSetTTLToggleRising);
            return;
        }
        public void setTTLToggleFallingEdge()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSetTTLToggleFalling);
            return;
        }
        public void setSyncDisabled()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteDisableSync);
            return;
        }
        public void setSyncHighOpen()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSyncHighOpen);
            return;
        }
        public void setSyncLowOpen()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSyncLowOpen);
            return;
        }
        public void setNewDefault()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSetNewDefault);
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
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSetCycles);
            lambdaCom.writeByte(0);//lambdaCom.writeByte(byte3);
            lambdaCom.writeByte(1);//lambdaCom.writeByte(byte1);
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteSetFreeRunTTLIn);//242 ttl
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
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(32);//set exposure zero hours, 0-5
            lambdaCom.writeByte(byte4);
            lambdaCom.writeByte(byte3);
            lambdaCom.writeByte(byte2);
            lambdaCom.writeByte(byte1);
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
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(16);//set exposure + zero hours, 0-5
            lambdaCom.writeByte(byte4);
            lambdaCom.writeByte(byte3);
            lambdaCom.writeByte(byte2);
            lambdaCom.writeByte(byte1);
            return;
        }
        public void restoreDefaults()
        {
            stopFreeRun();
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(byteResetDefault);
            return;
        }
        public void restoreLast()
        {
            stopFreeRun();
            lambdaCom.writeByte(byteRestorLast);
            return;
        }
        public void stopFreeRun()
        {
            lambdaCom.writeByte(byteStopFreeRun);
            return;
        }
        public void saveToLBSC()
        {
            lambdaCom.writeByte(byteLBSC_Prefix);
            lambdaCom.writeByte(193);//Save to controller
        }
        public byte getShutterHome()
        {
            byte home = 0;
            //byte mask = 240;//F0
            lambdaCom.writeByte(byteGetHomePos);
            while (home != byteGetHomePos || home == 0)//Clear initial zeros. and read echo
            {
                home = lambdaCom.readByte();
                Thread.Sleep(100);
            }
            Thread.Sleep(1500);
            //MessageBox.Show("echo" + home.ToString());//echo
            home = lambdaCom.readByte();//read home
            Thread.Sleep(5);
            //home = lambdaCom.readByte();//channel?
            home = lambdaCom.Util.getLowerNible(home);
            lambdaCom.clearBuffer();//Good to clear the buffer
            return home;
        }
        public byte getShutterHome(int channel)
        {
            byte home = 0;
            //byte mask = 240;//F0
            lambdaCom.writeByte(byteGetHomePos);
            while (home != byteGetHomePos  || home==0)//Clear initial zeros. and read echo
            {
                home = lambdaCom.readByte();
                Thread.Sleep(100);
            }
            Thread.Sleep(1500);
            //MessageBox.Show("echo" + home.ToString() +"\n");//echo
            home = lambdaCom.readByte();//read home
            //Thread.Sleep(1000);
            
            //home = lambdaCom.readByte();//channel?
            switch (channel)
            {
                case 1:
                    home = lambdaCom.Util.getLowerNible(home);
                    break;
                case 2:
                    home = lambdaCom.Util.getUpperNible(home);
                    break;
                case 3:
                    home = lambdaCom.readByte();
                    home = lambdaCom.Util.getLowerNible(home);
                    break;
                default:
                    home =  lambdaCom.Util.getLowerNible(home);
                    break;
            }
            //MessageBox.Show("Home " + home.ToString());//echo
            lambdaCom.clearBuffer();//Good to clear the buffer
            return home;
        }
    }
}
