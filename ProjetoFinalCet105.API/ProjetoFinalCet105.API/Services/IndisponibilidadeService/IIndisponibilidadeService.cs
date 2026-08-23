using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.UseCases.Common;
using ProjetoFinalCet105.API.UseCases.Indisponibilidades;

namespace ProjetoFinalCet105.API.Services.IndisponibilidadeService
{
    public interface IIndisponibilidadeService
    {
        Task<List<HorarioFuncionario>> ObterHorariosTrabalhoAsync(int funcionarioId,DateTime data);

        Task<List<Marcacao>> ObterMarcacoesDoDiaAsync(int funcionarioId,DateTime data);

        UseCaseResult<bool> ValidarTipoIndisponibilidade(bool diaCompleto,bool restoDoDia);

        UseCaseResult<PeriodoIndisponibilidade> CalcularPeriodo(
            DateTime dataHoraInicio,
            DateTime dataHoraFim,
            bool diaCompleto,
            bool restoDoDia,
            List<HorarioFuncionario> horariosTrabalho,
            List<Marcacao> marcacoesConcluidas);

        UseCaseResult<bool> ValidarConflitoComMarcacoesConfirmadas(DateTime inicio,DateTime fim,List<Marcacao> marcacoesConfirmadas);

        Task<bool> ExisteSobreposicaoAsync(int funcionarioId,DateTime inicio,DateTime fim,int? ignorarId = null);
    }
}
