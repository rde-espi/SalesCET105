using System;
using System.Collections.Generic;
using System.Text;

namespace ProjetoFinalCet105.Mobile.Models;

public class NovaMarcacaoRequest
{
    public int FuncionarioId { get; set; }

    public int ServicoId { get; set; }

    public DateTime DataHoraInicio { get; set; }

    public string? Observacoes { get; set; }

    public string? PromoCode { get; set; }
}
