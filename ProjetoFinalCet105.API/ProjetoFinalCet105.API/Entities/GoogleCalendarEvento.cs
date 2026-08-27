namespace ProjetoFinalCet105.API.Entities
{
    public class GoogleCalendarEvento:IEntity
    {
        public int Id { get; set; }

        public int MarcacaoId { get; set; }
        public Marcacao Marcacao { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        public string GoogleEventId { get; set; } = null!;

        public string CalendarId { get; set; } = "primary";

        public DateTime DataCriacao { get; set; }
        public DateTime? DataUltimaSincronizacao { get; set; }
    }
}
