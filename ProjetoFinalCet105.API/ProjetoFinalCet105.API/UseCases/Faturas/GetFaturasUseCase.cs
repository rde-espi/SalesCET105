using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Faturas
{
    public class GetFaturasUseCase
    {
        private readonly IFaturaRepository _faturaRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;

        public GetFaturasUseCase(
            IFaturaRepository faturaRepository,
            IFuncionarioRepository funcionarioRepository)
        {
            _faturaRepository = faturaRepository;
            _funcionarioRepository = funcionarioRepository;
        }

        public async Task<UseCaseResult<List<FaturaDTO>>> ExecuteAsync(
            string userId,
            bool isCliente,
            bool isFuncionario,
            bool isAdmin,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            string? numero = null,
            string? estado = null)
        {
            var query = _faturaRepository.GetAllWithDetails();

            // ----------------------------------------------------
            // AUTORIZAÇÃO
            // ----------------------------------------------------

            if (!isAdmin)
            {
                if (isCliente)
                {
                    query = query.Where(
                        f => f.Marcacao.ClienteId == userId);
                }
                else if (isFuncionario)
                {
                    var funcionario =
                        await _funcionarioRepository
                            .GetFuncionarioByUserIdAsync(userId);

                    if (funcionario == null)
                    {
                        return UseCaseResult<List<FaturaDTO>>.Falha(
                            "Funcionário autenticado não encontrado.",
                            TipoErro.Proibido);
                    }

                    query = query.Where(
                        f => f.Marcacao.FuncionarioId == funcionario.Id);
                }
                else
                {
                    return UseCaseResult<List<FaturaDTO>>.Falha(
                        "Não tem permissão para consultar faturas.",
                        TipoErro.Proibido);
                }
            }

            // ----------------------------------------------------
            // FILTROS
            // ----------------------------------------------------

            if (dataInicio.HasValue)
            {
                query = query.Where(
                    f => f.DataEmissao >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                // Inclui todo o dia indicado em dataFim.
                var limiteFinal = dataFim.Value.Date.AddDays(1);

                query = query.Where(
                    f => f.DataEmissao < limiteFinal);
            }

            if (!string.IsNullOrWhiteSpace(numero))
            {
                var numeroPesquisa = numero.Trim();

                query = query.Where(
                    f => f.Numero.Contains(numeroPesquisa));
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                var estadoPesquisa = estado.Trim();

                query = query.Where(
                    f => f.Estado == estadoPesquisa);
            }

            // ----------------------------------------------------
            // CONSULTA
            // ----------------------------------------------------

            var faturas = await query
                .OrderByDescending(f => f.DataEmissao)
                .ThenByDescending(f => f.NumeroSequencial)
                .ToListAsync();

            // ----------------------------------------------------
            // DTO
            // ----------------------------------------------------

            var resultado = faturas
                .Select(f => new FaturaDTO
                {
                    Id = f.Id,
                    MarcacaoId = f.MarcacaoId,
                    DataMarcacao = f.Marcacao.DataHoraInicio,

                    Numero = f.Numero,
                    Serie = f.Serie,
                    NumeroSequencial = f.NumeroSequencial,

                    DataEmissao = f.DataEmissao,

                    NomeCliente = f.NomeCliente,
                    NifCliente = f.NifCliente,
                    MoradaCliente = f.MoradaCliente,
                    CodigoPostalCliente = f.CodigoPostalCliente,
                    LocalidadeCliente = f.LocalidadeCliente,

                    Subtotal = f.Subtotal,
                    ValorDesconto = f.ValorDesconto,
                    ValorIva = f.ValorIva,
                    Total = f.Total,

                    Estado = f.Estado,

                    ComunicadaAT = f.ComunicadaAT,
                    DataComunicacaoAT = f.DataComunicacaoAT,
                    CodigoRespostaAT = f.CodigoRespostaAT,
                    MensagemRespostaAT = f.MensagemRespostaAT,

                    Itens = f.Itens
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
                })
                .ToList();

            return UseCaseResult<List<FaturaDTO>>.Ok(resultado);
        }
    }
}
