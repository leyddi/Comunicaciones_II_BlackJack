using System;
using System.Collections.Generic;
using System.Text;

namespace BlackJackServer
{
    class EnumMessage
    {
        public enum ValorMensaje : short
        {
            NoAdmitido = -1,
            SolicitarUnirse = 0,
            Admitido = 1,
            CartaCrepier = 2,
            MisPrimerasDosCartas = 3,
            OtrosJugadoresPrimerasDosCartas = 4,
            Pedir = 5,
            Plantar = 6,
            Ganador = 7,
            Perdedor = 8,
            Comunicaciones = 99
        }

    }
}
