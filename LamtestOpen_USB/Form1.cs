//Original source
//http://msmvps.com/blogs/coad/archive/2005/03/23/SerialPort-_2800_RS_2D00_232-Serial-COM-Port_2900_-in-C_2300_-.NET.aspx
//C# namespace + Includes
//http://www.aspfree.com/c/a/C-Sharp/C-Sharp-Classes-Explained/
//Saving + printing the text box
//http://dotnetperls.com/textbox
//http://www.c-sharpcorner.com/UploadFile/mgold/NotepadDotNet07312005142055PM/NotepadDotNet.aspx
/*
**	FILENAME			lambdaSerial.cs
**
**	PURPOSE				This class is the front end of the application.  LambdaSerial.cs
 *                      handles the communications, this includes special lambda
 *                      commands.  lambdaUtils handles special function unique to 
 *                      the lambda imaging line.
**
**	CREATION DATE		10-01-2010
**	LAST MODIFICATION	07-28-2014
**
**	AUTHOR				Dan Carte
**
*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
//using System.DateTime;//for timing sequence

//FTDI 
using FTD2XX_NET;


namespace Lamtest
{
    public partial class Mainform : Form
    {
        //TextBox txtCom = new TextBox();
        lambdaSerial lambdaCom;
        lambdaUtils lamUtil;
        byteCommands byteCom;
        Random random;
        //wheelTestFunctions wheeltest;
        //Stream needed for ring buffer
        StreamReader tr;
        StreamWriter tw;
        string[] filterValue = new string[50];
        //end stream stuff
        string[] deviceList = new string[128];
        public string controller, LB10B_SB;
        public bool stopProcess = false;
        public bool stopSeq = false;//public for stop sequence
        public bool nextSeq = false;

        public Mainform()
        {
            InitializeComponent();
            lambdaCom = new lambdaSerial(this);
            lamUtil = new lambdaUtils();
            byteCom = new byteCommands();
            //wheeltest= new wheelTestFunctions(this);
            //random = new Random();
            uint ui = 0;
            do
            {
                try
                {
                    if (lambdaCom.GetSerialNum(ui) != null)
                    {
                        txtUSBList.Items.Add((lambdaCom.GetSerialNum(ui) + ": " + lambdaCom.GetDescription(ui)));
                        deviceList[ui] = lambdaCom.GetSerialNum(ui);
                    }
                }
                catch
                {

                }
                ui++;
            } while (lambdaCom.GetSerialNum((ui - 1)) != "No Device" && ui < 129);
            try
            {
                if (lambdaCom.GetSerialNum(0) != null)
                {
                    txtUSBList.SetSelected(0, true);
                }
            }
            catch
            {

            }
            for (int i = 0; i <= 49; i++)
            {
                filterValue[i] = "eof";
            }
            //lambdaCom.openCom();
        }
        ~Mainform()
        {
            lambdaCom.clearBuffer();
            lambdaCom.closeCom();
        }
        public void UpdateTxtCom(string text)
        {
            this.txtCom.AppendText(text);
        }
        private void Mainform_Load(object sender, EventArgs e)
        {
            //txtCom.AppendText("devCount" + ftdiDeviceCount.ToString() + Environment.NewLine);
        }
        /*private void txtComPort_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("You can add custom values here if necessary." + Environment.NewLine);
        }*/
        private void btnOpenCom_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Opens the com port and reports the status of the unit." + Environment.NewLine);
        }
        private void btnOpenCom_Click(object sender, EventArgs e)
        {
            //UInt32 SelectedIndex = 1;
            string myMode = txtComPort.Text.ToString();
            int usbMode = txtOpenBy.SelectedIndex;
            uint usbBuad = uint.Parse(txtBaudSelect.Text.ToString());
            int rs232Buad = int.Parse(txtBaudSelect.Text.ToString());

            switch (myMode)//if (myMode == "USB")
            {
                case "USB":
                    switch (usbMode)
                    {
                        case 0:
                            lambdaCom.OpenBySerialNum(lambdaCom.GetSerialNum());
                            txtHelp.AppendText("Case 0." + Environment.NewLine);
                            break;
                        case 1:
                            lambdaCom.openByIndex(0);
                            txtHelp.AppendText("Case 1." + Environment.NewLine);
                            break;
                        /*case 2:
                            lambdaCom.OpenByDescription(lambdaCom.GetDescription());
                            break; Does not work properly
                         * */
                        default:
                            lambdaCom.openCom(myMode);
                            txtHelp.AppendText("Case D." + Environment.NewLine);
                            //MessageBox.Show("Default" + usbMode); 
                            break;
                    }
                    lambdaCom.SetUSBBaudrate(usbBuad);
                    //MessageBox.Show("Baud set" + usbBuad.ToString()); 
                    break;
                case "LPT1":
                    lambdaCom.openCom(myMode);
                    lambdaCom.writeByte(238);
                    break;
                case "LPT2":
                    lambdaCom.openCom(myMode);
                    lambdaCom.writeByte(238);
                    break;
                default:
                    lambdaCom.openCom(myMode);
                    lambdaCom.SetRS232Baudrate(rs232Buad);
                    break;
            }
            byte[] txbuf = new byte[25];
            byte[] rxbuf = new byte[25];
            lambdaCom.clearBuffer();
            lambdaCom.writeByte(byteCom.byteGoOnline);
            //readString();
            //if (myMode!="USB"){ lambdaCom.readPort();}//messes up usb!
            controller = lambdaCom.getController();
            //txtCom.AppendText("R-" + controller + Environment.NewLine);
            if (lambdaCom.isOpen() || controller == "LB10-2")
            {
                txtBoxDialog.AppendText(txtComPort.Text.ToString() + " :  is Open" + Environment.NewLine);
            }
            else
            {
                txtBoxDialog.AppendText("Could not open: " + txtComPort.Text.ToString() + Environment.NewLine);
            }
            lambdaCom.clearBuffer();
            txtBoxDialog.AppendText("Config String: " + lambdaCom.getConfig() + Environment.NewLine);
            txtBoxDialog.AppendText("Controller= " + controller + Environment.NewLine);
            //writeConfig();
            controller = lambdaCom.getController();//Sets controller for entire application
            LB10B_SB = lambdaCom.getConfig();
            try
            {
                LB10B_SB = LB10B_SB.Substring(13, 2);//IQ for dual shutter version
                
            }
            catch { LB10B_SB = "NA"; }
            lambdaCom.clearBuffer();
            txtBoxDialog.AppendText("End" + Environment.NewLine);
            //txtBoxDialog.AppendText("LB10B_SB: " + LB10B_SB + Environment.NewLine);
        }//End open com methods

        private void btnClose_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Closes the com port and clears dialog box." + Environment.NewLine);
        }
        //**************************************************************************************************************
        //Works with a DG-4 as well.
        private void radioFilter0_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("On click moves to position 0." + Environment.NewLine);
        }

        private void txtSpeedBox_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Set the movement speed." + Environment.NewLine);
            txtHelp.AppendText("Speed 0 is reserved for the high speed 4 Position wheel." + Environment.NewLine);
        }
        private void txtWheelBox_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Set the active wheel." + Environment.NewLine);
        }
        //Send Command**************************************************************************************************
        private void btnSendCommand_MouseClick(object sender, MouseEventArgs e)
        {
            byte myCommand = (byte)decCommand.Value;
            lambdaCom.writeByte(myCommand);
            byte returnString = lambdaCom.readByte();
            //txtCom.AppendText("R1-" + returnString + " : " + (char)returnString + Environment.NewLine);
            returnString = lambdaCom.readByte();
        }
        //**************************************************************************************************************
        //Shutter test panel.
        private void txtShutterSelect_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Select the active shutter." + Environment.NewLine);
        }
        private void radioOpenA_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Opens shutter A." + Environment.NewLine);
            txtHelp.AppendText("The mode is set by 'Shutter mode'." + Environment.NewLine);
        }
        private void txtModeA_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText(" Shutter mode: Default mode open in the last configuration." + Environment.NewLine);
        }
        private void decNDA_Enter(object sender, EventArgs e)
        {
            txtHelp.AppendText("For ND mode. Useful values 16-128" + Environment.NewLine);
        }
        private void openAButton_MouseClick(object sender, MouseEventArgs e)
        {
            //lambdaCom.writeByte(byteCom.bytePowerOn);
            //Thread.Sleep(2);
            if (lambdaCom.isOpen() && radioOpenA.Checked)
            {
                string shutterMode = txtModeA.Text.ToString();
                byte ndSetting = (byte)decNDA.Value;
                switch (shutterMode)
                {
                    case "Fast":
                        lambdaCom.lbX.openA(controller,LB10B_SB);
                        lambdaCom.readPort();
                        break;
                    case "Soft":
                        lambdaCom.lbX.openASoft(controller,LB10B_SB);
                        lambdaCom.readPort();
                        break;
                    case "ND Mode":
                        lambdaCom.lbX.openA(controller, ndSetting,LB10B_SB);
                        lambdaCom.readPort();
                        break;
                    case "Default Mode":
                        lambdaCom.lbX.openAdefault(controller, LB10B_SB);
                        lambdaCom.readPort();
                        break;
                    default:
                        lambdaCom.lbX.openAdefault(controller, LB10B_SB);
                        //txtCom.AppendText("W-Open A Default" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                }
                txtBoxDialog.AppendText(" Shutter A: Opened " + shutterMode + "mode " + Environment.NewLine);
            }
            return;
        }
        private void radioCloseA_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Close shutter A." + Environment.NewLine);
        }
        private void closeAButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioCloseA.Checked)
            {
                lambdaCom.lbX.closeShutterA();
                lambdaCom.readPort();
                txtBoxDialog.AppendText(" Shutter A: Closed " + Environment.NewLine);
            }
            return;
        }
        private void radioOpenB_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Opens shutter B." + Environment.NewLine);
            txtHelp.AppendText("The mode is set by 'Shutter mode'." + Environment.NewLine);
        }
        private void txtModeB_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText(" Shutter mode: Default mode open in the last configuration." + Environment.NewLine);
        }
        private void decNDB_Enter(object sender, EventArgs e)
        {
            txtHelp.AppendText("For ND mode. Useful values 16-128" + Environment.NewLine);
        }
        private void openBButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioOpenB.Checked)
            {
                string shutterMode = txtModeB.Text.ToString();
                byte ndSetting = (byte)decNDB.Value;
                switch (shutterMode)
                {
                    case "Fast":
                        lambdaCom.lbX.openB();
                        lambdaCom.readPort();
                        break;
                    case "Soft":
                        lambdaCom.lbX.openBSoft();
                        lambdaCom.readPort();
                        break;
                    case "ND Mode":
                        lambdaCom.lbX.openB(ndSetting);
                        lambdaCom.readPort();
                        break;
                    case "Default Mode":
                        lambdaCom.lbX.openBdefault();
                        lambdaCom.readPort();
                        break;
                    default:
                        lambdaCom.lbX.openBdefault();
                        lambdaCom.readPort();
                        break;
                }
                txtBoxDialog.AppendText(" Shutter B: Opened " + shutterMode + "mode " + Environment.NewLine);
            }
            return;
        }
        private void radioCloseB_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Close shutter B." + Environment.NewLine);
        }
        private void closeBButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioCloseB.Checked)
            {
                lambdaCom.lbX.closeShutterB();
                lambdaCom.readPort();
                txtBoxDialog.AppendText(" Shutter B: Closed " + Environment.NewLine);
            }
            return;
        }
        private void radioOpenC_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Opens shutter C." + Environment.NewLine);
            txtHelp.AppendText("The mode is set by 'Shutter mode'." + Environment.NewLine);
            txtHelp.AppendText("The jumper MUST be set for a shutter on port C" + Environment.NewLine);
        }
        private void txtModeC_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText(" Shutter mode: Default mode open in the last configuration." + Environment.NewLine);
        }
        private void decNDC_Enter(object sender, EventArgs e)
        {
            txtHelp.AppendText("For ND mode. Useful values 16-128" + Environment.NewLine);
        }
        private void openCButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioOpenC.Checked)
            {
                string shutterMode = txtModeC.Text.ToString();
                byte ndSetting = (byte)decNDC.Value;
                switch (shutterMode)
                {
                    case "Fast":
                        lambdaCom.lbX.openC();
                        lambdaCom.readPort();
                        break;
                    case "Soft":
                        lambdaCom.lbX.openCSoft();
                        lambdaCom.readPort();
                        break;
                    case "ND Mode":
                        lambdaCom.lbX.openC(ndSetting);
                        lambdaCom.readPort();
                        break;
                    case "Default Mode":
                        lambdaCom.lbX.openCdefault();
                        lambdaCom.readPort();
                        break;
                    default:
                        lambdaCom.lbX.openCdefault();
                        lambdaCom.readPort();
                        break;
                }
                txtBoxDialog.AppendText(" Shutter C: Opened " + shutterMode + "mode " + Environment.NewLine);
            }
            return;
        }
        private void radioCloseC_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Close shutter C." + Environment.NewLine);
        }
        private void closeCButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioCloseC.Checked)
            {
                lambdaCom.lbX.closeShutterC();
                lambdaCom.readPort();
                txtBoxDialog.AppendText(" Shutter C: Closed " + Environment.NewLine);
            }
            return;
        }
        private void decHertz_Enter(object sender, EventArgs e)
        {
            txtHelp.AppendText("Choose the desired frequency for the test." + Environment.NewLine);
            txtHelp.AppendText("Frequencies above 18hz can be unstable." + Environment.NewLine);
            txtHelp.AppendText("Frequencies from 28-42hz are extremely unstable." + Environment.NewLine);
            txtHelp.AppendText("42hz for the  25mm and 38hz for the 35mm can be quite good." + Environment.NewLine);
        }
        private void btnShutterTest_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Test the designated shutter at the designated frequency." + Environment.NewLine);
        }
        private void btnShutterTest_MouseClick(object sender, MouseEventArgs e)
        {

            stopProcess = false;
            float sleep;
            int delay;
            string shutter = txtShutterSelect.Text.ToString();
            int hertz = (int)decHertz.Value;
            sleep = (1000 - 24 * hertz) / hertz;
            delay = (int)Math.Round(sleep, 0);//lambdaUtils.getDelay();
            if (delay < 0) { delay = 0; }//It can go negative when it shouldn't
            //hertz = lambdaCom.getHertz(hertz);
            txtBoxDialog.AppendText("delay=" + delay);
            lambdaCom.lbsc.cycleShutter(shutter, delay);
            return;
        }
        //************************************************************************************
        //Wheel test panel
        private void chkBoxAddDel0_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Increased delay between moves for speed 0." + Environment.NewLine);
            txtHelp.AppendText("Speed 0 should only be used with a high speed wheel." + Environment.NewLine);
        }
        private void chkBoxAddDel1_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Increased delay between moves for speed 1." + Environment.NewLine);
            txtHelp.AppendText("Speed 1 is best for a light to marginally loaded wheel" + Environment.NewLine);
        }
        private void chkBoxAddDel2_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Increased delay between moves for speed 2." + Environment.NewLine);
            txtHelp.AppendText("Speed 2 tends to be rough unless the wheel is loaded." + Environment.NewLine);
            txtHelp.AppendText("So speed 2 requires additional delay." + Environment.NewLine);
        }
        private void chkBoxAddDel3_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Increased delay between moves for speed 3." + Environment.NewLine);
            txtHelp.AppendText("Speed 3 tends to be rough unless the wheel is loaded." + Environment.NewLine);
            txtHelp.AppendText("So speed 3 requires additional delay." + Environment.NewLine);
        }
        private void chkBoxAddDel4_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Increased delay between moves for speed 4." + Environment.NewLine);
            txtHelp.AppendText("Speed 4 tends to be rough unless the wheel is loaded." + Environment.NewLine);
            txtHelp.AppendText("So speed 4 might require additional delay." + Environment.NewLine);
        }
        private void chkBoxAddDel5_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Increased delay between moves for speed 5." + Environment.NewLine);
            txtHelp.AppendText("Speed 5 tends to be rough unless the wheel is loaded." + Environment.NewLine);
            txtHelp.AppendText("Speed 5 should be reliable without delay." + Environment.NewLine);
        }
        private void chkBoxAddDel6_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Increased delay between moves for speed 6." + Environment.NewLine);
            txtHelp.AppendText("Speed 6 tends to be rough unless the wheel is loaded." + Environment.NewLine);
            txtHelp.AppendText("Speed 6 should be reliable without delay." + Environment.NewLine);
        }
        private void chkBoxAddDel7_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Increased delay between moves for speed 7." + Environment.NewLine);
            txtHelp.AppendText("Speed 7 tends to be rough unless the wheel is loaded." + Environment.NewLine);
            txtHelp.AppendText("Speed 7 should be reliable without delay." + Environment.NewLine);
        }
        private void DecDelayMultiplier_MouseClick(object sender, MouseEventArgs e)
        {
            txtHelp.AppendText("Multiplies the standard delay by 1-5." + Environment.NewLine);
            txtHelp.AppendText("For checked boxes only." + Environment.NewLine);
        }
        private void txtDelay_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("The delay in mili-seconds to be used in the test." + Environment.NewLine);
        }
        private void txtNumSteps_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("The number of steps to be used in the test." + Environment.NewLine);
        }
        private void txtTopSpeed_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("The fastest speed to be used in the test." + Environment.NewLine);
        }
        private void txtLastSpeed_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("The slowest speed to be used in the test." + Environment.NewLine);
        }
        private void txtTestMode_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("This box determines the wheels to be tested." + Environment.NewLine);
            txtHelp.AppendText("LB10-3 batch mode tests all 3 wheel." + Environment.NewLine);
            txtHelp.AppendText("LB10-2 batch mode wheels A && B only." + Environment.NewLine);
        }
        private void btnTestWheel_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Performs a test moving the wheels to a random position from 0-9 using the specified parameters." + Environment.NewLine);
        }
        private void testButton_Click(object sender, EventArgs e)
        {

        }
        private void btnTestWheel_MouseClick(object sender, MouseEventArgs e)
        {
            if (txtComPort.Text.ToString() == "LPT1" || txtComPort.Text.ToString() == "LPT2")
            {
                txtBoxDialog.AppendText("The error detection does not work on the parallel port!" + Environment.NewLine);
            }
            lambdaCom.wheelTest.testWheel();// testWheel();
        }
        private void btnRandomTest_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Performs a test of the wheels to a random position from 0-9 using a random delay." + Environment.NewLine);
            txtHelp.AppendText("This test us useful for finding a good speed / delay combination for the current load of the wheel." + Environment.NewLine);
        }
        private void randomButton_Click(object sender, EventArgs e)
        {

        }
        private void filter1UpDown_MouseClick(object sender, MouseEventArgs e)
        {
            txtHelp.AppendText("Determines the first position to test in the fixed test." + Environment.NewLine);
        }
        private void filter2UpDown_MouseClick(object sender, MouseEventArgs e)
        {
            txtHelp.AppendText("Determines the second position to test in the fixed test." + Environment.NewLine);
        }
        private void btnFixTest_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Performs a test of the wheels to two positions using a random delay." + Environment.NewLine);
            txtHelp.AppendText("This test us useful for finding a good speed / delay combination." + Environment.NewLine);
            txtHelp.AppendText("For the current load of the wheel between 2 predetermined positions." + Environment.NewLine);
        }


        //******************************************************************************************************
        //LB-SC panel
        private void txtTrigger_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Sets the TTL trigger mode." + Environment.NewLine);
            txtHelp.AppendText("Default: open on TTL high." + Environment.NewLine);
            txtHelp.AppendText("Toggle mode triggers on the rising edge." + Environment.NewLine);
        }
        private void btnSetTTL_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Save defaults to the LB-SC controller." + Environment.NewLine);
        }
        private void txtSync_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Determines the TTL Sync mode." + Environment.NewLine);
            txtHelp.AppendText("Default: TTL high on shutter open." + Environment.NewLine);
        }
        private void setTTL_Click(object sender, EventArgs e)
        {

        }
        //The LB-SC has a unique protocol.  The minutes, seconds,mili-Seconds and micro-seconds
        //are all encoded separately.
        private void decMin_MouseClick(object sender, MouseEventArgs e)
        {
            txtHelp.AppendText("Set the minutes." + Environment.NewLine);
        }
        private void decSec_MouseClick(object sender, MouseEventArgs e)
        {
            txtHelp.AppendText("Set the seconds." + Environment.NewLine);
        }
        private void decMs_MouseClick(object sender, MouseEventArgs e)
        {
            txtHelp.AppendText("Set the mili-seconds." + Environment.NewLine);
        }
        private void decUs_MouseClick(object sender, MouseEventArgs e)
        {
            txtHelp.AppendText("Set the micro-seconds." + Environment.NewLine);
        }
        private void btnExposure_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Uses the timer values to set the exposure time." + Environment.NewLine);
        }
        private void btnDelay_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Uses the timer values to set the delay time." + Environment.NewLine);
        }
        private void btnRun_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Start free run." + Environment.NewLine);
        }
        private void decCycles_MouseClick(object sender, MouseEventArgs e)
        {
            txtHelp.AppendText("Sets the free run Cycles." + Environment.NewLine);
            txtHelp.AppendText("Sets the value to 1 to use the delay on the first move." + Environment.NewLine);
        }
        private void btnRestoreSC_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Restore factory default to LB-SC." + Environment.NewLine);
        }
        private void restoreButton_Click(object sender, EventArgs e)
        {
            lambdaCom.lbsc.stopFreeRun();
            lambdaCom.lbsc.restoreDefaults();
        }
        private void btnResetSC_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Reset LB-SC to last configuration." + Environment.NewLine);
            txtHelp.AppendText("Not Implemented." + Environment.NewLine);
        }
        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            txtHomePos.Text = lambdaCom.lbsc.getShutterHome().ToString() + " uSteps";
        }
        private void btnStopAutoRun_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Stop free run." + Environment.NewLine);
        }
        private void stopButton_Click(object sender, EventArgs e)
        {
            lambdaCom.lbsc.stopFreeRun();
        }
        private void btnRun_MouseClick(object sender, MouseEventArgs e)
        {
            string myMode = txtRunMode.Text;
            int runCycles = (int)decCycles.Value;
            txtBoxDialog.Text = ("Run Mode: On " + myMode + Environment.NewLine);
            lambdaCom.lbsc.setFreeRun(myMode, runCycles);
        }
        private void btnSetTTL_MouseClick(object sender, MouseEventArgs e)
        {
            string ttlIn, ttlOut;
            ttlIn = txtTrigger.Text;
            ttlOut = txtSync.Text;
            switch (ttlIn)
            {
                case "TTL High":
                    lambdaCom.lbsc.setTTLHigh();
                    break;
                case "TTL Low":
                    lambdaCom.lbsc.setTTLLow();
                    break;
                case "Rising Edge":
                    lambdaCom.lbsc.setTTLToggleRisingEdge();
                    break;
                case "Falling Edge":
                    lambdaCom.lbsc.setTTLToggleFallingEdge();
                    break;
                case "Disabled":
                    lambdaCom.lbsc.setTTLDisabled();
                    break;
            }
            switch (ttlOut)
            {
                case "High on Open":
                    lambdaCom.lbsc.setSyncHighOpen();
                    break;
                case "Low on Open":
                    lambdaCom.lbsc.setSyncLowOpen();
                    break;
                case "Disabled":
                    lambdaCom.lbsc.setSyncDisabled();
                    break;
            }
            lambdaCom.lbsc.setNewDefault();
            return;
        }
        private void resetTTL_Click(object sender, EventArgs e)
        {
            lambdaCom.lbsc.setTTLHigh();
            lambdaCom.lbsc.setSyncLowOpen();
            lambdaCom.lbsc.setNewDefault();
            return;
        }
        private void btnExposure_MouseClick(object sender, MouseEventArgs e)
        {
            uint usTime = (uint)decUs.Value;
            uint msTime = (uint)decMs.Value;
            uint secTime = (uint)decSec.Value;
            uint minTime = (uint)decMin.Value;
            lambdaCom.lbsc.setExposureTimer(minTime, secTime, msTime, usTime);
            decUs.Value = 0;
            decMs.Value = 0;
            decSec.Value = 0;
            decMin.Value = 0;
        }
        private void btnDelay_MouseClick(object sender, MouseEventArgs e)
        {
            uint usTime = (uint)decUs.Value;
            uint msTime = (uint)decMs.Value;
            uint secTime = (uint)decSec.Value;
            uint minTime = (uint)decMin.Value;
            lambdaCom.lbsc.setDelayTimer(minTime, secTime, msTime, usTime);
            decUs.Value = 0;
            decMs.Value = 0;
            decSec.Value = 0;
            decMin.Value = 0;
        }
        private void btnStopAutoRun_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.lbsc.stopFreeRun();
            //lambdaCom.lbsc.saveToLBSC();
        }
        private void btnRestoreSC_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.lbsc.stopFreeRun();
            lambdaCom.lbsc.restoreDefaults();
            lambdaCom.lbsc.saveToLBSC();
        }
        //****************************************************************************************************** 
        //dialog box methods
        private void getPortsButton_Click(object sender, EventArgs e)
        {
            string writePorts = lambdaCom.getPorts();
            txtBoxDialog.AppendText("The following ports are available on your PC:" + "\r\n");
            txtBoxDialog.AppendText(writePorts);
        }
        private void statusButton_Click(object sender, EventArgs e)
        {
            writeConfig();
            txtBoxDialog.AppendText("End" + Environment.NewLine);
        }
        private void writeConfig()
        {
            string controller, wheelA, wheelB, wheelC, shutterA, shutterB, shutterC, status, controllerString;
            status = lambdaCom.getConfig();
            controller = lambdaCom.getController();

            switch (controller)
            {
                case "10-3":
                    wheelA = lambdaCom.getWheelA(status);
                    wheelB = lambdaCom.getWheelB(status);
                    wheelC = lambdaCom.getWheelC(status);
                    shutterA = lambdaCom.getShutterA(status);
                    shutterB = lambdaCom.getShutterB(status);//Note case statements change the scope of the dialog box!
                    shutterC = lambdaCom.getShutterC(status);
                    controllerString = lambdaCom.getConfigString(wheelA, wheelB, wheelC, shutterA, shutterB, shutterC);
                    //dialogBox.AppendText("Lambda 10-3" + Environment.NewLine);
                    txtBoxDialog.AppendText(controllerString);
                    break;
                case "10-B":
                    wheelA = lambdaCom.getWheelA(status);
                    shutterA = lambdaCom.getShutterA(status);
                    shutterB = lambdaCom.getShutterB(status);
                    controllerString = lambdaCom.getConfigString(wheelA, shutterA, shutterB);
                    txtBoxDialog.AppendText(controllerString);
                    break;
                case "LB10-2":
                    controllerString = lambdaCom.getConfigString();
                    txtBoxDialog.AppendText(controllerString);
                    break;
                case "SC":
                    shutterA = lambdaCom.getShutterA(status);
                    //dialogBox.AppendText("Lambda SC" + Environment.NewLine);
                    controllerString = lambdaCom.getConfigString(shutterA);
                    txtBoxDialog.AppendText(controllerString);
                    break;
                /* default:
                     wheelA = lambdaCom.getWheelA(status);
                     wheelB = lambdaCom.getWheelB(status);
                     wheelC = lambdaCom.getWheelC(status);
                     shutterA = lambdaCom.getShutterA(status);
                     shutterB = lambdaCom.getShutterB(status);//Note case statements change the scope of the dialog box!
                     shutterC = lambdaCom.getShutterC(status);
                     controllerString = lambdaCom.lambdaCom.getConfigString(wheelA, wheelB, wheelC, shutterA, shutterB, shutterC);
                     //dialogBox.AppendText("Lambda 10-3" + Environment.NewLine);
                     txtBoxDialog.AppendText(controllerString);
                     break;*/
            }
            lambdaCom.clearBuffer();
            return;
        }
        //*********************************************************************************************************
        //DG-4 panel

        private void radioTurboOn_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Turns on turbo-blanking." + Environment.NewLine);
            txtHelp.AppendText("Turbo-blanking prevents light leakage for non adjacent filter moves.." + Environment.NewLine);
        }
        private void radioTurboOn_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.writeByte(byteCom.byteOpenB);
        }
        private void radioTurboOff_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Turns on turbo-blanking." + Environment.NewLine);
            txtHelp.AppendText("Turbo-blanking prevents light leakage for non adjacent filter moves.." + Environment.NewLine);
        }
        private void radioTurboOff_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.writeByte(byteCom.byteCloseB);
        }
        private void shutterOpenButton_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Opens the shutter / move to position." + Environment.NewLine);
        }
        private void shutterOpenButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && shutterOpenButton.Checked)
            {
                lambdaCom.lbX.openA();
                txtBoxDialog.AppendText(" Shutter: Opened " + Environment.NewLine);
            }
            return;
        }
        private void shutterCloseButton_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Close the shutter / move to position 0." + Environment.NewLine);
        }
        private void shutterCloseButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && shutterCloseButton.Checked)
            {
                lambdaCom.lbX.closeShutterA();
                txtBoxDialog.AppendText(" Shutter: Closed " + Environment.NewLine);
            }
            return;
        }
        private void radioMove1_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 1." + Environment.NewLine);
        }
        private void move1RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove1.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(1);
                    txtBoxDialog.AppendText(" Moved Position 1 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(17);
                }
            }
            return;
        }
        private void radioMove2_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 2." + Environment.NewLine);
        }
        private void move2RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove2.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(2);
                    txtBoxDialog.AppendText(" position 2 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(18);
                }
            }
            return;
        }
        private void radioMove3_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 3." + Environment.NewLine);
        }
        private void move3RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove3.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(3);
                    txtBoxDialog.AppendText(" position 3 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(19);
                }
            }
            return;
        }
        private void radioMove4_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 4." + Environment.NewLine);
        }
        private void move4RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove4.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(4);
                    txtBoxDialog.AppendText(" position 4 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(20);
                }
            }
            return;
        }
        private void radioMove5_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 5 on the DG-5." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 1 66% power." + Environment.NewLine);
        }
        private void move5RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove5.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(5);
                    txtBoxDialog.AppendText(" position 5 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(21);
                }
            }
            return;
        }
        private void radioMove6_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 6." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 2 66% power." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 1 66% power." + Environment.NewLine);
        }
        private void move6RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove6.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(6);
                    txtBoxDialog.AppendText(" position 6 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(22);
                }
            }
            return;
        }
        private void radioMove7_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 7." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 3 66% power." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 2 66% power." + Environment.NewLine);
        }
        private void move7RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove7.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(7);
                    txtBoxDialog.AppendText(" position 7 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(23);
                }
            }
            return;
        }
        private void radioMove8_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 8." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 4 66% power." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 3 66% power." + Environment.NewLine);
        }
        private void move8RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove8.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(8);
                    txtBoxDialog.AppendText(" position 8 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(24);
                }
            }
            return;
        }
        private void radioMove9_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 9." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 1 33% power." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 4 66% power." + Environment.NewLine);
        }
        private void move9RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove9.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(9);
                    txtBoxDialog.AppendText(" position 9 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(25);
                }
            }
            return;
        }
        private void radioMove10_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 10." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 2 33% power." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 5 66% power." + Environment.NewLine);
        }
        private void move10RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove10.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(10);
                    txtBoxDialog.AppendText(" position 10 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(26);
                }
            }
            return;
        }
        private void radioMove11_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 11." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 3 33% power." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 1 33% power." + Environment.NewLine);
        }
        private void move11RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove11.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(11);
                    txtBoxDialog.AppendText(" position 11 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(27);
                }
            }
            return;
        }
        private void radioMove12_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 11." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 4 33% power." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 2 33% power." + Environment.NewLine);
        }
        private void move12RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove12.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(12);
                    txtBoxDialog.AppendText(" position 12 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(28);
                }
            }
            return;
        }
        private void radioMove13_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 12." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 0 / shutter." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 3 33% power." + Environment.NewLine);
        }
        private void move13RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove13.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(13);
                    txtBoxDialog.AppendText(" position 13 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(29);
                }
            }
            return;
        }
        private void radioMove14_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 12." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 0 / shutter." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 4 33% power." + Environment.NewLine);
        }
        private void move14RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove14.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(14);
                    txtBoxDialog.AppendText(" position 14 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(30);
                }
            }
            return;
        }
        private void radioMove15_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Move to Position 12." + Environment.NewLine);
            txtHelp.AppendText("DG-4: equals Position 0 / shutter." + Environment.NewLine);
            txtHelp.AppendText("DG-5: equals Position 5 33% power." + Environment.NewLine);
        }
        private void move15RadioButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioMove15.Checked)
            {
                if (chkTriggerd.Checked == false)
                {
                    lambdaCom.writeByte(15);
                    txtBoxDialog.AppendText(" position 15 " + Environment.NewLine);
                    shutterOpenButton.Checked = true;
                }
                else
                {
                    lambdaCom.writeByte(31);
                }
            }
            return;
        }

        //***************************************************************************************
        //Ring buffer methods

        private void radioStrobeOff_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Deactivate TTL / strobe." + Environment.NewLine);
        }
        private void ttlOffButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioStrobeOff.Checked)
            {
                lambdaCom.writeByte(203);
                txtBoxDialog.AppendText("TTL's Disabled " + Environment.NewLine);
            }
            return;
        }
        private void radioStrobeOn_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("activate TTL / strobe." + Environment.NewLine);
            txtHelp.AppendText("Must be actively driven." + Environment.NewLine);
        }
        private void strobeButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioStrobeOn.Checked)
            {
                lambdaCom.writeByte(202);
                txtBoxDialog.AppendText("Triggered by Strobe Pulse " + Environment.NewLine);
            }
            return;
        }
        private void decFilterNum_Enter_1(object sender, EventArgs e)
        {
            txtHelp.AppendText("Specifies the filter number in the sequence." + Environment.NewLine);
        }
        private void decSeqNum_Enter_1(object sender, EventArgs e)
        {
            txtHelp.AppendText("Specifies the sequence number." + Environment.NewLine);
        }
        private void btnAddVal_MouseHover_1(object sender, EventArgs e)
        {
            txtHelp.AppendText("Adds the filter number at the sequence number." + Environment.NewLine);
        }
        private void btnAddValue_MouseClick(object sender, MouseEventArgs e)
        {
            string filter = decFilterNum.Value.ToString();
            int sequence = (int)decSeqNum.Value;
            filterValue[sequence] = filter;
            decSeqNum.Value++;
            if (filterValue[sequence + 1] != "eof" && filterValue[sequence + 1] != null)
            {
                decFilterNum.Value = decimal.Parse(filterValue[sequence + 1]);
            }
            txtBoxDialog.AppendText("Current value" + filterValue[sequence + 1] + Environment.NewLine);
            txtBoxDialog.AppendText("Ring buffer filter# " + filter + " in sequence " + sequence + Environment.NewLine);
        }
        private void btnGetFile_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Gets a specific file and stores it in memory." + Environment.NewLine);
        }
        private void btnGetFile_MouseClick(object sender, MouseEventArgs e)
        {
            string file = txtFileName.Text;
            getFile(file);
        }
        private void btnSaveFile_MouseClick(object sender, MouseEventArgs e)
        {
            string file = txtFileName.Text;
            saveFile(file);
            txtBoxDialog.AppendText(file + ".txt Saved" + Environment.NewLine);
        }
        private void btnLoadBuffer_MouseHover_1(object sender, EventArgs e)
        {
            txtHelp.AppendText("Loads the file in memory into the DG-4." + Environment.NewLine);
        }
        private void btnClearRingBuffer_MouseClick(object sender, MouseEventArgs e)
        {
            decSeqNum.Value = 0;
            decFilterNum.Value = 0;
        }
        private void btnClearRingBuffer_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Resets filter number and sequence number to 0." + Environment.NewLine);
        }
        private void btnLoadBuffer_MouseClick(object sender, MouseEventArgs e)
        {
            int i = 0;
            lambdaCom.writeByte(byteCom.byteLB102Batch);//Start loading buffer
            if (filterValue[i] == "eof") { lambdaCom.writeByte(0); }
            while (filterValue[i] != "eof")
            {
                lambdaCom.writeByte(byte.Parse(filterValue[i]));//Load value
                i++;
            }
            lambdaCom.writeByte(byteCom.byteSetCycles);//End loading buffer
        }
        private void btnRingEnable_MouseHover_1(object sender, EventArgs e)
        {
            txtHelp.AppendText("Sets the DG-4 into ring buffer mode." + Environment.NewLine);
        }
        private void btnRingEnable_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.writeByte(byteCom.byteSetFreeRunPowerOn);
            lambdaCom.writeByte(202);
        }
        private void btnRingDisable_MouseHover_1(object sender, EventArgs e)
        {
            txtHelp.AppendText("Disables the ring buffer mode." + Environment.NewLine);
        }
        private void btnRingDisable_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.writeByte(byteCom.byteSetFreeRunTTLIn);
            lambdaCom.writeByte(203);
        }
        //******************************************************************************************
        //      File methods 
        //need load buffer methods.
        //need to initialize buffer file
        //**********************************************************************************
        private void txtFileName_MouseHover_1(object sender, EventArgs e)
        {
            txtHelp.AppendText("This name is used to both retrieve saved files as well as to save the file.." + Environment.NewLine);
        }
        public void getFile(string fileName)
        {// create reader & open file
            tr = new StreamReader(fileName + ".txt");
            for (int i = 0; i <= 49; i++)
            {
                if (filterValue[i] == "eof" || filterValue[i] == null)
                {
                    filterValue[i] = "eof";
                }
                else
                {
                    filterValue[i] = i.ToString();// tr.ReadLine();
                }
            }
            decSeqNum.Value = 0;
            tr.Close();
        }
        private void btnSaveFile_MouseHover_1(object sender, EventArgs e)
        {
            txtHelp.AppendText("Saves the file using the name in the text box." + Environment.NewLine);
        }
        public void saveFile(string fileName)
        {// create reader & open file
            int fileLength = (int.Parse(decSeqNum.Value.ToString()) - 1);
            tw = new StreamWriter(fileName + ".txt");
            for (int i = 0; i <= fileLength; i++)
            {
                //dialogBox.AppendText("i" + Environment.NewLine);
                tw.WriteLine(filterValue[i]);
            }
            decSeqNum.Value = 0;
            decFilterNum.Value = decimal.Parse(filterValue[0]);
            tw.Close();
        }
        private void txtMoves_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("This is used to track the number of moves once in test mode." + Environment.NewLine);
        }
        private void btnClearTxt_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Clears the text in the text box.." + Environment.NewLine);
        }
        private void clearButton_Click(object sender, EventArgs e)
        {
        }
        private void btnStop_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("Stops all running tests." + Environment.NewLine);
        }
        private void btnStop_MouseClick(object sender, MouseEventArgs e)
        {
            stopProcess = true;
            checkBoxStop.Checked = true;
        }
        private void btnGetConfitg_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("returns the configuration information of the controller." + Environment.NewLine);
        }
        private void btnGetPorts_MouseHover(object sender, EventArgs e)
        {
            txtHelp.AppendText("returns the com ports available for use." + Environment.NewLine);
        }
        private void btnGteWL_Click(object sender, EventArgs e)
        {
        }
        private void btnClearTxt_MouseClick(object sender, MouseEventArgs e)
        {
            txtBoxDialog.Text = "";
        }

        private void btnClose_MouseClick(object sender, MouseEventArgs e)
        {
            txtBoxDialog.Text = "";
            lambdaCom.closeCom();
            //txtCom.AppendText("W-Close Port" + Environment.NewLine);
            lambdaCom.readPort();
        }
        private void btnRandomTest_MouseClick(object sender, MouseEventArgs e)
        {
            if (txtComPort.Text.ToString() == "LPT1" || txtComPort.Text.ToString() == "LPT2")
            {
                txtBoxDialog.AppendText("The error detection does not work on the parallel port!" + Environment.NewLine);
            }
            lambdaCom.wheelTest.randomTest();
        }


        //******************************************************************************************
        // Versa Chrome Methods
        //need load buffer methods.
        //need to initialize buffer file
        //**********************************************************************************

        private void btnMoveNM_MouseClick(object sender, MouseEventArgs e)
        {
            int freqVal = (int)decNanoM.Value;
            byte speed = (byte)decTiltSpeed.Value;
            lambdaCom.vf5.moveNM(speed, freqVal);//moveNM(speed, freqVal);
            return;
        }
        private void btnSweep_MouseClick(object sender, MouseEventArgs e)
        {
            byte speed = 3;
            stopProcess = false;
            checkBoxStop.Checked = false;

            if (radio380.Checked)
            {
                lambdaCom.vf5.sweep380(speed);
            }
            if (radio440.Checked)
            {
                lambdaCom.vf5.sweep440(speed);
            }
            if (radio490.Checked)
            {
                lambdaCom.vf5.sweep490(speed);
            }
            if (radio550.Checked)
            {
                lambdaCom.vf5.sweep550(speed);
            }
            if (radio620.Checked)
            {
                lambdaCom.vf5.sweep620(speed);
            }
            if (radio700.Checked)
            {
                lambdaCom.vf5.sweep700(speed);
            }
            if (radio800.Checked)
            {
                lambdaCom.vf5.sweep800(speed);
            }
            return;
        }
        private void btnMoveStepAngle_MouseClick(object sender, MouseEventArgs e)
        {
            byte filterVal = (byte)decVF5Filter.Value;
            byte moveByte = (byte)(lamUtil.getMoveByte(2, filterVal));
            int stepIncrementLong = (int)decStepInc.Value;
            //http://stackoverflow.com/questions/1318933/c-int-to-byte
            byte[] intBytes = BitConverter.GetBytes(stepIncrementLong);
            //Array.Reverse(intBytes);
            byte[] result = intBytes;
            lambdaCom.vf5.setStepAngle(filterVal, moveByte, stepIncrementLong);
            return;
        }
        //Need to clean up return string.  Fist echo then return data!
        private void btnGteWL_MouseClick(object sender, MouseEventArgs e)
        {
            int range;
            int[] baseval = lambdaCom.vf5.getBaseFilters();
            txtBoxDialog.AppendText("Command" + byteCom.byteVFPosition + " ");
            txtBoxDialog.AppendText("+ " + byteCom.byteGetVFAll + Environment.NewLine);
            //Filter 0
            txtFilter1Base.AppendText(baseval[0].ToString());
            range = baseval[0] - lambdaCom.vf5.getRange(baseval[0]);
            txtF1Range.AppendText(range.ToString());
            //Filter 1
            txtFilter2Base.AppendText(baseval[1].ToString());
            range = baseval[1] - lambdaCom.vf5.getRange(baseval[1]);
            txtF2Range.AppendText(range.ToString());
            //Filter 2
            txtFilter3Base.AppendText(baseval[2].ToString());
            range = baseval[2] - lambdaCom.vf5.getRange(baseval[2]);
            txtF3Range.AppendText(range.ToString());
            //Filter 3
            txtFilter4Base.AppendText(baseval[3].ToString());
            range = baseval[3] - lambdaCom.vf5.getRange(baseval[3]);
            txtF4Range.AppendText(range.ToString());
            //Filter 3
            txtFilter5Base.AppendText(baseval[4].ToString());
            range = baseval[4] - lambdaCom.vf5.getRange(baseval[4]);
            txtF5Range.AppendText(range.ToString());
        }

        private void btnStep5_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.vf5.Step5();
        }
        private void txtBaudSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            lambdaCom.SetUSBBaudrate(uint.Parse(txtBaudSelect.SelectedItem.ToString()));
        }
        private void txtComPort_SelectedValueChanged(object sender, EventArgs e)
        {
            if (txtComPort.Text.ToString() != "USB") { txtBaudSelect.SelectedIndex = 0; }
        }
        private void btnStepAll_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.vf5.sweepAll();
        }
        private void btnGetZero_MouseClick(object sender, MouseEventArgs e)
        {
            txtVF5Home.Text = lambdaCom.lbsc.getShutterHome().ToString() + " uSteps";
        }

        private void btnPowerDown_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.writeByte(byteCom.bytePowerOff);
        }

        private void btnPowerUp_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.writeByte(byteCom.bytePowerOn);
        }

        private void btnGetConfitg_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.clearBuffer();
            txtBoxDialog.AppendText("ConfigString: " + lambdaCom.getConfig() + Environment.NewLine);
        }

        private void btnGetStatus_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.clearBuffer();
            txtBoxDialog.AppendText("Status String: " + lambdaCom.getStatus() + Environment.NewLine);
        }
        //________________________________________________________________________________________________________


        //Read write  methodes
        public string readConfigString()
        {//can not properly read the <CR> from the lambda so readTo(\r)?? should be \n weird
            txtCom.AppendText(Environment.NewLine + "Read Config. Str: " + Environment.NewLine);
            string inputStr = lambdaCom.readConfigString();
            char[] inputChars = inputStr.ToCharArray();//can not properly read the <CR> from the lambda!
            for (int i = 0; i < inputStr.Length; i++)
            {
                txtCom.AppendText("Char at " + i + Environment.NewLine);
                txtCom.AppendText("Sts.| Dec. | Hex. " + Environment.NewLine);
                string hexOutput = String.Format("{0:X}", (byte)(inputChars[i]));
                string decOutput = String.Format("{0:g}", (byte)(inputChars[i]));
                txtCom.AppendText("   " + inputChars[i].ToString() + "      " + decOutput + "      " + hexOutput + Environment.NewLine);
                //txtCom.AppendText(inputChars[i].ToString()+ " ");
            }
            txtCom.AppendText(Environment.NewLine);
            return inputStr;
        }

        public string readString()
        {//can not properly read the <CR> from the lambda so readTo(\r)?? should be \n weird
            string inputStr = lambdaCom.readString();
            char[] inputChars = inputStr.ToCharArray();//can not properly read the <CR> from the lambda!
            txtCom.AppendText(Environment.NewLine + "Read Str: " + Environment.NewLine);
            for (int i = 0; i < inputStr.Length; i++)
            {
                try
                {
                    txtCom.AppendText("Read char at " + i + Environment.NewLine);
                    txtCom.AppendText("Sts.| Dec. | Hex. " + Environment.NewLine);
                    string hexOutput = String.Format("{0:X}", (byte)(inputChars[i]));
                    string decOutput = String.Format("{0:g}", (byte)(inputChars[i]));
                    txtCom.AppendText("   " + inputChars[i].ToString() + "      " + decOutput + "      " + hexOutput + Environment.NewLine);
                }
                catch
                {
                    i = inputStr.Length;
                }
            }
            txtCom.AppendText(Environment.NewLine);
            return inputStr;
        }

        //VF Specific commands.*************************************************************************

        private void btnLB10B_Batch_MouseClick(object sender, MouseEventArgs e)
        {
            string ShutterA = txtBatchA.Text;
            string ShutterB = txtBatchB.Text;
            lambdaCom.writeByte(189);
            if (ShutterA == "Open") { lambdaCom.lbX.openAdefault(); }
            else { lambdaCom.lbX.closeShutterA(); }
            if (ShutterB == "Open") { lambdaCom.lbX.openBdefault(); }
            else { lambdaCom.lbX.closeShutterB(); }
            lambdaCom.writeByte(190);
        }

        private void btnSeqTest_MouseClick(object sender, MouseEventArgs e)
        {//move to whel test functions
            lambdaCom.wheelTest.seqTest();
            return;
        }

        private void btnShutterTimer_MouseClick(object sender, MouseEventArgs e)
        {
            int openTime = (int)decOpenMs.Value;
            int closeTime = (int)decCloseMs.Value;
            lambdaCom.writeByte(byteCom.byteOpenA);
            do
            {
                Thread.Sleep(openTime);
                lambdaCom.writeByte(byteCom.byteCloseA);
                Thread.Sleep(closeTime);
                lambdaCom.writeByte(byteCom.byteOpenA);
                Application.DoEvents();
            } while (stopProcess != true);
            stopProcess = false;
            return;
        }
        //************************************************************************************************
        //Wheel test methods=>Moved to subClass

        private void btnHS4Test_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.wheelTest.btnHS4Test();
        }
        private void fixTestButton_MouseClick(object sender, MouseEventArgs e)
        {
            lambdaCom.wheelTest.fixedTest();
        }
        private void button2_MouseClick(object sender, MouseEventArgs e)
        {
            int[] wl = new int[3];
            wl = lambdaCom.vf5.getCurrentWL_LB103();
            txtWL10B.Text = wl[0].ToString();
            return;
        }

        private void btnGetAllLB103_MouseClick(object sender, MouseEventArgs e)
        {
            int range;
            int channel = (int)decChannel_LB103VF.Value;
            int[] baseval = new int[10];
            //Add channel + switch
            baseval = lambdaCom.vf5.getBaseFilters_LB103_VF10_Chan(channel);

            txtBoxDialog.AppendText("Command" + byteCom.byteVFPosition + " ");
            txtBoxDialog.AppendText("+ " + byteCom.byteGetVFAll + Environment.NewLine);
            //Filter 0
            txtBase0_VF10.Text = baseval[0].ToString();
            range = baseval[0] - lambdaCom.vf5.getRange(baseval[0]);
            txtF0Range_VF10.Text = range.ToString();
            //Filter 1
            txtBase1_VF10.Text = baseval[1].ToString();
            range = baseval[1] - lambdaCom.vf5.getRange(baseval[1]);
            txtF1Range_VF10.Text = range.ToString();
            //Filter 2
            txtBase2_VF10.Text = baseval[2].ToString();
            range = baseval[2] - lambdaCom.vf5.getRange(baseval[2]);
            txtF2Range_VF10.Text = range.ToString();
            //Filter 3
            txtBase3_VF10.Text = baseval[3].ToString();
            range = baseval[3] - lambdaCom.vf5.getRange(baseval[3]);
            txtF3Range_VF10.Text = range.ToString();
            //Filter 4
            txtBase4_VF10.Text = baseval[4].ToString();
            range = baseval[4] - lambdaCom.vf5.getRange(baseval[4]);
            txtF4Range_VF10.Text = range.ToString();
            //Filter 5
            txtBase5_VF10.Text = baseval[5].ToString();
            range = baseval[5] - lambdaCom.vf5.getRange(baseval[5]);
            txtF5Range_VF10.Text = range.ToString();
            //Filter 6
            txtBase6_VF10.Text = baseval[6].ToString();
            range = baseval[6] - lambdaCom.vf5.getRange(baseval[6]);
            txtF6Range_VF10.Text = range.ToString();
            //Filter 7
            txtBase7_VF10.Text = baseval[7].ToString();
            range = baseval[7] - lambdaCom.vf5.getRange(baseval[7]);
            txtF7Range_VF10.Text = range.ToString();
            //Filter 8
            txtBase8_VF10.Text = baseval[8].ToString();
            range = baseval[8] - lambdaCom.vf5.getRange(baseval[8]);
            txtF8Range_VF10.Text = range.ToString();
            //Filter 9
            txtBase9_VF10.Text = baseval[9].ToString();
            range = baseval[9] - lambdaCom.vf5.getRange(baseval[9]);
            txtF9Range_VF10.Text = range.ToString();
        }

        private void button5_MouseClick(object sender, MouseEventArgs e)
        {
            int freqVal = (int)decNanoM_LB103.Value;
            byte speed = (byte)decTiltSpeed_LB103.Value;
            byte channel = (byte)decChannel_LB103VF.Value;
            lambdaCom.vf5.moveNM_LB103(speed, freqVal, channel);//moveNM_LB10B(speed, freqVal);
            int[] wl = new int[3];
            wl = lambdaCom.vf5.getCurrentWL_LB103();
            txtWL_103.Text = wl[channel - 1].ToString();
            return;
        }

        private void button8_MouseClick(object sender, MouseEventArgs e)
        {
            byte filterVal = (byte)decVFPos.Value;
            byte moveByte = (byte)(lamUtil.getMoveByte(2, filterVal));
            int stepIncrementLong = (int)decVFStepInc.Value;
            int channel = (int)decChannel_LB103VF.Value;//For LB10-3
            //http://stackoverflow.com/questions/1318933/c-int-to-byte
            byte[] intBytes = BitConverter.GetBytes(stepIncrementLong);
            //Array.Reverse(intBytes);
            byte[] result = intBytes;
            lambdaCom.vf5.setStepAngle(moveByte, stepIncrementLong, channel);
            return;
        }

        private void button4_MouseClick(object sender, MouseEventArgs e)
        {
            int[] wl = new int[3];
            wl = lambdaCom.vf5.getCurrentWL_LB103();
            int channel = (int)decChannel_LB103VF.Value;
            txtWL_103.Text = wl[channel - 1].ToString();
        }

        private void button1_MouseClick_1(object sender, MouseEventArgs e)
        {
            int channel = (int)decChannel_LB103VF.Value;//For LB10-3
            txtVF10Home.Text = lambdaCom.lbsc.getShutterHome(channel).ToString() + " uSteps";
        }

        private void button3_MouseClick(object sender, MouseEventArgs e)
        {
            //int address = int.Parse(txtLPTAdd.Text.ToString());
            string hexString = txtLPTAdd.Text.ToString();//"EC00";
            int address = Int32.Parse(hexString, System.Globalization.NumberStyles.HexNumber);
            lambdaCom.setAdress(address);
            txtComPort.Text = "LPT";
        }

        private void button10_MouseClick(object sender, MouseEventArgs e)//Start Sequence
        {
            lambdaCom.wheelTest.doSequence();
        }

        private void btnNext_MouseClick(object sender, MouseEventArgs e)
        {
            nextSeq = true;
        }

        private void button9_MouseClick(object sender, MouseEventArgs e)
        {
            stopSeq = true;
        }

        private void btnMove_MouseClick(object sender, MouseEventArgs e)
        {
            byte speedByteA, speedByteB, speedByteC, filterByteA, filterByteB, filterByteC;
            int myAction;
            speedByteA = (byte)txtSpdA.SelectedIndex;
            filterByteA = (byte)txtFilterA.SelectedIndex;
            speedByteB = (byte)txtSpdB.SelectedIndex;
            filterByteB = (byte)txtFilterB.SelectedIndex;
            speedByteC = (byte)txtSpdC.SelectedIndex;
            filterByteC = (byte)txtFilterC.SelectedIndex;
            myAction = txtWheelBox.SelectedIndex;
            radioOpenA.Checked = true;
            radioOpenB.Checked = true;
            lambdaCom.writeByte(byteCom.byteOpenACond);//open a conditional
            Thread.Sleep(15);
            lambdaCom.writeByte(byteCom.byteOpenBCond);//open a conditional
            switch (myAction)
            {
                case 0:
                    lambdaCom.lbX.moveMyWheel("Wheel A", speedByteA, filterByteA);
                    break;
                case 1:
                    lambdaCom.lbX.moveMyWheel("Wheel B", speedByteB, filterByteB);
                    break;
                case 2:
                    lambdaCom.lbX.moveMyWheel("Wheel C", speedByteC, filterByteC);
                    break;
                case 3:
                    lambdaCom.writeByte(byteCom.byteLB103Batch);
                    lambdaCom.lbX.moveMyWheel("Wheel A", speedByteA, filterByteA);
                    lambdaCom.lbX.moveMyWheel("Wheel B", speedByteB, filterByteB);
                    lambdaCom.lbX.moveMyWheel("Wheel C", speedByteC, filterByteC);
                    lambdaCom.writeByte(byteCom.byteEndBatch);
                    break;
                case 4:
                    lambdaCom.writeByte(byteCom.byteLB102Batch);
                    lambdaCom.lbX.moveMyWheel("Wheel A", speedByteA, filterByteA);
                    lambdaCom.lbX.moveMyWheel("Wheel B", speedByteB, filterByteB);
                    break;
                default:
                    lambdaCom.writeByte(byteCom.byteLB102Batch);
                    lambdaCom.lbX.moveMyWheel("Wheel A", speedByteA, filterByteA);
                    lambdaCom.lbX.moveMyWheel("Wheel B", speedByteB, filterByteB);
                    break;
            }
        }

        private void btnLB10B_Batch_MouseClick_1(object sender, MouseEventArgs e)
        {
            string ShutterA = txtBatchA.Text;
            string ShutterB = txtBatchB.Text;
            lambdaCom.writeByte(189);
            if (ShutterA == "Open") { lambdaCom.lbX.openAdefault(); }
            else { lambdaCom.lbX.closeShutterA(); }
            if (ShutterB == "Open") { lambdaCom.lbX.openBdefault(); }
            else { lambdaCom.lbX.closeShutterB(); }
            lambdaCom.writeByte(190);
        }

        private void radioCloseA_MouseClick(object sender, MouseEventArgs e)
        {
            //writeByte(byteCom.bytePowerOn);
            //Thread.Sleep(2);
            if (lambdaCom.isOpen() && radioCloseA.Checked)
            {
                lambdaCom.lbX.closeShutterA();
                //txtCom.AppendText("W-Close Shutter A"  + Environment.NewLine);
                lambdaCom.readPort();
                txtBoxDialog.AppendText(" Shutter A: Closed " + Environment.NewLine);
            }
            return;
        }

        private void radioCloseB_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioCloseB.Checked)
            {
                lambdaCom.lbX.closeShutterB();
                //txtCom.AppendText("W-Close Shutter B" + Environment.NewLine);
                lambdaCom.readPort();
                txtBoxDialog.AppendText(" Shutter B: Closed " + Environment.NewLine);
            }
            return;
        }

        private void radioCloseC_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioCloseB.Checked)
            {
                lambdaCom.lbX.closeShutterC();
                //txtCom.AppendText("W-Close Shutter B" + Environment.NewLine);
                lambdaCom.readPort();
                txtBoxDialog.AppendText(" Shutter B: Closed " + Environment.NewLine);
            }
            return;
        }

        private void radioOpenA_MouseClick(object sender, MouseEventArgs e)
        {
            //writeByte(byteCom.bytePowerOn);
            //Thread.Sleep(2);
            if (lambdaCom.isOpen() && radioOpenA.Checked)
            {
                string shutterMode = txtModeA.Text.ToString();
                byte ndSetting = (byte)decNDA.Value;
                switch (shutterMode)
                {
                    case "Fast":
                        lambdaCom.lbX.openA();
                        //txtCom.AppendText("W-Open A fast" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "Soft":
                        lambdaCom.lbX.openASoft();
                        //txtCom.AppendText("W-Open A soft" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "ND Mode":
                        lambdaCom.lbX.openA(ndSetting);
                        //txtCom.AppendText("W-Open A ND" + ndSetting + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "Default Mode":
                        lambdaCom.lbX.openAdefault();
                        //txtCom.AppendText("W-Open A Default" + ndSetting + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    default:
                        lambdaCom.lbX.openAdefault();
                        //txtCom.AppendText("W-Open A Default" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                }
                txtBoxDialog.AppendText(" Shutter A: Opened " + shutterMode + "mode " + Environment.NewLine);
            }
            return;
        }

        private void radioOpenB_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioOpenB.Checked)
            {
                string shutterMode = txtModeB.Text.ToString();
                byte ndSetting = (byte)decNDB.Value;
                switch (shutterMode)
                {
                    case "Fast":
                        lambdaCom.lbX.openB();
                        //txtCom.AppendText("W-Open B fast" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "Soft":
                        lambdaCom.lbX.openBSoft();
                        //txtCom.AppendText("W-Open B soft" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "ND Mode":
                        lambdaCom.lbX.openB(ndSetting);
                        //txtCom.AppendText("W-Open B ND" + ndSetting + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "Default Mode":
                        lambdaCom.lbX.openBdefault();
                        //txtCom.AppendText("W-Open B Default" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    default:
                        lambdaCom.lbX.openBdefault();
                        //txtCom.AppendText("W-Open B Default" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                }
                txtBoxDialog.AppendText(" Shutter B: Opened " + shutterMode + "mode " + Environment.NewLine);
            }
            return;
        }

        private void radioOpenC_MouseClick(object sender, MouseEventArgs e)
        {
            if (lambdaCom.isOpen() && radioOpenC.Checked)
            {
                string shutterMode = txtModeC.Text.ToString();
                byte ndSetting = (byte)decNDC.Value;
                switch (shutterMode)
                {
                    case "Fast":
                        lambdaCom.lbX.openC();
                        //txtCom.AppendText("W-Open C Fast" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "Soft":
                        lambdaCom.lbX.openCSoft();
                        //txtCom.AppendText("W-Open C Soft" + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "ND Mode":
                        lambdaCom.lbX.openC(ndSetting);
                        //txtCom.AppendText("W-Open C ND" + ndSetting  + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    case "Default Mode":
                        lambdaCom.lbX.openCdefault();
                        //txtCom.AppendText("W-Open C Default"  + Environment.NewLine);
                        lambdaCom.readPort();
                        break;
                    default:
                        lambdaCom.lbX.openCdefault();
                        lambdaCom.readPort();
                        break;
                }
                txtBoxDialog.AppendText(" Shutter C: Opened " + shutterMode + "mode " + Environment.NewLine);
            }
            return;
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            // exit application
            lambdaCom.closeCom();
            Application.Exit();
        }

        /*This function is not reliable.  Most likly on the DG-4 side?
               private void btnAdjust_MouseClick(object sender, MouseEventArgs e)
               {
                   byte filter = (byte)decFilterToAdjust.Value;
                   int steps = (int)decAdjustSteps.Value;
                   lambdaCom.writeByte(filter);
                   txtBoxDialog.AppendText(" position " + filter + Environment.NewLine);
                   Thread.Sleep(10);//One way to add a delay
                   lambdaCom.writeByte(234);//Start nuetral desity adjustment
                   Thread.Sleep(10);//One way to add a delay
                   for (int i = 0; i < steps; i++)
                   {
                       if (radioInc.Checked)
                       { //Increment by one}
                           lambdaCom.writeByte(byteCom.byteOpenC);
                       }
                       else
                       { //Decrament by one}
                           lambdaCom.writeByte(236);
                       }
                       Thread.Sleep(10);//One way to add a delay
                       lambdaCom.writeByte(byteCom.byteOpenA);
                   }
                   Thread.Sleep(10);//One way to add a delay
                   lambdaCom.writeByte(byteCom.byteCloseC);//End nuetral desity adjustment
                   if (radioInc.Checked)
                   { //Increment by one}
                       txtBoxDialog.AppendText(" position " + filter + " incremented by: " + steps +Environment.NewLine);
                   }
                   else
                   { //Decrament by one}
                       txtBoxDialog.AppendText(" position " + filter + " decremented by: " + steps + Environment.NewLine);
                   }
                   lambdaCom.writeByte(219);
               }*/
    }
}
