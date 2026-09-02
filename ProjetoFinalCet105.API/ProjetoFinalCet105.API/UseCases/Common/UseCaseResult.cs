namespace ProjetoFinalCet105.API.UseCases.Common
{
    public class UseCaseResult<T>
    {
        public bool Sucesso { get; set; }
        public T? Dados { get; set; }
        public string? Erro { get; set; }
        public TipoErro TipoErro { get; set; }

        public static UseCaseResult<T> Ok(T dados)
        {
            return new UseCaseResult<T>
            {
                Sucesso = true,
                Dados = dados,
                TipoErro = TipoErro.Nenhum
            };
        }

        public static UseCaseResult<T> Falha( string erro, TipoErro tipoErro = TipoErro.Validacao)
        {
            return new UseCaseResult<T>
            {
                Sucesso = false,
                Erro = erro,
                TipoErro = tipoErro
            };
        }
    }
}
