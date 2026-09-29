export type EtapaFluxo = 'consulta' | 'dados' | 'revisao' | 'concluido'

const ETAPAS: { chave: EtapaFluxo; rotulo: string }[] = [
  { chave: 'consulta', rotulo: 'Consulta' },
  { chave: 'dados', rotulo: 'Dados do contrato' },
  { chave: 'revisao', rotulo: 'Revisão' },
  { chave: 'concluido', rotulo: 'Documento gerado' }
]

interface IndicadorEtapasProps {
  etapaAtual: EtapaFluxo
}

export function IndicadorEtapas({ etapaAtual }: IndicadorEtapasProps) {
  const indiceAtual = ETAPAS.findIndex((etapa) => etapa.chave === etapaAtual)

  return (
    <div className="etapas">
      {ETAPAS.map((etapa, indice) => {
        const classe = [
          'etapa',
          indice < indiceAtual ? 'etapa--concluida' : '',
          indice === indiceAtual ? 'etapa--atual' : ''
        ].filter(Boolean).join(' ')

        return (
          <div key={etapa.chave} className={classe}>
            <span className="etapa__marcador" />
            <span className="etapa__rotulo">{etapa.rotulo}</span>
          </div>
        )
      })}
    </div>
  )
}