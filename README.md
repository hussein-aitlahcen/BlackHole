# BlackHole
C# RAT (Remote Adminitration Tool)
For study purpose only :)

# Technologies
- protobuf-net -> message serialization/deserialization in slave<->master protocol
- NetMQ -> network library used for the slave<->master system (client<->server)
- Lz4.Net -> compression library
- 
# Working feature(s)
- Reverse connection (extremly simple)
- Slaves management (connection/disconnection)
- Remote file browser
- Remove file downloader

# Next..
- Remote file upload (almost done)
- Trojan persistence
- Reverse connection (fully working, slaves connecting to multiple masters etc...)

