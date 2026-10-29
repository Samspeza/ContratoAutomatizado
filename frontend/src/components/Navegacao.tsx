export type Secao = 'novo' | 'contratos'

interface NavegacaoProps {
  secaoAtual: Secao
  onMudar: (secao: Secao) => void
}

export function Navegacao({ secaoAtual, onMudar }: NavegacaoProps) {
  function classe(secao: Secao) {
    return secaoAtual === secao ? 'navegacao__item navegacao__item--ativo' : 'navegacao__item'
  }

  return (
    <nav className="navegacao">
      <button type="button" className={classe('novo')} onClick={() => onMudar('novo')}>
        Novo contrato
      </button>
      <button type="button" className={classe('contratos')} onClick={() => onMudar('contratos')}>
        Contratos
      </button>
    </nav>
  )
}