import type { ContratoResumo } from './contrato'

export interface ResumoContratos {
  criadosHoje: number
  enviadosHoje: number
  aguardandoEnvio: number
  comErro: number
  diasParado: number
  totalPendencias: number
  pendencias: ContratoResumo[]
}