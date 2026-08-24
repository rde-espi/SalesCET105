using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.HorarioFuncionarioService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.HorariosFuncionarios
{
    public class UpdateHorarioFuncionarioUseCase
    {
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IHorarioFuncionarioService _horarioFuncionarioService;

        public UpdateHorarioFuncionarioUseCase(
            IHorarioFuncionarioRepository horarioFuncionarioRepository,
            IFuncionarioRepository funcionarioRepository,
            IHorarioFuncionarioService horarioFuncionarioService)
        {
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _funcionarioRepository = funcionarioRepository;
            _horarioFuncionarioService = horarioFuncionarioService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(
            int id,
            string userId,
            bool isFuncionario,
            bool isAdmin,
            UpdateHorarioFuncionarioDTO dto)
        {
            // Verificar se o horário existe
            var horarioAtual =
                await _horarioFuncionarioRepository.GetByIdAsync(id);

            if (horarioAtual == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Horário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            // Validar autorização para alterar este horário
            var autorizacaoResult =
                await ValidarAutorizacaoAsync(
                    horarioAtual,
                    userId,
                    isFuncionario,
                    isAdmin);

            if (!autorizacaoResult.Sucesso)
            {
                return UseCaseResult<bool>.Falha(
                    autorizacaoResult.Erro!,
                    autorizacaoResult.TipoErro);
            }

            // O funcionário associado ao horário nunca é alterado
            var funcionarioId = horarioAtual.FuncionarioId;

            //Verificar se o funcionário existe e está ativo
            var funcionario =
                await _funcionarioRepository.GetByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Funcionário não encontrado.",
                    TipoErro.NaoEncontrado);
            }

            if (!funcionario.Ativo)
            {
                return UseCaseResult<bool>.Falha(
                    "O funcionário indicado não está ativo.");
            }

            // Validar período
            var periodoResult =
                _horarioFuncionarioService.ValidarPeriodo(
                    dto.HoraInicio,
                    dto.HoraFim);

            if (!periodoResult.Sucesso)
            {
                return UseCaseResult<bool>.Falha(
                    periodoResult.Erro!,
                    periodoResult.TipoErro);
            }

            //  Validar sobreposição com outros horários
            var existeSobreposicao =
                await _horarioFuncionarioService.ExisteSobreposicaoAsync(
                    funcionarioId,
                    dto.DiaSemana,
                    dto.HoraInicio,
                    dto.HoraFim,
                    id);

            if (existeSobreposicao)
            {
                return UseCaseResult<bool>.Falha(
                    "Já existe outro horário sobreposto para este funcionário nesse dia.",
                    TipoErro.Conflito);
            }

            // Verificar se existem marcações confirmadas futuras
            // que ficariam fora do novo horário

            var validacaoMarcacoes = await _horarioFuncionarioService
                .ValidarMarcacoesConfirmadasFuturasAsync(
                funcionarioId,
                dto.DiaSemana,
                dto.HoraInicio,
                dto.HoraFim);

            if (!validacaoMarcacoes.Sucesso)
            {
                return UseCaseResult<bool>.Falha(
                    validacaoMarcacoes.Erro!,
                    validacaoMarcacoes.TipoErro);
            }

            try
            {
                horarioAtual.DiaSemana = dto.DiaSemana;
                horarioAtual.HoraInicio = dto.HoraInicio;
                horarioAtual.HoraFim = dto.HoraFim;

                // Só Admin altera Ativo diretamente
                if (isAdmin && dto.Ativo.HasValue)
                {
                    horarioAtual.Ativo = dto.Ativo.Value;
                }

                await _horarioFuncionarioRepository.UpdateAsync(horarioAtual);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha(
                    "Ocorreu um erro ao alterar o horário do funcionário.");
            }
        }

        private async Task<UseCaseResult<bool>> ValidarAutorizacaoAsync(HorarioFuncionario horario,string userId,bool isFuncionario,bool isAdmin)
        {
            // Admin pode alterar qualquer horário
            if (isAdmin)
            {
                return UseCaseResult<bool>.Ok(true);
            }

            // Funcionário só pode alterar os próprios horários
            if (isFuncionario)
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository
                        .GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<bool>.Falha(
                        "Funcionário autenticado não encontrado.",
                        TipoErro.Proibido);
                }

                if (horario.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<bool>.Falha(
                        "Não tem permissão para alterar este horário.",
                        TipoErro.Proibido);
                }

                return UseCaseResult<bool>.Ok(true);
            }

            return UseCaseResult<bool>.Falha(
                "Utilizador sem permissão.",
                TipoErro.Proibido);
        }
    }
}
