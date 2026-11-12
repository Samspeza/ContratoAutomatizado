export interface ConfiguracaoEmail {
  remetente: string
  host: string
  porta: number
  usuario: string
  senhaConfigurada: boolean
  origemSenha: 'Nenhuma' | 'ArmazenamentoSeguro' | 'Configuracao'
}

export interface ConfiguracaoArmazenamento {
  pastaContratosGerados: string
  pastaPadraoContratos: string
  pastaPersonalizada: boolean
  pastaModelos: string
  pastaLogs: string
  pastaDados: string
}

export interface Configuracoes {
  email: ConfiguracaoEmail
  armazenamento: ConfiguracaoArmazenamento
  acompanhamento: { diasParado: number }
  pdf: { caminhoLibreOffice: string }
}

export interface Placeholder {
  nome: string
  descricao: string
  exemplo: string
}

export interface MensagemEmail {
  assunto: string
  corpo: string
  assuntoPadrao: string
  corpoPadrao: string
  placeholders: Placeholder[]
  aviso: string | null
}

export interface ItemVerificacao {
  chave: string
  titulo: string
  ok: boolean
  detalhe: string
}

export type Retorno = { sucesso: boolean; texto: string } | null