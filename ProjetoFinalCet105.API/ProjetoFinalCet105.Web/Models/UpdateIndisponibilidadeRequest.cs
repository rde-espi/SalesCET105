namespace ProjetoFinalCet105.Web.Models;

public class UpdateIndisponibilidadeRequest
{
    public int? FuncionarioId { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string? Motivo { get; set; }
    public bool DiaCompleto { get; set; }
    public bool RestoDoDia { get; set; }
}