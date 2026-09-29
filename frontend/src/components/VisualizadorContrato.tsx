import { useState } from 'react'
import { Document, Page, pdfjs } from 'react-pdf'
import 'react-pdf/dist/Page/AnnotationLayer.css'
import 'react-pdf/dist/Page/TextLayer.css'
import type { Contrato } from '../types/contrato'

pdfjs.GlobalWorkerOptions.workerSrc = new URL(
  'pdfjs-dist/build/pdf.worker.min.mjs',
  import.meta.url
).toString()

interface VisualizadorContratoProps {
  contrato: Contrato
  onVoltar: () => void
  onContinuarParaEnvio: () => void
  onNovoContrato: () => void
}

const ZOOM_MINIMO = 0.6
const ZOOM_MAXIMO = 2.0
const PASSO_ZOOM = 0.2

export function VisualizadorContrato({ contrato, onVoltar, onContinuarParaEnvio, onNovoContrato }: VisualizadorContratoProps) {
  const [totalPaginas, setTotalPaginas] = useState(0)
  const [paginaAtual, setPaginaAtual] = useState(1)
  const [zoom, setZoom] = useState(1)
  const [erroCarregamento, setErroCarregamento] = useState(false)

  const urlPdf = `/api/contratos/${contrato.id}/pdf`
  const urlDownloadPdf = `/api/contratos/${contrato.id}/pdf?baixar=true`

  function handleCarregado({ numPages }: { numPages: number }) {
    setTotalPaginas(numPages)
    setPaginaAtual(1)
    setErroCarregamento(false)
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Contrato gerado</h2>
      <p className="cartao__descricao">Confira o documento antes de continuar para o envio.</p>

      <div className="visualizador-pdf">
        {erroCarregamento ? (
          <p className="mensagem mensagem--erro" role="alert">
            Não foi possível exibir a prévia aqui dentro. Você ainda pode abrir ou baixar o arquivo pelos botões abaixo.
          </p>
        ) : (
          <Document
            file={urlPdf}
            onLoadSuccess={handleCarregado}
            onLoadError={() => setErroCarregamento(true)}
            loading={<p className="mensagem mensagem--info">Carregando prévia...</p>}
          >
            <Page pageNumber={paginaAtual} scale={zoom} />
          </Document>
        )}
      </div>

      {totalPaginas > 0 && (
        <div className="visualizador-pdf__controles">
          <div className="visualizador-pdf__navegacao">
            <button
              type="button"
              className="botao botao--secundario"
              onClick={() => setPaginaAtual((p) => Math.max(1, p - 1))}
              disabled={paginaAtual <= 1}
            >
              Anterior
            </button>
            <span>Página {paginaAtual} de {totalPaginas}</span>
            <button
              type="button"
              className="botao botao--secundario"
              onClick={() => setPaginaAtual((p) => Math.min(totalPaginas, p + 1))}
              disabled={paginaAtual >= totalPaginas}
            >
              Próxima
            </button>
          </div>

          <div className="visualizador-pdf__zoom">
            <button
              type="button"
              className="botao botao--secundario"
              onClick={() => setZoom((z) => Math.max(ZOOM_MINIMO, +(z - PASSO_ZOOM).toFixed(1)))}
              disabled={zoom <= ZOOM_MINIMO}
            >
              −
            </button>
            <span>{Math.round(zoom * 100)}%</span>
            <button
              type="button"
              className="botao botao--secundario"
              onClick={() => setZoom((z) => Math.min(ZOOM_MAXIMO, +(z + PASSO_ZOOM).toFixed(1)))}
              disabled={zoom >= ZOOM_MAXIMO}
            >
              +
            </button>
          </div>
        </div>
      )}

      <div className="acoes">
        <button type="button" className="botao botao--secundario" onClick={() => window.open(urlPdf, '_blank')}>
          Abrir PDF
        </button>
        <a className="botao botao--secundario" href={urlDownloadPdf} download>
          Baixar
        </a>
        <button type="button" className="botao botao--secundario" onClick={onVoltar}>
          Voltar e corrigir
        </button>
        <button
          type="button"
          className="botao botao--primario"
          onClick={onContinuarParaEnvio}
          disabled
          title="Disponível na próxima etapa"
        >
          Continuar para envio
        </button>
      </div>

      <div className="acoes">
        <button type="button" className="botao botao--texto" onClick={onNovoContrato}>
          Começar um novo contrato
        </button>
      </div>
    </section>
  )
}