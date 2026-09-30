import type { Contrato } from '../types/contrato'
import type { PreviaEmail, PreviaWhatsapp } from '../types/envio'

type ResultadoComMensagem<T> =
  | { sucesso: true; dados: T }
  | { sucesso: false; mensagem: string }

async function tratarResposta<T>(resposta: Response): Promise<ResultadoComMensagem<T>> {
  if (resposta.ok) {
    const dados = (await resposta.json()) as T
    return { sucesso: true, dados }
  }
  const corpoErro = await resposta.json().catch(() => null)
  const mensagem =
    corpoErro?.mensagens?.[0] ?? corpoErro?.mensagem ?? corpoErro?.detail ?? 'Não foi possível completar a operação.'
  return { sucesso: false, mensagem }
}

export async function avancarParaEnvio(id: number) {
  const resposta = await fetch(`/api/contratos/${id}/avancar-para-envio`, { method: 'POST' })
  return tratarResposta<Contrato>(resposta)
}

export async function atualizarDestinatarios(id: number, email: string, whatsapp: string) {
  const resposta = await fetch(`/api/contratos/${id}/destinatarios`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ emailDestinatario: email || null, whatsappDestinatario: whatsapp || null })
  })
  return tratarResposta<Contrato>(resposta)
}

export async function obterPreviaEmail(id: number) {
  const resposta = await fetch(`/api/contratos/${id}/previa-email`)
  return tratarResposta<PreviaEmail>(resposta)
}

export async function obterPreviaWhatsapp(id: number) {
  const resposta = await fetch(`/api/contratos/${id}/previa-whatsapp`)
  return tratarResposta<PreviaWhatsapp>(resposta)
}