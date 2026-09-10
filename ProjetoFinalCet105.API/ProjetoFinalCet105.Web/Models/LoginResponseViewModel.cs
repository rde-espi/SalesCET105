namespace ProjetoFinalCet105.Web.Models
{
    public class LoginResponseViewModel
    {
        public string? Token { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string NomeCompleto { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool RequiresTwoFactor { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();
    }
}