using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoFinalCet105.Mobile.Models
{
    public class DataDisponivel
    {
        public DateTime Data { get; set; }

        public int QuantidadeHorarios { get; set; }

        public string Dia => Data.ToString("dd");

        public string Mes => Data.ToString("MMM").ToUpper();

        public string DiaSemana => Data.ToString("ddd").ToUpper();

        public string TextoHorarios =>
            QuantidadeHorarios == 1
                ? "1 horário"
                : $"{QuantidadeHorarios} horários";
    }
}
