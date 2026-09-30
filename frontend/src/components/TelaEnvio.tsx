import { useState } from 'react'
import type { Contrato } from '../types/contrato'
import type { PreviaEmail, PreviaWhatsapp } from '../types/envio'
import { atualizarDestinatarios, obterPreviaEmail, obterPreviaWhatsapp } from '../services/envioService'

interface TelaEnvioProps {
  contrato: Contrato
  onVoltar: () => void
}

export function TelaEnvio({ contrato, onVoltar }: TelaEnvioProps) {
  const [emailDestinatario, setEmailDestinatario] = useState('')
  const [whatsappDestinatario, setWhatsappDestinatario] = useState('')
  const [erroEmail, setErroEmail] = useState('')
  const [erroWhatsapp, setErroWhatsapp] = useState('')
  const [previaEmail, setPreviaEmail] = useState<PreviaEmail | null>(null)
  const [previaWhatsapp, setPreviaWhatsapp] = useState<PreviaWhatsapp | null>(null)
  const [carregandoPreviaEmail, setCarregandoPreviaEmail] = useState(false)
  const [carregandoPreviaWhatsapp, setCarregandoPreviaWhatsapp] = useState(false)

  async function handlePreviaEmail() {
    setErroEmail('')
    setPreviaEmail(null)
    setCarregandoPreviaEmail(true)

    const salvo = await atualizarDestinatarios(contrato.id, emailDestinatario, whatsappDestinatario)
    if (!salvo.sucesso) {
      setErroEmail(salvo.mensagem)
      setCarregandoPreviaEmail(false)
      return
    }

    const previa = await obterPreviaEmail(contrato.id)
    if (previa.sucesso) {
      setPreviaEmail(previa.dados)
    } else {
      setErroEmail(previa.mensagem)
    }
    setCarregandoPreviaEmail(false)
  }

  async function handlePreviaWhatsapp() {
    setErroWhatsapp('')
    setPreviaWhatsapp(null)
    setCarregandoPreviaWhatsapp(true)

    const salvo = await atualizarDestinatarios(contrato.id, emailDestinatario, whatsappDestinatario)
    if (!salvo.sucesso) {
      setErroWhatsapp(salvo.mensagem)
      setCarregandoPreviaWhatsapp(false)
      return
    }

    const previa = await obterPreviaWhatsapp(contrato.id)
    if (previa.sucesso) {
      setPreviaWhatsapp(previa.dados)
    } else {
      setErroWhatsapp(previa.mensagem)
    }
    setCarregandoPreviaWhatsapp(false)
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Envio do contrato</h2>
      <p className="cartao__descricao">Contrato: {contrato.razaoSocial} — {contrato.cnpj}</p>

      <div className="bloco-canal">
        <h3 className="bloco-canal__titulo">E-mail</h3>

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

        <button type="button" className="botao botao--secundario" onClick={handlePreviaEmail} disabled={!emailDestinatario || carregandoPreviaEmail}>
          {carregandoPreviaEmail ? 'Carregando...' : 'Pré-visualizar e-mail'}
        </button>

        {previaEmail && (
          <div className="previa-envio">
            <p><strong>De:</strong> {previaEmail.remetente}</p>
            <p><strong>Para:</strong> {previaEmail.destinatario}</p>
            <p><strong>Assunto:</strong> {previaEmail.assunto}</p>
            <p className="previa-envio__mensagem">{previaEmail.mensagem}</p>
            <p><strong>Anexo:</strong> 📎 {previaEmail.nomeArquivoAnexo}</p>
            <div className="acoes">
              <button type="button" className="botao botao--secundario" onClick={() => setPreviaEmail(null)}>Fechar prévia</button>
              <button type="button" className="botao botao--primario" disabled title="Disponível na Etapa 6 (integração com o Outlook)">
                Confirmar e enviar
              </button>
            </div>
          </div>
        )}
      </div>

      <div className="bloco-canal">
        <h3 className="bloco-canal__titulo">WhatsApp</h3>

        <div className="campo">
          <label htmlFor="whatsapp-destinatario">Número</label>
          <input
            id="whatsapp-destinatario"
            type="text"
            value={whatsappDestinatario}
            onChange={(e) => setWhatsappDestinatario(e.target.value)}
            placeholder="+55 17 99999-9999"
          />
        </div>

        {erroWhatsapp && <p className="mensagem mensagem--erro" role="alert">{erroWhatsapp}</p>}

        <button type="button" className="botao botao--secundario" onClick={handlePreviaWhatsapp} disabled={!whatsappDestinatario || carregandoPreviaWhatsapp}>
          {carregandoPreviaWhatsapp ? 'Carregando...' : 'Pré-visualizar WhatsApp'}
        </button>

        {previaWhatsapp && (
          <div className="previa-envio">
            <p><strong>Para:</strong> {previaWhatsapp.numeroDestino}</p>
            <p className="previa-envio__mensagem previa-envio__mensagem--whatsapp">{previaWhatsapp.mensagem}</p>
            <p><strong>Anexo:</strong> 📎 {previaWhatsapp.nomeArquivoAnexo}</p>
            <div className="acoes">
              <button type="button" className="botao botao--secundario" onClick={() => setPreviaWhatsapp(null)}>Fechar prévia</button>
              <button type="button" className="botao botao--primario" disabled title="Disponível na Etapa 8 (integração com o WhatsApp)">
                Confirmar e enviar
              </button>
            </div>
          </div>
        )}
      </div>

      <div className="acoes">
        <button type="button" className="botao botao--secundario" onClick={onVoltar}>
          Voltar para o documento
        </button>
      </div>
    </section>
  )
}