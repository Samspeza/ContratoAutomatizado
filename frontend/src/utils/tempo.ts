const MS_POR_DIA = 24 * 60 * 60 * 1000

export function diasDesde(iso: string, agora: Date = new Date()): number {
  return Math.floor((agora.getTime() - new Date(iso).getTime()) / MS_POR_DIA)
}

export function descreverIdade(iso: string, agora: Date = new Date()): string {
  const minutos = Math.floor((agora.getTime() - new Date(iso).getTime()) / 60000)
  if (minutos < 1) return 'agora há pouco'
  if (minutos < 60) return `há ${minutos} min`

  const horas = Math.floor(minutos / 60)
  if (horas < 24) return `há ${horas} h`

  const dias = Math.floor(horas / 24)
  return dias === 1 ? 'há 1 dia' : `há ${dias} dias`
}