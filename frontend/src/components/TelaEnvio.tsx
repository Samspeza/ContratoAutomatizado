import { useState } from 'react'
import type { Contrato } from '../types/contrato'
import type { PreviaEmail } from '../types/envio'
import { atualizarDestinatarios, enviarEmail, obterContrato, obterPreviaEmail } from '../services/envioService'
import { rotuloStatus } from '../utils/rotuloStatus'
import { HistoricoContrato } from './HistoricoContrato'

interface TelaEnvioProps {
  contrato: Contrato
  onVoltar: () => void
  onContratoAtualizado: (contrato: Contrato) => void
  rotuloVoltar?: string
}

function formatarDataHora(iso: string) {
  return new Date(iso).toLocaleString('pt-BR')
}

export function TelaEnvio({ contrato, onVoltar, onContratoAtualizado, rotuloVoltar = 'Voltar para o documento' }: TelaEnvioProps) {
  const [emailDestinatario, setEmailDestinatario] = useState(contrato.emailDestinatario ?? '')
  const [erroEmail, setErroEmail] = useState('')
  const [previaEmail, setPreviaEmail] = useState<PreviaEmail | null>(null)
  const [carregandoPrevia, setCarregandoPrevia] = useState(false)
  const [enviando, setEnviando] = useState(false)
  const [chaveHistorico, setChaveHistorico] = useState(0)

  async function handlePreviaEmail() {
    setErroEmail('')
    setPreviaEmail(null)
    setCarregandoPrevia(true)

    const salvo = await atualizarDestinatarios(contrato.id, emailDestinatario)
    if (!salvo.sucesso) {
      setErroEmail(salvo.mensagem)
      setCarregandoPrevia(false)
      return
    }
    onContratoAtualizado(salvo.dados)

    const previa = await obterPreviaEmail(contrato.id)
    if (previa.sucesso) {
      setPreviaEmail(previa.dados)
    } else {
      setErroEmail(previa.mensagem)
    }
    setCarregandoPrevia(false)
  }

  async function handleConfirmarEnvio() {
    setEnviando(true)
    setErroEmail('')

    const resultado = await enviarEmail(contrato.id)
    if (resultado.sucesso) {
      onContratoAtualizado(resultado.dados)
      setPreviaEmail(null)
    } else {
      setErroEmail(resultado.mensagem)
      // O backend registrou a falha; recarrega o contrato para mostrar o estado real.
      const atualizado = await obterContrato(contrato.id)
      if (atualizado.sucesso) onContratoAtualizado(atualizado.dados)
    }

    setChaveHistorico((chave) => chave + 1)
    setEnviando(false)
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Envio do contrato</h2>
      <p className="cartao__descricao">Contrato: {contrato.razaoSocial} — {contrato.cnpj}</p>
      <p className="situacao">Situação: <strong>{rotuloStatus(contrato.status)}</strong></p>

      <div className="bloco-canal">
        <h3 className="bloco-canal__titulo">E-mail</h3>

        {contrato.statusEmail === 'Enviado' && (
          <p className="mensagem mensagem--sucesso">
            ✓ Enviado para {contrato.emailDestinatario}
            {contrato.emailEnviadoEm && ` em ${formatarDataHora(contrato.emailEnviadoEm)}`}
          </p>
        )}

        {contrato.statusEmail === 'Erro' && (
          <div className="mensagem mensagem--erro">
            <p style={{ margin: 0 }}>✕ {contrato.emailErroMensagem}</p>
            {contrato.emailEnviadoEm && (
              <p style={{ margin: '0.4rem 0 0' }}>
                Último envio bem-sucedido em {formatarDataHora(contrato.emailEnviadoEm)}.
              </p>
            )}
            <button
              type="button"
              className="botao botao--secundario"
              style={{ marginTop: '0.6rem' }}
              onClick={handlePreviaEmail}
              disabled={!emailDestinatario || carregandoPrevia}
            >
              Tentar novamente
            </button>
          </div>
        )}

        <div className="campo">
          <label htmlFor="email-destinatario">Destinatário</label>
          <input
            id="email-destinatario"
            type="text"
            value={emailDestinatario}
            onChange={(e) => setEmailDestinatario(e.target.value)}
            placeholder="cliente@empresa.com.br"
          />
        </div>

        {erroEmail && <p className="mensagem mensagem--erro" role="alert">{erroEmail}</p>}

        <button
          type="button"
          className="botao botao--secundario"
          onClick={handlePreviaEmail}
          disabled={!emailDestinatario || carregandoPrevia}
        >
          {carregandoPrevia ? 'Carregando...' : 'Pré-visualizar e-mail'}
        </button>

        {previaEmail && (
          <div className="previa-envio">
            <p><strong>De:</strong> {previaEmail.remetente}</p>
            <p><strong>Para:</strong> {previaEmail.destinatario}</p>
            <p><strong>Assunto:</strong> {previaEmail.assunto}</p>
            <p className="previa-envio__mensagem">{previaEmail.mensagem}</p>
            <p><strong>Anexo:</strong> 📎 {previaEmail.nomeArquivoAnexo}</p>
            <div className="acoes">
              <button type="button" className="botao botao--secundario" onClick={() => setPreviaEmail(null)} disabled={enviando}>
                Fechar prévia
              </button>
              <button type="button" className="botao botao--primario" onClick={handleConfirmarEnvio} disabled={enviando}>
                {enviando ? 'Enviando...' : contrato.statusEmail === 'Enviado' ? 'Confirmar e reenviar' : 'Confirmar e enviar'}
              </button>
            </div>
          </div>
        )}
      </div>

      <div className="acoes">
       <button type="button" className="botao botao--secundario" onClick={onVoltar}>
        {rotuloVoltar}
      </button>
      </div>

      <HistoricoContrato contratoId={contrato.id} chaveAtualizacao={chaveHistorico} />
    </section>
  )
}