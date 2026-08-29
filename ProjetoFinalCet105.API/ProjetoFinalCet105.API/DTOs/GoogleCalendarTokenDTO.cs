namespace ProjetoFinalCet105.API.DTOs
{
    public class GoogleCalendarTokenDTO
    {
        public string RefreshToken { get; set; } = null!;
        public string? GoogleEmail { get; set; }
    }
}
