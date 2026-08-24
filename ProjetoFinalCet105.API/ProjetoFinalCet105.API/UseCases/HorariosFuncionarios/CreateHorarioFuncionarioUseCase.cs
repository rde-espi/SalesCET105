using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.HorarioFuncionarioService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.HorariosFuncionarios
{
    public class CreateHorarioFuncionarioUseCase
    {
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IHorarioFuncionarioService _horarioFuncionarioService;

        public CreateHorarioFuncionarioUseCase(
            IHorarioFuncionarioRepository horarioFuncionarioRepository,
            IFuncionarioRepository funcionarioRepository,
            IHorarioFuncionarioService horarioFuncionarioService)
        {
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _funcionarioRepository = funcionarioRepository;
            _horarioFuncionarioService = horarioFuncionarioService;
        }

        public async Task<UseCaseResult<HorarioFuncionarioDTO>> ExecuteAsync(
            string userId,
            bool isFuncionario,
            bool isAdmin,
            NovoHorarioFuncionarioDTO dto)
        {
            // 1. Determinar o funcionário
            var funcionarioIdResult =
                await ObterFuncionarioIdAsync(
                    userId,
                    isFuncionario,
                    isAdmin,
                    dto.FuncionarioId);

            if (!funcionarioIdResult.Sucesso)
            {
                return UseCaseResult<HorarioFuncionarioDTO>.Falha(
                    funcionarioIdResult.Erro!,
                    funcionarioIdResult.TipoErro);
            }

            var funcionarioId = funcionarioIdResult.Dados;

            // 2. Verificar funcionário
            var funcionario =
                await _funcionarioRepository
                    .GetFuncionarioByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<HorarioFuncionarioDTO>.Falha(
                    "Funcionário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            if (!funcionario.Ativo)
            {
                return UseCaseResult<HorarioFuncionarioDTO>.Falha(
                    "Não é possível criar um horário para um funcionário inativo.");
            }

            // 3. Validar período
            var periodoResult =
                _horarioFuncionarioService.ValidarPeriodo(
                    dto.HoraInicio,
                    dto.HoraFim);

            if (!periodoResult.Sucesso)
            {
                return UseCaseResult<HorarioFuncionarioDTO>.Falha(
                    periodoResult.Erro!,
                    periodoResult.TipoErro);
            }

            // 4. Verificar sobreposição
            var existeSobreposicao =
                await _horarioFuncionarioService
                    .ExisteSobreposicaoAsync(
                        funcionarioId,
                        dto.DiaSemana,
                        dto.HoraInicio,
                        dto.HoraFim);

            if (existeSobreposicao)
            {
                return UseCaseResult<HorarioFuncionarioDTO>.Falha(
                    "Já existe um horário sobreposto para este funcionário nesse dia.",
                    TipoErro.Conflito);
            }

            // 5. Criar
            try
            {
                var horario = new HorarioFuncionario
                {
                    FuncionarioId = funcionarioId,
                    DiaSemana = dto.DiaSemana,
                    HoraInicio = dto.HoraInicio,
                    HoraFim = dto.HoraFim,
                    Ativo = true
                };

                await _horarioFuncionarioRepository
                    .CreateAsync(horario);

                var resposta = new HorarioFuncionarioDTO
                {
                    Id = horario.Id,
                    FuncionarioId = funcionario.Id,
                    FuncionarioNome = funcionario.User.NomeCompleto,
                    DiaSemana = horario.DiaSemana,
                    HoraInicio = horario.HoraInicio,
                    HoraFim = horario.HoraFim,
                    Ativo = horario.Ativo
                };

                return UseCaseResult<HorarioFuncionarioDTO>
                    .Ok(resposta);
            }
            catch (Exception)
            {
                return UseCaseResult<HorarioFuncionarioDTO>.Falha(
                    "Ocorreu um erro ao criar o horário do funcionário.");
            }
        }

        private async Task<UseCaseResult<int>> ObterFuncionarioIdAsync(
            string userId,
            bool isFuncionario,
            bool isAdmin,
            int? funcionarioIdDto)
        {
            // Funcionário cria horário apenas para si próprio
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

            // Admin escolhe o funcionário
            if (isAdmin)
            {
                if (!funcionarioIdDto.HasValue)
                {
                    return UseCaseResult<int>.Falha(
                        "É necessário indicar o funcionário.");
                }

                return UseCaseResult<int>.Ok(
                    funcionarioIdDto.Value);
            }

            return UseCaseResult<int>.Falha(
                "Utilizador sem permissão.",
                TipoErro.Proibido);
        }
    }
}
