namespace ContratoAutomatizado.Desktop;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(true, @"Local\ContratoAutomatizado.Desktop", out var primeiraInstancia);

        if (!primeiraInstancia)
        {
            MessageBox.Show(
                "O Contrato Automatizado já está aberto.",
                "Contrato Automatizado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new JanelaPrincipal());
    }
}