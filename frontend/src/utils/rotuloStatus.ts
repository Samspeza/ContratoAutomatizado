export const STATUS_DISPONIVEIS = [
  'Rascunho',
  'ErroGeracao',
  'Gerado',
  'ProntoParaEnvio',
  'Concluido',
  'ErroNoEnvio',
  'Cancelado'
] as const

const ROTULOS: Record<string, string> = {
  Rascunho: 'Rascunho',
  ErroGeracao: 'Erro na geração',
  Gerado: 'Contrato gerado',
  ProntoParaEnvio: 'Pronto para envio',
  Concluido: 'Envio concluído',
  ErroNoEnvio: 'Erro no envio',
  Cancelado: 'Cancelado'
}

export function rotuloStatus(status: string): string {
  return ROTULOS[status] ?? status
}

export type VarianteSelo = 'sucesso' | 'erro' | 'aviso' | 'neutro'

export function varianteStatus(status: string): VarianteSelo {
  switch (status) {
    case 'Concluido':
      return 'sucesso'
    case 'ErroGeracao':
    case 'ErroNoEnvio':
      return 'erro'
    case 'Gerado':
    case 'ProntoParaEnvio':
      return 'aviso'
    default:
      return 'neutro'
  }
}

export function rotuloEmail(statusEmail: string, statusContrato: string): { texto: string; variante: VarianteSelo } {
  if (statusEmail === 'Enviado') return { texto: 'Enviado', variante: 'sucesso' }
  if (statusEmail === 'Erro') return { texto: 'Erro', variante: 'erro' }
  if (statusContrato === 'ProntoParaEnvio') return { texto: 'Pendente', variante: 'aviso' }
  return { texto: '—', variante: 'neutro' }
}