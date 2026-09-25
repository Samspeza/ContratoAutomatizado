export interface Empresa {
  cnpj: string
  razaoSocial: string
  nomeFantasia: string | null
  logradouro: string
  numero: string | null
  complemento: string | null
  bairro: string
  municipio: string
  uf: string
  cep: string
  situacaoCadastral: string | null
}