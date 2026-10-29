import { useState } from 'react'
import type { Contrato } from '../types/contrato'
import { avancarParaEnvio } from '../services/envioService'
import { ListaContratos } from './ListaContratos'
import { DetalheContrato } from './DetalheContrato'
import { VisualizadorContrato } from './VisualizadorContrato'
import { TelaEnvio } from './TelaEnvio'

type Visao =
  | { tipo: 'lista' }
  | { tipo: 'detalhe'; id: number }
  | { tipo: 'documento'; contrato: Contrato }
  | { tipo: 'envio'; contrato: Contrato }

interface AreaContratosProps {
  ativa: boolean
}

export function AreaContratos({ ativa }: AreaContratosProps) {
  const [visao, setVisao] = useState<Visao>({ tipo: 'lista' })
  const [erro, setErro] = useState('')

  async function irParaEnvio(contrato: Contrato) {
    setErro('')

    if (contrato.status === 'Gerado') {
      const resultado = await avancarParaEnvio(contrato.id)
      if (!resultado.sucesso) {
        setErro(resultado.mensagem)
        return
      }
      setVisao({ tipo: 'envio', contrato: resultado.dados })
      return
    }

    setVisao({ tipo: 'envio', contrato })
  }

  return (
    <>
      {erro && <p className="mensagem mensagem--erro" role="alert">{erro}</p>}

      {visao.tipo === 'lista' && (
        <ListaContratos ativa={ativa} onAbrir={(id) => setVisao({ tipo: 'detalhe', id })} />
      )}

      {visao.tipo === 'detalhe' && (
        <DetalheContrato
          key={visao.id}
          id={visao.id}
          onVoltar={() => setVisao({ tipo: 'lista' })}
          onVerDocumento={(contrato) => setVisao({ tipo: 'documento', contrato })}
          onIrParaEnvio={irParaEnvio}
        />
      )}

      {visao.tipo === 'documento' && (
        <VisualizadorContrato
          contrato={visao.contrato}
          rotuloVoltar="Voltar aos detalhes"
          onVoltar={() => setVisao({ tipo: 'detalhe', id: visao.contrato.id })}
          podeContinuar={visao.contrato.status === 'Gerado' || visao.contrato.podeEnviarEmail}
          onContinuarParaEnvio={() => irParaEnvio(visao.contrato)}
        />
      )}

      {visao.tipo === 'envio' && (
        <TelaEnvio
          contrato={visao.contrato}
          rotuloVoltar="Voltar aos detalhes"
          onVoltar={() => setVisao({ tipo: 'detalhe', id: visao.contrato.id })}
          onContratoAtualizado={(contrato) => setVisao({ tipo: 'envio', contrato })}
        />
      )}
    </>
  )
}