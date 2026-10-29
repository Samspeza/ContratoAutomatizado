using System.Text.RegularExpressions;
using Contratos.Application.DTOs;
using Contratos.Domain.Exceptions;

namespace Contratos.Application.Validation;

public static class DestinatariosValidator
{
    private static readonly Regex PadraoEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static void Validar(AtualizarDestinatariosDto dados)
    {
        var mensagens = new List<string>();

        if (!string.IsNullOrWhiteSpace(dados.EmailDestinatario) && !PadraoEmail.IsMatch(dados.EmailDestinatario.Trim()))
            mensagens.Add("O e-mail do destinatário não parece válido.");

        if (mensagens.Count > 0)
            throw new DestinatariosInvalidosException(mensagens);
    }
}