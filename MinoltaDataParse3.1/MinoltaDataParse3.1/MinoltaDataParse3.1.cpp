// MinoltaDataParse3.1.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
// CODE AUTHORED BY: SHAWHIN TALEBI AND YICHAO ZHANG
// THE UNIVERSITY OF TEXAS AT DALLAS
// MULTI-SCALE INTEGRATED SENSING AND SIMULATION (MINTS)
// 5/31/2019

#include <stdio.h>
#include "pch.h"
#include "windows.h"
#include "direct.h"
#include "includeFile/CLAPI.h"
#include "includeFile/CLColorConditions.h"
#include "includeFile/CLConditions.h"
#include "includeFile/ErrorDefine.h"
#include "includeFile/TypeDefine.h"
#include "includeFile/Version.h"
#include "includeFile/Version.h"
#include "includeFile/stdafx.h"
#include <string>
#include <sstream>
#include <iostream>
#include <fstream>
#include <chrono>
#include <ctime>
#include <iomanip>

using namespace std;
bool RemoteOffClose(DEVICE_HANDLE hDevice);
bool Close(DEVICE_HANDLE hDevice);
void Measure();
void writeData();
void execute();
void Calibration();
char* getMinute();
void checkCalibration();
void testAutoPowerOff();
void printtime();

CL_MEASDATA Evxydata;		// intialize data store for EVXY data
CL_MEASDATA Evuvdata;		// intialize data store for Evuv data
CL_MEASDATA EvTduvdata;		// intialize data store for EVXYTduv data
CL_MEASDATA EvDWPedata;		// intialize data store for EvDWPe data
CL_MEASDATA XYZdata;		// intialize data store for XYZ data
CL_MEASDATA Renderingdata;	// intialize data store for Rendering data
CL_MEASDATA PWdata;			// intialize data store for PW data
CL_MEASDATA SPCdata;		// intialize data store for Spc data
CL_MEASDATA SCOTOPICdata;	// intialize data store for Scotopic data
CL_SYSTEMSETTING sys;		// initialize system setting data store
ofstream ofile;
class high_resolution_clock;

int main()
{
	int c;

	printf(" Press enter key to start program.\n");
	c = getchar();

	execute();
}


void testAutoPowerOff() {
	DEVICE_HANDLE hDevice = NULL; // The definition of the object handle for instrument-1

	// Get the object handle for each instrument
	ER_CODE ret = CLOpenDevice(&hDevice);
	ER_CODE ans = CLGetSystemSetting(hDevice, CL_SYSTEM_AUTOPOWEROFF, &sys);

	printf("%d", sys.AutoPowerOff);

}

void execute() {	
	Calibration();
	while (TRUE) {
		checkCalibration();
		Measure();
		writeData();
	}
}

void printtime() {
	auto timenow = chrono::system_clock::to_time_t(chrono::system_clock::now());
	char time_str[30];
	ctime_s(time_str, sizeof time_str, &timenow);
	printf("%s", time_str);
}

void checkCalibration() {
	auto timenow = chrono::system_clock::to_time_t(chrono::system_clock::now());
	char time_str[30];
	ctime_s(time_str, sizeof time_str, &timenow);

	char minute_str[3];
	char second_str[3];
	strncpy_s(minute_str, sizeof minute_str, time_str + 14, sizeof minute_str - 1);
	strncpy_s(second_str, sizeof second_str, time_str + 17, sizeof second_str - 1);

	int minute = atoi(minute_str);
	int second = atoi(second_str);
	// calibrate every 30 minutes
	if (minute % 30 == 0 && second < 30){ // constrain second to avoid duplicate
		Calibration();
	}
}

void Calibration(){
	printtime();

	// Close Shutter
	system("lamtestClose.exe");
	SLEEP(2000);

	printf("Call Zero Calibration:\n");
	// The definition of the object handle
	DEVICE_HANDLE hDevice = NULL; // The definition of the object handle for instrument-1
	
	// Get the object handle for each instrument
	printf("Call CLOpenDevice");
	ER_CODE ret = CLOpenDevice(&hDevice);
	if (ret != SUCCESS) {
		printf("... Failed.\n");
		printf("%d",ret);
		return;
	}
	printf("... Succeeded.\n");

	// Set the remote mode ON for each instrument
	printf("Call CLSetRemoteMode");
	ret = CLSetRemoteMode(hDevice, CL_RMODE_ON);
	if (ret != SUCCESS) {
		printf("... Failed.\n");
		CLCloseDevice(hDevice);
		return;
	}
	printf("... Succeeded.\n");

	// Perform zero calibration for each instrument
	printf("Call CLDoCalibration");
	ret = CLDoCalibration(hDevice);
	if (ret != SUCCESS) {
		printf("... Failed.\n");
		CLCloseDevice(hDevice);
		return;
	}
	printf("... Succeeded.\n");

	// Confirm the implementation status of zero calibration
	CL_CALIBMEASSTATUS calStatus = CL_CALIBMEAS_FREE; // Implementation status for instrument-1
	bool bFinish = false; // Completion flag for instrument-1
	
	// Confirm whether all instruments are completed a zero calibration
	while (!bFinish) {
		// Confirm the measurement status after waiting for a moment
		SLEEP(1000);

		printf(".");
		ret = CLPollingCalibration(hDevice, &calStatus);
		if (ret != SUCCESS) {
			printf("... Failed.\n");
			CLCloseDevice(hDevice);
			return;
		}
		if (calStatus == CL_CALIBMEAS_FINISH) bFinish = true;
	}
	printf("Complete the zero calibration\n");
	// turn remote mode off
	RemoteOffClose(hDevice);

	// Open Shutter
	system("lamtestOpen.exe");
	SLEEP(2000);

	printtime();
}

// SET REMOTE MODE OFF
bool RemoteOffClose(DEVICE_HANDLE hDevice) {

	ER_CODE ret;

	printf("Call CLSetRemoteMode(OFF)");
	ret = CLSetRemoteMode(hDevice, CL_RMODE_OFF);
	if (ret != SUCCESS) {
		printf("... Failed. \n");
		return false;

	}
	printf("... Succeeded.\n");

	if (Close(hDevice) == false) {
		return false;
	}

	return true;
}

// RELEASE THE DEVICE HANDLE
bool Close(DEVICE_HANDLE hDevice) {
	ER_CODE ret;

	printf("Call CLCloseDevice");
	ret = CLCloseDevice(hDevice);
	if (ret != SUCCESS) {
		printf("... Failed.\n");
		return false;
	}
	printf("... Succeeded.\n");
	return true;
}

// DEFINE MEASURE DATA FUNCTION
void Measure() {

	printf("Taking Measurement...\n");

	DEVICE_HANDLE hDevice;			// define device handle of object
	ER_CODE ret;					// initialize ret

	printf("Call CLOpenDevice");
	ret = CLOpenDevice(&hDevice);
	if (ret != SUCCESS) {
		printf("... Failed.\n");
		return;
	}
	printf("... Succeeded.\n");

	// set the remote mode on
	printf("Call CLSetRemoteMode(ON)");
	ret = CLSetRemoteMode(hDevice, CL_RMODE_ON);
	if (ret != SUCCESS) {
		printf("... Failed.\n");
		CLCloseDevice(hDevice);
		return;
	}
	printf("... Succeeded.\n");

	// set illuminant unit setting
	printf("Call CLSetProperty(CL_PR_ILLUNIT)");
	ret = CLSetProperty(hDevice, CL_PR_ILLUNIT, CL_ILLUNIT_LX);
	if (ret != SUCCESS) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");

	int32_km time;	// initialize time variable
	printf("Call CLDoMeasurement()");
	ret = CLDoMeasurement(hDevice, &time);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");

	// polling until completing measurement
	CL_MEASSTATUS status = CL_MEAS_FREE;		// initialize measurement status
	while (status != CL_MEAS_FINISH) {
		SLEEP(1000);
		printf("Call CLpollingMeasure()");
		ret = CLPollingMeasure(hDevice, &status);
		if (ret != SUCCESS) {
			printf("... Failed.\n");
			RemoteOffClose(hDevice);
			return;
		}
		printf("... Succeeded. (status:%d)\n", status);
	}

	// get measured data for Evxy
	printf("Call CLGetMeasData(CL_COLORSPACE_EVXY)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_EVXY, &Evxydata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get measured data for Evuv
	printf("Call CLGetMeasData(CL_COLORSPACE_EVUV)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_EVUV, &Evuvdata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get measured data for EvTduv
	printf("Call CLGetMeasData(CL_COLORSPACE_EVTCPDUV)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_EVTCPDUV, &EvTduvdata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get measured data for EvDWPe
	printf("Call CLGetMeasData(CL_COLORSPACE_EVDWPE)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_EVDWPE, &EvDWPedata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get measured data for XYZ
	printf("Call CLGetMeasData(CL_COLORSPACE_XYZ)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_XYZ, &XYZdata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get measured data for Rendering
	printf("Call CLGetMeasData(CL_COLORSPACE_RENDERING)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_RENDERING, &Renderingdata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get measured data for Pw
	printf("Call CLGetMeasData(CL_COLORSPACE_PW)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_PW, &PWdata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get measured data for SPC
	printf("Call CLGetMeasData(CL_COLORSPACE_SPC)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_SPC, &SPCdata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get measured data for SCOTOPIC
	printf("Call CLGetMeasData(CL_COLORSPACE_SCOTOPIC)");
	ret = CLGetMeasData(hDevice, CL_COLORSPACE_SCOTOPIC, &SCOTOPICdata);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");
	// get system data (specifically dateime)
	printf("Call CLGetSystemSetting()");
	ret = CLGetSystemSetting(hDevice, CL_SYSTEM_DATETIME, &sys);
	if (ret > WARNING) {
		printf("... Failed.\n");
		RemoteOffClose(hDevice);
		return;
	}
	printf("... Succeeded.\n");


	// turn remote mode off
	RemoteOffClose(hDevice);
}

// int to string with fixed length=2
string to_string_length_2(int num) {
	if (num < 10){
		return "0" + to_string(num);
	}
	else {
		return to_string(num);
	}
}

// WRITE DATA TO FILE
void writeData() {

	time_t now = time(0);
	tm  local_tm;
	localtime_s(&local_tm, &now); // should set the system time to UTC

	// convert measurements from numbers to strings
	string year_str = to_string(1900+local_tm.tm_year);//sys.Datetime.Year);
	string month_str = to_string_length_2(1+local_tm.tm_mon);//sys.Datetime.Month);
	string day_str = to_string_length_2(local_tm.tm_mday);//sys.Datetime.Day);
	string hour_str = to_string_length_2(local_tm.tm_hour);//sys.Datetime.Hour);
	string minute_str = to_string_length_2(local_tm.tm_min);//sys.Datetime.Minute);
	string second_str = to_string_length_2(local_tm.tm_sec);//sys.Datetime.Second);

	string Ev_str = to_string(Evxydata.Evxy.Ev);
	string x_str = to_string(Evxydata.Evxy.x);
	string y_str = to_string(Evxydata.Evxy.y);
	string u_str = to_string(Evuvdata.Evuv.u);
	string v_str = to_string(Evuvdata.Evuv.v);
	string T_str = to_string(EvTduvdata.EvTduv.T);
	string duv_str = to_string(EvTduvdata.EvTduv.duv);
	string DW_str = to_string(EvDWPedata.EvDWPe.DW);
	string Pe_str = to_string(EvDWPedata.EvDWPe.Pe);
	string X_str = to_string(XYZdata.XYZ.X);
	string Y_str = to_string(XYZdata.XYZ.Y);
	string Z_str = to_string(XYZdata.XYZ.Z);
	string Pw_str = to_string(PWdata.Pw.PeakWave);
	string Es_str = to_string(SCOTOPICdata.Scotopic.Es);
	string SP_str = to_string(SCOTOPICdata.Scotopic.SP);

	char *user_path;
	size_t size_path;
	errno_t err = _dupenv_s(&user_path, &size_path, "USERPROFILE");
	if (err) {
		printf("%s\n", "[Error]: Wrong Path");
	}
	else {
		// printf("%s\n", user_path);
	}
	string path_str;
	path_str = user_path;
	
	// create directory /Users/teamlary4 replaced with ..
	//string path = "/Users/MINI PC/Box Sync/Minolta/Data/" + year_str + "/";
	string path = path_str + "/Desktop/Minolta/10004098/" + year_str + "/";
	_mkdir(path.c_str());
	path = path + month_str + "/";
	_mkdir(path.c_str());
	path = path + day_str + "/";
	_mkdir(path.c_str());

	// open outfile (file to write to)
	//ofile.open("/Users/MINI PC/Box Sync/Minolta/Data/" + year_str + "/" + month_str + "/" + day_str + "/Minolta_" + year_str + "_" + month_str + "_" + day_str + "_" + hour_str + ".csv", ios::app);
	ofile.open(path_str + "/Desktop/Minolta/10004098/" + year_str + "/" + month_str + "/" + day_str + "/MINTS_Minolta_10004098_" + year_str + "_" + month_str + "_" + day_str + "_" + hour_str + ".csv", ios::app);

	// check if file to write to is empty if so write header to it
	//ifstream ifile("/Users/MINI PC/Box Sync/Minolta/Data/" + year_str + "/" + month_str + "/" + day_str + "/Minolta_" + year_str + "_" + month_str + "_" + day_str + "_" + hour_str + ".csv");
	ifstream ifile(path_str + "/Desktop/Minolta/10004098/" + year_str + "/" + month_str + "/" + day_str + "/MINTS_Minolta_10004098_" + year_str + "_" + month_str + "_" + day_str + "_" + hour_str + ".csv");


	if (ifile.peek() == std::ifstream::traits_type::eof()) {
		ofile << "Date, Time, Illuminance, x chromaticity, y chromaticity, u chromaticity, v chromaticity, Tcp, delta uv,";
		ofile << "Dom. Wavelength, Excit Purity, X Tristimulus, Y Tristimulus, Z Tristimulus,";
		for (int i = 0; i < 16; i++) {
			string i_str = to_string(i);
			ofile << "Rend[" << i_str << "],";
		}
		ofile << "Peak Wave,";
		for (int i = 0; i < IRRADIANCE_LEN; i++) {
			string i_str = to_string(i);
			ofile << "Spectrum[" << i_str << "],";
		}
		ofile << "Scotopic Lux," << "S/P Ratio";
		ofile << endl;
	}
	// close read file
	ifile.close();

	// write data to outfile
	ofile << year_str + "/" + month_str + "/" + day_str + "," + hour_str + ":" + minute_str + ":" + second_str + ",";
	ofile << Ev_str + "," + x_str + "," + y_str + ",";
	ofile << u_str + "," + v_str + "," + T_str + "," + duv_str + ",";
	ofile << DW_str + "," + Pe_str + "," + X_str + "," + Y_str + "," + Z_str + ",";
	for (int i = 0; i < 16; i++) {
		string Rend = to_string(Renderingdata.Rendering.Data[i]);
		ofile << Rend + ",";
	}
	ofile << Pw_str << ",";
	for (int i = 0; i < IRRADIANCE_LEN; i++) {
		string SPC = to_string(SPCdata.Spc.Data[i]);
		ofile << SPC << ",";
	}
	ofile << Es_str << "," << SP_str;
	ofile << endl;

	// close outfile
	ofile.close();
}



