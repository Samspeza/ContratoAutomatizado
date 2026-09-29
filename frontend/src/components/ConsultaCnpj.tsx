import { useState } from 'react'
import type { Empresa } from '../types/empresa'
import { consultarCnpj } from '../services/cnpjService'
import { apenasDigitos, formatarCnpj } from '../utils/cnpjFormat'
import { DadosEmpresa } from './DadosEmpresa'

type Status = 'idle' | 'consultando' | 'encontrada' | 'erro'

interface ConsultaCnpjProps {
  onEmpresaEncontrada: (empresa: Empresa) => void
}

export function ConsultaCnpj({ onEmpresaEncontrada }: ConsultaCnpjProps) {
  const [cnpjDigitado, setCnpjDigitado] = useState('')
  const [status, setStatus] = useState<Status>('idle')
  const [mensagemErro, setMensagemErro] = useState('')
  const [empresa, setEmpresa] = useState<Empresa | null>(null)

  const cnpjTemTamanhoValido = apenasDigitos(cnpjDigitado).length === 14

  async function handleConsultar() {
    setStatus('consultando')
    setMensagemErro('')
    setEmpresa(null)

    const resultado = await consultarCnpj(apenasDigitos(cnpjDigitado))

    if (resultado.sucesso) {
      setStatus('encontrada')
      setEmpresa(resultado.empresa)
      onEmpresaEncontrada(resultado.empresa)
    } else {
      setStatus('erro')
      setMensagemErro(resultado.mensagem)
    }
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Nova contratação</h2>
      <p className="cartao__descricao">Informe o CNPJ da empresa contratante para começar.</p>

      <div className="linha-cnpj">
        <div className="campo">
          <label htmlFor="cnpj">CNPJ</label>
          <input
            id="cnpj"
            type="text"
            value={formatarCnpj(cnpjDigitado)}
            onChange={(e) => setCnpjDigitado(e.target.value)}
            placeholder="00.000.000/0000-00"
            maxLength={18}
          />
        </div>
        <button
          type="button"
          className="botao botao--primario"
          onClick={handleConsultar}
          disabled={!cnpjTemTamanhoValido || status === 'consultando'}
        >
          Consultar
        </button>
      </div>

      {status === 'consultando' && <p className="mensagem mensagem--info">Consultando CNPJ...</p>}
      {status === 'erro' && <p className="mensagem mensagem--erro" role="alert">{mensagemErro}</p>}
      {status === 'encontrada' && (
        <>
          <p className="mensagem mensagem--sucesso">Empresa encontrada.</p>
          {empresa && <DadosEmpresa empresa={empresa} />}
        </>
      )}
    </section>
  )
}