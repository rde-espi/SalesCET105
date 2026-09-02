using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Faturas
{
    public class AnularFaturaUseCase
    {
        private readonly IFaturaRepository _faturaRepository;

        public AnularFaturaUseCase(
            IFaturaRepository faturaRepository)
        {
            _faturaRepository = faturaRepository;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync( int id, bool isAdmin)
        {
            if (!isAdmin)
            {
                return UseCaseResult<bool>.Falha( "Apenas o administrador pode anular faturas.", TipoErro.Proibido);
            }

            var fatura = await _faturaRepository.GetByIdAsync(id);

            if (fatura == null)
            {
                return UseCaseResult<bool>.Falha("Fatura não encontrada.", TipoErro.NaoEncontrado);
            }

            if (string.Equals(fatura.Estado, "Anulada", StringComparison.OrdinalIgnoreCase))
            {
                return UseCaseResult<bool>.Falha( "A fatura já se encontra anulada.");
            }

            if (!string.Equals(fatura.Estado, "Emitida", StringComparison.OrdinalIgnoreCase))
            {
                return UseCaseResult<bool>.Falha( $"Não é possível anular uma fatura no estado '{fatura.Estado}'.");
            }

            fatura.Estado = "Anulada";

            await _faturaRepository.UpdateAsync(fatura);

            return UseCaseResult<bool>.Ok(true);
        }
    }
}
