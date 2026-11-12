import type { Configuracoes, ItemVerificacao, MensagemEmail } from '../types/configuracoes'
import { chamar } from './envioService'

const CABECALHOS = { 'Content-Type': 'application/json' }

function enviar(url: string, metodo: 'PUT' | 'POST', corpo: unknown) {
  return fetch(url, { method: metodo, headers: CABECALHOS, body: JSON.stringify(corpo) })
}

export function obterConfiguracoes() {
  return chamar<Configuracoes>(() => fetch('/api/configuracoes'))
}

export function obterVerificacao() {
  return chamar<ItemVerificacao[]>(() => fetch('/api/configuracoes/verificacao'))
}

export function obterMensagemEmail() {
  return chamar<MensagemEmail>(() => fetch('/api/configuracoes/mensagem-email'))
}

export function salvarMensagemEmail(assunto: string, corpo: string) {
  return chamar<MensagemEmail>(() => enviar('/api/configuracoes/mensagem-email', 'PUT', { assunto, corpo }))
}

export function definirSenhaEmail(senha: string) {
  return chamar<Configuracoes>(() => enviar('/api/configuracoes/email/senha', 'PUT', { senha }))
}

export function removerSenhaEmail() {
  return chamar<Configuracoes>(() => fetch('/api/configuracoes/email/senha', { method: 'DELETE' }))
}

export function testarConexaoEmail() {
  return chamar<{ mensagem: string }>(() => fetch('/api/configuracoes/email/testar', { method: 'POST' }))
}

export function salvarPastaContratos(pasta: string) {
  return chamar<Configuracoes>(() => enviar('/api/configuracoes/armazenamento', 'PUT', { pasta }))
}

export function salvarDiasParado(dias: number) {
  return chamar<Configuracoes>(() => enviar('/api/configuracoes/acompanhamento', 'PUT', { dias }))
}

export function salvarCaminhoLibreOffice(caminho: string) {
  return chamar<Configuracoes>(() => enviar('/api/configuracoes/pdf', 'PUT', { caminho }))
}

export function abrirPasta(pasta: 'contratos' | 'modelos' | 'logs') {
  return chamar<{ mensagem: string }>(() => enviar('/api/configuracoes/abrir-pasta', 'POST', { pasta }))
}