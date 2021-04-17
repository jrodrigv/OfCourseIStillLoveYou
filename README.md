<p align="center">
    <a href="https://paypal.me/jrodrigv"><img src="https://img.shields.io/badge/paypal-donate-yellow.svg?style=flat&logo=paypal" alt="PayPal"/></a>
    <a href="https://dev.azure.com/jrodrigv/Personal/_build/latest?definitionId=6&branchName=main"><img src="https://dev.azure.com/jrodrigv/Personal/_apis/build/status/jrodrigv.OfCourseIStillLoveYou?branchName=main" alt="Azure Devops"/></a>
     <a href="../../releases"><img src="https://img.shields.io/github/downloads/jrodrigv/OfCourseIStillLoveYou/total.svg?style=flat&logo=github&logoColor=white" alt="Total downloads" /></a>
          <a href="../../releases"><img src="https://img.shields.io/github/release/jrodrigv/OfCourseIStillLoveYou.svg?style=flat&logo=github&logoColor=white" alt="Latest release" /></a>
</p>

# OfCourseIStillLoveYou

KSP mod to display hullcam cameras views on different GUI inside or outside the game using a Desktop app and Server app.

## Requirements:
* KSP 1.11.2
* NET 5 runtime https://dotnet.microsoft.com/download/dotnet/5.0
* Latest HullcamVDS https://github.com/linuxgurugamer/HullcamVDSContinued/releases

## Highly recommended mods:
* Physics Range Extender
* Latest Scatterer https://github.com/LGhassen/Scatterer/releases
* If you want to use TUFX you need to use this version -> TUFX JR edition https://github.com/jrodrigv/TUFX/releases 

## Mod Only Installation:
* Download the zip file for Windows, Linux or Mac.
* Copy the GameData folder into your KSP root folder

## Desktop & Server app setup:
* Unzip the OfCourseIStillLoveYou.Server.zip and OfCourseIStillLoveYou.DesktopClient.zip
* By default the mod, the server and the desktop client will connect to localhost:5077 but you can modify it:
  * Server: *OfCourseIStillLoveYou.Server.exe --endpoint 192.168.1.8  --port 5001* .
  * DesktopClient: Open the settings.json inside OfCourseIStillLoveYou.DesktopClient and modify the endpoint and port.
  * Mod: Inside the mod folder there is a settings.cfg file with the endpoint and port.
* Execute the OfCourseIStillLoveYou.Server.exe first, then OfCourseIStillLoveYou.DesktopClient.exe and finally start KSP

## Running the server as a Docker Container
* Pull the image *docker pull jrodrigv/ofcourseistillloveyou:server_v1.0*
* Create a new container - example overriding endpoint to listen everything and from port 5000: *docker run -d -p 192.168.0.14:5000:5000 ofcourseistillloveyou:server_v1.0 --port 5000 --endpoint 0.0.0.0*

## Mod usage

Take a look to this video tutorial :)

https://youtu.be/OV0Z4xpFYlA
