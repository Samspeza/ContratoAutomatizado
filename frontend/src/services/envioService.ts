import type { Contrato } from '../types/contrato'
import type { EventoHistorico, PreviaEmail } from '../types/envio'

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

const MENSAGEM_SEM_CONEXAO = 'Não foi possível se comunicar com o sistema. Tente novamente.'

async function chamar<T>(requisicao: () => Promise<Response>): Promise<ResultadoComMensagem<T>> {
  try {
    return await tratarResposta<T>(await requisicao())
  } catch {
    return { sucesso: false, mensagem: MENSAGEM_SEM_CONEXAO }
  }
}

export function obterContrato(id: number) {
  return chamar<Contrato>(() => fetch(`/api/contratos/${id}`))
}

export function avancarParaEnvio(id: number) {
  return chamar<Contrato>(() => fetch(`/api/contratos/${id}/avancar-para-envio`, { method: 'POST' }))
}

export function atualizarDestinatarios(id: number, email: string) {
  return chamar<Contrato>(() =>
    fetch(`/api/contratos/${id}/destinatarios`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ emailDestinatario: email || null })
    })
  )
}

export function obterPreviaEmail(id: number) {
  return chamar<PreviaEmail>(() => fetch(`/api/contratos/${id}/previa-email`))
}

export function enviarEmail(id: number) {
  return chamar<Contrato>(() => fetch(`/api/contratos/${id}/enviar-email`, { method: 'POST' }))
}

export function obterHistorico(id: number) {
  return chamar<EventoHistorico[]>(() => fetch(`/api/contratos/${id}/historico`))
}