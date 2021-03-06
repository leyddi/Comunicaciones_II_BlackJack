using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using Newtonsoft.Json;

namespace BlackJackClient
{
    class Program
    {
        private static Cliente cliente = new Cliente();
        private static TcpClient tcpClient;
        private static StreamWriter sWriter;
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine(@".------..------..------..------..------..------..------..------..------.
|B.--. ||L.--. ||A.--. ||C.--. ||K.--. ||J.--. ||A.--. ||C.--. ||K.--. |
| :(): || :/\: || (\/) || :/\: || :/\: || :(): || (\/) || :/\: || :/\: |
| ()() || (__) || :\/: || :\/: || :\/: || ()() || :\/: || :\/: || :\/: |
| '--'B|| '--'L|| '--'A|| '--'C|| '--'K|| '--'J|| '--'A|| '--'C|| '--'K|
`------'`------'`------'`------'`------'`------'`------'`------'`------'");
                Console.WriteLine("");

                Console.WriteLine("BIENVENIDO JUGADOR");
                Console.WriteLine("");

                Console.WriteLine("Deberás completar ciertos datos para poder iniciar el juego");
                Console.WriteLine("");
                Console.WriteLine("Ingresa dirección IP o DNS del servidor al cual deseas conectarte: ");
                string ip = Console.ReadLine();
                Console.WriteLine("Ingresa el puerto del servidor: ");
                string puerto = Console.ReadLine();
                Console.WriteLine("Ingresa tu nombre de jugador: ");
                string usuario = Console.ReadLine();
                cliente.Usuario = usuario;
                Console.WriteLine("Ingresa número de mesa: ");
                string mesa = Console.ReadLine();

                Console.WriteLine("");
                Console.WriteLine("Espera un momento... ");


                tcpClient = new TcpClient(ip, int.Parse(puerto));

                Console.WriteLine("");
                Console.WriteLine("Se ha establecido la conexión con el servidor");

                

                Thread thread = new Thread(Read);
                thread.Start(tcpClient);
                sWriter = new StreamWriter(tcpClient.GetStream());
                //sWriter.WriteLine(mesa);
                //sWriter.Flush();

                Mensaje mensaje = new Mensaje
                {
                    Tipo = EnumMessage.ValorMensaje.SolicitarUnirse,
                    Valor = cliente.Usuario + "##" + mesa
                };

                if (tcpClient.Connected)
                {
                    //Solicitud de unirse
                    sWriter.WriteLine(JsonConvert.SerializeObject(mensaje));
                    sWriter.Flush();
                    Console.WriteLine("Espera un momento... ");
                    Thread.Sleep(5000);
                

                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            Console.ReadKey();
        }

        static void Read(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            StreamReader sReader = new StreamReader(tcpClient.GetStream());

            while (true)
            {
                try
                {
                    string message = sReader.ReadLine();
                    Mensaje mensaje = JsonConvert.DeserializeObject<Mensaje>(message);
                    if (mensaje.Tipo == EnumMessage.ValorMensaje.Comunicaciones)
                    {
                        Console.WriteLine(mensaje.Valor);
                    }
                    if (mensaje.Tipo == EnumMessage.ValorMensaje.NoAdmitido)
                    {
                        Console.WriteLine("Lo siento no has sido admitido. Adios! ");
                        cliente.Conectado = false;
                        tcpClient.Close();
                        break;
                    }
                    if (mensaje.Tipo == EnumMessage.ValorMensaje.Admitido)
                    {
                        cliente.Conectado = true;
                        Console.WriteLine("Bienvenido a la mesa, espera se unan mas participantes");
                    }
                    if (mensaje.Tipo == EnumMessage.ValorMensaje.MisPrimerasDosCartas)
                    {
                        string[] valor = mensaje.Valor.Split("##");
                        Console.WriteLine("Tus Primeras dos cartas son: "+ valor[1]+" , "+ valor[2] );
                        if (cliente.Rondas == null) {
                            cliente.Rondas = new List<Ronda>();
                        }
                        Ronda ronda = new Ronda { 
                        NumeroRonda = int.Parse(valor[0])                        
                        };

                        ronda.Cartas = new List<Cartas>();
                        Cartas carta = new Cartas {Valor = valor[1] };
                        ronda.Cartas.Add(carta);
                        carta = new Cartas { Valor = valor[2] };
                        ronda.Cartas.Add(carta);

                        cliente.Rondas.Add(ronda);
                    }
                    if (mensaje.Tipo == EnumMessage.ValorMensaje.Pedir)
                    {
                        string[] valor = mensaje.Valor.Split("##");

                        Cartas carta = new Cartas { Valor = valor[1] };
                        cliente.Rondas.Find(x=> x.NumeroRonda==int.Parse(valor[0])).Cartas.Add(carta);
                        Console.WriteLine("Tus siguiente carta es: " + valor[1] + ", tienes las siguientes cartas en mano: " + String.Join(",", cliente.Rondas[int.Parse(valor[0])].Cartas.Select(u => u.Valor)));

                    }
                    if (mensaje.Tipo == EnumMessage.ValorMensaje.NotificarTurno)
                    {
                        Console.WriteLine("¿Deseas Pedir (Ingresa Si) o Deseas Plantarte (Ingresa No)?");

                        string respuesta = Console.ReadLine();
                        while (!respuesta.ToLower().Contains("si") && !respuesta.ToLower().Contains("no")) {
                            Console.WriteLine("Solo se admite Si o No como respuesta");
                            respuesta = Console.ReadLine();
                        }
                        Mensaje mensajeEnviar;
                        if (respuesta == "si")
                        {
                            mensajeEnviar = new Mensaje
                            {
                                Tipo = EnumMessage.ValorMensaje.Pedir,
                                Valor = cliente.Rondas.Count()+ "##" + cliente.Usuario
                            };
                        }
                        else {
                            mensajeEnviar = new Mensaje
                            {
                                Tipo = EnumMessage.ValorMensaje.Plantar,
                                Valor = cliente.Rondas.Count() + "##" + cliente.Usuario
                            };
                        }
                        sWriter.WriteLine(JsonConvert.SerializeObject(mensajeEnviar));
                        sWriter.Flush();
                    }
                    if (mensaje.Tipo == EnumMessage.ValorMensaje.FinJuego)
                    {
                        Console.WriteLine("Presione cualquier tecla para finalizar el juego");
                        Console.ReadLine();
                        break;
                    }
                }

                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                    break;
                }

            }
        }
    }
}
