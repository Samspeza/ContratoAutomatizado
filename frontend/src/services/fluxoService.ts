import type { ContratoResumo } from '../types/contrato'
import { chamar } from './envioService'

export function obterFluxo(diasConcluidos: number) {
  return chamar<ContratoResumo[]>(() => fetch(`/api/contratos/fluxo?diasConcluidos=${diasConcluidos}`))
}