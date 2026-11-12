import { useEffect, useState } from 'react'
import type { ConfiguracaoArmazenamento, Retorno } from '../types/configuracoes'
import { abrirPasta, salvarPastaContratos } from '../services/configuracoesService'
import { MensagemRetorno } from './MensagemRetorno'

const PASTAS = [
  { chave: 'contratos', titulo: 'Contratos gerados', campo: 'pastaContratosGerados' },
  { chave: 'modelos', titulo: 'Modelo do contrato', campo: 'pastaModelos' },
  { chave: 'logs', titulo: 'Registros (logs)', campo: 'pastaLogs' }
] as const

interface ConfigArmazenamentoProps {
  config: ConfiguracaoArmazenamento
  onAlterado: () => void
}

export function ConfigArmazenamento({ config, onAlterado }: ConfigArmazenamentoProps) {
  const [pasta, setPasta] = useState(config.pastaContratosGerados)
  const [ocupado, setOcupado] = useState(false)
  const [retorno, setRetorno] = useState<Retorno>(null)

  useEffect(() => {
    setPasta(config.pastaContratosGerados)
  }, [config.pastaContratosGerados])

  async function handleAbrir(chave: 'contratos' | 'modelos' | 'logs') {
    setRetorno(null)
    const resultado = await abrirPasta(chave)
    if (!resultado.sucesso) setRetorno({ sucesso: false, texto: resultado.mensagem })
  }

  async function handleSalvar(novaPasta: string) {
    setOcupado(true)
    setRetorno(null)

    const resultado = await salvarPastaContratos(novaPasta)
    if (resultado.sucesso) {
      onAlterado()
      setRetorno({
        sucesso: true,
        texto: novaPasta.trim()
          ? 'Pasta alterada. Os próximos contratos serão salvos nela; os já gerados continuam onde estão.'
          : 'Voltou para a pasta padrão. Os contratos já gerados continuam onde estão.'
      })
    } else {
      setRetorno({ sucesso: false, texto: resultado.mensagem })
    }

    setOcupado(false)
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Armazenamento</h2>
      <p className="cartao__descricao">Onde ficam os arquivos do sistema neste computador.</p>

      {PASTAS.map((item) => (
        <div key={item.chave} className="pasta-linha">
          <div>
            <strong>{item.titulo}</strong>
            <div className="pasta-linha__caminho">{config[item.campo]}</div>
          </div>
          <button type="button" className="botao botao--secundario" onClick={() => handleAbrir(item.chave)}>
            Abrir pasta
          </button>
        </div>
      ))}

      <div className="linha-campo" style={{ marginTop: '1.25rem' }}>
        <div className="campo">
          <label htmlFor="pasta-contratos">Pasta dos contratos gerados</label>
          <input
            id="pasta-contratos"
            type="text"
            value={pasta}
            onChange={(e) => setPasta(e.target.value)}
            placeholder="Exemplo: D:\Contratos"
          />
        </div>
        <button
          type="button"
          className="botao botao--primario"
          onClick={() => handleSalvar(pasta)}
          disabled={ocupado || pasta.trim() === '' || pasta.trim() === config.pastaContratosGerados}
        >
          Salvar pasta
        </button>
      </div>

      {config.pastaPersonalizada && (
        <button type="button" className="botao botao--texto" onClick={() => handleSalvar('')} disabled={ocupado}>
          Voltar para a pasta padrão ({config.pastaPadraoContratos})
        </button>
      )}

      <MensagemRetorno retorno={retorno} />
    </section>
  )
}