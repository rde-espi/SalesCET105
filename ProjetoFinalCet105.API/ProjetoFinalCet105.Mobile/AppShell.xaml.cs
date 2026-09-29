using ProjetoFinalCet105.Mobile.Views;

namespace ProjetoFinalCet105.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();


        //Rota Login -> HomePage
        Routing.RegisterRoute( nameof(ClienteHomePage), typeof(ClienteHomePage));

        //Rota Marcações
        Routing.RegisterRoute( nameof(MarcacoesPage), typeof(MarcacoesPage));
        //Rota NovaMarcação
        Routing.RegisterRoute( nameof(NovaMarcacaoPage), typeof(NovaMarcacaoPage));
        //rota reagendamento
        Routing.RegisterRoute( nameof(ReagendarMarcacaoPage),typeof(ReagendarMarcacaoPage));
    }
}
