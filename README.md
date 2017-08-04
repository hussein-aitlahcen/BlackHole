[![Build status](https://ci.appveyor.com/api/projects/status/sbpyroacrb52hb57?svg=true)](https://ci.appveyor.com/project/hussein-aitlahcen/blackhole)

# BlackHole
- C# RAT (Remote Adminitration Tool) 
- Educational purposes only

# Technologies
- protobuf-net : message serialization/deserialization in slave<->master protocol
- NetMQ : network library used for the slave<->master system (client<->server)
- Lz4.Net : compression library
- ILMerge : packaging assemblies into a single excecutable

# Working feature(s)
- Reverse connection (extremly simple)
- Slaves management (connection/disconnection)
- Remote file browser
- Remote file downloader
- Remote file execution
- Remote desktop (quality + fps)
- Cancelable download/upload
- Remote webcam capture
- Password recovery from differents sources

# How it works
We pack the slave into a single .net executable with ILMerge, then we create the according C++ file with the PayloadBuilder. Finally, we build the Loader. When the target start the loader, it will load the CLR and dynamically load the packed Slave from its memory (managed C# executed inside C++ launcher).

# Getting started
1. Build BlackHole.Slave (Will automaticly be packed with its dependencies using ILMerge)
2. Launch BlackHole.PayloadBuilder (will create a C++ file containing the packed slave in binary format)
3. Build BlackHole.Loader 
4. Enjoy delivering a single C++ executable

# Main window
![alt text](https://github.com/hussein-aitlahcen/BlackHole/raw/master/doc/images/blackhole_main_window.jpg "MainWindow")

# File manager
![alt text](https://github.com/hussein-aitlahcen/BlackHole/raw/master/doc/images/blackhole_filemanager_window.jpg "FileManager")

# Downloading command
![alt text](https://github.com/hussein-aitlahcen/BlackHole/raw/master/doc/images/blackhole_download_command.jpg "Downloading")

# Remote desktop
![alt text](https://github.com/hussein-aitlahcen/BlackHole/raw/master/doc/images/blackhole_remote_desktop.jpg "Remote desktop")
