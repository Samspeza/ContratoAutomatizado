import { useEffect, useState } from 'react'
import type { ContratoResumo } from '../types/contrato'
import type { ResumoContratos } from '../types/resumo'
import { obterResumo } from '../services/resumoService'
import { proximoPasso } from '../utils/proximoPasso'
import { descreverIdade } from '../utils/tempo'
import { rotuloStatus, varianteStatus } from '../utils/rotuloStatus'
import { Selo } from './Selo'

interface IndicadorProps {
  rotulo: string
  valor: number
  destaque?: 'erro' | 'aviso'
}

function Indicador({ rotulo, valor, destaque }: IndicadorProps) {
  const classe = destaque && valor > 0 ? `indicador indicador--${destaque}` : 'indicador'

  return (
    <div className={classe}>
      <span className="indicador__valor">{valor}</span>
      <span className="indicador__rotulo">{rotulo}</span>
    </div>
  )
}

function descreverMotivo(item: ContratoResumo): string {
  if (item.status === 'ErroGeracao' || item.status === 'ErroNoEnvio') {
    return `${rotuloStatus(item.status)} ${descreverIdade(item.atualizadoEm)}`
  }
  return `Parado ${descreverIdade(item.atualizadoEm)}`
}

interface PainelResumoProps {
  ativa: boolean
  onAbrir: (id: number) => void
}

export function PainelResumo({ ativa, onAbrir }: PainelResumoProps) {
  const [resumo, setResumo] = useState<ResumoContratos | null>(null)
  const [carregando, setCarregando] = useState(false)
  const [erro, setErro] = useState('')
  const [carregadoEm, setCarregadoEm] = useState<Date | null>(null)
  const [chaveAtualizacao, setChaveAtualizacao] = useState(0)

  // Recarrega quando a aba fica visível e quando se clica em "Atualizar".
  useEffect(() => {
    if (!ativa) return

    let cancelado = false
    setCarregando(true)

    obterResumo().then((resposta) => {
      if (cancelado) return
      if (resposta.sucesso) {
        setResumo(resposta.dados)
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
  }, [ativa, chaveAtualizacao])

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Resumo</h2>
      <p className="cartao__descricao">O que aconteceu hoje e o que precisa de atenção.</p>

      <div className="fluxo__barra" style={{ justifyContent: 'flex-end' }}>
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

      {resumo && (
        <>
          <div className="indicadores">
            <Indicador rotulo="Criados hoje" valor={resumo.criadosHoje} />
            <Indicador rotulo="Enviados hoje" valor={resumo.enviadosHoje} />
            <Indicador rotulo="Aguardando envio" valor={resumo.aguardandoEnvio} destaque="aviso" />
            <Indicador rotulo="Com erro" valor={resumo.comErro} destaque="erro" />
          </div>

          <div className="bloco-canal">
            <h3 className="bloco-canal__titulo">Pendências</h3>
            <p className="cartao__descricao" style={{ marginBottom: '0.75rem' }}>
              Contratos com erro ou parados há {resumo.diasParado} dias ou mais. Clique para abrir e agir.
            </p>

            {resumo.pendencias.length === 0 ? (
              <p className="mensagem mensagem--sucesso">Nenhuma pendência. Tudo em dia.</p>
            ) : (
              <ul className="pendencias">
                {resumo.pendencias.map((item) => (
                  <li key={item.id}>
                    <button type="button" className="pendencia" onClick={() => onAbrir(item.id)}>
                      <span className="pendencia__empresa">
                        <strong>{item.razaoSocial}</strong>
                        <span className="cartao-fluxo__cnpj">{item.cnpj}</span>
                      </span>
                      <Selo texto={rotuloStatus(item.status)} variante={varianteStatus(item.status)} />
                      <span className="cartao-fluxo__tempo">{descreverMotivo(item)}</span>
                      <span className="cartao-fluxo__passo">{proximoPasso(item.status)} ›</span>
                    </button>
                  </li>
                ))}
              </ul>
            )}

            {resumo.totalPendencias > resumo.pendencias.length && (
              <p className="cartao-fluxo__tempo" style={{ marginTop: '0.75rem' }}>
                Mostrando as {resumo.pendencias.length} mais antigas de {resumo.totalPendencias}. Veja todas na aba Fluxo.
              </p>
            )}
          </div>
        </>
      )}
    </section>
  )
}