import { useState } from 'react'
import type { Empresa } from './types/empresa'
import type { DadosContrato as DadosContratoType } from './types/dadosContrato'
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
    // Sempre que uma nova consulta de CNPJ é feita, qualquer dado de contrato
    // e resultado anteriores deixam de valer para essa empresa — reinicia o fluxo a partir daqui.
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
    setChaveReinicio((chave) => chave + 1) // força o ConsultaCnpj a remontar, limpando o campo de CNPJ
  }

  return (
    <div style={{ fontFamily: 'sans-serif', padding: '2rem', maxWidth: '600px', margin: '0 auto' }}>
      <h1>Contrato Automatizado</h1>

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
        <p role="alert" style={{ color: '#b00020' }}>{resultadoGeracao.mensagem}</p>
      )}

      {etapa === 'resultado' && resultadoGeracao?.sucesso && (
        <section>
          <h2>Contrato gerado</h2>
          <p style={{ color: '#1a7f37' }}>
            Arquivos gerados: <strong>{resultadoGeracao.arquivoDocx}</strong> e <strong>{resultadoGeracao.arquivoPdf}</strong>
          </p>
          <p>PDF salvo em: {resultadoGeracao.caminhoCompletoPdf}</p>
          <button type="button" onClick={handleNovoContrato}>
            Gerar novo contrato
          </button>
        </section>
      )}
    </div>
  )
}

export default App