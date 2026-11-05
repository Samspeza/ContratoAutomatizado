using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Contratos.Application.Interfaces;
using Contratos.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Configuracoes;

// Guarda a senha criptografada com a proteção de dados do Windows (DPAPI, escopo do usuário atual):
// só este usuário, neste computador, consegue ler o arquivo. Copiá-lo para outro lugar não revela a senha.
[SupportedOSPlatform("windows")]
public sealed class SegredosDpapiService : ISegredosService
{
    private const string ChaveSenhaConfiguracao = "Envio:Email:Smtp:Senha";
    private static readonly byte[] EntropiaAdicional = Encoding.UTF8.GetBytes("ContratoAutomatizado.Email.Smtp");

    private readonly IConfiguration _configuration;
    private readonly string _caminhoArquivo;

    public SegredosDpapiService(IConfiguration configuration)
    {
        _configuration = configuration;
        _caminhoArquivo = Path.Combine(new PastasAplicacao(configuration).Configuracoes, "email-senha.protegida");
    }

    public string? ObterSenhaSmtp()
    {
        return LerArmazenada() ?? SenhaDaConfiguracao();
    }

    public OrigemSenha ObterOrigemSenhaSmtp()
    {
        if (LerArmazenada() is not null) return OrigemSenha.ArmazenamentoSeguro;
        return SenhaDaConfiguracao() is not null ? OrigemSenha.Configuracao : OrigemSenha.Nenhuma;
    }

    public void DefinirSenhaSmtp(string senha)
    {
        if (string.IsNullOrEmpty(senha))
            throw new ArgumentException("A senha não pode ser vazia.", nameof(senha));

        Directory.CreateDirectory(Path.GetDirectoryName(_caminhoArquivo)!);

        var protegida = ProtectedData.Protect(Encoding.UTF8.GetBytes(senha), EntropiaAdicional, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(_caminhoArquivo, protegida);
    }

    public void RemoverSenhaSmtp()
    {
        if (File.Exists(_caminhoArquivo)) File.Delete(_caminhoArquivo);
    }

    private string? LerArmazenada()
    {
        if (!File.Exists(_caminhoArquivo)) return null;

        try
        {
            var bytes = ProtectedData.Unprotect(File.ReadAllBytes(_caminhoArquivo), EntropiaAdicional, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (CryptographicException)
        {
            // Arquivo vindo de outro usuário ou computador: trata como "não configurada".
            return null;
        }
    }

    private string? SenhaDaConfiguracao()
    {
        var senha = _configuration[ChaveSenhaConfiguracao];
        return string.IsNullOrWhiteSpace(senha) ? null : senha;
    }
}