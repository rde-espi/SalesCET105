using Microsoft.AspNetCore.Mvc;
using ProjetoFinalCet105.API.UseCases.Common;

namespace ProjetoFinalCet105.API
{
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult TratarErro<T>(UseCaseResult<T> resultado)
        {
            return resultado.TipoErro switch
            {
                TipoErro.NaoEncontrado =>
                    NotFound(resultado.Erro),

                TipoErro.Proibido =>
                    StatusCode(
                        StatusCodes.Status403Forbidden,
                        resultado.Erro),

                TipoErro.Conflito =>
                    Conflict(resultado.Erro),

                _ =>
                    BadRequest(resultado.Erro)
            };
        }

        protected ActionResult<T> TratarErroComDados<T>(
            UseCaseResult<T> resultado)
        {
            return resultado.TipoErro switch
            {
                TipoErro.NaoEncontrado =>
                    NotFound(resultado.Erro),

                TipoErro.Proibido =>
                    StatusCode(
                        StatusCodes.Status403Forbidden,
                        resultado.Erro),

                TipoErro.Conflito =>
                    Conflict(resultado.Erro),

                _ =>
                    BadRequest(resultado.Erro)
            };
        }
    }
}
