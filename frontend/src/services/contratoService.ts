import type { Empresa } from '../types/empresa'
import type { ResponsavelAssinatura } from '../types/dadosContrato'

interface GerarContratoPayload {
  empresa: Empresa
  complementoEndereco: string | null
  percentualHonorarios: number
  quantidadeParcelas: number
  dataContrato: string
  responsaveis: ResponsavelAssinatura[]
}

export type GerarContratoResultado =
  | { sucesso: true; arquivoDocx: string; arquivoPdf: string; caminhoCompletoPdf: string }
  | { sucesso: false; mensagem: string }

export async function gerarContrato(payload: GerarContratoPayload): Promise<GerarContratoResultado> {
  try {
    const resposta = await fetch('/api/contratos', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    })

    if (resposta.ok) {
      const corpo = await resposta.json()
      return {
        sucesso: true,
        arquivoDocx: corpo.arquivoDocx,
        arquivoPdf: corpo.arquivoPdf,
        caminhoCompletoPdf: corpo.caminhoCompletoPdf
      }
    }

    const corpoErro = await resposta.json().catch(() => null)
    return {
      sucesso: false,
      mensagem: corpoErro?.detail ?? corpoErro?.mensagem ?? 'Não foi possível gerar o contrato.'
    }
  } catch {
    return {
      sucesso: false,
      mensagem: 'Não foi possível gerar o contrato. Verifique sua conexão e tente novamente.'
    }
  }
}