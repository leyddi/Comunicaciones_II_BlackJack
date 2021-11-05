using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace BlackJackServer
{
    class Program
    {
        private static TcpListener tcpListener;
        

        //private static List<TcpClient> tcpClientsList = new List<TcpClient>();
        private static string Mesa;
        private static List<Cliente> Clientes = new List<Cliente>();
        private static bool closeMoreClients = false;
        private static DateTime fechaPrevioInicio;
        private static System.Timers.Timer aTimer = new System.Timers.Timer();

        static void Main(string[] args)
        {

            

            Random r = new Random();
            Console.WriteLine("BIENVENIDO A BLACK JACK - MODO SERVIDOR.");
            Console.WriteLine("Deberás completar ciertos datos para poder iniciar el juego");
            Console.WriteLine("");

            Console.WriteLine("Crea un número de mesa: " );
            Mesa = Console.ReadLine();


            tcpListener = new TcpListener(IPAddress.Any, 5000);
            tcpListener.Start();

            Console.WriteLine("Invita a otros jugadores a conectarse usando el Puerto: "+ 5000+ " y la IP 127.0.0.1");


            System.Timers.Timer Timer = new System.Timers.Timer();
            Timer.Interval = 2000;

            while (!closeMoreClients )
            {
                if (!tcpListener.Pending())
                {
                    Thread.Sleep(500); // choose a number (in milliseconds) that makes sense
                    continue; // skip to next iteration of loop
                }
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                    Cliente cliente = new Cliente();
                    cliente.tcpClient = tcpClient;
                    Clientes.Add(cliente);


                    if (Clientes.Count == 2)
                    {
                        Console.WriteLine("Se cierra la mesa en 20 Segundos:");
                        fechaPrevioInicio = DateTime.Now;
                        aTimer.Interval = 22000;

                        // Hook up the Elapsed event for the timer. 
                        aTimer.Elapsed += OnTimedEvent;
                        // Start the timer
                        aTimer.Enabled = true;

                    }
                    Thread thread = new Thread(ClientListener);
                    thread.Start(tcpClient);


                
            }
                        

            if (closeMoreClients) {
                Console.WriteLine("Inicia la ronda");
                Mensaje objMensajeEnviar = new Mensaje { Tipo = EnumMessage.ValorMensaje.Comunicaciones, Valor = "Inicia el Juego " };
                string mensajeEnviar = JsonConvert.SerializeObject(objMensajeEnviar);
                BroadCastAll(mensajeEnviar);
            }
        }
        private static void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            
            if ((DateTime.Now - fechaPrevioInicio).Seconds >20)
            {
                closeMoreClients = true;
                aTimer.Enabled = false;
                tcpListener.Stop();
            }                  
        }
        public static void ClientListener(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            StreamReader reader = new StreamReader(tcpClient.GetStream());

            Console.WriteLine("Client connected");

            while (true)
            {
                if (tcpClient.Connected)
                {
                    string readerText = reader.ReadLine();
                    if (!string.IsNullOrEmpty(readerText))
                    {

                        Mensaje mensaje = JsonConvert.DeserializeObject<Mensaje>(readerText);

                        if (mensaje.Tipo == EnumMessage.ValorMensaje.SolicitarUnirse)
                        {
                            String[] valores = mensaje.Valor.Split("##");
                            if (valores[1].ToString() != Mesa)
                            {
                                Mensaje objMensajeEnviar = new Mensaje { Tipo = EnumMessage.ValorMensaje.NoAdmitido };
                                string mensajeEnviar = JsonConvert.SerializeObject(objMensajeEnviar);
                                Unicast(mensajeEnviar, tcpClient);
                                Console.WriteLine("Un cliente ha intentado conectarse pero no ha enviado la mesa correcta");
                            }
                            else
                            {
                                Cliente cli = Clientes.Find(x => x.tcpClient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint);

                                if (cli != null && cli.Usuario == null)
                                {
                                    Clientes.Find(x => x.tcpClient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint).Usuario = valores[0];
                                    Mensaje objMensajeEnviar = new Mensaje { Tipo = EnumMessage.ValorMensaje.Admitido };
                                    string mensajeEnviar = JsonConvert.SerializeObject(objMensajeEnviar);
                                    Unicast(mensajeEnviar, tcpClient);

                                    objMensajeEnviar = new Mensaje { Tipo = EnumMessage.ValorMensaje.Comunicaciones, Valor = "El jugador " + valores[0] + " se ha unido a la mesa" };
                                    mensajeEnviar = JsonConvert.SerializeObject(objMensajeEnviar);
                                    Console.WriteLine("El jugador " + valores[0] + " se ha unido a la mesa");
                                    BroadCast(mensajeEnviar, tcpClient);

                                }
                            }
                        }
                    }
                }
            }
        }

        public static void BroadCast(string msg, TcpClient excludeClient)
        {
            foreach(Cliente cliente in Clientes)
            {
                if (cliente.tcpClient != excludeClient)
                {
                    StreamWriter sWriter = new StreamWriter(cliente.tcpClient.GetStream());
                    sWriter.WriteLine(msg);
                    sWriter.Flush();
                }
            }
            
            
        }
        public static void BroadCastAll(string msg)
        {
            foreach (Cliente cliente in Clientes)
            {
               
                    StreamWriter sWriter = new StreamWriter(cliente.tcpClient.GetStream());
                    sWriter.WriteLine(msg);
                    sWriter.Flush();
            }


        }
        public static void Unicast(string msg, TcpClient Client)
        {
            foreach (Cliente cliente in Clientes)
            {
                if (cliente.tcpClient == Client)
                {
                    StreamWriter sWriter = new StreamWriter(cliente.tcpClient.GetStream());
                    sWriter.WriteLine(msg);
                    sWriter.Flush();
                }
            }


        }
    }

}
