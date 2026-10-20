using System.Text;
using Contratos.Application.Interfaces;
using Contratos.Application.Messages;
using Contratos.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Envio;

public sealed class ModeloMensagemArquivoProvider : IModeloMensagemProvider
{
    private const string NomeArquivo = "mensagem-email.txt";
    private const string PrefixoAssunto = "Assunto:";

    // Texto fornecido pelo usuário, gravado só na primeira execução.
    // Depois disso, o que vale é o arquivo em Configuracoes (editável).
    private const string ConteudoPadrao = """
        Assunto: Carta de Contratação, Portaria CAT 28/2020

        Prezado(a),
        Encaminho, em anexo, Carta de Contratação referente - Recuperação de ICMS Substituição Tributária – Portaria CAT 42/2018 .
        Peço, por gentileza, que revise o documento e, estando de acordo, proceda com a assinatura. Após assinado, favor nos encaminhar a via digitalizada por este e-mail.
        Agradeço a confiança em nosso trabalho e permaneço à disposição para quaisquer esclarecimentos.
        Cordialmente,
        Leandro Mello
        Diretor Comercial | Mello Consultoria Tributária
        Ressarcimento ICMS-ST - 17 anos de experiência gerando economia tributária para empresas
        (17) 99783-8371
        www.consultoriamello.com.br
        """;

    private readonly string _pastaConfiguracoes;

    public ModeloMensagemArquivoProvider(IConfiguration configuration)
    {
        _pastaConfiguracoes = new PastasAplicacao(configuration).Configuracoes;
    }

    private string CaminhoArquivo => Path.Combine(_pastaConfiguracoes, NomeArquivo);

    public ModeloMensagem ObterModeloEmail()
    {
        GarantirArquivoPadrao();
        return Interpretar(File.ReadAllText(CaminhoArquivo, Encoding.UTF8));
    }

    private void GarantirArquivoPadrao()
    {
        if (File.Exists(CaminhoArquivo)) return;

        Directory.CreateDirectory(_pastaConfiguracoes);
        File.WriteAllText(CaminhoArquivo, ConteudoPadrao, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static ModeloMensagem Interpretar(string texto)
    {
        var linhas = texto.Replace("\r\n", "\n").Split('\n');

        if (!linhas[0].StartsWith(PrefixoAssunto, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "O arquivo da mensagem de e-mail está fora do formato esperado: a primeira linha deve começar com \"Assunto:\".");
        }

        var assunto = linhas[0][PrefixoAssunto.Length..].Trim();
        var corpo = string.Join("\n", linhas.Skip(1)).Trim();

        if (assunto.Length == 0 || corpo.Length == 0)
        {
            throw new InvalidOperationException(
                "A mensagem de e-mail precisa ter assunto e corpo preenchidos. Verifique o arquivo de mensagem.");
        }

        return new ModeloMensagem(assunto, corpo);
    }
}