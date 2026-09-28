using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.Web.Models;
using ProjetoFinalCet105.Web.Services;

namespace ProjetoFinalCet105.Web.Controllers.Publico
{
    public class ServicosController : Controller
    {
        private readonly ApiService _apiService;

        public ServicosController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index( int? categoriaId = null, string? pesquisa = null)
        {
            var servicos = await _apiService.GetAsync<List<ServicoViewModel>>( "api/Servicos") ?? new List<ServicoViewModel>();

            var categorias = await _apiService.GetAsync<List<CategoriaViewModel>>( "api/Categorias") ?? new List<CategoriaViewModel>();

            servicos = servicos
                .Where(s => s.Disponivel)
                .OrderBy(s => s.Nome)
                .ToList();

            var model = new ServicosPublicoViewModel
            {
                Categorias = categorias
                    .OrderBy(c => c.Nome)
                    .ToList(),

                Servicos = servicos,

                CategoriaSelecionadaId = categoriaId,

                Pesquisa = pesquisa
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var servico = await _apiService.GetAsync<ServicoViewModel>( $"api/Servicos/{id}");

                if (servico == null || !servico.Disponivel)
                {
                    return NotFound();
                }

                var funcionarios = await _apiService.GetAsync<List<FuncionarioViewModel>>( $"api/Funcionarios/servico/{id}") ?? new List<FuncionarioViewModel>();

                var profissionais = new List<FuncionarioMarcacaoClienteViewModel>();

                foreach (var funcionario in funcionarios)
                {
                    var resumo = await _apiService.GetAsync<FeedbackResumoViewModel>( $"api/Feedbacks/funcionario/{funcionario.Id}/resumo");

                    profissionais.Add(new FuncionarioMarcacaoClienteViewModel
                    {
                        Id = funcionario.Id,
                        NomeCompleto = funcionario.NomeCompleto,
                        Biografia = funcionario.Biografia,

                        MediaAvaliacao = resumo?.Media ?? 0,
                        TotalAvaliacoes = resumo?.TotalAvaliacoes ?? 0,

                        FotografiaUrl = Url.Action( nameof(FotografiaFuncionario), "Servicos", new { id = funcionario.Id })
                    });
                }

                ViewBag.Profissionais = profissionais
                    .OrderBy(f => f.NomeCompleto)
                    .ToList();
                                
                return View(servico);
            }
            catch
            {
                TempData["ErrorMessage"] = "Não foi possível carregar os detalhes do serviço.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> FotografiaFuncionario(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            using var response = await _apiService.GetResponseAsync( $"api/Funcionarios/{id}/fotografia");

            if (response == null || !response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            if (bytes.Length == 0)
            {
                return NotFound();
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

            return File(bytes, contentType);
        }

        [HttpGet]
        public async Task<IActionResult> Imagem(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            using var response =await _apiService.GetResponseAsync( $"api/Servicos/{id}/imagem");

            if (response == null || !response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();

            if (bytes.Length == 0)
            {
                return NotFound();
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

            return File(bytes, contentType);
        }



    }

}