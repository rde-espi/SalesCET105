using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Faturas
{
    public class GetFaturaByIdUseCase
    {
        private readonly IFaturaRepository _faturaRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;

        public GetFaturaByIdUseCase(
            IFaturaRepository faturaRepository,
            IFuncionarioRepository funcionarioRepository)
        {
            _faturaRepository = faturaRepository;
            _funcionarioRepository = funcionarioRepository;
        }

        public async Task<UseCaseResult<FaturaDTO>> ExecuteAsync(
            int id,
            string userId,
            bool isCliente,
            bool isFuncionario,
            bool isAdmin)
        {
            var fatura = await _faturaRepository.GetByIdWithDetailsAsync(id);

            if (fatura == null)
            {
                return UseCaseResult<FaturaDTO>.Falha( "Fatura não encontrada.", TipoErro.NaoEncontrado);
            }

            if (!isAdmin)
            {                
                if (isCliente)
                {
                    if (fatura.Marcacao.ClienteId != userId)
                    {
                        return UseCaseResult<FaturaDTO>.Falha("Não tem permissão para consultar esta fatura.", TipoErro.Proibido);
                    }
                }

                else if (isFuncionario)
                {
                    var funcionario = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                    if (funcionario == null)
                    {
                        return UseCaseResult<FaturaDTO>.Falha("Funcionário autenticado não encontrado.",TipoErro.Proibido);
                    }

                    if (fatura.Marcacao.FuncionarioId != funcionario.Id)
                    {
                        return UseCaseResult<FaturaDTO>.Falha( "Não tem permissão para consultar esta fatura.", TipoErro.Proibido);
                    }
                }
                else
                {
                    return UseCaseResult<FaturaDTO>.Falha( "Não tem permissão para consultar faturas.", TipoErro.Proibido);
                }
            }

            var resultado = MapToDTO(fatura);

            return UseCaseResult<FaturaDTO>.Ok(resultado);
        }

        private static FaturaDTO MapToDTO(Fatura fatura)
        {
            return new FaturaDTO
            {
                Id = fatura.Id,
                MarcacaoId = fatura.MarcacaoId,
                DataMarcacao = fatura.Marcacao.DataHoraInicio,

                Numero = fatura.Numero,
                Serie = fatura.Serie,
                NumeroSequencial = fatura.NumeroSequencial,

                DataEmissao = fatura.DataEmissao,

                NomeCliente = fatura.NomeCliente,
                NifCliente = fatura.NifCliente,
                MoradaCliente = fatura.MoradaCliente,
                CodigoPostalCliente = fatura.CodigoPostalCliente,
                LocalidadeCliente = fatura.LocalidadeCliente,

                Subtotal = fatura.Subtotal,
                ValorDesconto = fatura.ValorDesconto,
                ValorIva = fatura.ValorIva,
                Total = fatura.Total,

                Estado = fatura.Estado,

                ComunicadaAT = fatura.ComunicadaAT,
                DataComunicacaoAT = fatura.DataComunicacaoAT,
                CodigoRespostaAT = fatura.CodigoRespostaAT,
                MensagemRespostaAT = fatura.MensagemRespostaAT,

                Itens = fatura.Itens
                    .Select(i => new FaturaItemDTO
                    {
                        Id = i.Id,
                        ServicoId = i.ServicoId,
                        Descricao = i.Descricao,
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario,
                        PercentagemIva = i.PercentagemIva,
                        ValorIva = i.ValorIva,
                        Total = i.Total,
                        CodigoIva = i.CodigoIva,
                        MotivoIsencaoIva = i.MotivoIsencaoIva
                    })
                    .ToList()
            };
        }
    }
}
