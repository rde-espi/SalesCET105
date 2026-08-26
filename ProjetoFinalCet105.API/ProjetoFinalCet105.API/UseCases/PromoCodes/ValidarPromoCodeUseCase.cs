using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.PromoCodes
{
    public class ValidarPromoCodeUseCase
    {
        private readonly IPromoCodeRepository _promoCodeRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;

        public ValidarPromoCodeUseCase(
            IPromoCodeRepository promoCodeRepository,
            IMarcacaoRepository marcacaoRepository)
        {
            _promoCodeRepository = promoCodeRepository;
            _marcacaoRepository = marcacaoRepository;
        }

        public async Task<UseCaseResult<PromoCodeValidadoDTO>> ExecuteAsync(
            string codigo,
            string clienteId)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return UseCaseResult<PromoCodeValidadoDTO>
                    .Falha("O código promocional é obrigatório.");
            }

            codigo = codigo.Trim();

            var promoCode =
                await _promoCodeRepository
                    .GetByCodigoAsync(codigo);

            if (promoCode == null)
            {
                return UseCaseResult<PromoCodeValidadoDTO>
                    .Falha(
                        "Código promocional inválido.",
                        TipoErro.NaoEncontrado);
            }

            if (!promoCode.Ativo)
            {
                return UseCaseResult<PromoCodeValidadoDTO>
                    .Falha("Este código promocional não está ativo.");
            }

            var agora = DateTime.Now;

            if (agora < promoCode.DataInicio)
            {
                return UseCaseResult<PromoCodeValidadoDTO>
                    .Falha("Este código promocional ainda não está disponível.");
            }

            if (agora > promoCode.DataFim)
            {
                return UseCaseResult<PromoCodeValidadoDTO>
                    .Falha("Este código promocional expirou.");
            }

            if (promoCode.LimiteUtilizacoes.HasValue &&
                promoCode.NumeroUtilizacoes >=
                promoCode.LimiteUtilizacoes.Value)
            {
                return UseCaseResult<PromoCodeValidadoDTO>
                    .Falha("Este código promocional atingiu o limite de utilizações.");
            }

            var jaUtilizado =
                await _marcacaoRepository
                    .ClienteJaUsouPromoCodeAsync(
                        clienteId,
                        promoCode.Id);

            if (jaUtilizado)
            {
                return UseCaseResult<PromoCodeValidadoDTO>
                    .Falha("Já utilizou este código promocional.");
            }

            var resultado = new PromoCodeValidadoDTO
            {
                PromoCodeId = promoCode.Id,
                Codigo = promoCode.Codigo,
                PercentagemDesconto =
                    promoCode.PercentagemDesconto
            };

            return UseCaseResult<PromoCodeValidadoDTO>
                .Ok(resultado);
        }
    }
}
