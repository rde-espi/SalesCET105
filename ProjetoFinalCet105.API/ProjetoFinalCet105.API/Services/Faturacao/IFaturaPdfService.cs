using ProjetoFinalCet105.API.DTOs;

namespace ProjetoFinalCet105.API.Services.Faturacao
{
    public interface IFaturaPdfService
    {
        byte[] GerarPdf(FaturaDTO fatura);
    }
}
