# AnimatedBackground
A glsl shader that runs in windows 10 background

This project is an experiment for using GL shaders on the dekstop background for Windows 10.

### **Warning**
The current version is really early but it works so there are few things to know before starting.
- It may be considered a virus by your antivirus due to tricks I found for handling interactive session without asking user credentials during installation.
- It can take a lot of resources based on your config. it's designed to run on every screen @60fps for the moment.
- Uninstall is not handled properly. It can leave a unused service installed, useless registry keys, and few files in the installation folder.

### **Installation**
- Download the zip file.
https://github.com/seb776/AnimatedBackground/blob/master/SavedBuilds/Debug.zip
- Unzip it
- run setup.exe
- follow installation steps
- Done !

### **How to use it ?**
- It uses a service so for starting / stoping it Open windows "Services" app and look for "LoaderService"
- From there you can start/stop/set autostart the service

For getting back your background you can stop the service then reset your background or reboot.

### **You want to help ?**
**You can leave a message or an issue on the repo.**
Here a standard format for logging issues :
- Copy "test.log" file content from the installation folder
- Did you install in default location ?
- What is your display(s) configuration ? (multiple ?, arrangement, resolutions)
- Did it showed "normally" on your display configuration ?
**Fill free to do pull requests**

