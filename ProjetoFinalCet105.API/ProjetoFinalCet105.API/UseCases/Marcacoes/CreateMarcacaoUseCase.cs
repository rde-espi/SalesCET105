using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.MarcacaoService;
using ProjetoFinalCet105.API.Services.NotificacaoService;
using ProjetoFinalCet105.API.UseCases.Common;
using ProjetoFinalCet105.API.UseCases.PromoCodes;

namespace ProjetoFinalCet105.API.UseCases.Marcacoes
{
    public class CreateMarcacaoUseCase
    {
        private readonly IMarcacaoRepository _marcacaoRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IServicoRepository _servicoRepository;
        private readonly IEstadoMarcacaoRepository _estadoMarcacaoRepository;
        private readonly IHistoricoMarcacaoRepository _historicoMarcacaoRepository;
        private readonly UserManager<User> _userManager;
        private readonly IMarcacaoService _marcacaoService;
        private readonly INotificacaoService _notificacaoService;
        private readonly ValidarPromoCodeUseCase _validarPromoCodeUseCase;
        private readonly IPromoCodeRepository _promoCodeRepository;

        public CreateMarcacaoUseCase(
            IMarcacaoRepository marcacaoRepository,
            IFuncionarioRepository funcionarioRepository,
            IServicoRepository servicoRepository,
            IEstadoMarcacaoRepository estadoMarcacaoRepository,
            IHistoricoMarcacaoRepository historicoMarcacaoRepository,
            UserManager<User> userManager,
            IMarcacaoService marcacaoService,
            INotificacaoService notificacaoService,
            ValidarPromoCodeUseCase validarPromoCodeUseCase,
            IPromoCodeRepository promoCodeRepository)
        {
            _marcacaoRepository = marcacaoRepository;
            _funcionarioRepository = funcionarioRepository;
            _servicoRepository = servicoRepository;
            _estadoMarcacaoRepository = estadoMarcacaoRepository;
            _historicoMarcacaoRepository = historicoMarcacaoRepository;
            _userManager = userManager;
            _marcacaoService = marcacaoService;
            _notificacaoService = notificacaoService;
            _validarPromoCodeUseCase = validarPromoCodeUseCase;
            _promoCodeRepository = promoCodeRepository;
        }

        public async Task<UseCaseResult<MarcacaoDTO>> ExecuteAsync( string userId,bool isCliente,bool isFuncionario,bool isAdmin,NovaMarcacaoDTO dto)
        {
            string clienteId;
            int funcionarioId;

            if (isCliente)
            {
                clienteId = userId;
            }
            else if (isFuncionario || isAdmin)
            {
                if (string.IsNullOrEmpty(dto.ClienteId))
                {
                    return UseCaseResult<MarcacaoDTO>
                        .Falha("É necessário indicar o cliente da marcação");
                }

                clienteId = dto.ClienteId;
            }
            else
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("Utilizador sem permissão para criar marcações",TipoErro.Proibido);
            }

            var cliente = await _userManager.FindByIdAsync(clienteId);

            if (cliente == null)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O cliente indicado não existe", TipoErro.NaoEncontrado);
            }

            if (!await _userManager.IsInRoleAsync(cliente, "Cliente"))
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O utilizador indicado não é um cliente");
            }

            if (isFuncionario && !isAdmin)
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository
                        .GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<MarcacaoDTO>.Falha(
                        "Funcionário autenticado não encontrado.",
                        TipoErro.Proibido);
                }

                funcionarioId = funcionarioAutenticado.Id;
            }
            else
            {
                if (!dto.FuncionarioId.HasValue)
                {
                    return UseCaseResult<MarcacaoDTO>.Falha(
                        "É necessário indicar o funcionário da marcação.");
                }

                funcionarioId = dto.FuncionarioId.Value;
            }

            var funcionario =await _funcionarioRepository.GetFuncionarioByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O funcionário indicado não existe", TipoErro.NaoEncontrado);
            }

            if (!funcionario.Ativo || !funcionario.Disponivel)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O funcionário indicado não está disponível");
            }

            var servico =
                await _servicoRepository.GetByIdAsync(dto.ServicoId);

            if (servico == null)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O serviço indicado não existe", TipoErro.NaoEncontrado);
            }

            if (!servico.Disponivel)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O serviço indicado não está disponível");
            }

            var funcionarioServico = await _marcacaoService.GetFuncionarioServicoAsync(funcionarioId, dto.ServicoId);
            if (funcionarioServico == null)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O funcionário indicado não realiza este serviço");
            }

            if (dto.DataHoraInicio <= DateTime.Now)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("Não é possível criar uma marcação numa data/hora passada.");
            }

            var duracaoMinutos =
                funcionarioServico.DuracaoPersonalizadaMinutos.HasValue &&
                funcionarioServico.DuracaoPersonalizadaMinutos.Value > 0
                ? funcionarioServico.DuracaoPersonalizadaMinutos.Value
                : servico.DuracaoMinutos;


            var dataHoraFim = dto.DataHoraInicio.AddMinutes(duracaoMinutos);

            if (dataHoraFim.Date != dto.DataHoraInicio.Date)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("A duração do serviço ultrapassa o horário do mesmo dia.");
            }

            var precoOriginal =
                funcionarioServico.PrecoPersonalizado.HasValue &&
                funcionarioServico.PrecoPersonalizado.Value > 0
                ? funcionarioServico.PrecoPersonalizado.Value
                : servico.Preco;


            decimal precoFinal = precoOriginal;
            decimal? valorDesconto = null;
            decimal? percentagemAplicada = null;
            int? promoCodeId = null;
            string? codigoPromo = null;

            if (!string.IsNullOrWhiteSpace(dto.PromoCode))
            {
                var validacao = await _validarPromoCodeUseCase.ExecuteAsync(dto.PromoCode, clienteId);

                if (!validacao.Sucesso)
                {
                    return UseCaseResult<MarcacaoDTO>.Falha(validacao.Erro!, validacao.TipoErro);
                }

                promoCodeId = validacao.Dados!.PromoCodeId;
                codigoPromo = validacao.Dados.Codigo;

                percentagemAplicada = validacao.Dados.PercentagemDesconto;

                valorDesconto = Math.Round(precoOriginal * (percentagemAplicada.Value / 100m), 2);

                precoFinal = precoOriginal - valorDesconto.Value;
            }

            var horarioValido = await _marcacaoService.HorarioValidoAsync(funcionarioId,dto.DataHoraInicio,dataHoraFim);

            if (!horarioValido)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("A marcação está fora do horário de trabalho do funcionário");
            }

            var indisponivel = await _marcacaoService.ExisteIndisponibilidadeAsync(funcionarioId,dto.DataHoraInicio,dataHoraFim);

            if (indisponivel)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O funcionário está indisponível neste período");
            }

            if (await _marcacaoService.ExisteSobreposicaoAsync(funcionarioId,dto.DataHoraInicio,dataHoraFim))
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("Já existe uma marcação para este funcionário neste período", TipoErro.Conflito);
            }

            var estadoPendente = await _estadoMarcacaoRepository
                .GetAll()
                .FirstOrDefaultAsync(e => e.Nome == "Pendente");

            if (estadoPendente == null)
            {
                return UseCaseResult<MarcacaoDTO>
                    .Falha("O estado inicial da marcação não foi encontrado");
            }

            try
            {
                var marcacao = new Marcacao
                {
                    ClienteId = clienteId,
                    FuncionarioId = funcionarioId,
                    ServicoId = dto.ServicoId,
                    EstadoMarcacaoId = estadoPendente.Id,

                    DataHoraInicio = dto.DataHoraInicio,
                    DataHoraFim = dataHoraFim,

                    Preco = precoFinal,
                    Observacoes = dto.Observacoes,

                    DataCriacao = DateTime.Now,
                    PromoCodeId = promoCodeId,
                    PercentagemDescontoAplicada = percentagemAplicada,
                    ValorDesconto = valorDesconto
                };

                await _marcacaoRepository.CreateAsync(marcacao);
                if (promoCodeId.HasValue)
                {
                    try
                    {
                        await _promoCodeRepository
                            .IncrementarUtilizacaoAsync(promoCodeId.Value);
                    }
                    catch
                    {
                        // A falha no contador não deve impedir
                        // a criação da marcação.
                    }
                }

                var historico = new HistoricoMarcacao
                {
                    MarcacaoId = marcacao.Id,
                    UserId = userId,
                    Acao = "Criação",
                    Descricao =
                    promoCodeId.HasValue
                    ? $"Marcação criada com o código promocional '{codigoPromo}' " +
                    $"({percentagemAplicada}% de desconto)."
                    : "Marcação criada.",
                    DataAlteracao = DateTime.Now
                };

                await _historicoMarcacaoRepository.CreateAsync(historico);

                try
                {
                    await _notificacaoService.NotificarCriacaoMarcacaoAsync(
                        cliente.Id,
                        funcionario.UserId,
                        servico.Nome,
                        marcacao.DataHoraInicio,
                        isCliente,
                        isFuncionario,
                        isAdmin);
                }
                catch
                {
                    // Uma falha na notificação não deve anular
                    // uma marcação que já foi criada com sucesso.
                }

                var resposta = new MarcacaoDTO
                {
                    Id = marcacao.Id,

                    ClienteId = cliente.Id,
                    ClienteNome = cliente.NomeCompleto,

                    FuncionarioId = funcionario.Id,
                    FuncionarioNome = funcionario.User.NomeCompleto,

                    ServicoId = servico.Id,
                    ServicoNome = servico.Nome,

                    EstadoMarcacaoId = estadoPendente.Id,
                    EstadoMarcacaoNome = estadoPendente.Nome,

                    DataHoraInicio = marcacao.DataHoraInicio,
                    DataHoraFim = marcacao.DataHoraFim,

                    Preco = marcacao.Preco,
                    PromoCodeId = marcacao.PromoCodeId,
                    PromoCode = codigoPromo,
                    PercentagemDescontoAplicada = marcacao.PercentagemDescontoAplicada,
                    ValorDesconto = marcacao.ValorDesconto,
                    Observacoes = marcacao.Observacoes,
                    DataCriacao = marcacao.DataCriacao
                };
                                
                return UseCaseResult<MarcacaoDTO>.Ok(resposta);
            }
            catch (Exception ex)
            {
                var erro =
                    ex.InnerException?.Message
                    ?? ex.Message;

                return UseCaseResult<MarcacaoDTO>.Falha($"Ocorreu um erro ao criar a marcação: {erro}");
            }
        }
    }
}
