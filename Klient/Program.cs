using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klient
{
    class Program
    {
        const int PORT_NUMBER = 7;
        static int TCP_PORT_NUMBER;
        static int i = 0;
        static IPAddress MULTICAST_ADDRESS = IPAddress.Parse("239.0.0.222");
        static IPEndPoint REMOTE_ENDPOINT = new IPEndPoint(MULTICAST_ADDRESS, PORT_NUMBER);
        static IPEndPoint LISTEN_IP = new IPEndPoint(IPAddress.Any, PORT_NUMBER);
        static bool activeClient = false;

        static int frequencyValue = 0;
        static Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Unspecified);
        static TcpListener tcpListener = new TcpListener(IPAddress.Parse(getLocalHostIp(false)), TCP_PORT_NUMBER);
        static void Main(string[] args)
        {

            Console.WriteLine("podaj nr portu do polaczenia tcp/ip");
            TCP_PORT_NUMBER = int.Parse(Console.ReadLine()); while (!activeClient)
                connectWithUDPclient();

            connectTCPClient();
            Console.ReadKey();


        }

        private static void connectWithUDPclient()
        {

            UdpClient udpClient = new UdpClient();


            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(LISTEN_IP);
            udpClient.JoinMulticastGroup(MULTICAST_ADDRESS);
            Byte[] buffer = null;

            Console.WriteLine("listen on IP " + MULTICAST_ADDRESS.ToString() + " and port " + PORT_NUMBER);
            Console.WriteLine("moje ip = " + getLocalHostIp(true));
            string data = "";
            AddressFamily oldIpClient = new AddressFamily();



            if ((buffer = udpClient.Receive(ref LISTEN_IP)) != null)
            {
                oldIpClient = udpClient.Client.AddressFamily;
                Console.WriteLine("size of buffer " + buffer.Length);
                data = Encoding.ASCII.GetString(buffer);

                if (data.Equals("DISCOVER"))
                    activeClient = true;
                Console.WriteLine("MAM KLIENTA nr" + i);
                i++;


                Byte[] ipByte = Encoding.ASCII.GetBytes(getLocalHostIp(true).ToString());
                udpClient.Send(ipByte, ipByte.Length, new IPEndPoint(MULTICAST_ADDRESS, PORT_NUMBER));
                Console.WriteLine("i sended IP " + getLocalHostIp(true) + " data = " + data);

                buffer = null;

            }



            Console.WriteLine("============KOniec===========");
            udpClient.Close();
            activeClient = true;


        }

        private static String getLocalHostIp(bool port)
        {

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            if (port)
                return
                    Convert.ToString(ipHostInfo.AddressList.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork)) + ":" + TCP_PORT_NUMBER;
            else
                return
                   Convert.ToString(ipHostInfo.AddressList.FirstOrDefault(address => address.AddressFamily == AddressFamily.InterNetwork));
        }



        private static void connectTCPClient()
        {
            tcpListener.Start();
            TcpClient cli = null;
            Console.WriteLine("TCp CLIENT START +++++ ");
            string response = "";


            while (true)
            {
          
                    cli = tcpListener.AcceptTcpClient();
             
                NetworkStream stream = cli.GetStream();

                byte[] buffer = new byte[32];
                stream.Read(buffer, 0, 32);
                response = Encoding.ASCII.GetString(buffer).Trim();

                if (response.Contains("NICK:"))
                {
                    Console.WriteLine("uzytkownik :" + response.Split(':')[1]);
                }

                else if (response.Contains("FREQ:"))
                {
                    frequencyValue = int.Parse(response.Split(':')[1]);
                }

                else
                {
                    Console.WriteLine(response);
                }

                Console.WriteLine("dupa dupa dupa ");
            }

            
        }
    }
}
