# chat-client-server
Simple chat server that should listen on TCP port 10000 for clients. 
The chat protocol is very simple, clients connect with "telnet" and write single lines of text. 
On each new line of text, the server will broadcast that line to all other connected clients
