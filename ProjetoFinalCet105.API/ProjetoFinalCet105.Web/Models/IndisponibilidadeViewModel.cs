namespace ProjetoFinalCet105.Web.Models;

public class IndisponibilidadeViewModel
{
    public int Id { get; set; }

    public int FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = string.Empty;

    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }

    public string? Motivo { get; set; }

    public bool DiaCompleto { get; set; }
    public bool RestoDoDia { get; set; }

    public string Tipo
    {
        get
        {
            if (DiaCompleto)
                return "Dia completo";

            if (RestoDoDia)
                return "Resto do dia";

            return "Período";
        }
    }
}