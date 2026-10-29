export interface PreviaEmail {
  remetente: string
  destinatario: string
  assunto: string
  mensagem: string
  nomeArquivoAnexo: string
}

export interface EventoHistorico {
  dataHora: string
  tipo: string
  descricao: string
}