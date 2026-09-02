using ProjetoFinalCet105.API.DTOs;

namespace ProjetoFinalCet105.API.Services.NifService
{
    public interface INifService
    {
        bool ValidarNifPortugues(string nif);
        Task<ResultadoValidacaoNifDTO> ValidarAsync(string nif);
    }
}
