namespace ProjetoFinalCet105.API.DTOs
{
    public class NifPtResponseDTO
    {
        public string? Result { get; set; }

        public Dictionary<string, NifPtRecordDTO>? Records { get; set; }

        public bool Nif_Validation { get; set; }

        public bool Is_Nif { get; set; }
    }

    public class NifPtRecordDTO
    {
        public long Nif { get; set; }

        public string? Title { get; set; }

        public string? Status { get; set; }
    }
}
