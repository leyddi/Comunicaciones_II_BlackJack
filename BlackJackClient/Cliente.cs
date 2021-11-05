using System;
using System.Collections.Generic;
using System.Text;

namespace BlackJackClient
{
    class Cliente
    {
        public string Usuario { get; set; }
        public bool Conectado { get; set; }
        public List<Ronda> Rondas { get; set; }

    }
}
