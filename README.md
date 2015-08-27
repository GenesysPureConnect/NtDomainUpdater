# NtDomainUpdater
An application to bulk change the domain portion of users' NT domain name configuration

# Getting Started
The application doesn't require any install. Just download, extract, and run it.

1. Download the ZIP file from the latest [release](https://github.com/InteractiveIntelligence/NtDomainUpdater/releases).
2. Unzip to your favorite location
3. Run NtDomainUpdater.exe

![screenshot](https://raw.githubusercontent.com/InteractiveIntelligence/NtDomainUpdater/master/resources/screenshot.png)

1. Enter the CIC server, a user with admin rights to modify all users, and the user's password. Check the checkbox to use windows auth instead of entering a username and password.
2. Click _Connect_
3. Enter the existing domain to check for -- do not include the slash!
4. Enter the new domain to use when replacing the old domain
5. Click _Fetch Data_ to preview the changes. The data will populate showing the records that will be modified.
6. Click _Process Changes_ to make the update. This will first execute the Fetch Data process and then process the items. 
7. Each line item will recieve an icon to indicate the result of the change. If an error occured, a tooltip with the error message will be available when hovering over the red X icon.

# Building the application
If you download the code and build it yourself, just update the IceLib references and rebuild in Visual Studio. 

Pull requests are welcome!
