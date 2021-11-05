# OpenFTM - File Transfer Module
Ever found the process of moving a file to a server complicated or difficult? With OpenFTM you can move files without third-party servers or cdn servers. Just install <code>ftmd</code> on the server and <code>ftm</code> on the client and done! Now you can upload files easily to your server!
# Installation
<li><strong>OpenFTM - ftmd (for servers)</strong></li><br>
Setp 1 - Download the <code>ftmd</code> app from the releases tab with the version that you like and the os that you need. You cand download with <code>wget <download_link></code>          for linux servers. <br><br>
Setp 2 - The <code>ftmd</code> app requieres elevated privilages (root for linux | administartor for windows). Run <code>chmod 777 ./ftmd</code> to give the app the appropriate privilages (for linux). After that, run <code>sudo ./ftmd config</code> (linux) | <code>./ftmd config</code> (Administartor Powershell Windows). After completintg the setup, the app and it's config files will be moved to <code>/etc/ftmd</code> (linux) | <code>C:\ProgramFiles (x86)\ftmd</code>. <br><br>
Setp 3 - cd (Change Directory) into the ftmd directory (mentioned at step2) and start it in the background while also keeping a log file with the following command: <code>nohup sudo ./ftmd &</code>. This will start the app as a <code>daemon</code> and append the log to <code>nohup.out</code>. In <code>nohup.out</code> you will see all the logins and failed logins and all the files sent and received. After starting the app, the server will pe listening on port 84 for incoming <code>ftm</code> connections. Congratulations yo have successfully started ftmd!<br><br>
<h1>Usage</h1>
Step 1 - Download <code>ftm</code> from the releases tab for the os you have and prefer. Open powershell or the terminal in linux and give it the appropriate permissions (for linux <code>chmod 777 ./ftmd</code>) and run <code>./ftm <ip_address> <local_file_path> <destination_directory></code>. Example:<br>
  
```text
 ~$ ./ftm 10.0.0.1 "C:\Users\<user>\Desktop\test.txt" /home/<user>/Desktop
OpenFTM - ftm (C) MishDotCom. All rights reserved.

ftm: Enter 10.0.0.1's password: <password>
ftm: Successfully authenticated! Sending file...
ftm: Sent file data packet...
ftm: Begin sending file...
ftm: File transfer completed
```
<h1>Contribution</h1>
Help me work on more projects like this. Bitcoin accepted here ❤️<br>
btc public address : <code>GWP6Jbz4uZPhTr34b7WULviR5UMcruoGB</code><br>
<img align="center" src="https://user-images.githubusercontent.com/79113564/140509285-c6fda55d-26fe-4a3e-af66-3e064e52c8d0.jpg" width="200">
