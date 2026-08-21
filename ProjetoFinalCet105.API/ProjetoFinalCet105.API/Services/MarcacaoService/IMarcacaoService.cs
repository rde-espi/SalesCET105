using ProjetoFinalCet105.API.Entities;

namespace ProjetoFinalCet105.API.Services.MarcacaoService
{
    public interface IMarcacaoService
    {
        Task<FuncionarioServico?> GetFuncionarioServicoAsync(
            int funcionarioId,
            int servicoId);

        Task<bool> HorarioValidoAsync(
            int funcionarioId,
            DateTime inicio,
            DateTime fim);

        Task<bool> ExisteIndisponibilidadeAsync(
            int funcionarioId,
            DateTime inicio,
            DateTime fim);

        Task<bool> ExisteSobreposicaoAsync(
            int funcionarioId,
            DateTime inicio,
            DateTime fim,
            int? marcacaoIgnorarId = null);
    }
}
