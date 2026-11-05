namespace Contratos.Application.Interfaces;

public interface IConfiguracoesUsuarioService
{
    // Chave no formato "Secao:Subsecao:Nome". Valor nulo ou vazio remove a personalização
    // (volta a valer o padrão do appsettings.json). A mudança vale imediatamente.
    void Definir(string chave, string? valor);
}