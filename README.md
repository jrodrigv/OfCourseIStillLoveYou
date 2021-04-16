<p align="center">
    <a href="https://paypal.me/jrodrigv"><img src="https://img.shields.io/badge/paypal-donate-yellow.svg?style=flat&logo=paypal" alt="PayPal"/></a>
    <a href="https://dev.azure.com/jrodrigv/Personal/_build/latest?definitionId=6&branchName=main"><img src="https://dev.azure.com/jrodrigv/Personal/_apis/build/status/jrodrigv.OfCourseIStillLoveYou?branchName=main" alt="Azure Devops"/></a>
</p>

# OfCourseIStillLoveYou

KSP mod to display hullcam cameras views on different GUI inside or outside the game using a Desktop app and Server app.


## Requirements:
* KSP 1.11.2
* NET 5 runtime https://dotnet.microsoft.com/download/dotnet/5.0
* Latest HullcamVDS https://github.com/linuxgurugamer/HullcamVDSContinued/releases

* Latest Scatterer https://github.com/LGhassen/Scatterer/releases
* TUFX JR edition https://github.com/jrodrigv/TUFX/releases

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

## Mod usage

Take a look to this video tutorial :)
