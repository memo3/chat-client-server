using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace ChatServer
{
    class Program
    {
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();

        static List<string> userList = new List<string>();


        static void Main(string[] args)
        {
            int count = 1;

            TcpListener ServerSocket = new TcpListener(IPAddress.Any, 10000);
            ServerSocket.Start();

            Console.WriteLine($"Server started: {IPAddress.Any}:10000");

            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                lock (_lock) list_clients.Add(count, client);


                // When the client is connected, broadcast the current number of user(s)
                //broadcast($"NUMBER:{list_clients.Count}");

                broadcast($"NEW_GUEST:GUEST{list_clients.Count.ToString("D3")}");
                Console.WriteLine($"GUEST{list_clients.Count} has joined the room");
                

                Thread t = new Thread(handle_clients);
                t.Start(count);
                count++;
            }
        }

        public static int getUserCount()
        {
            return list_clients.Count;
        }

        public static void handle_clients(object o)
        {


            int id = (int)o;
            TcpClient client;

            lock (_lock) client = list_clients[id];

            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int byte_count = stream.Read(buffer, 0, buffer.Length);

                if (byte_count == 0)
                {
                    break;
                }
                
                // Converts the bytestream to string
                var data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                broadcast(data);

                // Splits the message type and the actual message
                var parsedPayload = data.Split(':');

                //Console.WriteLine(payload);

                string code = parsedPayload[0];
                string payload = parsedPayload[1];
                
            }

            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        public static void broadcast(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
