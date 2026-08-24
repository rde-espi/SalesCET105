namespace ProjetoFinalCet105.API.Entities
{
    public class DispositivoUser:IEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; }

        public string Fid { get; set; } = string.Empty;

        public string Plataforma { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime? DataAtualizacao { get; set; }
    }
}
