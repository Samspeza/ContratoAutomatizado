import { useEffect, useState } from 'react'
import type { EventoHistorico } from '../types/envio'
import { obterHistorico } from '../services/envioService'

interface HistoricoContratoProps {
  contratoId: number
  chaveAtualizacao: number
}

export function HistoricoContrato({ contratoId, chaveAtualizacao }: HistoricoContratoProps) {
  const [eventos, setEventos] = useState<EventoHistorico[]>([])
  const [erro, setErro] = useState(false)

  useEffect(() => {
    let cancelado = false

    obterHistorico(contratoId).then((resultado) => {
      if (cancelado) return
      if (resultado.sucesso) {
        setEventos(resultado.dados)
        setErro(false)
      } else {
        setErro(true)
      }
    })

    return () => {
      cancelado = true
    }
  }, [contratoId, chaveAtualizacao])

  return (
    <div className="bloco-canal">
      <h3 className="bloco-canal__titulo">Histórico</h3>

      {erro && <p className="mensagem mensagem--info">Não foi possível carregar o histórico agora.</p>}

      <ol className="historico">
        {eventos.map((evento, indice) => (
          <li
            key={`${evento.dataHora}-${indice}`}
            className={evento.tipo.startsWith('Falha') ? 'historico__item historico__item--erro' : 'historico__item'}
          >
            <span className="historico__data">{new Date(evento.dataHora).toLocaleString('pt-BR')}</span>
            <span>{evento.descricao}</span>
          </li>
        ))}
      </ol>
    </div>
  )
}