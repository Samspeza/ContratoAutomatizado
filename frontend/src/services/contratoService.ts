import type { Empresa } from '../types/empresa'
import type { ResponsavelAssinatura } from '../types/dadosContrato'
import type { Contrato } from '../types/contrato'

interface GerarContratoPayload {
  empresa: Empresa
  complementoEndereco: string | null
  percentualHonorarios: number
  quantidadeParcelas: number
  dataContrato: string
  responsaveis: ResponsavelAssinatura[]
}

export type GerarContratoResultado =
  | { sucesso: true; contrato: Contrato }
  | { sucesso: false; mensagem: string; contratoId?: number }

async function tratarResposta(resposta: Response): Promise<GerarContratoResultado> {
  if (resposta.ok) {
    const contrato = (await resposta.json()) as Contrato
    return { sucesso: true, contrato }
  }

  const corpoErro = await resposta.json().catch(() => null)
  return {
    sucesso: false,
    mensagem: corpoErro?.mensagem ?? corpoErro?.detail ?? 'Não foi possível gerar o contrato.',
    contratoId: corpoErro?.contratoId
  }
}

export async function gerarContrato(payload: GerarContratoPayload): Promise<GerarContratoResultado> {
  try {
    const resposta = await fetch('/api/contratos', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })
    return await tratarResposta(resposta)
  } catch {
    return { sucesso: false, mensagem: 'Não foi possível gerar o contrato. Verifique sua conexão e tente novamente.' }
  }
}

export async function gerarContratoNovamente(id: number): Promise<GerarContratoResultado> {
  try {
    const resposta = await fetch(`/api/contratos/${id}/gerar`, { method: 'POST' })
    return await tratarResposta(resposta)
  } catch {
    return { sucesso: false, mensagem: 'Não foi possível gerar o contrato. Verifique sua conexão e tente novamente.' }
  }
}