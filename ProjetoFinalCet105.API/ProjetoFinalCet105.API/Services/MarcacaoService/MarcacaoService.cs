using Microsoft.EntityFrameworkCore;
using ProjetoFinalCet105.API.Entities;
using ProjetoFinalCet105.API.Repositories;

namespace ProjetoFinalCet105.API.Services.MarcacaoService
{
    public class MarcacaoService : IMarcacaoService
    {
        private readonly IFuncionarioServicoRepository _funcionarioServicoRepository;
        private readonly IHorarioFuncionarioRepository _horarioFuncionarioRepository;
        private readonly IIndisponibilidadeRepository _indisponibilidadeRepository;
        private readonly IMarcacaoRepository _marcacaoRepository;

        public MarcacaoService(
            IFuncionarioServicoRepository funcionarioServicoRepository,
            IHorarioFuncionarioRepository horarioFuncionarioRepository,
            IIndisponibilidadeRepository indisponibilidadeRepository,
            IMarcacaoRepository marcacaoRepository)
        {
            _funcionarioServicoRepository = funcionarioServicoRepository;
            _horarioFuncionarioRepository = horarioFuncionarioRepository;
            _indisponibilidadeRepository = indisponibilidadeRepository;
            _marcacaoRepository = marcacaoRepository;
        }

        public async Task<FuncionarioServico?> GetFuncionarioServicoAsync(int funcionarioId,int servicoId)
        {
            return await _funcionarioServicoRepository
                .GetAll()
                .FirstOrDefaultAsync(fs =>
                    fs.FuncionarioId == funcionarioId &&
                    fs.ServicoId == servicoId &&
                    fs.Ativo);
        }

        public async Task<bool> HorarioValidoAsync(int funcionarioId,DateTime inicio,DateTime fim)
        {
            var diaSemana = inicio.DayOfWeek;

            return await _horarioFuncionarioRepository
                .GetAll()
                .AnyAsync(h =>
                    h.FuncionarioId == funcionarioId &&
                    h.DiaSemana == diaSemana &&
                    h.Ativo &&
                    inicio.TimeOfDay >= h.HoraInicio &&
                    fim.TimeOfDay <= h.HoraFim);
        }

        public async Task<bool> ExisteIndisponibilidadeAsync(int funcionarioId,DateTime inicio,DateTime fim)
        {
            return await _indisponibilidadeRepository
                .GetAll()
                .AnyAsync(i =>
                    i.FuncionarioId == funcionarioId &&
                    inicio < i.DataHoraFim &&
                    fim > i.DataHoraInicio);
        }

        public async Task<bool> ExisteSobreposicaoAsync(int funcionarioId,DateTime inicio, DateTime fim,int? marcacaoIgnorarId = null)
        {
            return await _marcacaoRepository.ExisteSobreposicaoAsync(
                funcionarioId,
                inicio,
                fim,
                marcacaoIgnorarId);
        }
    }
}
