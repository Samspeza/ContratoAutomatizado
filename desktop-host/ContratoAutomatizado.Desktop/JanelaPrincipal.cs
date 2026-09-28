using System.Diagnostics;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace ContratoAutomatizado.Desktop;

internal sealed class JanelaPrincipal : Form
{
    private readonly WebView2 _webView = new() { Dock = DockStyle.Fill, Visible = false };

    private readonly Label _mensagem = new()
    {
        Dock = DockStyle.Fill,
        TextAlign = ContentAlignment.MiddleCenter,
        Font = new Font("Segoe UI", 12f),
        Text = "Iniciando o sistema..."
    };

    private readonly ServidorLocal _servidor = new();

    public JanelaPrincipal()
    {
        Text = "Contrato Automatizado";
        Width = 1200;
        Height = 850;
        StartPosition = FormStartPosition.CenterScreen;

        Controls.Add(_webView);
        Controls.Add(_mensagem);

        Shown += async (_, _) => await IniciarAsync();
        FormClosed += (_, _) => _servidor.Dispose();
    }

    private async Task IniciarAsync()
    {
        try
        {
            var pastaDadosWebView = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ContratoAutomatizado",
                "WebView2");
            Directory.CreateDirectory(pastaDadosWebView);

            CoreWebView2Environment ambiente;
            try
            {
                ambiente = await CoreWebView2Environment.CreateAsync(null, pastaDadosWebView);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                MostrarErroEFechar(
                    "Falta um componente do Windows necessário para exibir o sistema (Microsoft Edge WebView2). " +
                    "Instale-o em https://developer.microsoft.com/microsoft-edge/webview2 e abra o sistema novamente.");
                return;
            }

            await _servidor.IniciarAsync(TimeSpan.FromSeconds(40));

            await _webView.EnsureCoreWebView2Async(ambiente);
            ConfigurarWebView();

            _webView.CoreWebView2.Navigate(_servidor.Endereco);
            _mensagem.Visible = false;
            _webView.Visible = true;
        }
        catch (ServidorIndisponivelException ex)
        {
            MostrarErroEFechar(ex.Message);
        }
        catch (Exception ex)
        {
            RegistrarErro(ex);
            MostrarErroEFechar(
                "Não foi possível iniciar o sistema. Feche o programa e tente novamente.");
        }
    }

    private void ConfigurarWebView()
    {
        var web = _webView.CoreWebView2;

        web.Settings.AreDevToolsEnabled = false;
        web.Settings.IsStatusBarEnabled = false;

        web.NavigationStarting += (_, e) =>
        {
            if (!e.Uri.StartsWith(_servidor.Endereco, StringComparison.OrdinalIgnoreCase))
            {
                e.Cancel = true;
                AbrirNoNavegadorPadrao(e.Uri);
            }
        };

        web.NewWindowRequested += (_, e) =>
        {
            e.Handled = true;
            AbrirNoNavegadorPadrao(e.Uri);
        };
    }

    private static void AbrirNoNavegadorPadrao(string endereco)
    {
        if (Uri.TryCreate(endereco, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }
    }

    private void MostrarErroEFechar(string mensagem)
    {
        MessageBox.Show(mensagem, "Contrato Automatizado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        Close();
    }

    private static void RegistrarErro(Exception ex)
    {
        try
        {
            var pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ContratoAutomatizado");
            Directory.CreateDirectory(pasta);
            File.AppendAllText(
                Path.Combine(pasta, "janela.log"),
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}");
        }
        catch
        {
            // se nem o registro do erro funcionar, não há mais o que fazer
        }
    }
}