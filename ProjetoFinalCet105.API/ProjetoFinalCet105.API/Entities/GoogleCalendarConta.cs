namespace ProjetoFinalCet105.API.Entities
{
    public class GoogleCalendarConta : IEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public string CalendarId { get; set; } = "primary";

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public string? GoogleEmail { get; set; }
    }
}
