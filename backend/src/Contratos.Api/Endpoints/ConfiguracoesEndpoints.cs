using System.Diagnostics;
using Contratos.Application.DTOs;
using Contratos.Application.Interfaces;
using Contratos.Application.Messages;
using Contratos.Domain.Exceptions;
using Contratos.Infrastructure.Storage;

namespace Contratos.Api.Endpoints;

public static class ConfiguracoesEndpoints
{
    private const string ChavePastaContratos = "Armazenamento:PastaContratosGerados";
    private const string ChaveDiasParado = "Acompanhamento:DiasParado";
    private const string ChaveLibreOffice = "Pdf:CaminhoExecutavelLibreOffice";

    public static void MapConfiguracoesEndpoints(this WebApplication app)
    {
        var grupo = app.MapGroup("/api/configuracoes");

        grupo.MapGet("", ObterConfiguracoes);
        grupo.MapGet("/verificacao", (IVerificacaoSistemaService verificacao) => Results.Ok(verificacao.Verificar()));

        grupo.MapGet("/mensagem-email", (IModeloMensagemProvider modelos) => Results.Ok(MontarMensagem(modelos)));
        grupo.MapPut("/mensagem-email", SalvarMensagemEmail);

        grupo.MapPut("/email/senha", DefinirSenhaEmail);
        grupo.MapDelete("/email/senha", RemoverSenhaEmail);
        grupo.MapPost("/email/testar", TestarConexaoEmail);

        grupo.MapPut("/armazenamento", AtualizarArmazenamento);
        grupo.MapPut("/acompanhamento", AtualizarAcompanhamento);
        grupo.MapPut("/pdf", AtualizarPdf);
        grupo.MapPost("/abrir-pasta", AbrirPasta);
    }

    private static IResult ObterConfiguracoes(IConfiguration configuracao, ISegredosService segredos) =>
        Results.Ok(MontarConfiguracoes(configuracao, segredos));

    private static ConfiguracoesDto MontarConfiguracoes(IConfiguration c, ISegredosService segredos)
    {
        var pastas = new PastasAplicacao(c);
        var origem = segredos.ObterOrigemSenhaSmtp();

        return new ConfiguracoesDto(
            new ConfiguracaoEmailDto(
                c["Envio:Email:Remetente"] ?? string.Empty,
                c["Envio:Email:Smtp:Host"] ?? string.Empty,
                c.GetValue<int?>("Envio:Email:Smtp:Porta") ?? 0,
                c["Envio:Email:Smtp:Usuario"] ?? string.Empty,
                origem != OrigemSenha.Nenhuma,
                origem.ToString()),
            new ConfiguracaoArmazenamentoDto(
                pastas.ContratosGerados,
                Path.Combine(pastas.Raiz, "ContratosGerados"),
                !string.IsNullOrWhiteSpace(c[ChavePastaContratos]),
                pastas.Templates,
                pastas.Logs,
                pastas.Dados),
            new ConfiguracaoAcompanhamentoDto(Math.Clamp(c.GetValue<int?>(ChaveDiasParado) ?? 3, 1, 60)),
            new ConfiguracaoPdfDto(c[ChaveLibreOffice] ?? string.Empty));
    }

    private static MensagemEmailConfiguradaDto MontarMensagem(IModeloMensagemProvider modelos)
    {
        var padrao = modelos.ObterModeloPadraoEmail();
        ModeloMensagem atual;
        string? aviso = null;

        try
        {
            atual = modelos.ObterModeloEmail();
        }
        catch (InvalidOperationException)
        {
            atual = padrao;
            aviso = "O arquivo da mensagem estava fora do formato esperado. Mostrando o texto original; ao salvar, o arquivo será corrigido.";
        }

        var placeholders = PlaceholdersMensagem.Disponiveis
            .Select(p => new PlaceholderDto(p.Nome, p.Descricao, p.Exemplo))
            .ToList();

        return new MensagemEmailConfiguradaDto(atual.Assunto, atual.Corpo, padrao.Assunto, padrao.Corpo, placeholders, aviso);
    }

    private static IResult SalvarMensagemEmail(SalvarMensagemEmailDto dados, IModeloMensagemProvider modelos)
    {
        try
        {
            modelos.SalvarModeloEmail(new ModeloMensagem(dados.Assunto ?? string.Empty, dados.Corpo ?? string.Empty));
            return Results.Ok(MontarMensagem(modelos));
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { mensagem = ex.Message });
        }
    }

    private static IResult DefinirSenhaEmail(DefinirSenhaEmailDto dados, ISegredosService segredos, IConfiguration configuracao)
    {
        if (string.IsNullOrEmpty(dados.Senha))
            return Results.BadRequest(new { mensagem = "Informe a senha." });

        if (dados.Senha.Length > 256)
            return Results.BadRequest(new { mensagem = "A senha informada é longa demais." });

        segredos.DefinirSenhaSmtp(dados.Senha);
        return Results.Ok(MontarConfiguracoes(configuracao, segredos));
    }

    private static IResult RemoverSenhaEmail(ISegredosService segredos, IConfiguration configuracao)
    {
        segredos.RemoverSenhaSmtp();
        return Results.Ok(MontarConfiguracoes(configuracao, segredos));
    }

    private static async Task<IResult> TestarConexaoEmail(IEmailEnvioService emailEnvio)
    {
        try
        {
            await emailEnvio.TestarConexaoAsync();
            return Results.Ok(new { mensagem = "Conexão com o servidor de e-mail funcionando." });
        }
        catch (EnvioEmailFalhouException ex)
        {
            return Results.Json(new { mensagem = ex.Message }, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static IResult AtualizarArmazenamento(
        AtualizarPastaContratosDto dados,
        IConfiguracoesUsuarioService usuario,
        ISegredosService segredos,
        IConfiguration configuracao)
    {
        var pasta = dados.Pasta?.Trim();

        if (string.IsNullOrWhiteSpace(pasta))
        {
            usuario.Definir(ChavePastaContratos, null);
            return Results.Ok(MontarConfiguracoes(configuracao, segredos));
        }

        if (!Path.IsPathFullyQualified(pasta))
        {
            return Results.BadRequest(new
            {
                mensagem = "Informe o caminho completo da pasta, por exemplo D:\\Contratos."
            });
        }

        try
        {
            Directory.CreateDirectory(pasta);
            var teste = Path.Combine(pasta, $".verificacao-{Guid.NewGuid():N}");
            File.WriteAllText(teste, "ok");
            File.Delete(teste);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return Results.BadRequest(new
            {
                mensagem = "Não foi possível gravar nessa pasta. Escolha outra pasta ou verifique a permissão de acesso."
            });
        }

        usuario.Definir(ChavePastaContratos, pasta);
        return Results.Ok(MontarConfiguracoes(configuracao, segredos));
    }

    private static IResult AtualizarAcompanhamento(
        AtualizarDiasParadoDto dados,
        IConfiguracoesUsuarioService usuario,
        ISegredosService segredos,
        IConfiguration configuracao)
    {
        if (dados.Dias < 1 || dados.Dias > 60)
            return Results.BadRequest(new { mensagem = "Informe uma quantidade de dias entre 1 e 60." });

        usuario.Definir(ChaveDiasParado, dados.Dias.ToString());
        return Results.Ok(MontarConfiguracoes(configuracao, segredos));
    }

    private static IResult AtualizarPdf(
        AtualizarCaminhoLibreOfficeDto dados,
        IConfiguracoesUsuarioService usuario,
        ISegredosService segredos,
        IConfiguration configuracao)
    {
        var caminho = dados.Caminho?.Trim();

        if (!string.IsNullOrWhiteSpace(caminho) && !File.Exists(caminho))
            return Results.BadRequest(new { mensagem = "Arquivo não encontrado nesse caminho. Confira se o LibreOffice está instalado." });

        usuario.Definir(ChaveLibreOffice, caminho);
        return Results.Ok(MontarConfiguracoes(configuracao, segredos));
    }

    private static IResult AbrirPasta(AbrirPastaDto dados, IConfiguration configuracao, ILogger<Program> logger)
    {
        var pastas = new PastasAplicacao(configuracao);

        var caminho = dados.Pasta switch
        {
            "contratos" => pastas.ContratosGerados,
            "modelos" => pastas.Templates,
            "logs" => pastas.Logs,
            _ => null
        };

        if (caminho is null)
            return Results.BadRequest(new { mensagem = "Pasta desconhecida." });

        try
        {
            Directory.CreateDirectory(caminho);
            Process.Start(new ProcessStartInfo { FileName = caminho, UseShellExecute = true });
            return Results.Ok(new { mensagem = "Pasta aberta." });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Não foi possível abrir a pasta {Pasta}.", caminho);
            return Results.BadRequest(new { mensagem = $"Não foi possível abrir a pasta. Ela fica em {caminho}." });
        }
    }
}