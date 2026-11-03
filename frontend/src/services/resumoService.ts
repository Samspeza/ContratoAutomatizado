import type { ResumoContratos } from '../types/resumo'
import { chamar } from './envioService'

export function obterResumo() {
  return chamar<ResumoContratos>(() => fetch('/api/contratos/resumo'))
}