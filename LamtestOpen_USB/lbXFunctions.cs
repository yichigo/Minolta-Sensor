using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lamtest
{
    public class lbXFunctions : byteCommands
    {
       private lambdaSerial lambdaCom;//create comport object
       public lbXFunctions(lambdaSerial com)//com object passed to VF5fuctions object
       {
           //byteCom = new byteCommands();
           this.lambdaCom = com;//set reference of comport child to serial parent
       }
       public void moveMyWheel(string wheel, byte speedByte, byte filterByte)
       {
           byte moveByte = (byte)lambdaCom.getMoveByte(speedByte, filterByte);
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
            lambdaCom.writeByte(myByte);
        }
        public void moveWheelB(byte myByte)
        {
            myByte = (byte)(myByte + 128);
            lambdaCom.writeByte(myByte);
        }
        public void writeShutterA(byte myByte)
        {
            if (myByte <= byteCloseA && myByte >= byteOpenA)
            {
                lambdaCom.writeByte(myByte);
            }
            else
            {
                lambdaCom.writeByte(byteOpenACond);
            }
        }
        public void selectA(string controller)
        {//Default is fast mode!
            if (controller == "10-3") { lambdaCom.writeByte(1); } //For LB10-3 only OR LB10-B 2 shutter
        }
        public void selectA(string controller, string LB10B_SB)
        {//Default is fast mode!
            if (controller == "10-3" || (LB10B_SB == "IQ" && controller == "10-B")) { lambdaCom.writeByte(1); } //For LB10-3 only OR LB10-B 2 shutter
        }
        public void openAdefault(string controller, string LB10B_SB)
        {
            selectA(controller, LB10B_SB);
            lambdaCom.writeByte(byteOpenA);
        }
        public void openAdefault()
        {
            lambdaCom.writeByte(byteOpenA);
        }
        public void openA(string controller, string LB10B_SB)
        {//Default is fast mode! 
            lambdaCom.writeByte(byteSetFast);
            selectA(controller, LB10B_SB);
            lambdaCom.writeByte(byteOpenA);
        }
        public void openA()
        {//Default is fast mode! ;
            lambdaCom.writeByte(byteSetFast);
            lambdaCom.writeByte(byteOpenA);
        }
        public void openA(string controller, byte ND, string LB10B_SB)
        {//Only ND mode requires a byte setting
            lambdaCom.writeByte(byteSetND);
            selectA(controller, LB10B_SB); 
            lambdaCom.writeByte(ND);
            lambdaCom.writeByte(byteOpenA);
        }
        public void openA(byte ND)
        {//Only ND mode requires a byte setting
            lambdaCom.writeByte(byteSetND);
            lambdaCom.writeByte(ND);
            lambdaCom.writeByte(byteOpenA);
        }
        public void openACond(string controller, string LB10B_SB)
        {//Shutter only opens when the wheel is stopped
            selectA(controller, LB10B_SB);
            lambdaCom.writeByte(byteOpenACond);
        }
        public void openACond()
        {//Shutter only opens when the wheel is stopped
            lambdaCom.writeByte(byteOpenACond);
        }
        public void setAfast(string controller, string LB10B_SB)
        {
            selectA(controller, LB10B_SB);
            lambdaCom.writeByte(byteSetFast);
        }
        public void setAfast()
        {
            lambdaCom.writeByte(byteSetFast);
        }
        public void openASoft(string controller, string LB10B_SB)
        {//soft mode
            lambdaCom.writeByte(byteSetSoft);
            selectA(controller, LB10B_SB); 
            lambdaCom.writeByte(byteOpenA);
        }
        public void openASoft()
        {//soft mode
            lambdaCom.writeByte(byteSetSoft);
            lambdaCom.writeByte(byteOpenA);
        }
        public void closeShutterA()
        {
            lambdaCom.writeByte(byteCloseA);
        }
        public void writeShutterB(byte myByte)
        {
            if (myByte <= byteCloseB && myByte >= byteOpenB)
            {
                lambdaCom.writeByte(myByte);
            }
            else
            {
                lambdaCom.writeByte(byteOpenBCond);
            }
        }
        public void openANoMode()
        {//Shutter only opens when the wheel is stpped
            lambdaCom.writeByte(byteOpenA);
        }
        public void openBdefault()
        {//Use the current default!
            lambdaCom.writeByte(byteOpenB);
        }
        public void openB()
        {//Default is fast mode!
            lambdaCom.writeByte(byteSetFast);
            lambdaCom.writeByte(2);
            lambdaCom.writeByte(byteOpenB);
        }
        public void openB(byte ND)
        {//Only ND mode requires a byte setting
            lambdaCom.writeByte(byteSetND);
            lambdaCom.writeByte(2);
            lambdaCom.writeByte(ND);
            lambdaCom.writeByte(byteOpenB);
        }
        public void openBSoft()
        {//soft mode
            lambdaCom.writeByte(byteSetSoft);
            lambdaCom.writeByte(2);
            lambdaCom.writeByte(byteOpenB);
        }
        public void openBCond()
        {//Shutter only opens when the wheel is stopped
            lambdaCom.writeByte(byteOpenBCond);
        }
        public void openBNoMode()
        {//Shutter only opens when the wheel is stopped
            lambdaCom.writeByte(byteOpenB);
        }
        public void closeShutterB()
        {
            lambdaCom.writeByte(byteCloseB);
        }
        public void openCdefault()
        {//Use the current default!
            lambdaCom.writeByte(byteOpenC);
        }
        public void openC()
        {//Default is fast mode!//Use the current default!
            lambdaCom.writeByte(byteSetFast);
            lambdaCom.writeByte(3);
            lambdaCom.writeByte(byteOpenC);
        }
        public void openC(byte ND)
        {//Only ND mode requires a byte setting
            lambdaCom.writeByte(byteSetND);
            lambdaCom.writeByte(3);
            lambdaCom.writeByte(ND);
            lambdaCom.writeByte(byteOpenC);
        }
        public void openCSoft()
        {//soft mode
            lambdaCom.writeByte(byteSetSoft);
            lambdaCom.writeByte(3);
            lambdaCom.writeByte(byteOpenC);
        }
        public void openCCond()
        {//Shutter only opens when the wheel is stopped
            lambdaCom.writeByte(byteOpenCCond);
        }
        public void openCNoMode()
        {//Shutter only opens when the wheel is stopped
            lambdaCom.writeByte(byteOpenC);
        }
        public void closeShutterC()
        {
            lambdaCom.writeByte(byteCloseC);
        }
        public void moveWheelC(byte myByte)
        {
            lambdaCom.writeByte(byteSelectC);
            lambdaCom.writeByte(myByte);
        }
        public void moveBatch(byte A, byte B, byte C)
        {
            
            lambdaCom.writeByte(byteLB103Batch);//start batch
            if (A != 255) { moveWheelA(A); }
            //writeByte(byteCloseA);//for 3 move / shutter test
            if (B != 255) { moveWheelB(B); }
            if (C != 255) { moveWheelC(C); }
            lambdaCom.writeByte(190);//end batch
            //writeByte(byteOpenA);//Open shutter in batch
            
        }
        public void moveBatch2(byte A, byte AS, byte B, byte BS)
        {
            lambdaCom.writeByte(byteLB102Batch);//start batch
            moveWheelA(A);
            closeShutterA();
            moveWheelB(B);
            closeShutterB();
        }

       }
    }
