import type { Empresa } from '../types/empresa'

export type ConsultaCnpjResultado =
  | { sucesso: true; empresa: Empresa }
  | { sucesso: false; mensagem: string }

export async function consultarCnpj(cnpj: string): Promise<ConsultaCnpjResultado> {
  try {
    const resposta = await fetch(`/api/cnpj?numero=${encodeURIComponent(cnpj)}`)

    if (resposta.ok) {
      const empresa = (await resposta.json()) as Empresa
      return { sucesso: true, empresa }
    }

    const corpo = await resposta.json().catch(() => null)
    const mensagem =
      corpo?.mensagem ??
      corpo?.detail ??
      'Não foi possível consultar os dados do CNPJ neste momento.'
    return { sucesso: false, mensagem }
  } catch {
    return {
      sucesso: false,
      mensagem: 'Não foi possível consultar os dados do CNPJ neste momento. Verifique sua conexão e tente novamente.'
    }
  }
}