import type { ItemVerificacao } from '../types/configuracoes'

interface VerificacaoSistemaProps {
  itens: ItemVerificacao[]
  onVerificar: () => void
}

export function VerificacaoSistema({ itens, onVerificar }: VerificacaoSistemaProps) {
  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Verificação do sistema</h2>
      <p className="cartao__descricao">Confere se o que o sistema precisa está funcionando neste computador.</p>

      <ul className="verificacao">
        {itens.map((item) => (
          <li key={item.chave} className={item.ok ? 'verificacao__item' : 'verificacao__item verificacao__item--erro'}>
            <span className="verificacao__icone">{item.ok ? '✓' : '✕'}</span>
            <span>
              <strong>{item.titulo}</strong>
              <span className="verificacao__detalhe">{item.detalhe}</span>
            </span>
          </li>
        ))}
      </ul>

      <div className="acoes">
        <button type="button" className="botao botao--secundario" onClick={onVerificar}>
          Verificar novamente
        </button>
      </div>
    </section>
  )
}