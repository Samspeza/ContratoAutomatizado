import type { ResponsavelAssinatura as ResponsavelAssinaturaType } from '../types/dadosContrato'
import { formatarCpf } from '../utils/cpfFormat'

interface ResponsavelAssinaturaProps {
  indice: number
  responsavel: ResponsavelAssinaturaType
  podeRemover: boolean
  erroNome?: string
  erroCpf?: string
  onAlterar: (responsavel: ResponsavelAssinaturaType) => void
  onRemover: () => void
}

export function ResponsavelAssinatura({
  indice,
  responsavel,
  podeRemover,
  erroNome,
  erroCpf,
  onAlterar,
  onRemover
}: ResponsavelAssinaturaProps) {
  return (
    <fieldset className="responsavel">
      <legend>Responsável pela assinatura {indice + 1}</legend>

      <div className="campo">
        <label htmlFor={`nome-${indice}`}>Nome</label>
        <input
          id={`nome-${indice}`}
          type="text"
          value={responsavel.nome}
          onChange={(e) => onAlterar({ ...responsavel, nome: e.target.value })}
        />
        {erroNome && <p className="mensagem mensagem--erro" role="alert">{erroNome}</p>}
      </div>

      <div className="campo">
        <label htmlFor={`cpf-${indice}`}>CPF (opcional)</label>
        <input
          id={`cpf-${indice}`}
          type="text"
          value={formatarCpf(responsavel.cpf)}
          onChange={(e) => onAlterar({ ...responsavel, cpf: e.target.value })}
          placeholder="000.000.000-00"
          maxLength={14}
        />
        {erroCpf && <p className="mensagem mensagem--erro" role="alert">{erroCpf}</p>}
      </div>

      {podeRemover && (
        <button type="button" className="botao botao--texto" onClick={onRemover}>
          Remover este responsável
        </button>
      )}
    </fieldset>
  )
}