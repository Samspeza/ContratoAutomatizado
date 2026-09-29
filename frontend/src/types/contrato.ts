import type { ResponsavelAssinatura } from './dadosContrato'

export interface Contrato {
  id: number
  cnpj: string
  razaoSocial: string
  endereco: string
  percentualHonorarios: number
  quantidadeParcelas: number
  dataContrato: string
  status: string
  arquivoDisponivel: boolean
  responsaveis: ResponsavelAssinatura[]
}