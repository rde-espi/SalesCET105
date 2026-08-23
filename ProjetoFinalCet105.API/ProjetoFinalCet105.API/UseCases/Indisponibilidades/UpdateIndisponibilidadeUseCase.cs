using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.DTOs;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;
using ProjetoFinalCet105.API.Services.IndisponibilidadeService;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API.UseCases.Indisponibilidades
{
    public class UpdateIndisponibilidadeUseCase
    {
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IFuncionarioRepository _funcionarioRepository;
        private readonly IIndisponibilidadeService _indisponibilidadeService;

        public UpdateIndisponibilidadeUseCase(
            IIndisponibilidadeRepository indisponibilidadeRepository,
            IFuncionarioRepository funcionarioRepository,
            IIndisponibilidadeService indisponibilidadeService)
        {
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _funcionarioRepository = funcionarioRepository;
            _indisponibilidadeService = indisponibilidadeService;
        }

        public async Task<UseCaseResult<bool>> ExecuteAsync(int id,string userId,bool isFuncionario,bool isAdmin,UpdateIndisponibilidadeDTO dto)
        {
            // 1. Verificar se a indisponibilidade existe
            var indisponibilidade =
                await _indisponibilidadeRepository.GetByIdAsync(id);

            if (indisponibilidade == null)
            {
                return UseCaseResult<bool>.Falha(
                    "Indisponibilidade não encontrada.",
                    TipoErro.NaoEncontrado);
            }


            // 2. Determinar o funcionário e validar autorização
            var funcionarioIdResult =
                await ObterFuncionarioIdAsync(
                    indisponibilidade,
                    userId,
                    isFuncionario,
                    isAdmin,
                    dto.FuncionarioId);

            if (!funcionarioIdResult.Sucesso)
            {
                return UseCaseResult<bool>.Falha(
                    funcionarioIdResult.Erro!,
                    funcionarioIdResult.TipoErro);
            }

            var funcionarioId = funcionarioIdResult.Dados;


            // 3. Validar funcionário
            var funcionario =
                await _funcionarioRepository
                    .GetFuncionarioByIdAsync(funcionarioId);

            if (funcionario == null)
            {
                return UseCaseResult<bool>.Falha(
                    "O funcionário indicado não existe.",
                    TipoErro.NaoEncontrado);
            }

            if (!funcionario.Ativo)
            {
                return UseCaseResult<bool>.Falha(
                    "O funcionário indicado não está ativo.");
            }


            // 4. DiaCompleto e RestoDoDia não podem ser true ao mesmo tempo
            var validacaoTipo = _indisponibilidadeService.ValidarTipoIndisponibilidade(dto.DiaCompleto, dto.RestoDoDia);

            if (!validacaoTipo.Sucesso)
            {
                return UseCaseResult<bool>.Falha(
                    validacaoTipo.Erro!,
                    validacaoTipo.TipoErro);
            }


            // 5. Obter horário de trabalho
            var horariosTrabalho = await _indisponibilidadeService.ObterHorariosTrabalhoAsync(funcionarioId, dto.DataHoraInicio);
           

            if (!horariosTrabalho.Any())
            {
                return UseCaseResult<bool>.Falha(
                    "O funcionário não possui horário de trabalho definido para este dia.");
            }


            // 6. Obter marcações do dia
            var marcacoesDoDia = await _indisponibilidadeService.ObterMarcacoesDoDiaAsync(funcionarioId,dto.DataHoraInicio);
                
            var marcacoesConfirmadas = marcacoesDoDia
                .Where(m => m.EstadoMarcacao.Nome == "Confirmada")
                .ToList();

            var marcacoesConcluidas = marcacoesDoDia
                .Where(m => m.EstadoMarcacao.Nome == "Concluida")
                .ToList();


            // 7. Calcular o período real
            var periodoResult = _indisponibilidadeService.CalcularPeriodo(
               dto.DataHoraInicio,
               dto.DataHoraFim,
               dto.DiaCompleto,
               dto.RestoDoDia,
               horariosTrabalho,
               marcacoesConcluidas);

            if (!periodoResult.Sucesso)
            {
                return UseCaseResult<bool>.Falha(
                    periodoResult.Erro!,
                    periodoResult.TipoErro);
            }

            var inicio = periodoResult.Dados.Inicio;
            var fim = periodoResult.Dados.Fim;


            // 8. Funcionário não pode alterar para um período passado
            if (!isAdmin && !dto.RestoDoDia && inicio <= DateTime.Now)
            {
                return UseCaseResult<bool>.Falha("Não é possível alterar a indisponibilidade para uma data/hora passada.");
            }


            // 9. Não pode colidir com marcações confirmadas
            var conflitoMarcacao =
                _indisponibilidadeService.ValidarConflitoComMarcacoesConfirmadas(
                    inicio,
                    fim,
                    marcacoesConfirmadas);

            if (!conflitoMarcacao.Sucesso)
            {
                return UseCaseResult<bool>.Falha(
                    conflitoMarcacao.Erro!,
                    conflitoMarcacao.TipoErro);
            }


            // 10. Verificar sobreposição com OUTRAS indisponibilidades
            // O próprio id é ignorado
            var existeSobreposicao = await _indisponibilidadeService.ExisteSobreposicaoAsync(funcionarioId, inicio, fim, id);

            if (existeSobreposicao)
            {
                return UseCaseResult<bool>.Falha(
                    "Já existe outra indisponibilidade sobreposta para este funcionário.",
                    TipoErro.Conflito);
            }

            // 11. Atualizar
            try
            {
                indisponibilidade.FuncionarioId = funcionarioId;

                indisponibilidade.DataHoraInicio = inicio;
                indisponibilidade.DataHoraFim = fim;

                indisponibilidade.Motivo = dto.Motivo;

                indisponibilidade.DiaCompleto = dto.DiaCompleto;
                indisponibilidade.RestoDoDia = dto.RestoDoDia;

                await _indisponibilidadeRepository
                    .UpdateAsync(indisponibilidade);

                return UseCaseResult<bool>.Ok(true);
            }
            catch (Exception)
            {
                return UseCaseResult<bool>.Falha(
                    "Ocorreu um erro ao alterar a indisponibilidade.");
            }
        }

        private async Task<UseCaseResult<int>> ObterFuncionarioIdAsync(Indisponibilidade indisponibilidade,string userId,bool isFuncionario,bool isAdmin,int? funcionarioIdDto)
        {
            // Funcionário só pode alterar as próprias indisponibilidades
            if (isFuncionario && !isAdmin)
            {
                var funcionarioAutenticado =
                    await _funcionarioRepository
                        .GetFuncionarioByUserIdAsync(userId);

                if (funcionarioAutenticado == null)
                {
                    return UseCaseResult<int>.Falha(
                        "Funcionário autenticado não encontrado.",
                        TipoErro.Proibido);
                }

                if (indisponibilidade.FuncionarioId != funcionarioAutenticado.Id)
                {
                    return UseCaseResult<int>.Falha(
                        "Não tem permissão para alterar esta indisponibilidade.",
                        TipoErro.Proibido);
                }

                return UseCaseResult<int>.Ok(
                    funcionarioAutenticado.Id);
            }

            // Admin pode manter ou alterar o funcionário
            if (isAdmin)
            {
                return UseCaseResult<int>.Ok(
                    funcionarioIdDto
                    ?? indisponibilidade.FuncionarioId);
            }

            return UseCaseResult<int>.Falha(
                "Utilizador sem permissão.",
                TipoErro.Proibido);
        }

    }
}
