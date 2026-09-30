import { useState } from 'react'
import type { Empresa } from './types/empresa'
import type { DadosContrato as DadosContratoType } from './types/dadosContrato'
import type { Contrato } from './types/contrato'
import { Cabecalho } from './components/Cabecalho'
import { IndicadorEtapas, type EtapaFluxo } from './components/IndicadorEtapas'
import { ConsultaCnpj } from './components/ConsultaCnpj'
import { DadosContrato } from './components/DadosContrato'
import { RevisaoContrato } from './components/RevisaoContrato'
import { VisualizadorContrato } from './components/VisualizadorContrato'
import { TelaEnvio } from './components/TelaEnvio'
import { gerarContrato, gerarContratoNovamente, type GerarContratoResultado } from './services/contratoService'
import { avancarParaEnvio } from './services/envioService'
import './App.css'

type Etapa = 'formulario' | 'revisao' | 'visualizacao' | 'envio'

function App() {
  const [empresaSelecionada, setEmpresaSelecionada] = useState<Empresa | null>(null)
  const [dadosContrato, setDadosContrato] = useState<DadosContratoType | null>(null)
  const [etapa, setEtapa] = useState<Etapa>('formulario')
  const [contratoGerado, setContratoGerado] = useState<Contrato | null>(null)
  const [resultadoGeracao, setResultadoGeracao] = useState<GerarContratoResultado | null>(null)
  const [gerando, setGerando] = useState(false)
  const [chaveReinicio, setChaveReinicio] = useState(0)

  function handleEmpresaEncontrada(empresa: Empresa) {
    setEmpresaSelecionada(empresa)
    setDadosContrato(null)
    setContratoGerado(null)
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

  function aplicarResultado(resultado: GerarContratoResultado) {
    setResultadoGeracao(resultado)
    setGerando(false)
    if (resultado.sucesso) {
      setContratoGerado(resultado.contrato)
      setEtapa('visualizacao')
    }
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

    aplicarResultado(resultado)
  }

  async function handleTentarGerarNovamente(id: number) {
    setGerando(true)
    setResultadoGeracao(null)
    aplicarResultado(await gerarContratoNovamente(id))
  }

  async function handleContinuarParaEnvio() {
    if (!contratoGerado) return
    const resultado = await avancarParaEnvio(contratoGerado.id)
    if (resultado.sucesso) {
      setContratoGerado(resultado.dados)
      setEtapa('envio')
    }
  }

  function handleVoltarParaVisualizacao() {
    setEtapa('visualizacao')
  }

  function handleNovoContrato() {
    setEmpresaSelecionada(null)
    setDadosContrato(null)
    setContratoGerado(null)
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
        : etapa === 'envio'
          ? 'envio'
          : 'documento'

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
          <div className="mensagem mensagem--erro" role="alert">
            <p style={{ margin: 0 }}>{resultadoGeracao.mensagem}</p>
            {resultadoGeracao.contratoId && (
              <button
                type="button"
                className="botao botao--secundario"
                style={{ marginTop: '0.6rem' }}
                onClick={() => handleTentarGerarNovamente(resultadoGeracao.contratoId!)}
                disabled={gerando}
              >
                {gerando ? 'Tentando...' : 'Tentar gerar novamente'}
              </button>
            )}
          </div>
        )}

        {etapa === 'visualizacao' && contratoGerado && (
          <VisualizadorContrato
            contrato={contratoGerado}
            onVoltar={handleVoltarParaEdicao}
            onContinuarParaEnvio={handleContinuarParaEnvio}
            onNovoContrato={handleNovoContrato}
          />
        )}

        {etapa === 'envio' && contratoGerado && (
          <TelaEnvio contrato={contratoGerado} onVoltar={handleVoltarParaVisualizacao} />
        )}
      </div>
    </div>
  )
}

export default App