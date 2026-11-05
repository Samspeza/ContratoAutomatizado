namespace Contratos.Application.Interfaces;

public enum OrigemSenha
{
    Nenhuma,
    ArmazenamentoSeguro,
    Configuracao
}

public interface ISegredosService
{
    // Senha guardada com segurança neste computador; se não houver, a da configuração
    // (User Secrets / variável de ambiente, uso técnico).
    string? ObterSenhaSmtp();
    OrigemSenha ObterOrigemSenhaSmtp();
    void DefinirSenhaSmtp(string senha);
    void RemoverSenhaSmtp();
}