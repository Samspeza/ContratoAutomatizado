namespace Contratos.Application.DTOs;

public sealed record ResponsavelAssinaturaDto(string Nome, string Cpf);

public sealed record DadosContratoDto(
    EmpresaDto Empresa,
    string? ComplementoEndereco,
    decimal PercentualHonorarios,
    int QuantidadeParcelas,
    DateOnly? DataContrato,
    IReadOnlyList<ResponsavelAssinaturaDto> Responsaveis);