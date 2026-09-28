using System.Net;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ProjetoFinalCet105.API.DTOs;

using ProjetoFinalCet105.API.Services.EmailService;

namespace ProjetoFinalCet105.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContactosController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ContactosController(
        IEmailService emailService,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Enviar(ContactoDTO dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var emailInfinityBeauty = _configuration["EmailSettings:SenderEmail"];

        if (string.IsNullOrWhiteSpace(emailInfinityBeauty))
            return StatusCode(StatusCodes.Status500InternalServerError, "O email do Infinity Beauty não está configurado.");

        var assunto = $"Contacto através do site - {dto.Assunto}";

        var mensagem = $"""
                <h2>Novo contacto através do site</h2>

                <p><strong>Nome:</strong> {WebUtility.HtmlEncode(dto.Nome)}</p>
                <p><strong>Email:</strong> {WebUtility.HtmlEncode(dto.Email)}</p>
                <p><strong>Assunto:</strong> {WebUtility.HtmlEncode(dto.Assunto)}</p>

                <hr>

                <p><strong>Mensagem:</strong></p>

                <p>
                    {WebUtility.HtmlEncode(dto.Mensagem)
                    .Replace("\r\n", "<br>")
                    .Replace("\n", "<br>")}
                </p>
                """;

        await _emailService.EnviarEmailAsync(emailInfinityBeauty, assunto, mensagem);

        return Ok(new
        {
            mensagem = "Mensagem enviada com sucesso."
        });
    }
}