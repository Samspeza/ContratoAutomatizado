import { useEffect, useState } from 'react'
import type { Contrato } from '../types/contrato'
import { obterContrato } from '../services/envioService'
import { gerarContratoNovamente } from '../services/contratoService'
import { cancelarContrato } from '../services/gestaoContratosService'
import { formatarDataBr } from '../utils/formatarRevisao'
import { rotuloEmail, rotuloStatus, varianteStatus } from '../utils/rotuloStatus'
import { HistoricoContrato } from './HistoricoContrato'
import { Selo } from './Selo'

interface DetalheContratoProps {
  id: number
  onVoltar: () => void
  onVerDocumento: (contrato: Contrato) => void
  onIrParaEnvio: (contrato: Contrato) => void
}

export function DetalheContrato({ id, onVoltar, onVerDocumento, onIrParaEnvio }: DetalheContratoProps) {
  const [contrato, setContrato] = useState<Contrato | null>(null)
  const [erroCarregamento, setErroCarregamento] = useState('')
  const [mensagemAcao, setMensagemAcao] = useState('')
  const [processando, setProcessando] = useState(false)
  const [confirmandoCancelamento, setConfirmandoCancelamento] = useState(false)
  const [chaveHistorico, setChaveHistorico] = useState(0)

  useEffect(() => {
    let cancelado = false

    obterContrato(id).then((resultado) => {
      if (cancelado) return
      if (resultado.sucesso) {
        setContrato(resultado.dados)
      } else {
        setErroCarregamento(resultado.mensagem)
      }
    })

    return () => {
      cancelado = true
    }
  }, [id])

  async function handleGerarNovamente() {
    setProcessando(true)
    setMensagemAcao('')

    const resultado = await gerarContratoNovamente(id)
    if (resultado.sucesso) {
      setContrato(resultado.contrato)
    } else {
      setMensagemAcao(resultado.mensagem)
      const atualizado = await obterContrato(id)
      if (atualizado.sucesso) setContrato(atualizado.dados)
    }

    setChaveHistorico((chave) => chave + 1)
    setProcessando(false)
  }

  async function handleCancelar() {
    setProcessando(true)
    setMensagemAcao('')

    const resultado = await cancelarContrato(id)
    if (resultado.sucesso) {
      setContrato(resultado.dados)
    } else {
      setMensagemAcao(resultado.mensagem)
    }

    setConfirmandoCancelamento(false)
    setChaveHistorico((chave) => chave + 1)
    setProcessando(false)
  }

  if (erroCarregamento) {
    return (
      <section className="cartao">
        <p className="mensagem mensagem--erro" role="alert">{erroCarregamento}</p>
        <div className="acoes">
          <button type="button" className="botao botao--secundario" onClick={onVoltar}>Voltar à lista</button>
        </div>
      </section>
    )
  }

  if (!contrato) {
    return (
      <section className="cartao">
        <p className="mensagem mensagem--info">Carregando...</p>
      </section>
    )
  }

  const email = rotuloEmail(contrato.statusEmail, contrato.status)
  const podeContinuarParaEnvio = contrato.status === 'Gerado' || contrato.podeEnviarEmail
  const rotuloEnvio =
    contrato.status === 'Gerado' ? 'Continuar para envio' : contrato.status === 'Concluido' ? 'Ver envio' : 'Ir para o envio'
  const situacaoArquivo = contrato.arquivoDisponivel
    ? 'Disponível'
    : contrato.status === 'Rascunho' || contrato.status === 'ErroGeracao'
      ? 'Ainda não gerado'
      : 'Arquivo não encontrado neste computador'

  return (
    <section className="cartao">
      <div className="detalhe__cabecalho">
        <div>
          <h2 className="cartao__titulo">{contrato.razaoSocial}</h2>
          <p className="cartao__descricao">CNPJ {contrato.cnpj}</p>
        </div>
        <Selo texto={rotuloStatus(contrato.status)} variante={varianteStatus(contrato.status)} />
      </div>

      {contrato.status === 'ErroGeracao' && (
        <p className="mensagem mensagem--erro" role="alert">
          A geração deste contrato falhou. Você pode tentar novamente sem digitar os dados de novo.
        </p>
      )}

      <dl className="dados-lista">
        <dt>Endereço</dt>
        <dd>{contrato.endereco}</dd>

        <dt>Data do contrato</dt>
        <dd>{formatarDataBr(contrato.dataContrato)}</dd>

        <dt>Honorários</dt>
        <dd>{contrato.percentualHonorarios}%</dd>

        <dt>Parcelas</dt>
        <dd>{contrato.quantidadeParcelas}</dd>

        <dt>{contrato.responsaveis.length > 1 ? 'Responsáveis' : 'Responsável'}</dt>
        <dd>
          {contrato.responsaveis.map((responsavel, indice) => (
            <div key={indice}>
              {responsavel.nome}
              {responsavel.cpf ? ` — CPF ${responsavel.cpf}` : ''}
            </div>
          ))}
        </dd>

        <dt>Criado em</dt>
        <dd>{new Date(contrato.criadoEm).toLocaleString('pt-BR')}</dd>

        <dt>Arquivo</dt>
        <dd>{situacaoArquivo}</dd>

        <dt>E-mail</dt>
        <dd>
          {contrato.emailDestinatario ?? '—'} <Selo texto={email.texto} variante={email.variante} />
          {contrato.emailEnviadoEm && (
            <div className="tabela-contratos__cnpj">
              Último envio com sucesso em {new Date(contrato.emailEnviadoEm).toLocaleString('pt-BR')}
            </div>
          )}
        </dd>
      </dl>

      {contrato.statusEmail === 'Erro' && contrato.emailErroMensagem && (
        <p className="mensagem mensagem--erro" role="alert">✕ {contrato.emailErroMensagem}</p>
      )}

      {mensagemAcao && <p className="mensagem mensagem--erro" role="alert">{mensagemAcao}</p>}

      <div className="acoes" style={{ flexWrap: 'wrap' }}>
        <button type="button" className="botao botao--secundario" onClick={onVoltar} disabled={processando}>
          Voltar à lista
        </button>

        {contrato.arquivoDisponivel && (
          <button type="button" className="botao botao--secundario" onClick={() => onVerDocumento(contrato)}>
            Ver documento
          </button>
        )}

        {contrato.podeGerarArquivos && (
          <button type="button" className="botao botao--primario" onClick={handleGerarNovamente} disabled={processando}>
            {processando ? 'Gerando...' : 'Tentar gerar novamente'}
          </button>
        )}

        {podeContinuarParaEnvio && (
          <button type="button" className="botao botao--primario" onClick={() => onIrParaEnvio(contrato)}>
            {rotuloEnvio}
          </button>
        )}
      </div>

      {contrato.arquivoDisponivel && (
        <div className="acoes">
          <a className="botao botao--texto" href={`/api/contratos/${contrato.id}/pdf?baixar=true`} download>
            Baixar PDF
          </a>
          <a className="botao botao--texto" href={`/api/contratos/${contrato.id}/docx`} download>
            Baixar DOCX
          </a>
        </div>
      )}

      {contrato.podeCancelar && !confirmandoCancelamento && (
        <div className="acoes">
          <button type="button" className="botao botao--texto" onClick={() => setConfirmandoCancelamento(true)} disabled={processando}>
            Cancelar este contrato
          </button>
        </div>
      )}

      {confirmandoCancelamento && (
        <div className="mensagem mensagem--aviso">
          <p style={{ margin: 0 }}>
            Cancelar este contrato? Ele deixa de poder ser enviado. Essa ação não pode ser desfeita.
          </p>
          <div className="acoes">
            <button type="button" className="botao botao--secundario" onClick={() => setConfirmandoCancelamento(false)} disabled={processando}>
              Voltar
            </button>
            <button type="button" className="botao botao--primario" onClick={handleCancelar} disabled={processando}>
              {processando ? 'Cancelando...' : 'Sim, cancelar contrato'}
            </button>
          </div>
        </div>
      )}

      <HistoricoContrato contratoId={contrato.id} chaveAtualizacao={chaveHistorico} />
    </section>
  )
}