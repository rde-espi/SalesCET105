using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.Faturacao;
using ProjetoFinalCet105.API.UseCases.Common;
using ProjetoFinalCet105.API.UseCases.Marcacoes;
using System.Data;

namespace ProjetoFinalCet105.API.UseCases.Faturas
{
    public class CreateFaturaUseCase
    {
        private readonly IFaturaRepository _faturaRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FaturacaoSettings _settings;

        public CreateFaturaUseCase(
            IFaturaRepository faturaRepository,
            IMarcacaoRepository marcacaoRepository,
            IFuncionarioRepository funcionarioRepository,
            UserManager<User> userManager,
            IUnitOfWork unitOfWork,
            IOptions<FaturacaoSettings> options)
        {
            _faturaRepository = faturaRepository;
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _settings = options.Value;
        }

        public async Task<UseCaseResult<FaturaDTO>> ExecuteAsync(int marcacaoId,string userId,bool isFuncionario, bool isAdmin)
        {
            if (!isAdmin && !isFuncionario)
            {
                return UseCaseResult<FaturaDTO>.Falha( "Não tem permissão para emitir faturas.", TipoErro.Proibido);
            }

            var marcacao = await _marcacaoRepository.GetByIdWithDetailsAsync(marcacaoId);

            if (marcacao == null)
            {
                return UseCaseResult<FaturaDTO>.Falha("Marcação não encontrada.", TipoErro.NaoEncontrado);
            }

            if (isFuncionario && !isAdmin)
            {
                var funcionario = await _funcionarioRepository.GetFuncionarioByUserIdAsync(userId);

                if (funcionario == null)
                {
                    return UseCaseResult<FaturaDTO>.Falha("Funcionário autenticado não encontrado.", TipoErro.Proibido);
                }

                if (marcacao.FuncionarioId != funcionario.Id)
                {
                    return UseCaseResult<FaturaDTO>.Falha("Não tem permissão para emitir a fatura desta marcação.", TipoErro.Proibido);
                }
            }

            if (!string.Equals( marcacao.EstadoMarcacao.Nome, "Concluida", StringComparison.OrdinalIgnoreCase))
            {
                return UseCaseResult<FaturaDTO>.Falha( "A fatura apenas pode ser emitida para uma marcação concluída.");
            }

            var faturaExistente = await _faturaRepository.GetByMarcacaoIdAsync(marcacaoId);

            if (faturaExistente != null)
            {
                return UseCaseResult<FaturaDTO>.Falha("Já existe uma fatura emitida para esta marcação.");
            }

            // Obter o cliente para guardar os dados como snapshot na fatura
            var cliente = await _userManager.FindByIdAsync(marcacao.ClienteId);

            if (cliente == null)
            {
                return UseCaseResult<FaturaDTO>.Falha("Cliente associado à marcação não encontrado.", TipoErro.NaoEncontrado);
            }

            // VALORES DA FATURA
          
            decimal total = Math.Round(marcacao.Preco, 2);

            decimal valorDesconto = Math.Round(marcacao.ValorDesconto ?? 0m, 2);

            // Preço antes do desconto.
            decimal subtotal = Math.Round(total + valorDesconto, 2);

            // CÁLCULO DO IVA

            decimal valorIva;

            if (_settings.PrecosIncluemIva)
            {
                decimal divisorIva = 1m + (_settings.TaxaIva / 100m);

                decimal baseTributavel = Math.Round(total / divisorIva, 2);

                valorIva =Math.Round(total - baseTributavel, 2);
            }
            else
            {              
                valorIva =Math.Round( total * (_settings.TaxaIva / 100m), 2);
            }
            // SNAPSHOT DOS DADOS
            string? nifCliente = string.IsNullOrWhiteSpace(cliente.Contribuinte) ? null : cliente.Contribuinte.Trim();

            string nomeCliente = cliente.NomeCompleto;

            string? moradaCliente = string.IsNullOrWhiteSpace(cliente.Morada) ? null: cliente.Morada.Trim();

            string? codigoPostalCliente = string.IsNullOrWhiteSpace(cliente.CodigoPostal) ? null : cliente.CodigoPostal.Trim();

            string? localidadeCliente = string.IsNullOrWhiteSpace(cliente.Localidade) ? null : cliente.Localidade.Trim();

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Revalidar dentro da transação
                var faturaExistenteNaTransacao = await _faturaRepository.GetByMarcacaoIdAsync(marcacaoId);
                
                if (faturaExistenteNaTransacao != null)
                {
                    return UseCaseResult<FaturaDTO>.Falha("Já existe uma fatura emitida para esta marcação.");
                }
                
                int numeroSequencial = await _faturaRepository.GetProximoNumeroSequencialAsync(_settings.Serie);
                
                string numeroFatura = $"{_settings.Serie}/{numeroSequencial:D6}";
                
                var fatura = new Fatura
                {
                    MarcacaoId = marcacao.Id,
                    
                    Numero = numeroFatura,
                    Serie = _settings.Serie,
                    NumeroSequencial = numeroSequencial,
                    
                    DataEmissao = DateTime.Now,
                    
                    NomeCliente = nomeCliente,
                    NifCliente = nifCliente,
                    MoradaCliente = moradaCliente,
                    CodigoPostalCliente = codigoPostalCliente,
                    LocalidadeCliente = localidadeCliente,
                    
                    Subtotal = subtotal,
                    ValorDesconto = valorDesconto,
                    ValorIva = valorIva,
                    Total = total,
                    
                    Estado = "Emitida",
                    
                    ComunicadaAT = false
                };
                
                var item = new FaturaItem
                {
                    ServicoId = marcacao.ServicoId,
                    Descricao = marcacao.Servico.Nome,
                    
                    Quantidade = 1m,
                    PrecoUnitario = total,
                    
                    PercentagemIva = _settings.TaxaIva,
                    ValorIva = valorIva,
                    Total = total,
                    
                    CodigoIva = _settings.CodigoIva,
                    MotivoIsencaoIva = null
                };
                
                fatura.Itens.Add(item);
                
                await _faturaRepository.CreateAsync(fatura);
                
                var resultado = new FaturaDTO
                {
                    Id = fatura.Id,
                    MarcacaoId = fatura.MarcacaoId,
                    DataMarcacao = marcacao.DataHoraInicio,
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
                
                return UseCaseResult<FaturaDTO>.Ok(resultado);
            },
            IsolationLevel.Serializable);
        }
    }
}
