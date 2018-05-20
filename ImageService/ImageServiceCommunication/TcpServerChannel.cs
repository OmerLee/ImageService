﻿
using ImageServiceInfrastructure.Event;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ImageServiceCommunication
{
    public class TcpServerChannel
    {
        private readonly int _port;
        private readonly string _ip;
        private TcpListener _listener;
        public static List<ClientHandler> Clients;
        public event EventHandler<NewClientEventArgs> NewHandler;
        private bool _running;


        public TcpServerChannel(int port, string ip)
        {
            this._port = port;
            _ip = ip;
            _running = true;
            Clients = new List<ClientHandler>();
        }

        public void Close()
        {
            //to dooo
        }

        public void Start()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(_ip), _port);
            this._listener = new TcpListener(ep);

            this._listener.Start();

            Console.WriteLine("Waiting for connections...");

            Task task = new Task(() =>
            {

                while (_running)
                {
                    try
                    {
                        TcpClient client = this._listener.AcceptTcpClient();
                        Console.WriteLine("Client Connected");
                        ClientHandler newclient = new ClientHandler(client);
                        newclient.Start();
                        Clients.Add(newclient);
                        System.Threading.Thread.Sleep(500);
                        NewHandler?.Invoke(this, new NewClientEventArgs(client));
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(e.Message);

                    }
                }
            });
            task.Start();
        }

        public void SendToAll(string data)
        {
            Task task = new Task(() =>
            {
                foreach (ClientHandler ch in Clients)
                {
                    try
                    {
                        //writerMut.WaitOne();
                        ch.Writer.Write(data);
                        //writerMut.ReleaseMutex();
                    }
                    catch (Exception e)
                    {
                        Clients.Remove(ch);
                        ch.Client().Close();
                    }
                }
                Console.WriteLine("write " + data);
            });
            task.Start();
        }

    }
}