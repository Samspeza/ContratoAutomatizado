import { useEffect, useState } from 'react'
import type { Configuracoes, Retorno } from '../types/configuracoes'
import { salvarCaminhoLibreOffice, salvarDiasParado } from '../services/configuracoesService'
import { MensagemRetorno } from './MensagemRetorno'

interface ConfigAvancadaProps {
  configuracoes: Configuracoes
  onAlterado: () => void
}

export function ConfigAvancada({ configuracoes, onAlterado }: ConfigAvancadaProps) {
  const [dias, setDias] = useState(String(configuracoes.acompanhamento.diasParado))
  const [caminho, setCaminho] = useState(configuracoes.pdf.caminhoLibreOffice)
  const [ocupado, setOcupado] = useState(false)
  const [retornoDias, setRetornoDias] = useState<Retorno>(null)
  const [retornoPdf, setRetornoPdf] = useState<Retorno>(null)

  useEffect(() => {
    setDias(String(configuracoes.acompanhamento.diasParado))
  }, [configuracoes.acompanhamento.diasParado])

  useEffect(() => {
    setCaminho(configuracoes.pdf.caminhoLibreOffice)
  }, [configuracoes.pdf.caminhoLibreOffice])

  async function handleSalvarDias() {
    const valor = Number(dias)
    if (!Number.isInteger(valor) || valor < 1 || valor > 60) {
      setRetornoDias({ sucesso: false, texto: 'Informe uma quantidade de dias entre 1 e 60.' })
      return
    }

    setOcupado(true)
    setRetornoDias(null)

    const resultado = await salvarDiasParado(valor)
    if (resultado.sucesso) {
      onAlterado()
      setRetornoDias({ sucesso: true, texto: 'Salvo. Vale no Resumo e no Fluxo.' })
    } else {
      setRetornoDias({ sucesso: false, texto: resultado.mensagem })
    }

    setOcupado(false)
  }

  async function handleSalvarCaminho(novoCaminho: string) {
    setOcupado(true)
    setRetornoPdf(null)

    const resultado = await salvarCaminhoLibreOffice(novoCaminho)
    if (resultado.sucesso) {
      onAlterado()
      setRetornoPdf({
        sucesso: true,
        texto: novoCaminho.trim() ? 'Caminho salvo.' : 'Voltou para o caminho padrão.'
      })
    } else {
      setRetornoPdf({ sucesso: false, texto: resultado.mensagem })
    }

    setOcupado(false)
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Alertas e PDF</h2>

      <div className="linha-campo">
        <div className="campo">
          <label htmlFor="dias-parado">Considerar um contrato parado após (dias)</label>
          <input id="dias-parado" type="number" min={1} max={60} value={dias} onChange={(e) => setDias(e.target.value)} />
        </div>
        <button
          type="button"
          className="botao botao--primario"
          onClick={handleSalvarDias}
          disabled={ocupado || dias === String(configuracoes.acompanhamento.diasParado)}
        >
          Salvar
        </button>
      </div>
      <MensagemRetorno retorno={retornoDias} />

      <div className="linha-campo" style={{ marginTop: '1.25rem' }}>
        <div className="campo">
          <label htmlFor="caminho-libreoffice">Caminho do LibreOffice (conversão para PDF)</label>
          <input
            id="caminho-libreoffice"
            type="text"
            value={caminho}
            onChange={(e) => setCaminho(e.target.value)}
            placeholder={'C:\\Program Files\\LibreOffice\\program\\soffice.exe'}
          />
        </div>
        <button
          type="button"
          className="botao botao--primario"
          onClick={() => handleSalvarCaminho(caminho)}
          disabled={ocupado || caminho.trim() === '' || caminho.trim() === configuracoes.pdf.caminhoLibreOffice}
        >
          Salvar
        </button>
      </div>

      <button type="button" className="botao botao--texto" onClick={() => handleSalvarCaminho('')} disabled={ocupado}>
        Usar o caminho padrão
      </button>
      <MensagemRetorno retorno={retornoPdf} />
    </section>
  )
}