import { useEffect, useState } from 'react'
import type { PaginaContratos } from '../types/contrato'
import { listarContratos } from '../services/gestaoContratosService'
import { STATUS_DISPONIVEIS, rotuloEmail, rotuloStatus, varianteStatus } from '../utils/rotuloStatus'
import { Selo } from './Selo'

const TAMANHO_PAGINA = 20

interface ListaContratosProps {
  ativa: boolean
  onAbrir: (id: number) => void
}

function formatarDataHora(iso: string) {
  return new Date(iso).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
}

export function ListaContratos({ ativa, onAbrir }: ListaContratosProps) {
  const [busca, setBusca] = useState('')
  const [buscaAplicada, setBuscaAplicada] = useState('')
  const [status, setStatus] = useState('')
  const [de, setDe] = useState('')
  const [ate, setAte] = useState('')
  const [pagina, setPagina] = useState(1)
  const [resultado, setResultado] = useState<PaginaContratos | null>(null)
  const [carregando, setCarregando] = useState(false)
  const [erro, setErro] = useState('')

  // A busca só é aplicada depois que a pessoa para de digitar.
  useEffect(() => {
    const espera = setTimeout(() => {
      setBuscaAplicada(busca)
      setPagina(1)
    }, 300)
    return () => clearTimeout(espera)
  }, [busca])

  // Recarrega sempre que um filtro muda e também quando a aba volta a ficar visível,
  // para mostrar contratos criados em "Novo contrato".
  useEffect(() => {
    if (!ativa) return

    let cancelado = false
    setCarregando(true)

    listarContratos({ busca: buscaAplicada, status, de, ate, pagina, tamanho: TAMANHO_PAGINA }).then((resposta) => {
      if (cancelado) return
      if (resposta.sucesso) {
        setResultado(resposta.dados)
        setErro('')
      } else {
        setErro(resposta.mensagem)
      }
      setCarregando(false)
    })

    return () => {
      cancelado = true
    }
  }, [ativa, buscaAplicada, status, de, ate, pagina])

  const filtrosAtivos = Boolean(busca || status || de || ate)
  const totalPaginas = resultado ? Math.max(1, Math.ceil(resultado.total / resultado.tamanho)) : 1

  function limparFiltros() {
    setBusca('')
    setBuscaAplicada('')
    setStatus('')
    setDe('')
    setAte('')
    setPagina(1)
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Contratos</h2>
      <p className="cartao__descricao">Acompanhe, reabra e reenvie os contratos gerados.</p>

      <div className="filtros">
        <div className="campo">
          <label htmlFor="filtro-busca">Pesquisar</label>
          <input
            id="filtro-busca"
            type="text"
            value={busca}
            onChange={(e) => setBusca(e.target.value)}
            placeholder="Empresa ou CNPJ"
          />
        </div>

        <div className="campo">
          <label htmlFor="filtro-status">Status</label>
          <select
            id="filtro-status"
            value={status}
            onChange={(e) => {
              setStatus(e.target.value)
              setPagina(1)
            }}
          >
            <option value="">Todos</option>
            {STATUS_DISPONIVEIS.map((s) => (
              <option key={s} value={s}>{rotuloStatus(s)}</option>
            ))}
          </select>
        </div>

        <div className="campo">
          <label htmlFor="filtro-de">Criado de</label>
          <input
            id="filtro-de"
            type="date"
            value={de}
            onChange={(e) => {
              setDe(e.target.value)
              setPagina(1)
            }}
          />
        </div>

        <div className="campo">
          <label htmlFor="filtro-ate">Criado até</label>
          <input
            id="filtro-ate"
            type="date"
            value={ate}
            onChange={(e) => {
              setAte(e.target.value)
              setPagina(1)
            }}
          />
        </div>
      </div>

      {filtrosAtivos && (
        <button type="button" className="botao botao--texto" onClick={limparFiltros}>
          Limpar filtros
        </button>
      )}

      {erro && <p className="mensagem mensagem--erro" role="alert">{erro}</p>}

      {resultado && resultado.itens.length > 0 && (
        <table className="tabela-contratos">
          <thead>
            <tr>
              <th>Empresa</th>
              <th>Criado em</th>
              <th>Status</th>
              <th>E-mail</th>
              <th />
            </tr>
          </thead>
          <tbody>
            {resultado.itens.map((item) => {
              const email = rotuloEmail(item.statusEmail, item.status)
              return (
                <tr
                  key={item.id}
                  tabIndex={0}
                  onClick={() => onAbrir(item.id)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') onAbrir(item.id)
                  }}
                >
                  <td className="tabela-contratos__empresa">
                    <strong>{item.razaoSocial}</strong>
                    <span className="tabela-contratos__cnpj">{item.cnpj}</span>
                  </td>
                  <td>{formatarDataHora(item.criadoEm)}</td>
                  <td><Selo texto={rotuloStatus(item.status)} variante={varianteStatus(item.status)} /></td>
                  <td><Selo texto={email.texto} variante={email.variante} /></td>
                  <td className="tabela-contratos__abrir">Abrir ›</td>
                </tr>
              )
            })}
          </tbody>
        </table>
      )}

      {resultado && resultado.itens.length === 0 && !carregando && !erro && (
        <p className="mensagem mensagem--info">
          {filtrosAtivos
            ? 'Nenhum contrato encontrado com esses filtros.'
            : 'Ainda não há contratos. Os contratos gerados a partir de agora aparecem aqui.'}
        </p>
      )}

      {carregando && <p className="mensagem mensagem--info">Carregando...</p>}

      {resultado && resultado.total > 0 && (
        <div className="paginacao">
          <span>{resultado.total} {resultado.total === 1 ? 'contrato' : 'contratos'}</span>
          <div className="acoes" style={{ margin: 0 }}>
            <button
              type="button"
              className="botao botao--secundario"
              onClick={() => setPagina((p) => Math.max(1, p - 1))}
              disabled={pagina <= 1}
            >
              Anterior
            </button>
            <span style={{ alignSelf: 'center' }}>Página {pagina} de {totalPaginas}</span>
            <button
              type="button"
              className="botao botao--secundario"
              onClick={() => setPagina((p) => Math.min(totalPaginas, p + 1))}
              disabled={pagina >= totalPaginas}
            >
              Próxima
            </button>
          </div>
        </div>
      )}
    </section>
  )
}