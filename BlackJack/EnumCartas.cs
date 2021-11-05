using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text;

namespace BlackJackServer
{
    enum EnumCartas
    {
        [EnumMember(Value = "2")]
        Dos = 2,
        [EnumMember(Value = "3")]
        Tres = 3,
        [EnumMember(Value = "4")]
        Cuatro = 4,
        [EnumMember(Value = "5")]
        Cinco = 5,
        [EnumMember(Value = "6")]
        Seis = 6,
        [EnumMember(Value = "7")]
        Siete = 7,
        [EnumMember(Value = "8")]
        Ocho = 8,
        [EnumMember(Value = "9")]
        Nueve = 9,
        [EnumMember(Value = "J")]
        J = 'J',
        [EnumMember(Value = "Q")]
        Q = 'Q',
        [EnumMember(Value = "K")]
        K = 'K',
        [EnumMember(Value = "A")]
        AZ = 1
    }
}
  enum EnumPalo
  { pica, 
    trebol,
    corazones, 
    diamante 
 };
