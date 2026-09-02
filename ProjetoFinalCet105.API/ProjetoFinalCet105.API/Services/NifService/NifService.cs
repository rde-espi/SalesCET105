using ProjetoFinalCet105.API.DTOs;

namespace ProjetoFinalCet105.API.Services.NifService
{
    public class NifService : INifService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NifService> _logger;

        public NifService( HttpClient httpClient, IConfiguration configuration, ILogger<NifService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public bool ValidarNifPortugues(string nif)
        {
            if (string.IsNullOrWhiteSpace(nif))
            {
                return false;
            }

            nif = nif.Trim();

            if (nif.Length != 9 || !nif.All(char.IsDigit))
            {
                return false;
            }

            var soma = 0;

            for (var i = 0; i < 8; i++)
            {
                var digito = nif[i] - '0';

                soma += digito * (9 - i);
            }

            var resto = soma % 11;

            var digitoControlo = resto < 2 ? 0  : 11 - resto;

            var ultimoDigito = nif[8] - '0';

            return digitoControlo == ultimoDigito;
        }

        public async Task<ResultadoValidacaoNifDTO> ValidarAsync( string nif)
        {
            var resultado = new ResultadoValidacaoNifDTO
            {
                Nif = nif, FormatoValido = ValidarNifPortugues(nif)
            };

            // Se falhar matematicamente, nem consulta o NIF.PT.
            if (!resultado.FormatoValido)
            {
                return resultado;
            }

            var apiKey = _configuration["NifPt:ApiKey"];
            var baseUrl = _configuration["NifPt:BaseUrl"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogWarning("A configuração da API NIF.PT não está disponível.");

                return resultado;
            }

            try
            {
                var url = $"{baseUrl}?json=1&q={Uri.EscapeDataString(nif)}" +
                    $"&key={Uri.EscapeDataString(apiKey)}";

                var resposta = await _httpClient.GetFromJsonAsync<NifPtResponseDTO>(url);

                if (resposta == null)
                {
                    return resultado;
                }

                resultado.VerificadoExternamente = true;

                resultado.EncontradoExternamente =
                    resposta.Result == "success" &&
                    resposta.Nif_Validation &&
                    resposta.Is_Nif;

                if (resposta.Records != null && resposta.Records.TryGetValue(nif, out var record))
                {
                    resultado.Nome = record.Title;
                    resultado.Estado = record.Status;
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,"Não foi possível consultar o NIF {Nif} no serviço NIF.PT.", nif);

                return resultado;
            }
        }
    }
}
