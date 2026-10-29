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
  emailDestinatario: string | null
  statusEmail: 'Pendente' | 'Enviado' | 'Erro'
  emailEnviadoEm: string | null
  emailErroMensagem: string | null
  criadoEm: string
  podeGerarArquivos: boolean
  podeEnviarEmail: boolean
  podeCancelar: boolean
}

export interface ContratoResumo {
  id: number
  cnpj: string
  razaoSocial: string
  criadoEm: string
  dataContrato: string
  status: string
  statusEmail: string
}

export interface PaginaContratos {
  itens: ContratoResumo[]
  total: number
  pagina: number
  tamanho: number
}