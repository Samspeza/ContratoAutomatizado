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

        if (!string.IsNullOrWhiteSpace(dados.WhatsappDestinatario))
        {
            var digitos = new string(dados.WhatsappDestinatario.Where(char.IsDigit).ToArray());
            if (digitos.Length < 10 || digitos.Length > 15)
                mensagens.Add("O número de WhatsApp do destinatário não parece válido. Inclua o DDD (e o código do país, se for o caso).");
        }

        if (mensagens.Count > 0)
            throw new DestinatariosInvalidosException(mensagens);
    }
}