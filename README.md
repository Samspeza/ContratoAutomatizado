# Contrato Automatizado

Sistema para automatizar a geração de contratos: consulta de CNPJ,
preenchimento do modelo Word e geração de DOCX e PDF.

## Requisitos

- Desenvolvimento: .NET 10 SDK, Node.js, LibreOffice
- Uso final: Windows 10/11 e LibreOffice (o instalador será tratado em etapa própria)

## Desenvolvimento

Backend (porta 5099):
    cd backend/src/Contratos.Api
    dotnet run

Frontend (porta 5173):
    cd frontend
    npm install
    npm run dev

Testes:
    cd backend/src/Contratos.Tests
    dotnet test

## Publicação (produção)

    powershell -ExecutionPolicy Bypass -File scripts/publicar.ps1

Gera a aplicação em `publish/app`. O backend serve o frontend compilado.

## Onde ficam os arquivos (produção)

Pasta `Documentos\ContratoAutomatizado`:
- `Templates`: modelo do contrato (`modelo.docx`), copiado na primeira execução
- `ContratosGerados`: contratos em DOCX e PDF
- `Logs`: um arquivo por dia (os últimos 30 são mantidos)

## Configuração

Valores em `appsettings.json`. Qualquer um pode ser sobrescrito por variável de
ambiente, trocando `:` por `__` (exemplo: `Pdf__CaminhoExecutavelLibreOffice`).

## Segurança

- O backend escuta somente em 127.0.0.1 e rejeita cabeçalhos Host desconhecidos.
- Nenhuma chave de API é usada hoje (BrasilAPI é pública). Se for contratada uma
  API paga, as chaves devem ficar em User Secrets (desenvolvimento) ou em
  variáveis de ambiente (produção), nunca no repositório nem no frontend.

## Placeholders do modelo Word

`{{RAZAO_SOCIAL_CONTRATANTE}}`, `{{CNPJ_CONTRATANTE}}`, `{{ENDERECO_CONTRATANTE}}`,
`{{COMPLEMENTO_ENDERECO_CONTRATANTE}}`, `{{RESPONSAVEL_1}}`, `{{RESPONSAVEL_2}}`,
`{{DATA_CONTRATO_NUMERICA}}`, `{{DATA_CONTRATO_EXTENSO}}`,
`{{PERCENTUAL_HONORARIOS}}`, `{{QUANTIDADE_PARCELAS}}`