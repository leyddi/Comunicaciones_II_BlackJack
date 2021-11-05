using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization;

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
        private static List<Cartas> Mazo = new List<Cartas>();
        private static List<Ronda> RondasCrepier = new List<Ronda>();
        private static int NumRonda = 0;

        static void Main(string[] args)
        {


            GenerarMazo();
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

                Ronda();

            }
        }
        public static Cartas ExtraerCartaMazo() {
            Random r = new Random();
            
            int numero = r.Next(0, 48);
            while (Mazo[numero].Entregada) {
                numero = r.Next(0, 48);
            }
            Cartas cartas = new Cartas { 
            Entregada = true,
            Palo = Mazo[numero].Palo,
            Valor = Mazo[numero].Valor
            };
            Mazo[numero].Entregada = true;

                return cartas;
        }

        public static void Ronda()
        {
            string mensajeEnviar;
            Mensaje objMensajeEnviar;

            //SE GENERA LA PRIMERA CARTA PARA EL CREPIER
            Cartas cartasJugador = ExtraerCartaMazo();
            Ronda rondaServidor = new Ronda {
                NumeroRonda = NumRonda
            };
            rondaServidor.Cartas = new List<Cartas>();
            rondaServidor.Cartas.Add(cartasJugador);
            RondasCrepier.Add(rondaServidor);

            Console.WriteLine("Carta Crepier" + cartasJugador.Valor);
            objMensajeEnviar = new Mensaje { Tipo = EnumMessage.ValorMensaje.Comunicaciones, Valor = "Carta Crepier "+ cartasJugador.Valor };
            mensajeEnviar = JsonConvert.SerializeObject(objMensajeEnviar);
            BroadCastAll(mensajeEnviar);

            int i = 0;
            foreach (Cliente cliente in Clientes) {
                Ronda ronda = new Ronda();
                ronda.Cartas = new List<Cartas>();

                if (Clientes[i].Rondas == null)
                {
                    Clientes[i].Rondas = new List<Ronda>();
                }

                //PRIMERA CARTA
                cartasJugador = ExtraerCartaMazo();
                ronda.Cartas.Add(cartasJugador);

                //SEGUNDA CARTA
                Cartas cartasJugador2 = ExtraerCartaMazo();
                ronda.Cartas.Add(cartasJugador2);

                Clientes[i].Rondas.Add(ronda);

                objMensajeEnviar = new Mensaje { Tipo = EnumMessage.ValorMensaje.MisPrimerasDosCartas, Valor = NumRonda+"##"+cartasJugador.Valor+"##"+cartasJugador2.Valor };
                mensajeEnviar = JsonConvert.SerializeObject(objMensajeEnviar);
                Unicast(mensajeEnviar, cliente.tcpClient);
                i++;
            }
            int j = 0;
            foreach (Cliente cliente in Clientes)
            {
                objMensajeEnviar = new Mensaje { Tipo = EnumMessage.ValorMensaje.NotificarTurno, Valor = NumRonda.ToString()};
                mensajeEnviar = JsonConvert.SerializeObject(objMensajeEnviar);
                Unicast(mensajeEnviar, cliente.tcpClient);
                while (Clientes[j].Rondas[NumRonda].Plantado) {
                    
                }
                j++;
            }
        }

        public static void PedirCarta(Cliente cliente) {
            Cartas cartasJugador = ExtraerCartaMazo();
            string mensajeEnviar;
            Mensaje objMensajeEnviar;

            objMensajeEnviar = new Mensaje { Tipo = EnumMessage.ValorMensaje.Pedir, Valor = NumRonda + "##" + cartasJugador.Valor};
            mensajeEnviar = JsonConvert.SerializeObject(objMensajeEnviar);
            Unicast(mensajeEnviar, cliente.tcpClient);
        }

        public static void GenerarMazo() {

            foreach (EnumPalo palo in (EnumPalo[])Enum.GetValues(typeof(EnumPalo))) {
                foreach (EnumCartas cartas in (EnumCartas[])Enum.GetValues(typeof(EnumCartas))) {
                    var enumType = typeof(EnumCartas);
                    var str = GetEnumMemberAttrValue(enumType, cartas);

                    Cartas Carta = new Cartas
                    {
                        Valor = str.ToString(),
                        Palo = palo.ToString(),
                        Entregada = false
                        
                    };
                    Mazo.Add(Carta);
                }
            }
        }
        public static string GetEnumMemberAttrValue(Type enumType, object enumVal)
        {
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo[0].GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();
            if (attr != null)
            {
                return attr.Value;
            }

            return null;
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
                        if (mensaje.Tipo == EnumMessage.ValorMensaje.Pedir)
                        {
                            Console.WriteLine("El jugador " + Clientes.Find(x => x.tcpClient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint).Usuario + " ha pedido una nueva carta");

                            PedirCarta(Clientes.Find(x => x.tcpClient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint));
                        }
                        if (mensaje.Tipo == EnumMessage.ValorMensaje.Plantar) {
                            Console.WriteLine("El jugador " + Clientes.Find(x => x.tcpClient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint).Usuario + " se ha plantado");

                            Clientes.Find(x => x.tcpClient.Client.RemoteEndPoint == tcpClient.Client.RemoteEndPoint).Rondas[NumRonda].Plantado = true;
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
