using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.IndisponibilidadeService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Indisponibilidades
{
    public class CreateIndisponibilidadeUseCase
    {
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IIndisponibilidadeService _indisponibilidadeService;

        public CreateIndisponibilidadeUseCase(
            IIndisponibilidadeRepository indisponibilidadeRepository,
            IFuncionarioRepository funcionarioRepository,
            IIndisponibilidadeService indisponibilidadeService)
        {
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _funcionarioRepository = funcionarioRepository;
            _indisponibilidadeService = indisponibilidadeService;
        }

        public async Task<UseCaseResult<IndisponibilidadeDTO>> ExecuteAsync(string userId, bool isFuncionario, bool isAdmin, NovaIndisponibilidadeDTO dto)
        {
            // 1. Determinar qual funcionário será afetado
            var funcionarioIdResult =
                await ObterFuncionarioIdAsync(
                    userId,
                    isFuncionario,
                    isAdmin,
                    dto.FuncionarioId);

            if (!funcionarioIdResult.Sucesso)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    funcionarioIdResult.Erro!,
                    funcionarioIdResult.TipoErro);
            }

            var funcionarioId = funcionarioIdResult.Dados;


            // 2. Validar funcionário
            var funcionario =
                await _funcionarioRepository.GetFuncionarioByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    "O funcionário indicado não existe.",
                    TipoErro.NaoEncontrado);
            }

            if (!funcionario.Ativo)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    "O funcionário indicado não está ativo.");
            }


            // 3. Validar opções DiaCompleto / RestoDoDia
            var validacaoTipo = _indisponibilidadeService.ValidarTipoIndisponibilidade(dto.DiaCompleto, dto.RestoDoDia);

            if (!validacaoTipo.Sucesso)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    validacaoTipo.Erro!,
                    validacaoTipo.TipoErro);
            }


            // 4. Obter horário de trabalho desse dia
            var horariosTrabalho = await _indisponibilidadeService.ObterHorariosTrabalhoAsync(funcionarioId, dto.DataHoraInicio);

            if (!horariosTrabalho.Any())
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    "O funcionário não possui horário de trabalho definido para este dia.");
            }


            // 5. Obter marcações desse dia
            var marcacoesDoDia = await _indisponibilidadeService.ObterMarcacoesDoDiaAsync(funcionarioId, dto.DataHoraInicio);

            var marcacoesConfirmadas = marcacoesDoDia
                .Where(m => m.EstadoMarcacao.Nome == "Confirmada")
                .ToList();

            var marcacoesConcluidas = marcacoesDoDia
                .Where(m => m.EstadoMarcacao.Nome == "Concluida")
                .ToList();


            // 6. Determinar o período real da indisponibilidade
            var periodoResult = _indisponibilidadeService.CalcularPeriodo(
                dto.DataHoraInicio,
                dto.DataHoraFim,
                dto.DiaCompleto,
                dto.RestoDoDia,
                horariosTrabalho,
                marcacoesConcluidas);

            if (!periodoResult.Sucesso)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    periodoResult.Erro!,
                    periodoResult.TipoErro);
            }

            var inicio = periodoResult.Dados.Inicio;
            var fim = periodoResult.Dados.Fim;


            // 7. Funcionário não pode criar indisponibilidade no passado
            if (!isAdmin && !dto.RestoDoDia && inicio <= DateTime.Now)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    "Não é possível criar uma indisponibilidade numa data/hora passada.");
            }


            // 8. Validar conflitos com marcações confirmadas
            var conflitoMarcacao = _indisponibilidadeService.ValidarConflitoComMarcacoesConfirmadas(inicio, fim, marcacoesConfirmadas);

            if (!conflitoMarcacao.Sucesso)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    conflitoMarcacao.Erro!,
                    conflitoMarcacao.TipoErro);
            }


            // 9. Validar sobreposição com outras indisponibilidades
            var existeSobreposicao = await _indisponibilidadeService.ExisteSobreposicaoAsync(funcionarioId, inicio, fim);

            if (existeSobreposicao)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha(
                    "Já existe uma indisponibilidade sobreposta para este funcionário.",
                    TipoErro.Conflito);
            }


            // 10. Criar indisponibilidade
            try
            {
                var indisponibilidade = new Indisponibilidade
                {
                    FuncionarioId = funcionarioId,
                    DataHoraInicio = inicio,
                    DataHoraFim = fim,
                    Motivo = dto.Motivo,
                    DiaCompleto = dto.DiaCompleto,
                    RestoDoDia = dto.RestoDoDia
                };

                await _indisponibilidadeRepository.CreateAsync(indisponibilidade);

                var resposta = new IndisponibilidadeDTO
                {
                    Id = indisponibilidade.Id,
                    FuncionarioId = funcionario.Id,
                    FuncionarioNome = funcionario.User.NomeCompleto,
                    DataHoraInicio = indisponibilidade.DataHoraInicio,
                    DataHoraFim = indisponibilidade.DataHoraFim,
                    Motivo = indisponibilidade.Motivo,
                    DiaCompleto = indisponibilidade.DiaCompleto,
                    RestoDoDia = indisponibilidade.RestoDoDia
                };

                return UseCaseResult<IndisponibilidadeDTO>.Ok(resposta);
            }
            catch (Exception)
            {
                return UseCaseResult<IndisponibilidadeDTO>.Falha("Ocorreu um erro ao criar a indisponibilidade.");
            }
        }

        private async Task<UseCaseResult<int>> ObterFuncionarioIdAsync(string userId, bool isFuncionario, bool isAdmin, int? funcionarioIdDto)
        {
            if (isFuncionario && !isAdmin)
            {
                var funcionario =
                    await _funcionarioRepository
                        .GetFuncionarioByUserIdAsync(userId);

                if (funcionario == null)
                {
                    return UseCaseResult<int>.Falha(
                        "Funcionário autenticado não encontrado.",
                        TipoErro.Proibido);
                }

                return UseCaseResult<int>.Ok(funcionario.Id);
            }

            if (isAdmin)
            {
                if (!funcionarioIdDto.HasValue)
                {
                    return UseCaseResult<int>.Falha(
                        "É necessário indicar o funcionário.");
                }

                return UseCaseResult<int>.Ok(funcionarioIdDto.Value);
            }

            return UseCaseResult<int>.Falha("Utilizador sem permissão.", TipoErro.Proibido);
        }
    }
}
