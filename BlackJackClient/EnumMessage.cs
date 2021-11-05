using System;
using System.Collections.Generic;
using System.Text;

namespace BlackJackClient
{
    class EnumMessage
    {
        public enum ValorMensaje : short
        { 
            NoAdmitido = -1,
            SolicitarUnirse = 0,
            Admitido = 1,
            Ronda = 2,
            OtrosJugadoresCartas = 3,
            MisCartas = 4,
            Pedir = 5,
            Plantar = 6,
            DefinirCarta = 7,
            GanadorRonda = 8,
            GanadorFinal = 9,
            Comunicaciones = 99

        }

    }
}
