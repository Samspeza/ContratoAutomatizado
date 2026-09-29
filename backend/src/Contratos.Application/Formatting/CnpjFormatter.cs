namespace Contratos.Application.Formatting;

public static class CnpjFormatter
{
    public static string Formatar(string cnpj)
    {
        var digitos = new string(cnpj.Where(char.IsDigit).ToArray());
        if (digitos.Length != 14) return cnpj;

        return $"{digitos[..2]}.{digitos[2..5]}.{digitos[5..8]}/{digitos[8..12]}-{digitos[12..14]}";
    }
}