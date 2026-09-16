using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProjetoFinalCet105.Web.Controllers.Funcionario;

[Authorize(Roles = "Funcionario")]
public class FuncionariosController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}