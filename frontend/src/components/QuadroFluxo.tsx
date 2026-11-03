import { useEffect, useState } from 'react'
import type { ContratoResumo } from '../types/contrato'
import { obterFluxo } from '../services/fluxoService'
import {
  COLUNAS_FLUXO,
  DIAS_CONCLUIDOS_PADRAO,
  DIAS_PARADO_ALERTA,
  OPCOES_DIAS_CONCLUIDOS,
  type ColunaFluxo
} from '../config/configFluxo'
import { proximoPasso } from '../utils/proximoPasso'
import { descreverIdade, diasDesde } from '../utils/tempo'
import { rotuloStatus, varianteStatus } from '../utils/rotuloStatus'
import { Selo } from './Selo'

interface CartaoFluxoProps {
  item: ContratoResumo
  coluna: ColunaFluxo
  onAbrir: (id: number) => void
}

function CartaoFluxo({ item, coluna, onAbrir }: CartaoFluxoProps) {
  const passo = proximoPasso(item.status)
  const parado = coluna.abertas && diasDesde(item.atualizadoEm) >= DIAS_PARADO_ALERTA

  let textoTempo: string
  if (!coluna.abertas) {
    textoTempo = `Enviado ${descreverIdade(item.emailEnviadoEm ?? item.atualizadoEm)}`
  } else if (parado) {
    textoTempo = `Parado ${descreverIdade(item.atualizadoEm)}`
  } else {
    textoTempo = `Atualizado ${descreverIdade(item.atualizadoEm)}`
  }

  return (
    <button type="button" className="cartao-fluxo" onClick={() => onAbrir(item.id)}>
      <strong>{item.razaoSocial}</strong>
      <span className="cartao-fluxo__cnpj">{item.cnpj}</span>
      {coluna.status.length > 1 && (
        <Selo texto={rotuloStatus(item.status)} variante={varianteStatus(item.status)} />
      )}
      {passo && <span className="cartao-fluxo__passo">{passo}</span>}
      <span className={parado ? 'cartao-fluxo__tempo cartao-fluxo__tempo--alerta' : 'cartao-fluxo__tempo'}>
        {textoTempo}
      </span>
    </button>
  )
}

interface QuadroFluxoProps {
  ativa: boolean
  onAbrir: (id: number) => void
}

export function QuadroFluxo({ ativa, onAbrir }: QuadroFluxoProps) {
  const [dias, setDias] = useState(DIAS_CONCLUIDOS_PADRAO)
  const [itens, setItens] = useState<ContratoResumo[]>([])
  const [carregando, setCarregando] = useState(false)
  const [erro, setErro] = useState('')
  const [carregadoEm, setCarregadoEm] = useState<Date | null>(null)
  const [chaveAtualizacao, setChaveAtualizacao] = useState(0)

  // Recarrega quando a aba fica visível, quando o período muda e quando se clica em "Atualizar".
  useEffect(() => {
    if (!ativa) return

    let cancelado = false
    setCarregando(true)

    obterFluxo(dias).then((resposta) => {
      if (cancelado) return
      if (resposta.sucesso) {
        setItens(resposta.dados)
        setErro('')
        setCarregadoEm(new Date())
      } else {
        setErro(resposta.mensagem)
      }
      setCarregando(false)
    })

    return () => {
      cancelado = true
    }
  }, [ativa, dias, chaveAtualizacao])

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Fluxo dos contratos</h2>
      <p className="cartao__descricao">
        Onde cada contrato está. Clique em um cartão para abrir o contrato e agir. Cancelados ficam só na aba Contratos.
      </p>

      <div className="fluxo__barra">
        <div className="campo">
          <label htmlFor="fluxo-dias">Concluídos dos últimos</label>
          <select id="fluxo-dias" value={dias} onChange={(e) => setDias(Number(e.target.value))}>
            {OPCOES_DIAS_CONCLUIDOS.map((opcao) => (
              <option key={opcao} value={opcao}>{opcao} dias</option>
            ))}
          </select>
        </div>

        <div className="fluxo__atualizar">
          {carregadoEm && (
            <span className="cartao-fluxo__tempo">
              Atualizado às {carregadoEm.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}
            </span>
          )}
          <button
            type="button"
            className="botao botao--secundario"
            onClick={() => setChaveAtualizacao((chave) => chave + 1)}
            disabled={carregando}
          >
            {carregando ? 'Atualizando...' : 'Atualizar'}
          </button>
        </div>
      </div>

      {erro && <p className="mensagem mensagem--erro" role="alert">{erro}</p>}

      <div className="quadro">
        {COLUNAS_FLUXO.map((coluna) => {
          const cartoes = itens
            .filter((item) => coluna.status.includes(item.status))
            .sort((a, b) => {
              const diferenca = new Date(a.atualizadoEm).getTime() - new Date(b.atualizadoEm).getTime()
              // Em aberto: o mais antigo primeiro (é o que está esperando há mais tempo).
              // Concluídos: o mais recente primeiro.
              return coluna.abertas ? diferenca : -diferenca
            })

          return (
            <div key={coluna.chave} className={`quadro__coluna quadro__coluna--${coluna.variante}`}>
              <div className="quadro__cabecalho">
                <h3 className="quadro__titulo">{coluna.titulo}</h3>
                <span className="quadro__total">{cartoes.length}</span>
              </div>
              <p className="quadro__descricao">{coluna.descricao}</p>

              {cartoes.length === 0 && <p className="quadro__vazio">Nenhum contrato aqui.</p>}

              {cartoes.map((item) => (
                <CartaoFluxo key={item.id} item={item} coluna={coluna} onAbrir={onAbrir} />
              ))}
            </div>
          )
        })}
      </div>
    </section>
  )
}