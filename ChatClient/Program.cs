using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace ChatClient
{
    class Program
    {

        static string _guestName = "";

        public static string GuestName { get => _guestName; set => _guestName = value; }

        static void Main(string[] args)
        {
            Console.Title = "Chat Application";

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 10000;
            TcpClient client = new TcpClient();

            client.Connect(ip, port);

            NetworkStream ns = client.GetStream();

            // Initialize guest name
            GuestName = getGuestName(ns);



            // Starts a new thread for each client
            Thread thread = new Thread(o => ReceiveData((TcpClient)o));
            thread.Start(client);

            // Welcome the user
            Console.WriteLine($"Welcome {GuestName}!");

            string input;

            // Listens to keyboard input
            while (!string.IsNullOrEmpty((input = Console.ReadLine())))
            {
                    ClearCurrentConsoleLine();
                    writeToStream($"CHAT:{GuestName}>{input}");
            }

            // Notifies the users when someone leave the room
            writeToStream($"EXIT:{GuestName}");


            client.Client.Shutdown(SocketShutdown.Send);
            thread.Join();
            ns.Close();
            client.Close();
            Console.ReadKey();


            // Helper function: broadcast when the user left the room
            void writeToStream(string data)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data);
                ns.Write(buffer, 0, buffer.Length);
            }

        }

        // Invoked when data is sent to the stream, it serves as an active connection between client(s) and the server
        static void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {

                // Converts the bytestream to string
                var data = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);

                // Splits the message type and the actual message
                var parsedData = data.Split(':');
                string code = parsedData[0];
                string payload = parsedData[1];

                // Removed unwanted carriage return and new line
                payload = payload.TrimEnd('\r', '\n');
                if (code == "EXIT")
                {
                    if (GuestName != payload)
                    {
                        Console.WriteLine($"\n({payload} has left the room.)\n");
                    } else
                    {
                        Console.WriteLine($"\n(You left the room.)\nPress 'enter' to quit.");
                    }
                }

            }

        }

        // Generate a temporary guest name based on the current number of users
        static string getGuestName(NetworkStream ns)
        {
            byte[] receivedBytes = new byte[1024];
            int byte_count;

            byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length);

            // Converts the bytestream to string
            var data = Encoding.ASCII.GetString(receivedBytes, 0, byte_count);

            Console.WriteLine(data);

            // Splits the message type and the actual message
            var parsedData = data.Split(':');

           

            string code = parsedData[0];
            string payload = parsedData[1].TrimEnd('\r', '\n'); ;

            return $"{payload}";
        }


        public static void ClearCurrentConsoleLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }



    }
}
