using System;
using System.Collections.Generic;
using System.Text;

namespace BlackJackServer
{
    class Ronda
    {
        public int NumeroRonda { get; set; }
        public List<Cartas> Cartas { get; set; }
        public int Puntos { get; set; }
    }
}
