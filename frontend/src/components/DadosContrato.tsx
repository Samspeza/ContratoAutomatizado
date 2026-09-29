import { useState } from 'react'
import type { DadosContrato as DadosContratoType, ResponsavelAssinatura as ResponsavelAssinaturaType } from '../types/dadosContrato'
import { CONFIG_CONTRATO } from '../config/configContrato'
import { apenasDigitosCpf, cpfValido } from '../utils/cpfFormat'
import { ResponsavelAssinatura } from './ResponsavelAssinatura'

interface DadosContratoProps {
  onDadosConfirmados: (dados: DadosContratoType) => void
}

interface Erros {
  honorarios?: string
  parcelas?: string
  data?: string
  responsaveis: { nome?: string; cpf?: string }[]
}

const responsavelVazio: ResponsavelAssinaturaType = { nome: '', cpf: '' }

export function DadosContrato({ onDadosConfirmados }: DadosContratoProps) {
  const [percentualHonorarios, setPercentualHonorarios] = useState('')
  const [quantidadeParcelas, setQuantidadeParcelas] = useState('')
  const [dataContrato, setDataContrato] = useState('')
  const [responsaveis, setResponsaveis] = useState<ResponsavelAssinaturaType[]>([{ ...responsavelVazio }])
  const [erros, setErros] = useState<Erros>({ responsaveis: [] })

  function adicionarResponsavel() {
    if (responsaveis.length >= CONFIG_CONTRATO.responsaveis.maximo) return
    setResponsaveis([...responsaveis, { ...responsavelVazio }])
  }

  function removerResponsavel(indice: number) {
    setResponsaveis(responsaveis.filter((_, i) => i !== indice))
  }

  function alterarResponsavel(indice: number, responsavel: ResponsavelAssinaturaType) {
    setResponsaveis(responsaveis.map((r, i) => (i === indice ? responsavel : r)))
  }

  function validar(): Erros {
    const novosErros: Erros = { responsaveis: [] }

    const honorarios = Number(percentualHonorarios)
    if (!percentualHonorarios || Number.isNaN(honorarios) ||
        honorarios < CONFIG_CONTRATO.honorarios.minimoPercentual ||
        honorarios > CONFIG_CONTRATO.honorarios.maximoPercentual) {
      novosErros.honorarios = `Informe um percentual entre ${CONFIG_CONTRATO.honorarios.minimoPercentual}% e ${CONFIG_CONTRATO.honorarios.maximoPercentual}%.`
    }

    const parcelas = Number(quantidadeParcelas)
    if (!quantidadeParcelas || !Number.isInteger(parcelas) ||
        parcelas < CONFIG_CONTRATO.parcelas.minimo ||
        parcelas > CONFIG_CONTRATO.parcelas.maximo) {
      novosErros.parcelas = `Informe uma quantidade de parcelas entre ${CONFIG_CONTRATO.parcelas.minimo} e ${CONFIG_CONTRATO.parcelas.maximo}.`
    }

    if (!dataContrato) {
      novosErros.data = 'Informe a data do contrato.'
    }

    novosErros.responsaveis = responsaveis.map((responsavel) => {
      const erroResponsavel: { nome?: string; cpf?: string } = {}
      if (!responsavel.nome.trim()) {
        erroResponsavel.nome = 'Informe o nome do responsável.'
      }
      const cpfFoiPreenchido = apenasDigitosCpf(responsavel.cpf).length > 0
      if (cpfFoiPreenchido && !cpfValido(responsavel.cpf)) {
        erroResponsavel.cpf = 'CPF inválido.'
      }
      return erroResponsavel
    })

    return novosErros
  }

  function temErro(erros: Erros): boolean {
    return Boolean(erros.honorarios || erros.parcelas || erros.data) ||
      erros.responsaveis.some((e) => e.nome || e.cpf)
  }

  function handleContinuar() {
    const novosErros = validar()
    setErros(novosErros)

    if (temErro(novosErros)) return

    onDadosConfirmados({
      percentualHonorarios: Number(percentualHonorarios),
      quantidadeParcelas: Number(quantidadeParcelas),
      dataContrato,
      responsaveis
    })
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Dados do contrato</h2>
      <p className="cartao__descricao">Preencha as informações específicas desta contratação.</p>

      <div className="campo">
        <label htmlFor="honorarios">Percentual de honorários</label>
        <div className="campo-com-sufixo">
          <input
            id="honorarios"
            type="number"
            step="0.01"
            value={percentualHonorarios}
            onChange={(e) => setPercentualHonorarios(e.target.value)}
          />
          <span className="campo-com-sufixo__unidade">%</span>
        </div>
        {erros.honorarios && <p className="mensagem mensagem--erro" role="alert">{erros.honorarios}</p>}
      </div>

      <div className="campo">
        <label htmlFor="parcelas">Quantidade de parcelas</label>
        <input
          id="parcelas"
          type="number"
          value={quantidadeParcelas}
          onChange={(e) => setQuantidadeParcelas(e.target.value)}
        />
        {erros.parcelas && <p className="mensagem mensagem--erro" role="alert">{erros.parcelas}</p>}
      </div>

      <div className="campo">
        <label htmlFor="data">Data do contrato</label>
        <input
          id="data"
          type="date"
          value={dataContrato}
          onChange={(e) => setDataContrato(e.target.value)}
        />
        {erros.data && <p className="mensagem mensagem--erro" role="alert">{erros.data}</p>}
      </div>

      {responsaveis.map((responsavel, indice) => (
        <ResponsavelAssinatura
          key={indice}
          indice={indice}
          responsavel={responsavel}
          podeRemover={responsaveis.length > CONFIG_CONTRATO.responsaveis.minimo}
          erroNome={erros.responsaveis[indice]?.nome}
          erroCpf={erros.responsaveis[indice]?.cpf}
          onAlterar={(novoResponsavel) => alterarResponsavel(indice, novoResponsavel)}
          onRemover={() => removerResponsavel(indice)}
        />
      ))}

      {responsaveis.length < CONFIG_CONTRATO.responsaveis.maximo && (
        <button type="button" className="botao botao--texto" onClick={adicionarResponsavel}>
          + Adicionar outro responsável
        </button>
      )}

      <div className="acoes">
        <button type="button" className="botao botao--primario" onClick={handleContinuar}>
          Continuar
        </button>
      </div>
    </section>
  )
}