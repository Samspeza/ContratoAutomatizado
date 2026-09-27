namespace Contratos.Application.Interfaces;

public interface IArmazenamentoContratoService
{
    string ObterPastaContratosGerados();
    string DeterminarNomeBaseDisponivel(string cnpj);
    string SalvarArquivo(byte[] conteudo, string nomeBase, string extensaoComPonto);
}