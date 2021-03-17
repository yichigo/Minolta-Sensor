using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Lamtest
{
    public class vf5Functions : byteCommands

    {
       //byteCommands byteCom;
       private lambdaSerial com;//create comport object
       //private Mainform mf;
       lambdaUtils util;
       public vf5Functions(lambdaSerial com)//com object passed to VF5fuctions object
       {
           util = new lambdaUtils();
           //byteCom = new byteCommands();
           this.com = com;//set reference of comport child to serial parent
       }
       public void moveNM(byte speed, int freqency)
        {
            setWaveLegnth(speed, freqency);
            return;
        }
       public void moveNM(int freqency)
        {
            byte speed = 1;
            setWaveLegnth(speed, freqency);
            return;
        }
       public void moveNM_LB103(int freqency, byte channel)
       {
           byte speed = 1;
           setWaveLength_LB103(speed, freqency, channel);
           return;
       }
       public void moveNM_LB103(byte speed, int freqency, byte channel)
       {
           setWaveLength_LB103(speed, freqency, channel);
           return;
       }
       public void setWaveLegnth(byte speed, int freqVal)
        {
            //http://stackoverflow.com/questions/1318933/c-int-to-byte
            byte[] intBytes = BitConverter.GetBytes(freqVal);
            byte mult = 64;
            //Array.Reverse(intBytes);
            byte[] result = intBytes;
            result[1] = (byte)(result[1] + (byte)(speed * mult));//Add the speed component to the high position byte
               com.writeByte(byteSetWlength);
               com.writeByte(result[0]);
               com.writeByte(result[1]);
               com.txtBoxDialog.AppendText("New position: " + freqVal + Environment.NewLine); //**can no longer access mainform
            return;
        }
       public void setWaveLength_LB103(byte speed, int freqVal, byte channel)
       {
           //http://stackoverflow.com/questions/1318933/c-int-to-byte
           byte[] intBytes = BitConverter.GetBytes(freqVal);
           if (channel != 1 && channel != 2) { channel = 1; }
           byte mult = 64;
           //Array.Reverse(intBytes);
           byte[] result = intBytes;
           result[1] = (byte)(result[1] + (byte)(speed * mult));//Add the speed component to the high position byte
           com.writeByte(byteSetWlength);
           com.writeByte(channel);//must specify channel for LB10-3 VF
           com.writeByte(result[0]);
           com.writeByte(result[1]);
           com.txtBoxDialog.AppendText("New Position: " + freqVal + Environment.NewLine); //**can no longer access mainform
           return;
       }
        //Utility
       //VF-5 methods_________________________________________________
       public int getRange(int wl)
       {
           int range = 60;
           switch (wl)
           {
               case 0://No filter installed
                   range = 0;
                   break;
               case 378://Same as 380
                   range = 42;
                   break;
               case 380:
                   range = 42;
                   break;
               //Extended overlap series
               case 400://Same as 402
                   range = 45;
                   break;
               case 402:
                   range = 45;
                   break;
               case 438://Same as 440
                   range = 52;
                   break;
               case 440:
                   range = 52;
                   break;
               //Extended overlap series
               case 449://Same as 451
                   range = 53;
                   break;
               case 451:
                   range = 53;
                   break;
               case 487://Same as 490
                   range = 60;
                   break;
               case 490:
                   range = 60;
                   break;
               case 501://Same as 503
                   range = 57;
                   break;
               case 503:
                   range = 57;
                   break;
               case 547://Same as 550
                   range = 64;
                   break;
               case 550:
                   range = 64;
                   break;
               case 561://Same as 564
                   range = 66;
                   break;
               case 564:
                   range = 66;
                   break;
               case 617://Same as 620
                   range = 73;
                   break;
               case 620:
                   range = 73;
                   break;
               case 628://Same as 632
                   range = 75;
                   break;
               case 632:
                   range = 75;
                   break;
               case 697://Same as 700
                   range = 84;
                   break;
               case 700:
                   range = 84;
                   break;
               case 704://Same as 708
                   range = 85;
                   break;
               case 708:
                   range = 85;
                   break;
               case 790://Same as 794
                   range = 98;
                   break;
               case 794:
                   range = 98;
                   break;
               case 796://Same as 800
                   range = 101;
                   break;
               case 800:
                   range = 101;
                   break;
               case 900:
                   range = 113;
                   break;
               default:
                   range = 60;
                   break;
           }
           return range;
       }
       public int[] getBaseFilters()
       {
           byte loop = 0;
           int[] baseVal = new int[5];
           int filter_0 = 0;
           int filter_1 = 0;
           int filter_2 = 0;
           int filter_3 = 0;
           int filter_4 = 0;
           com.writeByte(byteVFPosition);
           com.clearBuffer();
           com.writeByte(byteGetVFAll);
           while (loop != byteCR)
           {
               Thread.Sleep(1);
               System.Windows.Forms.Application.DoEvents();
               loop = com.readByte();
               switch (loop)
               {
                   case 240:
                       filter_0 = com.readByte();
                       Thread.Sleep(1);
                       Thread.Sleep(1);
                       baseVal[0] = filter_0 + (256 * com.readByte());
                       break;
                   case 255://Filter 0 : 0xFF FOR VF-1 channel 1
                       filter_0 = com.readByte();
                       Thread.Sleep(1);
                       //MessageBox.Show("255-1" + loop.ToString());
                       baseVal[0] = filter_0 + (256 * com.readByte());
                       //MessageBox.Show("255-2" + loop.ToString());
                       break;
                   case 254://Filter 1 : 0xFE FOR VF-1 channel 2
                       filter_1 = com.readByte();
                       Thread.Sleep(1);
                       //MessageBox.Show("254-1" + loop.ToString());
                       baseVal[1] = filter_1 + (256 * com.readByte());
                       //MessageBox.Show("254-2" + loop.ToString());
                       break;
                   case 242://Filter 1 : 0xFE FOR VF-1 channel 2
                       filter_1 = com.readByte();
                       Thread.Sleep(1);
                       //MessageBox.Show("254-1" + loop.ToString());
                       baseVal[1] = filter_1 + (256 * com.readByte());
                       //MessageBox.Show("254-2" + loop.ToString());
                       break;
                   case 244:
                       Thread.Sleep(1);
                       filter_2 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[2] = filter_2 + (256 * com.readByte());
                       break;
                   case 246:
                       Thread.Sleep(1);
                       filter_3 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[3] = filter_3 + (256 * com.readByte());
                       break;
                   case 248:
                       //Thread.Sleep(1);
                       Thread.Sleep(1);
                       filter_4 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[4] = filter_4 + (256 * com.readByte());
                       break;
               }
           }
           return baseVal;
       }
       //VF 5 Commands
      public string GetVFAll()
       {
           com.clearBuffer();
           com.writeByte(252);
           com.writeByte(byteGetVFAll);
           string filterPoss = com.readString();
           return filterPoss;
       }
       public UInt16 getWaveLegnth()//Need to implement
       {
           byte[] readBytes = new byte[2];
           UInt16 w;
           com.clearBuffer();
           com.writeByte(byteGetWlength);
           readBytes[0] = com.readByte();//Clear DB return
           readBytes[0] = com.readByte();
           Thread.Sleep(10);
           readBytes[1] = com.readByte();//txtBoxDialog.AppendText("base position " + b + Environment.NewLine);
           w = BitConverter.ToUInt16(readBytes, 0);//w = int (w1 >> 8) + int (w2);
           return w;
       }
       public void setStepAngle(byte moveByte, int stepIncrementLong, int channel)
       {
           //http://stackoverflow.com/questions/1318933/c-int-to-byte
           byte[] result = BitConverter.GetBytes(stepIncrementLong);
           byte myMove = (byte)(((channel - 1) * 128) + moveByte);
           if (com.isOpen())
           {
               //com.writeByte(byteVFposition);//Both ways work.  this is the batch way
               com.writeByte(myMove);//move at speed 2 ** does not work for channel C
               com.writeByte(byteSetUSteps);
               //com.writeByte((byte)channel);
               com.writeByte(result[0]);
               com.writeByte(result[1]);
           }
           return;
       }
       public int[] getCurrentWL()
       {
           int[] baseVal = new int[1];
           int filter_0 = 0;
           int filter_x = 0;
           int counter = 0;
           com.clearBuffer();
           com.writeByte(byteGetWlength);
           Thread.Sleep(5);
           filter_0 = com.readByte();//clear return
           while (filter_0 != 219)//Clear initial zeros. and read echo
           {
               Thread.Sleep(5);
               filter_0 = com.readByte();
               if (filter_0 == 0) { counter++; }
           }
           //filter_0 = com.readByte();//clear channel
           Thread.Sleep(1);
           System.Windows.Forms.Application.DoEvents();
           filter_0 = com.readByte();
           Thread.Sleep(1);
           System.Windows.Forms.Application.DoEvents();
           filter_x = getLowerNible(com.readByte());
           baseVal[0] = filter_0 + (filter_x * 256);
           com.clearBuffer();
           return baseVal;
       }
       public int[] getCurrentWL_LB103()
       {
           int[] baseVal = new int[3];
           int filter_0 = 0;
           int filter_1 = 0;
           int filter_2 = 0;
           int filter_x = 0;
           int counter = 0;
           com.clearBuffer();
           com.writeByte(byteGetWlength);
           Thread.Sleep(5);
           filter_0 = com.readByte();//clear return
           while (filter_0 != 219)//Clear initial zeros. and read echo
           {
               Thread.Sleep(5);
               filter_0 = com.readByte();
               if (filter_0 == 0) { counter++; }
           }
           //filter_0 = com.readByte();//clear channel
           Thread.Sleep(1);
           System.Windows.Forms.Application.DoEvents();
           filter_0 = com.readByte();//clear channel
           filter_0 = com.readByte();
           Thread.Sleep(1);
           System.Windows.Forms.Application.DoEvents();
           filter_x = getLowerNible(com.readByte());
           baseVal[0] = filter_0 + (filter_x * 256);
           Thread.Sleep(1);
           filter_x = com.readByte();//Clear channel
           Thread.Sleep(1);
           System.Windows.Forms.Application.DoEvents();
           filter_1 = com.readByte();
           Thread.Sleep(1);
           filter_x = getLowerNible(com.readByte());
           baseVal[1] = filter_1 + (filter_x * 256);
           Thread.Sleep(1);
           filter_x = com.readByte();//Clear channel
           Thread.Sleep(1);
           filter_2 = com.readByte();
           Thread.Sleep(1);
           filter_x = getLowerNible(com.readByte());
           baseVal[2] = filter_2 + (256 * filter_x);
           com.clearBuffer();
           return baseVal;
       }
       public byte getLowerNible(byte myByte)
       {
           byte lNible = (byte)((int)myByte << 2);
           lNible = (byte)((int)lNible >> 2);  //Make sure that the ,lower nible contains only zeroes.
           return lNible;
       }
       /*public int[] getBaseFilters_LB103_VF1_Chan()
       {

       }*/
       public int[] getBaseFilters_LB103_VF10_Chan(int port)
       {
           byte loop = 0;
           int[] baseVal = new int[10];
           int filter_0 = 0;
           int filter_1 = 0;
           int filter_2 = 0;
           int filter_3 = 0;
           int filter_4 = 0;
           int filter_5 = 0;
           int filter_6 = 0;
           int filter_7 = 0;
           int filter_8 = 0;
           int filter_9 = 0;
           int counter = 0;
           com.clearBuffer();
           com.writeByte(byteVFPosition);
           com.writeByte(byteGetVFAll);
           //loop = com.readByte();
           while (loop == 0 && counter < 1)//Clear initial zeros. 240 is the first valid filter
           {
               Thread.Sleep(5);
               loop = com.readByte();
               if (loop != 240) { counter++; }
           }

           counter = 0;
           com.clearBuffer();
           com.writeByte(byteVFPosition);
           com.writeByte(byteGetVFAll);
           //loop = com.readByte();
           while (loop == 0 && counter < 1)//Clear initial zeros. 240 is the first valid filter
           {
               Thread.Sleep(5);
               loop = com.readByte();
               if (loop != 240) { counter++; }
           }

           counter = 0;
           if (port == 2)
           {
               for (int i = 0; i < 33; i++)
               {
                   loop = com.readByte();
                   Thread.Sleep(10);
                   //MessageBox.Show("CH-2  " + loop); //Need to slow it dowwn!
               }
           }
           if (port == 3)
           {
               for (int i = 0; i < 60; i++)
               {
                   loop = com.readByte();
                   Thread.Sleep(10);
                   //MessageBox.Show("CH-2  " + loop); //Need to slow it dowwn!
               }
           }
           while (loop != byteCR)
           {
               Thread.Sleep(5);
               System.Windows.Forms.Application.DoEvents();
               if (counter < 1) { loop = com.readByte(); }//Pick out out filter number if it has not alredy been done
               switch (loop)//Skip all non valid cases
               {
                   case 240://Filter 0 : 0xF0
                       filter_0 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[0] = filter_0 + (256 * com.readByte());
                       break;
                   case 255://Filter 0 : 0xFF FOR VF-1 channel 1
                       filter_0 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[0] = filter_0 + (256 * com.readByte());
                       break;
                   case 241://Filter 1 : 0xF1
                       filter_1 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[1] = filter_1 + (256 * com.readByte());
                       break;
                   case 254://Filter 1 : 0xFE FOR VF-1 channel 2
                       filter_1 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[1] = filter_1 + (256 * com.readByte());
                       break;
                   case 242://Filter 1 : 0xFE
                       filter_2 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[2] = filter_2 + (256 * com.readByte());
                       break;
                   case 243:////Filter 2 : 0xF3
                       Thread.Sleep(1);
                       filter_3 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[3] = filter_3 + (256 * com.readByte());
                       break;
                   case 244://Filter 4 : 0xF4
                       filter_4 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[4] = filter_4 + (256 * com.readByte());
                       break;
                   case 245://Filter 5 : 0xF5
                       Thread.Sleep(1);
                       filter_5 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[5] = filter_5 + (256 * com.readByte());
                       break;
                   case 246://Filter 6 : 0xF6
                       Thread.Sleep(1);
                       filter_6 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[6] = filter_6 + (256 * com.readByte());
                       break;
                   case 247://Filter 7 : 0xF7
                       Thread.Sleep(1);
                       filter_7 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[7] = filter_7 + (256 * com.readByte());
                       break;
                   case 248://Filter 8 : 0xF8
                       //Thread.Sleep(1);
                       Thread.Sleep(1);
                       filter_8 = com.readByte();
                       Thread.Sleep(1);
                       baseVal[8] = filter_8 + (256 * com.readByte());
                       break;
                   case 249://Filter 9 : 0xF9
                       //Thread.Sleep(1);
                       Thread.Sleep(1);
                       filter_9 = com.readByte();
                       Thread.Sleep(1); ;
                       baseVal[9] = filter_9 + (256 * com.readByte());
                       loop = 13;
                       break;

               }
           }
           return baseVal;
       }
//Sweep methods
        public void sweep380(byte speed)
        {
            byte loop = 0;
            com.txtBoxDialog.AppendText("Sweep started:" + Environment.NewLine); //**can no longer access mainform
            for (int i = 380; i > 337; i--)//Filter 1
             {
                 if (com.isOpen() == false || com.stop.Checked) { com.stop.Checked = false;  break; }//stop process, reset check box
                 com.txtBoxDialog.AppendText("New Position" + i + " nM" + Environment.NewLine); //**can no longer access mainform
                 System.Windows.Forms.Application.DoEvents(); 
                 moveNM(speed, i);
                 while (loop != byteCR)
                 {
                     Thread.Sleep(1);
                     System.Windows.Forms.Application.DoEvents();
                     loop = com.readByte();
                     //com.txtBoxDialog.AppendText("Return" + loop + " ");
                 }
                 loop = 0;
                 com.txtBoxDialog.AppendText(Environment.NewLine);
                 Thread.Sleep(1000);
             }
             Thread.Sleep(1000);
        }
        public void sweep440(byte speed)
        {
            byte loop = 0;
            com.txtBoxDialog.AppendText("Sweep started:" + Environment.NewLine); //**can no longer access mainform
            for (int i = 440; i > 387; i--)//Filter 2
            {
                if (com.isOpen() == false || com.stop.Checked) { com.stop.Checked = false; break; }//stop process, reset check box
                com.txtBoxDialog.AppendText("New Position" + i + " nM" + Environment.NewLine); //**can no longer access mainform
                System.Windows.Forms.Application.DoEvents();
                moveNM(speed, i);
                while (loop != byteCR)
                {
                    Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                    loop = com.readByte();
                    //com.txtBoxDialog.AppendText("Return" + loop + " ");
                }
                loop = 0;
                com.txtBoxDialog.AppendText(Environment.NewLine);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }
        public void sweep490(byte speed)
        {
            byte loop = 0;
            com.txtBoxDialog.AppendText("Sweep started:" + Environment.NewLine); //**can no longer access mainform
            for (int i = 490; i > 429; i--)//Filter 3
            {
                if (com.isOpen() == false || com.stop.Checked) { com.stop.Checked = false; break; }//stop process, reset check box
                com.txtBoxDialog.AppendText("New Position" + i + " nM" + Environment.NewLine); //**can no longer access mainform
                System.Windows.Forms.Application.DoEvents();
                moveNM(speed, i);
                while (loop != byteCR)
                {
                    Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                    loop = com.readByte();
                    //com.txtBoxDialog.AppendText("Return" + loop + " ");
                }
                loop = 0;
                com.txtBoxDialog.AppendText(Environment.NewLine);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }
        public void sweep550(byte speed)
        {
            byte loop = 0;
            com.txtBoxDialog.AppendText("Sweep started:" + Environment.NewLine); //**can no longer access mainform
            for (int i = 550; i > 486; i--)//Filter 4
            {
                if (com.isOpen() == false || com.stop.Checked) { com.stop.Checked = false; break; }//stop process, reset check box
                com.txtBoxDialog.AppendText("New Position" + i + " nM" + Environment.NewLine); //**can no longer access mainform
                System.Windows.Forms.Application.DoEvents();
                moveNM(speed, i);
                while (loop != byteCR)
                {
                    Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                    loop = com.readByte();
                    //com.txtBoxDialog.AppendText("Return" + loop + " ");
                }
                loop = 0;
                com.txtBoxDialog.AppendText(Environment.NewLine);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }
        public void sweep620(byte speed)
        {
            byte loop = 0;
            com.txtBoxDialog.AppendText("Sweep started:" + Environment.NewLine); //**can no longer access mainform
            for (int i = 620; i > 547; i--)//Filter 5
            {
                if (com.isOpen() == false || com.stop.Checked) { com.stop.Checked = false; break; }//stop process, reset check box
                com.txtBoxDialog.AppendText("New Position" + i + " nM" + Environment.NewLine); //**can no longer access mainform
                System.Windows.Forms.Application.DoEvents();
                moveNM(speed, i);
                while (loop != byteCR)
                {
                    Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                    loop = com.readByte();
                    //com.txtBoxDialog.AppendText("Return" + loop + " ");
                }
                loop = 0;
                com.txtBoxDialog.AppendText(Environment.NewLine);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }
        public void sweep700(byte speed)
        {
            byte loop = 0;
            com.txtBoxDialog.AppendText("Sweep started:" + Environment.NewLine); //**can no longer access mainform
            for (int i = 700; i > 621; i--)//Filter 5
            {
                if (com.isOpen() == false || com.stop.Checked) { com.stop.Checked = false; break; }//stop process, reset check box
                com.txtBoxDialog.AppendText("New Position" + i + " nM" + Environment.NewLine); //**can no longer access mainform
                System.Windows.Forms.Application.DoEvents();
                moveNM(speed, i);
                while (loop != byteCR)
                {
                    Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                    loop = com.readByte();
                    //com.txtBoxDialog.AppendText("Return" + loop + " ");
                }
                loop = 0;
                com.txtBoxDialog.AppendText(Environment.NewLine);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }
        public void sweep800(byte speed)
        {
            byte loop = 0;
            com.txtBoxDialog.AppendText("Sweep started:" + Environment.NewLine); //**can no longer access mainform
            for (int i = 800; i > 699; i--)//Filter 5
            {
                if (com.isOpen() == false || com.stop.Checked) { com.stop.Checked = false; break; }//stop process, reset check box
                com.txtBoxDialog.AppendText("New Position" + i + " nM" + Environment.NewLine); //**can no longer access mainform
                System.Windows.Forms.Application.DoEvents();
                moveNM(speed, i);
                while (loop != byteCR)
                {
                    Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                    loop = com.readByte();
                    //com.txtBoxDialog.AppendText("Return" + loop + " ");
                }
                loop = 0;
                com.txtBoxDialog.AppendText(Environment.NewLine);
                Thread.Sleep(1000);
            }
            Thread.Sleep(1000);
        }
        public void Step5()
        {
            Thread.Sleep(10);
            //byte[] intBytes;
            int[] baseFilter = new int[5];
            byte loop = 0;
            byte speed = 3;
            baseFilter = getBaseFilters();
            for (int i = 0; i < 5; i++)
            {
                com.txtBoxDialog.AppendText(" Position: " + baseFilter[i] + Environment.NewLine);
                moveNM(speed, baseFilter[i]);
                Thread.Sleep(10);
                while (loop != byteCR)
                {
                    Thread.Sleep(1);
                    System.Windows.Forms.Application.DoEvents();
                    loop = com.readByte();
                }
                loop = 0;
                Thread.Sleep(1000);
                System.Windows.Forms.Application.DoEvents();
                if (com.stop.Checked) { i = 5; }
            }
        }
        public void sweepAll()
        {
            byte[] intBytes = new byte[2];
            byte loop = 0;
            byte speed = 3;
            int[] baseFilter = new int[5];
            baseFilter = getBaseFilters();
            com.stop.Checked = false;
            for (int i = 0; i < 5; i++)
            {
                int range = getRange(baseFilter[i]);
                for (int j = baseFilter[i]; j > (baseFilter[i] - range); j--)//Filter 1
                {
                    if (com.isOpen() == false || com.stop.Checked) { com.stop.Checked = false; break; }//stop process, reset check box
                    com.txtBoxDialog.AppendText("New Position" + i + " nM" + Environment.NewLine); //**can no longer access mainform
                    System.Windows.Forms.Application.DoEvents();
                    moveNM(speed, j);
                    while (loop != byteCR)
                    {
                        Thread.Sleep(1);
                        System.Windows.Forms.Application.DoEvents();
                        loop = com.readByte();
                        //com.txtBoxDialog.AppendText("Return" + loop + " ");
                    }
                    loop = 0;
                    com.txtBoxDialog.AppendText(Environment.NewLine);
                    Thread.Sleep(1000);
                }

            }
        }
    }
}
/* http://stackoverflow.com/questions/1940165/how-to-access-to-the-parent-object-in-c-sharp
Question
--I have a "meter" class. One property of "meter" is another class called "production". 
-- I need to access to a property of meter class (power rating) from production class by reference. 
--The powerRating is not known at the instantiation of Meter.


public class lambdaSerial//Meter
{
   --private vf5Functions vf5;/Production _production;

   public lambdaSerial();//Meter()
   {
      vf5 = new vf5Functions();//_production = new Production();
   }
}
Answer
public class vf5Functions{//Production {
  //The other members, properties etc...
 
  private lambdaSerial com;// Meter m;

  Production(lambdaSerial com){ //Meter m) {
    this.com = com;
  }
 public class lambdaSerial//Meter
{
   private vf5Functions vf5;//Production _production;

   public lambdaSerial()//Meter()
   {
      vf5 = new vf5Functions(this);//_production = new Production(this);
   }
}
}
*/