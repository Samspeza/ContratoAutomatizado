import { useState } from 'react'
import type { Empresa } from './types/empresa'
import type { DadosContrato as DadosContratoType } from './types/dadosContrato'
import { Cabecalho } from './components/Cabecalho'
import { IndicadorEtapas, type EtapaFluxo } from './components/IndicadorEtapas'
import { ConsultaCnpj } from './components/ConsultaCnpj'
import { DadosContrato } from './components/DadosContrato'
import { RevisaoContrato } from './components/RevisaoContrato'
import { gerarContrato, type GerarContratoResultado } from './services/contratoService'
import './App.css'

type Etapa = 'formulario' | 'revisao' | 'resultado'

function App() {
  const [empresaSelecionada, setEmpresaSelecionada] = useState<Empresa | null>(null)
  const [dadosContrato, setDadosContrato] = useState<DadosContratoType | null>(null)
  const [etapa, setEtapa] = useState<Etapa>('formulario')
  const [resultadoGeracao, setResultadoGeracao] = useState<GerarContratoResultado | null>(null)
  const [gerando, setGerando] = useState(false)
  const [chaveReinicio, setChaveReinicio] = useState(0)

  function handleEmpresaEncontrada(empresa: Empresa) {
    setEmpresaSelecionada(empresa)
    setDadosContrato(null)
    setResultadoGeracao(null)
    setEtapa('formulario')
  }

  function handleDadosConfirmados(dados: DadosContratoType) {
    setDadosContrato(dados)
    setEtapa('revisao')
  }

  function handleVoltarParaEdicao() {
    setEtapa('formulario')
  }

  async function handleConfirmarGeracao() {
    if (!empresaSelecionada || !dadosContrato) return

    setGerando(true)
    setResultadoGeracao(null)

    const resultado = await gerarContrato({
      empresa: empresaSelecionada,
      complementoEndereco: null,
      percentualHonorarios: dadosContrato.percentualHonorarios,
      quantidadeParcelas: dadosContrato.quantidadeParcelas,
      dataContrato: dadosContrato.dataContrato,
      responsaveis: dadosContrato.responsaveis
    })

    setResultadoGeracao(resultado)
    setGerando(false)
    if (resultado.sucesso) {
      setEtapa('resultado')
    }
  }

  function handleNovoContrato() {
    setEmpresaSelecionada(null)
    setDadosContrato(null)
    setResultadoGeracao(null)
    setEtapa('formulario')
    setChaveReinicio((chave) => chave + 1)
  }

  const etapaIndicador: EtapaFluxo = !empresaSelecionada
    ? 'consulta'
    : etapa === 'formulario'
      ? 'dados'
      : etapa === 'revisao'
        ? 'revisao'
        : 'concluido'

  return (
    <div className="aplicacao">
      <Cabecalho />

      <div className="conteudo">
        <IndicadorEtapas etapaAtual={etapaIndicador} />

        <ConsultaCnpj key={chaveReinicio} onEmpresaEncontrada={handleEmpresaEncontrada} />

        {empresaSelecionada && etapa === 'formulario' && (
          <DadosContrato onDadosConfirmados={handleDadosConfirmados} />
        )}

        {empresaSelecionada && dadosContrato && etapa === 'revisao' && (
          <RevisaoContrato
            empresa={empresaSelecionada}
            dados={dadosContrato}
            gerando={gerando}
            onConfirmar={handleConfirmarGeracao}
            onVoltar={handleVoltarParaEdicao}
          />
        )}

        {resultadoGeracao && !resultadoGeracao.sucesso && (
          <p className="mensagem mensagem--erro" role="alert">{resultadoGeracao.mensagem}</p>
        )}

        {etapa === 'resultado' && resultadoGeracao?.sucesso && (
          <section className="cartao">
            <h2 className="cartao__titulo">Contrato gerado</h2>
            <p className="mensagem mensagem--sucesso">
              Arquivos gerados: <strong>{resultadoGeracao.arquivoDocx}</strong> e <strong>{resultadoGeracao.arquivoPdf}</strong>
            </p>
            <p className="mensagem mensagem--info">PDF salvo em: {resultadoGeracao.caminhoCompletoPdf}</p>
            <div className="acoes">
              <button type="button" className="botao botao--primario" onClick={handleNovoContrato}>
                Gerar novo contrato
              </button>
            </div>
          </section>
        )}
      </div>
    </div>
  )
}

export default App