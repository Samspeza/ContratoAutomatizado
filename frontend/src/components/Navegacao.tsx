export type Secao = 'novo' | 'contratos' | 'fluxo' | 'resumo' | 'configuracoes'

interface NavegacaoProps {
  secaoAtual: Secao
  onMudar: (secao: Secao) => void
}

const ITENS: { secao: Secao; rotulo: string }[] = [
  { secao: 'novo', rotulo: 'Novo contrato' },
  { secao: 'contratos', rotulo: 'Contratos' },
  { secao: 'fluxo', rotulo: 'Fluxo' },
  { secao: 'resumo', rotulo: 'Resumo' },
  { secao: 'configuracoes', rotulo: 'Configurações' }
]

export function Navegacao({ secaoAtual, onMudar }: NavegacaoProps) {
  return (
    <nav className="navegacao">
      {ITENS.map((item) => (
        <button
          key={item.secao}
          type="button"
          className={secaoAtual === item.secao ? 'navegacao__item navegacao__item--ativo' : 'navegacao__item'}
          onClick={() => onMudar(item.secao)}
        >
          {item.rotulo}
        </button>
      ))}
    </nav>
  )
}