import type { Contrato, PaginaContratos } from '../types/contrato'
import { chamar } from './envioService'

export interface FiltroListagem {
  busca: string
  status: string
  de: string
  ate: string
  pagina: number
  tamanho: number
}

export function listarContratos(filtro: FiltroListagem) {
  const parametros = new URLSearchParams()
  if (filtro.busca.trim()) parametros.set('busca', filtro.busca.trim())
  if (filtro.status) parametros.set('status', filtro.status)
  if (filtro.de) parametros.set('de', filtro.de)
  if (filtro.ate) parametros.set('ate', filtro.ate)
  parametros.set('pagina', String(filtro.pagina))
  parametros.set('tamanho', String(filtro.tamanho))

  return chamar<PaginaContratos>(() => fetch(`/api/contratos?${parametros.toString()}`))
}

export function cancelarContrato(id: number) {
  return chamar<Contrato>(() => fetch(`/api/contratos/${id}/cancelar`, { method: 'POST' }))
}