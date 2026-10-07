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
  whatsappDestinatario: string | null
  statusWhatsapp: 'Pendente' | 'Enviado' | 'Erro'
  whatsappEnviadoEm: string | null
  whatsappErroMensagem: string | null
}