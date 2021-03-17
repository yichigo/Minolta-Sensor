using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;//For delay
using System.Windows.Forms;//For forms elements

namespace Lamtest
{
    public class wheelTestFunctions : byteCommands
    {
        private lambdaSerial lambdaCom;//create comport object
        byteCommands byteCom;
        Random random;
        private byte speedByte = 1;//0 is a bad speed for most filter wheel loads
        private byte filterByte = 0;
        private byte moveByte = 0;
        private string wheel, controller, wheelAConfig, wheelBConfig, wheelCConfig, LB10B_SB;

        public wheelTestFunctions(lambdaSerial com)//com object passed to VF5fuctions object
       {//Got it!
           //http://forum.codecall.net/topic/35009-c-calling-parent-functions-from-child-form/
            this.lambdaCom = com;//set reference of comport child to serial parent;
            byteCom = new byteCommands();
            random = new Random();
       }

        public void testWheel()
        {
            string testType = "fixedTest";
            string status = lambdaCom.getConfig();
            lambdaCom.userInterface.stopProcess= false;
            int errors, topSpeed, lastSpeed, cycleCount, delay;
            //this.speedByte = 1;
            delay = int.Parse(lambdaCom.userInterface.txtDelay.Text.ToString());
            topSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtTopSpeed.Text.ToString());
            lastSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtLastSpeed.Text.ToString());
            cycleCount = int.Parse(lambdaCom.userInterface.txtNumSteps.Text.ToString());
            wheel = lambdaCom.userInterface.txtTestMode.Text.ToString();
            if (wheel == "LB10-3 Batch") { controller = lambdaCom.getController(); }
            if (controller == "LB10-3" || controller == "LB10-B")
            {
                wheelAConfig = lambdaCom.getWheelA(status);
            }
            if (controller == "LB10-3")
            {
                wheelBConfig = lambdaCom.getWheelB(status);
                wheelCConfig = lambdaCom.getWheelC(status);
            }
            //Speed loop
            for (int s = topSpeed; s <= lastSpeed; s++)
            {
                Application.DoEvents();//Need to process the close com event while in the loop.
                if (lambdaCom.isOpen() == false || lambdaCom.userInterface.stopProcess == true) { break; }
                //char sCheck = Convert.ToChar(s);//toString(s);
                delay = int.Parse(lambdaCom.userInterface.txtDelay.Text.ToString());
                delay = getDelay(s, delay);
                errors = myRandom(cycleCount, delay, s, testType);
                lambdaCom.userInterface.txtBoxDialog.AppendText(errors + " Errors at Speed " + s.ToString() + Environment.NewLine);
                //Branch for LB10-3 batch errors
                if (errors > 0 && wheel == "LB10-3 Batch")
                {
                    if (wheelAConfig != "N.A." && wheelAConfig != "WA-NC")
                    {
                        wheel = "Wheel A";
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Errors on batch mode.  Testing wheel A. " + Environment.NewLine);
                        errors = myRandom(cycleCount, delay, s, testType);
                        lambdaCom.userInterface.txtBoxDialog.AppendText(wheel + "  " + errors + " Errors at Speed " + s.ToString() + Environment.NewLine);
                    }
                    if (wheelBConfig != "N.A." && wheelAConfig != "WB-NC")
                    {
                        wheel = "Wheel B";
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Errors on batch mode.  Testing wheel B. " + Environment.NewLine);
                        errors = myRandom(cycleCount, delay, s, testType);
                        lambdaCom.userInterface.txtBoxDialog.AppendText(wheel + "  " + errors + " Errors at Speed " + s.ToString() + Environment.NewLine);
                    }
                    if (wheelCConfig != "N.A." && wheelAConfig != "WC-NC")
                    {
                        wheel = "Wheel C";
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Errors on batch mode.  Testing wheel C. " + Environment.NewLine);
                        errors = myRandom(cycleCount, delay, s, testType);
                        lambdaCom.userInterface.txtBoxDialog.AppendText(wheel + "  " + errors + " Errors at Speed " + s.ToString() + Environment.NewLine);
                    }
                    wheel = "LB10-3 Batch";
                }
                if (errors > 0 && wheel == "LB10-2 Batch")
                {
                    wheel = "Wheel A";
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Errors on batch mode.  Testing wheel A. " + Environment.NewLine);
                    errors = myRandom(cycleCount, delay, s, testType);
                    lambdaCom.userInterface.txtBoxDialog.AppendText(wheel + "  " + errors + " Errors at Speed " + s.ToString() + Environment.NewLine);
                    wheel = "Wheel B";
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Errors on batch mode.  Testing wheel B. " + Environment.NewLine);
                    errors = myRandom(cycleCount, delay, s, testType);
                    lambdaCom.userInterface.txtBoxDialog.AppendText(wheel + "  " + errors + " Errors at Speed " + s.ToString() + Environment.NewLine);
                    wheel = "LB10-2 Batch";
                }
            }
            lambdaCom.userInterface.stopProcess = false;
            //Exit = false;
        }
        public void btnHS4Test()
        {
            lambdaCom.userInterface.stopProcess = false;
            int errors, topSpeed, lastSpeed, cycleCount, delay, filter1, filter2;
            errors = 0;
            delay = int.Parse(lambdaCom.userInterface.txtDelay.Text.ToString());
            speedByte = 0;
            topSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtTopSpeed.Text.ToString());
            lastSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtLastSpeed.Text.ToString());
            cycleCount = int.Parse(lambdaCom.userInterface.txtNumSteps.Text.ToString());
            filter1 = (int)lambdaCom.userInterface.filter1UpDown.Value;
            filter2 = (int)lambdaCom.userInterface.filter2UpDown.Value;
            wheel = lambdaCom.userInterface.txtTestMode.Text.ToString();
            string status = lambdaCom.getConfig();
            controller = lambdaCom.getController();
            if (controller == "LB10-3" || controller == "LB10-B")
            {
                wheelAConfig = lambdaCom.getWheelA(status);
            }
            if (controller == "LB10-3")
            {
                wheelBConfig = lambdaCom.getWheelB(status);
                wheelCConfig = lambdaCom.getWheelC(status);
            }
            //Speed loop
            for (int s = topSpeed; s <= lastSpeed; s++)
            {
                Application.DoEvents();
                if (lambdaCom.isOpen() == false || lambdaCom.userInterface.stopProcess == true) { break; }//escape
                //char sCheck = Convert.ToChar(s);//toString(s);
                errors = myRandom(cycleCount, delay, s, filter1, filter2, "fixedDelay");
                if (errors > 0)
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText(errors + " Errors at speed " + s.ToString() + ". At Delay " + delay + Environment.NewLine);
                }
                else
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText("A goog speed is:" + Environment.NewLine);
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Speed " + s.ToString() + " at  delay " + delay.ToString() + Environment.NewLine);
                }
                errors = 0;
            }
            lambdaCom.userInterface.stopProcess = false;
            return;
        }
        public void fixedTest()
        {
            lambdaCom.userInterface.stopProcess = false;
            int errors, topSpeed, lastSpeed, cycleCount, delay, filter1, filter2;
            errors = 0;
            delay = int.Parse(lambdaCom.userInterface.txtDelay.Text.ToString());
            speedByte = 0;
            topSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtTopSpeed.Text.ToString());
            lastSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtLastSpeed.Text.ToString());
            cycleCount = int.Parse(lambdaCom.userInterface.txtNumSteps.Text.ToString());
            filter1 = (int)lambdaCom.userInterface.filter1UpDown.Value;
            filter2 = (int)lambdaCom.userInterface.filter2UpDown.Value;
            wheel = lambdaCom.userInterface.txtTestMode.Text.ToString();
            controller = lambdaCom.getController();
            string status = lambdaCom.getConfig();
            if (lambdaCom.userInterface.txtComPort.Text.ToString() == "LPT1" || lambdaCom.userInterface.txtComPort.Text.ToString() == "LPT2")
            {
                lambdaCom.userInterface.txtBoxDialog.AppendText("The error detection does not work on the parallel port!" + Environment.NewLine);
            }
            if (controller == "LB10-3" || controller == "LB10-B")
            {
                wheelAConfig = lambdaCom.getWheelA(status);
            }
            if (controller == "LB10-3")
            {
                wheelBConfig = lambdaCom.getWheelB(status);
                wheelCConfig = lambdaCom.getWheelC(status);
            }
            //Speed loop
            for (int s = topSpeed; s <= lastSpeed; s++)
            {
                if (lambdaCom.isOpen() == false || lambdaCom.userInterface.stopProcess == true) { return; }//escape
                errors = myRandom(cycleCount, delay, s, filter1, filter2, "fixedDelay");
                if (errors > 0)
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Speed " + s.ToString() + " is not a good speed at delay-" + delay.ToString() + " . " + Environment.NewLine);
                }
                else
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText("A goog speed is:" + Environment.NewLine);
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Speed " + s.ToString() + " at  delay " + delay.ToString() + Environment.NewLine);
                }
                //delay = 10;
                errors = 0;
            }
            lambdaCom.userInterface.stopProcess = false;
            return;
        }
        public void randomTest()
        {
            string testType = "randomDelay";
            string status = lambdaCom.getConfig();
            lambdaCom.userInterface.stopProcess = false;
            int errors, topSpeed, lastSpeed, cycleCount, delay;
            errors = 0;
            delay = 10;
            speedByte = 0;
            topSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtTopSpeed.Text.ToString());
            lastSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtLastSpeed.Text.ToString());
            cycleCount = int.Parse(lambdaCom.userInterface.txtNumSteps.Text.ToString());
            wheel = lambdaCom.userInterface.txtTestMode.Text.ToString();
            controller = lambdaCom.getController();
            if (controller == "LB10-3" || controller == "LB10-B")
            {
                wheelAConfig = lambdaCom.getWheelA(status);
            }
            if (controller == "LB10-3")
            {
                wheelBConfig = lambdaCom.getWheelB(status);
                wheelCConfig = lambdaCom.getWheelC(status);
            }
            //Speed loop
            for (int s = topSpeed; s <= lastSpeed; s++)
            {
                Application.DoEvents();//Need to process the close com event while in the loop.
                if (lambdaCom.isOpen() == false || lambdaCom.userInterface.stopProcess == true) { break; }
                do
                {
                    errors = this.myRandom(cycleCount, delay, s, testType);
                    if (errors > 0)
                    {
                        delay = delay + 10;
                    }
                } while (errors > 0 && delay < 200 && lambdaCom.userInterface.stopProcess == false);
                if (errors > 0)
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Speed " + s.ToString() + " is not a good speed. " + Environment.NewLine);
                }
                else
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText("A goog speed is:" + Environment.NewLine);
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Speed " + s.ToString() + " at  delay " + delay.ToString() + Environment.NewLine);
                }
                delay = 10;
                errors = 0;
            }
            lambdaCom.userInterface.stopProcess = false;
            return;
        }
        public void seqTest()
        {//move to whel test functions
            lambdaCom.userInterface.stopProcess = false;
            int errors, topSpeed, lastSpeed, cycleCount, delay, filter1, filter2;
            errors = 0;
            delay = 0;
            speedByte = 0;
            topSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtTopSpeed.Text.ToString());
            lastSpeed = (int)lambdaCom.getSpeedByte(lambdaCom.userInterface.txtLastSpeed.Text.ToString());
            cycleCount = int.Parse(lambdaCom.userInterface.txtNumSteps.Text.ToString());
            filter1 = (int)lambdaCom.userInterface.filter1UpDown.Value;
            filter2 = (int)lambdaCom.userInterface.filter2UpDown.Value;
            lambdaCom.userInterface.txtBoxDialog.AppendText("cycleCount: " + cycleCount + " filter1-" + filter1 + " filter2-" + filter2 + "  " + Environment.NewLine);
            wheel = lambdaCom.userInterface.txtTestMode.Text.ToString();
            controller = lambdaCom.getController();
            string status = lambdaCom.getConfig();
            if (lambdaCom.userInterface.txtComPort.Text.ToString() == "LPT1" || lambdaCom.userInterface.txtComPort.Text.ToString() == "LPT2")
            {
                lambdaCom.userInterface.txtBoxDialog.AppendText("The error detection does not work on the parallel port!" + Environment.NewLine);
            }
            if (controller == "LB10-3" || controller == "LB10-B")
            {
                wheelAConfig = lambdaCom.getWheelA(status);
            }
            if (controller == "LB10-3")
            {
                wheelBConfig = lambdaCom.getWheelB(status);
                wheelCConfig = lambdaCom.getWheelC(status);
            }
            //Speed loop
            for (int s = topSpeed; s <= lastSpeed; s++)
            {
                Application.DoEvents();//Need to process the close com event while in the loop.
                if (lambdaCom.isOpen() == false || lambdaCom.userInterface.stopProcess == true) { break; }
                if (lambdaCom.isOpen() == false || lambdaCom.userInterface.stopProcess == true) { return; }//escape
                errors = lambdaCom.wheelTest.myRandom(cycleCount, delay, s, filter1, filter2, "seqRandom");
                if (errors > 0 || delay > 200)
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Speed " + s.ToString() + " is not a good speed at delay-" + delay.ToString() + " . " + Environment.NewLine);
                    errors = 0;
                    delay = delay + 10;
                    s--;
                }
                else
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText("A goog speed is:" + Environment.NewLine);
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Speed " + s.ToString() + " at  delay " + delay.ToString() + Environment.NewLine);
                    delay = 10;
                }
                //delay = 10;
                errors = 0;
            }
            lambdaCom.userInterface.stopProcess = false;
        }
        private int myRandom(int cycleCount, int delay, int s, string testType)
        {//move to lbXFunctions
            int totalTime;//used to calculate an error.
            int errors = 0;
            int oldErrors = 0;
            int counter = 0;
            int lastMove = 0;
            speedByte = (byte)lambdaCom.getByte(s);
            for (int i = 0; i < cycleCount; i++)
            {
                do//You do NOT want to send the same command out twice  
                {//especially to the LB10-2
                    filterByte = (byte)random.Next(0, 9);//(0,9)
                } while (filterByte == lastMove);

                moveByte = (byte)lambdaCom.getMoveByte(speedByte, filterByte);
                if (i == 0) { counter = 0; }
                lambdaCom.lbX.moveMyWheel(wheel, speedByte, filterByte);
                totalTime = getTime();
                errors = getErrors(controller, totalTime, s, errors);
                //lambdaCom.userInterface.txtBoxDialog.AppendText("Errors: " + errors + Environment.NewLine);
                counter++;
                //lambdaCom.userInterface.txtBoxDialog.AppendText(lastMove.ToString() + " to " + filterByte.ToString() + " at " + totalTime + "ms " + Environment.NewLine);
                if (errors > oldErrors)
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Error at Speed " + s.ToString() + " from " + lastMove.ToString());
                    lambdaCom.userInterface.txtBoxDialog.AppendText(" to " + filterByte.ToString() + " at " + totalTime + "ms " + Environment.NewLine);
                    lambdaCom.userInterface.txtBoxDialog.AppendText("Controller=" + lambdaCom.userInterface.controller + Environment.NewLine);

                    oldErrors = errors;
                }
                if (errors > 0 && testType == "randomDelay")
                {
                    i = cycleCount;
                    //break;
                }
                lambdaCom.userInterface.txtMoves.Text = counter.ToString();
                lastMove = filterByte;
                Thread.Sleep(delay);//One way to add a delay
                if (lambdaCom.userInterface.txtComPort.Text == "LPT1" || lambdaCom.userInterface.txtComPort.Text == "LPT2") { Thread.Sleep(delay * s); }
                Application.DoEvents();//Need to process the close com event while in the loop.
                if (lambdaCom.isOpen() == false || lambdaCom.userInterface.stopProcess == true) { break; }
            }
            return errors;
        }

        public int myRandom(int cycleCount, int delay, int s, int filter1, int filter2, string testType)
        {
            int totalTime;//used to calculate an error.
            int errors = 0;
            int oldErrors = 0;
            int counter = 0;
            int lastMove = 0;
            byte f1 = (byte)filter1;
            byte f2 = (byte)filter2;
            filterByte = f1;
            speedByte = (byte)lambdaCom.getByte(s);
            for (int i = 1; i <= cycleCount; i++)
            {
                Application.DoEvents();//Need to process the close com event while in the loop.
                if (lambdaCom.isOpen() == false || lambdaCom.userInterface.stopProcess == true) { break; }
                if ((i % 2) == 0 && testType != "seqRandom")
                {
                    filterByte = f1;
                }
                if ((i % 2) != 0 && testType != "seqRandom")
                {
                    filterByte = f2;
                }
                if (testType == "seqRandom")
                {
                    if (filterByte < f2 && (filterByte + 1) >= f1) { filterByte++; }
                    else { filterByte = f1; }
                }
                moveByte = (byte)lambdaCom.getMoveByte(speedByte, filterByte);
                if (i == 1) { counter = 0; }
                lambdaCom.lbX.moveMyWheel(wheel, speedByte, filterByte);
                totalTime = getTime();
                errors = getErrors(controller, totalTime, s, errors);
                lambdaCom.userInterface.txtBoxDialog.AppendText("errors: " + errors + Environment.NewLine);
                if (errors > oldErrors && testType == "fixedDelay")
                {
                    lambdaCom.userInterface.txtBoxDialog.AppendText(errors + " Errors at speed " + s.ToString() + " from " + lastMove.ToString());
                    lambdaCom.userInterface.txtBoxDialog.AppendText(" to " + filterByte.ToString() + Environment.NewLine);
                    oldErrors = errors;
                }
                if (errors > 0 && (testType == "randomDelay" || testType == "seqRandom"))
                {
                    i = cycleCount;
                }
                counter = i;
                lambdaCom.userInterface.txtMoves.Text = counter.ToString();
                lastMove = filterByte;
                Thread.Sleep(delay);//One way to add a delay
            }
            return errors;
        }
        public void doSequence()//Start Sequence
        {//move to wheel test functions
            //string wheel;
            double ms;
            int totalTime, tic;
            int resolution = 1000;//1 Minute in miliseconds
            int counter = 0;
            tic = resolution;//For incrementing timer
            byte speedByteA, filterByteA, speedByteB, filterByteB;
            int repeat = (int)lambdaCom.userInterface.decMsAdjust.Value;
            lambdaCom.userInterface.stopSeq = false;
            //lambdaCom.writeByte(bytelambdaCom.byteLB103Batch);
            lambdaCom.writeByte(byteCom.byteOpenACond);//open a conditional
            Thread.Sleep(15);
            lambdaCom.writeByte(byteCom.byteOpenBCond);//open a conditional
            //lambdaCom.lbX.closeShutterB();
            //lambdaCom.writeByte(byteCom.byteEndBatch);
            Thread.Sleep(15);
            DateTime startTime, curTime;
            DateTime initTime = DateTime.Now;
            TimeSpan elapsedTime, totTime;
            do
            {
                if (lambdaCom.userInterface.chBoxSkip1.Checked == false && lambdaCom.userInterface.stopSeq == false)
                {
                    elapsedTime = DateTime.Now - DateTime.Now;//0 in timespan
                    //wheel = txtWheelSeq1.Text;
                    speedByteA = (byte)lambdaCom.userInterface.txtSpdSeq1A.SelectedIndex;
                    filterByteA = (byte)lambdaCom.userInterface.txtSeqItem1A.SelectedIndex;
                    speedByteB = (byte)lambdaCom.userInterface.txtSpdSeq1B.SelectedIndex;
                    filterByteB = (byte)lambdaCom.userInterface.txtSeqItem1B.SelectedIndex;
                    ms = (int)lambdaCom.userInterface.decHrSeq1.Value * 3600000;//Convert hours to ms
                    ms += (int)lambdaCom.userInterface.decMinSeq1.Value * 60000;//Convert min to ms;
                    ms += (int)lambdaCom.userInterface.decSecSeq1.Value * 1000;//Convert to sec to ms;
                    lambdaCom.writeByte(byteCom.byteLB103Batch);
                    lambdaCom.lbX.moveMyWheel("Wheel A", speedByteA, filterByteA);
                    lambdaCom.lbX.moveMyWheel("Wheel B", speedByteB, filterByteB);
                    lambdaCom.writeByte(byteCom.byteEndBatch);
                    totalTime = lambdaCom.wheelTest.getTime();//Wait for CR Accounts for errors as well!
                    startTime = DateTime.Now;
                    while (elapsedTime.TotalMilliseconds < ms && lambdaCom.userInterface.nextSeq == false && lambdaCom.userInterface.stopSeq == false)
                    {
                        Thread.Sleep(resolution);
                        tic += resolution;
                        Application.DoEvents();
                        curTime = DateTime.Now;
                        elapsedTime = curTime - startTime;
                        lambdaCom.userInterface.txtTimeSeq1.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
                        totTime = curTime - initTime;
                        lambdaCom.userInterface.txtTotalTime.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", totTime.Hours, totTime.Minutes, totTime.Seconds);
                        curTime = DateTime.Now;
                        elapsedTime = curTime - startTime;//Scope issues
                    }
                    tic = resolution;
                    lambdaCom.userInterface.nextSeq = false;
                }
                if (lambdaCom.userInterface.chBoxSkip2.Checked == false && lambdaCom.userInterface.stopSeq == false)
                {
                    elapsedTime = DateTime.Now - DateTime.Now;//0 in timespan
                    //wheel = txtWheelSeq2.Text;
                    speedByteA = (byte)lambdaCom.userInterface.txtSpdSeq2A.SelectedIndex;
                    filterByteA = (byte)lambdaCom.userInterface.txtSeqItem2A.SelectedIndex;
                    speedByteB = (byte)lambdaCom.userInterface.txtSpdSeq2B.SelectedIndex;
                    filterByteB = (byte)lambdaCom.userInterface.txtSeqItem2B.SelectedIndex;
                    ms = (int)lambdaCom.userInterface.decHrSeq2.Value * 3600000;//Convert hour to ms
                    ms += (int)lambdaCom.userInterface.decMinSeq2.Value * 60000;//Convert min to ms
                    ms += (int)lambdaCom.userInterface.decSecSeq2.Value * 1000;//Convert sec to ms;
                    lambdaCom.writeByte(byteCom.byteLB103Batch);
                    lambdaCom.lbX.moveMyWheel("Wheel A", speedByteA, filterByteA);
                    lambdaCom.lbX.moveMyWheel("Wheel B", speedByteB, filterByteB);
                    lambdaCom.writeByte(byteCom.byteEndBatch);
                    totalTime = lambdaCom.wheelTest.getTime();//Wait for CR Accounts for errors as well!
                    startTime = DateTime.Now;
                    while (elapsedTime.TotalMilliseconds < ms && lambdaCom.userInterface.nextSeq == false && lambdaCom.userInterface.stopSeq == false)
                    {
                        Thread.Sleep(resolution);
                        tic += resolution;
                        Application.DoEvents();
                        curTime = DateTime.Now;
                        elapsedTime = curTime - startTime;
                        lambdaCom.userInterface.txtTimeSeq2.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
                        totTime = curTime - initTime;
                        lambdaCom.userInterface.txtTotalTime.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", totTime.Hours, totTime.Minutes, totTime.Seconds);
                    }
                    tic = resolution;
                    curTime = DateTime.Now;
                    elapsedTime = curTime - startTime;//Scope issues
                    //txtTotalTime.Text = elapsedTime.Seconds.ToString();
                    lambdaCom.userInterface.nextSeq = false;
                }
                Thread.Sleep(20);//give the close shutter command enough time so that they will respond to the open command.
                if (lambdaCom.userInterface.chBoxSkip3.Checked == false && lambdaCom.userInterface.stopSeq == false)
                {
                    elapsedTime = DateTime.Now - DateTime.Now;//0 in timespan
                    //wheel = txtWheelSeq3.Text;
                    speedByteA = (byte)lambdaCom.userInterface.txtSpdSeq3A.SelectedIndex;
                    filterByteA = (byte)lambdaCom.userInterface.txtSeqItem3A.SelectedIndex;
                    speedByteB = (byte)lambdaCom.userInterface.txtSpdSeq3B.SelectedIndex;
                    filterByteB = (byte)lambdaCom.userInterface.txtSeqItem3B.SelectedIndex;
                    ms = (int)lambdaCom.userInterface.decHrSeq3.Value * 3600000;//Convert hours to ms
                    ms += (int)lambdaCom.userInterface.decMinSeq3.Value * 60000;//Convert min to ms
                    ms += (int)lambdaCom.userInterface.decSecSeq3.Value * 1000;//Convert sec to ms;
                    lambdaCom.writeByte(byteCom.byteLB103Batch);
                    lambdaCom.lbX.moveMyWheel("Wheel A", speedByteA, filterByteA);
                    lambdaCom.lbX.moveMyWheel("Wheel B", speedByteB, filterByteB);
                    lambdaCom.writeByte(byteCom.byteEndBatch);
                    totalTime = lambdaCom.wheelTest.getTime();//Wait for CR Accounts for errors as well!
                    startTime = DateTime.Now;
                    while (elapsedTime.TotalMilliseconds < ms && lambdaCom.userInterface.nextSeq == false && lambdaCom.userInterface.stopSeq == false)
                    {
                        Thread.Sleep(resolution);
                        tic += resolution;
                        Application.DoEvents();
                        curTime = DateTime.Now;
                        elapsedTime = curTime - startTime;
                        lambdaCom.userInterface.txtTimeSeq3.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
                        totTime = curTime - initTime;
                        lambdaCom.userInterface.txtTotalTime.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", totTime.Hours, totTime.Minutes, totTime.Seconds);
                    }
                    tic = resolution;
                    curTime = DateTime.Now;
                    elapsedTime = curTime - startTime;//Scope issues
                    //txtTotalTime.Text = elapsedTime.Seconds.ToString();
                    lambdaCom.userInterface.nextSeq = false;
                }
                Thread.Sleep(20);//give the close shutter command enough time so that they will respond to the open command.
                if (lambdaCom.userInterface.chBoxSkip4.Checked == false && lambdaCom.userInterface.stopSeq == false)
                {
                    elapsedTime = DateTime.Now - DateTime.Now;//0 in timespan
                    //wheel = txtWheelSeq4.Text;
                    speedByteA = (byte)lambdaCom.userInterface.txtSpdSeq4A.SelectedIndex;
                    filterByteA = (byte)lambdaCom.userInterface.txtSeqItem4A.SelectedIndex;
                    speedByteB = (byte)lambdaCom.userInterface.txtSpdSeq4B.SelectedIndex;
                    filterByteB = (byte)lambdaCom.userInterface.txtSeqItem4B.SelectedIndex;
                    ms = (int)lambdaCom.userInterface.decHrSeq4.Value * 3600000;//Convert hour to ms
                    ms += (int)lambdaCom.userInterface.decMinSeq4.Value * 60000;//Convert min to ms
                    ms += (int)lambdaCom.userInterface.decSecSeq4.Value * 1000;//Convert sec to ms;
                    lambdaCom.writeByte(byteCom.byteLB103Batch);
                    lambdaCom.lbX.moveMyWheel("Wheel A", speedByteA, filterByteA);
                    lambdaCom.lbX.moveMyWheel("Wheel B", speedByteB, filterByteB);
                    lambdaCom.writeByte(byteCom.byteEndBatch);
                    totalTime = lambdaCom.wheelTest.getTime();//Wait for CR Accounts for errors as well!
                    startTime = DateTime.Now;
                    while (elapsedTime.TotalMilliseconds < ms && lambdaCom.userInterface.nextSeq == false && lambdaCom.userInterface.stopSeq == false)
                    {
                        Thread.Sleep(resolution);
                        tic += resolution;
                        Application.DoEvents();
                        curTime = DateTime.Now;
                        elapsedTime = curTime - startTime;
                        lambdaCom.userInterface.txtTimeSeq4.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
                        totTime = curTime - initTime;
                        lambdaCom.userInterface.txtTotalTime.Text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", totTime.Hours, totTime.Minutes, totTime.Seconds);
                        curTime = DateTime.Now;
                        elapsedTime = curTime - startTime;//Scope issues
                        //txtTotalTime.Text = elapsedTime.Milliseconds.ToString();
                    }
                    tic = resolution;
                    curTime = DateTime.Now;
                    elapsedTime = curTime - startTime;//Scope issues
                    //txtTotalTime.Text = elapsedTime.Seconds.ToString();
                    lambdaCom.userInterface.nextSeq = false;
                }
                Thread.Sleep(20);//give the close shutter command enough time so that they will respond to the open command.
                counter++;
                lambdaCom.userInterface.txtCycle.Text = counter.ToString();
            } while (lambdaCom.userInterface.stopSeq == false && (counter <= repeat || repeat == 99));
            Thread.Sleep(15);
            lambdaCom.lbX.closeShutterA();//Close shutters and reset to default
            Thread.Sleep(15);
            lambdaCom.lbX.closeShutterB();
            lambdaCom.userInterface.stopSeq = false;
        }
         public int getTime()
        {
            int beginTime, endTime, totalTime;
            byte loop = 1;
            beginTime = Environment.TickCount;//returns time in ms
            loop = lambdaCom.readByte();
            while (loop != byteCR && lambdaCom.isOpen())
            {
                //lambdaCom.clearBuffer();
                loop = lambdaCom.readByte();
                if (lambdaCom.userInterface.txtComPort.Text.ToString() == "LPT1" || lambdaCom.userInterface.txtComPort.Text.ToString() == "LPT2")//to deal with LPT
                {
                    if (loop == lambdaCom.readByte()) { loop = byteCR; }
                }
                Application.DoEvents();
            }
            lambdaCom.clearBuffer();//You might read the CR twice if you do not do this!
            lambdaCom.clearBuffer();
            endTime = Environment.TickCount;
            totalTime = endTime - beginTime;
            return totalTime;
        }
        public int getDelay(int s, int delay)
        {
            //lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Method" + delay + Environment.NewLine);
            switch (s)
            {
                case 0:
                    if (lambdaCom.userInterface.chkBoxAddDel0.Checked == true)
                    {
                        delay = delay * (int)lambdaCom.userInterface.DecDelayMultiplier.Value;
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Added. New Delay is: " + delay + "Ms" + Environment.NewLine);
                    }
                    break;
                case 1:
                    if (lambdaCom.userInterface.chkBoxAddDel1.Checked == true)
                    {
                        delay = delay * (int)lambdaCom.userInterface.DecDelayMultiplier.Value;
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Added. New Delay is: " + delay + "Ms" + Environment.NewLine);
                    }
                    break;
                case 2:
                    if (lambdaCom.userInterface.chkBoxAddDel2.Checked == true)
                    {
                        delay = delay * (int)lambdaCom.userInterface.DecDelayMultiplier.Value;
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Added. New Delay is: " + delay + "Ms" + Environment.NewLine);
                    }
                    break;
                case 3:
                    if (lambdaCom.userInterface.chkBoxAddDel3.Checked == true)
                    {
                        delay = delay * (int)lambdaCom.userInterface.DecDelayMultiplier.Value;
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Added. New Delay is: " + delay + "Ms" + Environment.NewLine);
                    }
                    break;
                case 4:
                    if (lambdaCom.userInterface.chkBoxAddDel4.Checked == true)
                    {
                        delay = delay * (int)lambdaCom.userInterface.DecDelayMultiplier.Value;
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Added. New Delay is: " + delay + "Ms" + Environment.NewLine);
                    }
                    break;
                case 5:
                    if (lambdaCom.userInterface.chkBoxAddDel5.Checked == true)
                    {
                        delay = delay * (int)lambdaCom.userInterface.DecDelayMultiplier.Value;
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Added. New Delay is: " + delay + "Ms" + Environment.NewLine);
                    }
                    break;
                case 6:
                    if (lambdaCom.userInterface.chkBoxAddDel6.Checked == true)
                    {
                        delay = delay * (int)lambdaCom.userInterface.DecDelayMultiplier.Value;
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Added. New Delay is: " + delay + "Ms" + Environment.NewLine);
                    }
                    break;
                case 7:
                    if (lambdaCom.userInterface.chkBoxAddDel7.Checked == true)
                    {
                        delay = delay * (int)lambdaCom.userInterface.DecDelayMultiplier.Value;
                        lambdaCom.userInterface.txtBoxDialog.AppendText("Delay Added. New Delay is: " + delay + "Ms" + Environment.NewLine);
                    }
                    break;
            }
            return delay;
        }
        private int getErrors(string controller, int totalTime, int s, int errors)
        {
            //12-14-2013 DELAY TIMES FOR ERRORS INCREASED BY 100MS AT SPEEDS 4-7? No idea why the time went up?
            switch (s)//calculate an error.
            {
                case 0:
                    if (controller != "LB10-2")
                    {
                        if (totalTime > 230)
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        if (controller != "LB10-2" && controller != "NA")
                        {
                            errors++;
                        }
                    }
                    break;
                case 1:
                    if (controller != "LB10-2")
                    {
                        if (totalTime > 380)
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        if (totalTime > 620)//190 or 220
                        {
                            errors++;
                        }
                    }
                    break;
                case 2:
                    if (controller != "LB10-2")
                    {
                        if (totalTime > 530)//220
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        if (totalTime > 730)//300 or 390
                        {
                            errors++;
                        }
                    }
                    break;
                case 3:
                    if (controller != "LB10-2")
                    {
                        if (totalTime > 410)
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        if (totalTime > 810)//350 OR 410
                        {
                            errors++;
                        }
                    }
                    break;
                case 4:
                    if (controller != "LB10-2")
                    {
                        if (totalTime > 490)
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        if (totalTime > 920)//490 OR 520
                        {
                            errors++;
                        }
                    }
                    break;
                case 5:
                    if (totalTime > 320)
                        if (controller != "LB10-2")
                        {
                            if (totalTime > 800)
                            {
                                errors++;
                            }
                        }
                        else
                        {
                            if (totalTime > 1100)//720 OR 1000
                            {
                                errors++;
                            }
                        }
                    break;
                case 6:
                    if (controller != "LB10-2")
                    {
                        if (totalTime > 1000)
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        if (totalTime > 1600)//1300 or 1550
                        {
                            errors++;
                        }
                    }
                    break;
                case 7:
                    if (controller != "LB10-2")
                    {
                        if (totalTime > 2200)
                        {
                            errors++;
                        }
                    }
                    else
                    {
                        if (totalTime > 2200)//2200 or 2700
                        {
                            errors++;
                        }
                    }
                    break;
            }
            return errors;
        }
    }
}
