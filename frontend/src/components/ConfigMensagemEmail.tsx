import { useEffect, useMemo, useRef, useState } from 'react'
import type { MensagemEmail, Retorno } from '../types/configuracoes'
import { obterMensagemEmail, salvarMensagemEmail } from '../services/configuracoesService'
import { MensagemRetorno } from './MensagemRetorno'

const PADRAO_CAMPO = /\{\{([^{}]*)\}\}/g

function substituir(texto: string, exemplos: Record<string, string>): string {
  return texto.replace(PADRAO_CAMPO, (completo, nome: string) => exemplos[nome] ?? completo)
}

function encontrarDesconhecidos(texto: string, conhecidos: string[]): string[] {
  const encontrados = Array.from(texto.matchAll(PADRAO_CAMPO), (m) => m[1])
  return Array.from(new Set(encontrados.filter((nome) => !conhecidos.includes(nome))))
}

interface ConfigMensagemEmailProps {
  ativa: boolean
}

export function ConfigMensagemEmail({ ativa }: ConfigMensagemEmailProps) {
  const [original, setOriginal] = useState<MensagemEmail | null>(null)
  const [assunto, setAssunto] = useState('')
  const [corpo, setCorpo] = useState('')
  const [erroCarregamento, setErroCarregamento] = useState('')
  const [salvando, setSalvando] = useState(false)
  const [retorno, setRetorno] = useState<Retorno>(null)
  const corpoRef = useRef<HTMLTextAreaElement>(null)

  // Carrega na primeira vez que a aba é aberta; depois mantém o que você estiver editando.
  useEffect(() => {
    if (!ativa || original) return

    let cancelado = false
    obterMensagemEmail().then((resultado) => {
      if (cancelado) return
      if (resultado.sucesso) {
        setOriginal(resultado.dados)
        setAssunto(resultado.dados.assunto)
        setCorpo(resultado.dados.corpo)
        setErroCarregamento('')
      } else {
        setErroCarregamento(resultado.mensagem)
      }
    })

    return () => {
      cancelado = true
    }
  }, [ativa, original])

  const exemplos = useMemo(
    () => Object.fromEntries((original?.placeholders ?? []).map((p) => [p.nome, p.exemplo])),
    [original]
  )

  if (erroCarregamento) {
    return (
      <section className="cartao">
        <h2 className="cartao__titulo">Mensagem do e-mail</h2>
        <p className="mensagem mensagem--erro" role="alert">{erroCarregamento}</p>
      </section>
    )
  }

  if (!original) {
    return (
      <section className="cartao">
        <h2 className="cartao__titulo">Mensagem do e-mail</h2>
        <p className="mensagem mensagem--info">Carregando...</p>
      </section>
    )
  }

  const nomesConhecidos = original.placeholders.map((p) => p.nome)
  const desconhecidos = encontrarDesconhecidos(`${assunto}\n${corpo}`, nomesConhecidos)
  const alterada = assunto !== original.assunto || corpo !== original.corpo
  const podeSalvar = alterada && assunto.trim() !== '' && corpo.trim() !== '' && desconhecidos.length === 0 && !salvando

  function inserirCampo(nome: string) {
    const area = corpoRef.current
    const inicio = area?.selectionStart ?? corpo.length
    const fim = area?.selectionEnd ?? corpo.length
    const campo = `{{${nome}}}`

    setCorpo(corpo.slice(0, inicio) + campo + corpo.slice(fim))

    setTimeout(() => {
      area?.focus()
      const posicao = inicio + campo.length
      area?.setSelectionRange(posicao, posicao)
    }, 0)
  }

  async function handleSalvar() {
    setSalvando(true)
    setRetorno(null)

    const resultado = await salvarMensagemEmail(assunto, corpo)
    if (resultado.sucesso) {
      setOriginal(resultado.dados)
      setAssunto(resultado.dados.assunto)
      setCorpo(resultado.dados.corpo)
      setRetorno({ sucesso: true, texto: 'Mensagem salva. Vale a partir do próximo e-mail.' })
    } else {
      setRetorno({ sucesso: false, texto: resultado.mensagem })
    }

    setSalvando(false)
  }

  function handleRestaurar() {
    if (!original) return
    setAssunto(original.assuntoPadrao)
    setCorpo(original.corpoPadrao)
    setRetorno(null)
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Mensagem do e-mail</h2>
      <p className="cartao__descricao">
        Texto padrão enviado junto com o contrato. Os campos entre chaves são preenchidos automaticamente com os dados
        de cada contrato.
      </p>

      {original.aviso && <p className="mensagem mensagem--aviso">{original.aviso}</p>}

      <div className="campo">
        <label htmlFor="mensagem-assunto">Assunto</label>
        <input id="mensagem-assunto" type="text" value={assunto} onChange={(e) => setAssunto(e.target.value)} />
      </div>

      <div className="campo">
        <label htmlFor="mensagem-corpo">Mensagem</label>
        <textarea id="mensagem-corpo" ref={corpoRef} rows={14} value={corpo} onChange={(e) => setCorpo(e.target.value)} />
      </div>

      <p className="cartao__descricao" style={{ marginBottom: '0.5rem' }}>
        Clique para inserir um campo na mensagem, na posição do cursor:
      </p>
      <div className="chips">
        {original.placeholders.map((p) => (
          <button key={p.nome} type="button" className="chip" title={p.descricao} onClick={() => inserirCampo(p.nome)}>
            {`{{${p.nome}}}`}
          </button>
        ))}
      </div>

      {desconhecidos.length > 0 && (
        <p className="mensagem mensagem--erro" role="alert">
          Campos que não existem: {desconhecidos.map((nome) => `{{${nome}}}`).join(', ')}. Use somente os campos
          listados acima, exatamente como aparecem.
        </p>
      )}

      <div className="previa-envio">
        <p><strong>Prévia com dados de exemplo</strong></p>
        <p><strong>Assunto:</strong> {substituir(assunto, exemplos)}</p>
        <p className="previa-envio__mensagem">{substituir(corpo, exemplos)}</p>
      </div>

      <MensagemRetorno retorno={retorno} />

      <div className="acoes">
        <button type="button" className="botao botao--primario" onClick={handleSalvar} disabled={!podeSalvar}>
          {salvando ? 'Salvando...' : 'Salvar mensagem'}
        </button>
        <button type="button" className="botao botao--secundario" onClick={handleRestaurar} disabled={salvando}>
          Restaurar texto original
        </button>
      </div>
    </section>
  )
}