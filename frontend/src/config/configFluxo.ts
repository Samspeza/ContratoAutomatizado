import type { VarianteSelo } from '../utils/rotuloStatus'

export interface ColunaFluxo {
  chave: string
  titulo: string
  descricao: string
  status: string[]
  variante: VarianteSelo
  // Colunas "abertas" mostram contratos que ainda exigem ação (e destacam os parados).
  abertas: boolean
}

export const COLUNAS_FLUXO: ColunaFluxo[] = [
  {
    chave: 'atencao',
    titulo: 'Precisa de atenção',
    descricao: 'Rascunhos e contratos com erro',
    status: ['Rascunho', 'ErroGeracao', 'ErroNoEnvio'],
    variante: 'erro',
    abertas: true
  },
  {
    chave: 'conferir',
    titulo: 'Para conferir',
    descricao: 'Documento gerado, aguardando conferência',
    status: ['Gerado'],
    variante: 'aviso',
    abertas: true
  },
  {
    chave: 'envio',
    titulo: 'Pronto para envio',
    descricao: 'Conferido, aguardando o envio',
    status: ['ProntoParaEnvio'],
    variante: 'neutro',
    abertas: true
  },
  {
    chave: 'concluidos',
    titulo: 'Concluídos',
    descricao: 'E-mail enviado',
    status: ['Concluido'],
    variante: 'sucesso',
    abertas: false
  }
]

export const OPCOES_DIAS_CONCLUIDOS = [7, 30, 90]
export const DIAS_CONCLUIDOS_PADRAO = 7

// A partir de quantos dias sem atividade um contrato em aberto é destacado.
export const DIAS_PARADO_ALERTA = 3