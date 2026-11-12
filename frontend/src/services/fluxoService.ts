import type { ContratoResumo } from '../types/contrato'
import { chamar } from './envioService'

export interface FluxoResposta {
  diasParado: number
  contratos: ContratoResumo[]
}

export function obterFluxo(diasConcluidos: number) {
  return chamar<FluxoResposta>(() => fetch(`/api/contratos/fluxo?diasConcluidos=${diasConcluidos}`))
}