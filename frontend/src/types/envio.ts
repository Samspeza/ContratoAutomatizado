export interface PreviaEmail {
  remetente: string
  destinatario: string
  assunto: string
  mensagem: string
  nomeArquivoAnexo: string
}

export interface PreviaWhatsapp {
  numeroDestino: string
  mensagem: string
  nomeArquivoAnexo: string
}