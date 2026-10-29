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