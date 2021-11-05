using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace BlackJackServer
{
    class Cliente
    {
        public string Usuario { get; set; }
        public string Ip { get; set; }
        public List<Ronda> Rondas { get; set; }
        public TcpClient tcpClient { get; set; }
        
    }
}
