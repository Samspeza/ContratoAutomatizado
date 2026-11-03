export function proximoPasso(status: string): string {
  switch (status) {
    case 'Rascunho':
    case 'ErroGeracao':
      return 'Gerar o contrato novamente'
    case 'Gerado':
      return 'Conferir o documento'
    case 'ProntoParaEnvio':
      return 'Enviar por e-mail'
    case 'ErroNoEnvio':
      return 'Tentar enviar novamente'
    default:
      return ''
  }
}