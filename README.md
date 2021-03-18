## Application
In the application, we use "MinoltaDataParse*.exe" to collect data from Minolta sensor.
"MinoltaDataParse*.exe" also calls "LamtestOpen.exe" and "LamtestClose.exe" to control the auto shutter during the calibration every 30 minutes.

### Data Path
The data is collected to the folder: "~/Desktop/Minolta/10004098/"
This fold should be created before running.

## MinoltaDataParse
"MinoltaDataParse*.exe" is generated from the visual studio solution of MinoltaDataParse, which is written in c++.

### Time Zone
The time zone of the PC system should be set to UTC before running the application.
The application read the time from PC system rather than Minolta Sensor, because Minolta sensor's time may not be precise but PC synchronizes the time through internet. 

## LamtestOpen_USB
"LamtestOpen.exe" and "LamtestClose.exe" are generated from the visual studio solution of LamtestOpen_USB, which is written in c#.

### Sutter Instrument Unified Lambda USB Driver
The drive of the auto shutter be downloaded from https://www.sutter.com/SOFTWARE/software_index.html
