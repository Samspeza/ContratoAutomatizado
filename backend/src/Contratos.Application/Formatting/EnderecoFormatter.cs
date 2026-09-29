using Contratos.Application.DTOs;

namespace Contratos.Application.Formatting;

public static class EnderecoFormatter
{
    public static string Formatar(EmpresaDto empresa)
    {
        var partes = new List<string> { empresa.Logradouro };

        if (!string.IsNullOrWhiteSpace(empresa.Numero))
            partes[0] += $", {empresa.Numero}";

        partes.Add(empresa.Bairro);

        return $"{string.Join(", ", partes)}, {empresa.Municipio}/{empresa.Uf}– CEP {empresa.Cep}";
    }
}