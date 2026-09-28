using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ContratoAutomatizado.Desktop;

internal sealed class ServidorIndisponivelException : Exception
{
    public ServidorIndisponivelException(string mensagem) : base(mensagem) { }
}

internal sealed class ServidorLocal : IDisposable
{
    private Process? _processo;

    public string Endereco { get; private set; } = string.Empty;

    private static string CaminhoExecutavel =>
        Path.Combine(AppContext.BaseDirectory, "servidor", "Contratos.Api.exe");

    public async Task IniciarAsync(TimeSpan tempoLimite)
    {
        if (!File.Exists(CaminhoExecutavel))
        {
            throw new ServidorIndisponivelException(
                "Alguns arquivos do sistema não foram encontrados. Reinstale o programa.");
        }

        var porta = EncontrarPortaLivre();
        Endereco = $"http://127.0.0.1:{porta}";

        var informacoes = new ProcessStartInfo
        {
            FileName = CaminhoExecutavel,
            WorkingDirectory = Path.GetDirectoryName(CaminhoExecutavel)!,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        informacoes.ArgumentList.Add("--urls");
        informacoes.ArgumentList.Add(Endereco);
        informacoes.ArgumentList.Add("--parent-pid");
        informacoes.ArgumentList.Add(Environment.ProcessId.ToString());

        _processo = Process.Start(informacoes)
            ?? throw new ServidorIndisponivelException("Não foi possível iniciar o sistema.");

        await AguardarFicarProntoAsync(tempoLimite);
    }

    private async Task AguardarFicarProntoAsync(TimeSpan tempoLimite)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var limite = DateTime.UtcNow + tempoLimite;

        while (DateTime.UtcNow < limite)
        {
            if (_processo!.HasExited)
            {
                throw new ServidorIndisponivelException(
                    "O sistema foi encerrado logo após iniciar. Consulte os registros em Documentos\\ContratoAutomatizado\\Logs.");
            }

            try
            {
                var resposta = await httpClient.GetAsync($"{Endereco}/api/health");
                if (resposta.IsSuccessStatusCode)
                    return;
            }
            catch (HttpRequestException)
            {
                // servidor ainda subindo — tenta de novo
            }
            catch (TaskCanceledException)
            {
                // resposta demorou — tenta de novo
            }

            await Task.Delay(300);
        }

        throw new ServidorIndisponivelException(
            "O sistema demorou mais do que o esperado para iniciar. Feche e abra novamente.");
    }

    private static int EncontrarPortaLivre()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var porta = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return porta;
    }

    public void Dispose()
    {
        if (_processo is null) return;

        try
        {
            if (!_processo.HasExited)
            {
                _processo.Kill(entireProcessTree: true);
                _processo.WaitForExit(3000);
            }
        }
        catch (InvalidOperationException)
        {
            // processo já encerrado
        }
        finally
        {
            _processo.Dispose();
            _processo = null;
        }
    }
}