using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lamtest
{
    public class lbXFunctions : byteCommands
    {
      //byteCommands byteCom;
       private lambdaSerial com;//create comport object
       private Mainform mf;
       public lbXFunctions(lambdaSerial com)//com object passed to VF5fuctions object
       {
           //byteCom = new byteCommands();
           this.com = com;//set reference of comport child to serial parent
       }
       public void moveMyWheel(string wheel, byte speedByte, byte filterByte)
       {
           byte moveByte = (byte)com.getMoveByte(speedByte, filterByte);
           moveMyWheel(wheel, moveByte);
           return;
       }
       public void moveMyWheel(string wheel, byte moveByte)
       {
           switch (wheel)
           {
               case "Wheel A":
                   moveWheelA(moveByte);
                   break;
               case "Wheel B":
                   moveWheelB(moveByte);
                   break;
               case "Wheel C":
                   moveWheelC(moveByte);
                   break;
               case "LB10-3 Batch":
                   moveBatch(moveByte, moveByte, moveByte);
                   break;
               case "LB10-2 Batch":
                   moveBatch2(moveByte, byteCloseA, moveByte, byteCloseB);//Close shutter by default
                   break;
           }
       }
           public void moveWheelA(byte myByte)
        {
            com.writeByte(myByte);
        }
        public void moveWheelB(byte myByte)
        {
            myByte = (byte)(myByte + 128);
            com.writeByte(myByte);
        }
        public void writeShutterA(byte myByte)
        {
            if (myByte <= byteCloseA && myByte >= byteOpenA)
            {
                com.writeByte(myByte);
            }
            else
            {
                com.writeByte(byteOpenACond);
            }
        }
        public void selectA(string controller)
        {//Default is fast mode!
            if (controller == "10-3") { com.writeByte(1); } //For LB10-3 only OR LB10-B 2 shutter
        }
        public void selectA(string controller, string LB10B_SB)
        {//Default is fast mode!
            if (controller == "10-3" || (LB10B_SB == "IQ" && controller == "10-B")) { com.writeByte(1); } //For LB10-3 only OR LB10-B 2 shutter
        }
        public void openAdefault(string controller, string LB10B_SB)
        {
            selectA(controller, LB10B_SB);
            com.writeByte(byteOpenA);
        }
        public void openAdefault()
        {
            com.writeByte(byteOpenA);
        }
        public void openA(string controller, string LB10B_SB)
        {//Default is fast mode! 
            com.writeByte(byteSetFast);
            selectA(controller, LB10B_SB);
            com.writeByte(byteOpenA);
        }
        public void openA()
        {//Default is fast mode! ;
            com.writeByte(byteSetFast);
            com.writeByte(byteOpenA);
        }
        public void openA(string controller, byte ND, string LB10B_SB)
        {//Only ND mode requires a byte setting
            com.writeByte(byteSetND);
            selectA(controller, LB10B_SB); 
            com.writeByte(ND);
            com.writeByte(byteOpenA);
        }
        public void openA(byte ND)
        {//Only ND mode requires a byte setting
            com.writeByte(byteSetND);
            com.writeByte(ND);
            com.writeByte(byteOpenA);
        }
        public void openACond(string controller, string LB10B_SB)
        {//Shutter only opens when the wheel is stopped
            selectA(controller, LB10B_SB);
            com.writeByte(byteOpenACond);
        }
        public void openACond()
        {//Shutter only opens when the wheel is stopped
            com.writeByte(byteOpenACond);
        }
        public void setAfast(string controller, string LB10B_SB)
        {
            selectA(controller, LB10B_SB);
            com.writeByte(byteSetFast);
        }
        public void setAfast()
        {
            com.writeByte(byteSetFast);
        }
        public void openASoft(string controller, string LB10B_SB)
        {//soft mode
            com.writeByte(byteSetSoft);
            selectA(controller, LB10B_SB); 
            com.writeByte(byteOpenA);
        }
        public void openASoft()
        {//soft mode
            com.writeByte(byteSetSoft);
            com.writeByte(byteOpenA);
        }
        public void closeShutterA()
        {
            com.writeByte(byteCloseA);
        }
        public void writeShutterB(byte myByte)
        {
            if (myByte <= byteCloseB && myByte >= byteOpenB)
            {
                com.writeByte(myByte);
            }
            else
            {
                com.writeByte(byteOpenBCond);
            }
        }
        public void openANoMode()
        {//Shutter only opens when the wheel is stpped
            com.writeByte(byteOpenA);
        }
        public void openBdefault()
        {//Use the current default!
            com.writeByte(byteOpenB);
        }
        public void openB()
        {//Default is fast mode!
            com.writeByte(byteSetFast);
            com.writeByte(2);
            com.writeByte(byteOpenB);
        }
        public void openB(byte ND)
        {//Only ND mode requires a byte setting
            com.writeByte(byteSetND);
            com.writeByte(2);
            com.writeByte(ND);
            com.writeByte(byteOpenB);
        }
        public void openBSoft()
        {//soft mode
            com.writeByte(byteSetSoft);
            com.writeByte(2);
            com.writeByte(byteOpenB);
        }
        public void openBCond()
        {//Shutter only opens when the wheel is stopped
            com.writeByte(byteOpenBCond);
        }
        public void openBNoMode()
        {//Shutter only opens when the wheel is stopped
            com.writeByte(byteOpenB);
        }
        public void closeShutterB()
        {
            com.writeByte(byteCloseB);
        }
        public void openCdefault()
        {//Use the current default!
            com.writeByte(byteOpenC);
        }
        public void openC()
        {//Default is fast mode!//Use the current default!
            com.writeByte(byteSetFast);
            com.writeByte(3);
            com.writeByte(byteOpenC);
        }
        public void openC(byte ND)
        {//Only ND mode requires a byte setting
            com.writeByte(byteSetND);
            com.writeByte(3);
            com.writeByte(ND);
            com.writeByte(byteOpenC);
        }
        public void openCSoft()
        {//soft mode
            com.writeByte(byteSetSoft);
            com.writeByte(3);
            com.writeByte(byteOpenC);
        }
        public void openCCond()
        {//Shutter only opens when the wheel is stopped
            com.writeByte(byteOpenCCond);
        }
        public void openCNoMode()
        {//Shutter only opens when the wheel is stopped
            com.writeByte(byteOpenC);
        }
        public void closeShutterC()
        {
            com.writeByte(byteCloseC);
        }
        public void moveWheelC(byte myByte)
        {
            com.writeByte(byteSelectC);
            com.writeByte(myByte);
        }
        public void moveBatch(byte A, byte B, byte C)
        {
            
            com.writeByte(byteLB103Batch);//start batch
            if (A != 255) { moveWheelA(A); }
            //writeByte(byteCloseA);//for 3 move / shutter test
            if (B != 255) { moveWheelB(B); }
            if (C != 255) { moveWheelC(C); }
            com.writeByte(190);//end batch
            //writeByte(byteOpenA);//Open shutter in batch
            
        }
        public void moveBatch2(byte A, byte AS, byte B, byte BS)
        {
            com.writeByte(byteLB102Batch);//start batch
            moveWheelA(A);
            //closeA();
            moveWheelB(B);
            //closeB();
        }
       }
    }
