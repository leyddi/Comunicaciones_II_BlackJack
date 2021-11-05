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
            CartaCrepier = 2,
            MisPrimerasDosCartas = 3,
            OtrosJugadoresPrimerasDosCartas = 4,
            NotificarTurno = 5,
            Pedir = 6,
            Plantar = 7,
            Ganador = 8,
            Perdedor = 9,
            Comunicaciones = 99

        }

    }
}
