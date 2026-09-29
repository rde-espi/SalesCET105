using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoFinalCet105.Mobile.Models
{
    public class UpdateMarcacaoRequest
    {
        public int ServicoId { get; set; }

        public DateTime DataHoraInicio { get; set; }

        public string? Observacoes { get; set; }
    }
}
