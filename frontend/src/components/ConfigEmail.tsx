import { useState } from 'react'
import type { ConfiguracaoEmail, Retorno } from '../types/configuracoes'
import { definirSenhaEmail, removerSenhaEmail, testarConexaoEmail } from '../services/configuracoesService'
import { MensagemRetorno } from './MensagemRetorno'

const TEXTO_ORIGEM: Record<ConfiguracaoEmail['origemSenha'], string> = {
  ArmazenamentoSeguro: 'Guardada com segurança neste computador.',
  Configuracao: 'Definida na configuração do sistema (uso técnico).',
  Nenhuma: 'Ainda não informada.'
}

interface ConfigEmailProps {
  config: ConfiguracaoEmail
  onAlterado: () => void
}

export function ConfigEmail({ config, onAlterado }: ConfigEmailProps) {
  const [senha, setSenha] = useState('')
  const [ocupado, setOcupado] = useState(false)
  const [retorno, setRetorno] = useState<Retorno>(null)

  async function executar(acao: () => Promise<Retorno>) {
    setOcupado(true)
    setRetorno(null)
    setRetorno(await acao())
    setOcupado(false)
  }

  function salvarSenha() {
    return executar(async () => {
      const resultado = await definirSenhaEmail(senha)
      if (!resultado.sucesso) return { sucesso: false, texto: resultado.mensagem }

      setSenha('')
      onAlterado()
      return { sucesso: true, texto: 'Senha guardada com segurança neste computador.' }
    })
  }

  function removerSenha() {
    return executar(async () => {
      const resultado = await removerSenhaEmail()
      if (!resultado.sucesso) return { sucesso: false, texto: resultado.mensagem }

      onAlterado()
      return { sucesso: true, texto: 'Senha removida deste computador.' }
    })
  }

  function testarConexao() {
    return executar(async () => {
      const resultado = await testarConexaoEmail()
      return resultado.sucesso
        ? { sucesso: true, texto: resultado.dados.mensagem }
        : { sucesso: false, texto: resultado.mensagem }
    })
  }

  return (
    <section className="cartao">
      <h2 className="cartao__titulo">E-mail</h2>
      <p className="cartao__descricao">
        Os contratos são enviados por esta caixa. Para trocar servidor, usuário ou remetente, é preciso alterar o
        arquivo de configuração do sistema.
      </p>

      <dl className="dados-lista">
        <dt>Remetente</dt>
        <dd>{config.remetente}</dd>

        <dt>Servidor</dt>
        <dd>{config.host}:{config.porta}</dd>

        <dt>Usuário</dt>
        <dd>{config.usuario}</dd>

        <dt>Senha</dt>
        <dd>{TEXTO_ORIGEM[config.origemSenha]}</dd>
      </dl>

      <div className="linha-campo" style={{ marginTop: '1.25rem' }}>
        <div className="campo">
          <label htmlFor="senha-email">{config.senhaConfigurada ? 'Nova senha' : 'Senha da caixa de e-mail'}</label>
          <input
            id="senha-email"
            type="password"
            autoComplete="new-password"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            placeholder="A mesma usada hoje no Outlook"
          />
        </div>
        <button type="button" className="botao botao--primario" onClick={salvarSenha} disabled={!senha || ocupado}>
          Salvar senha
        </button>
      </div>

      <MensagemRetorno retorno={retorno} />

      <div className="acoes">
        <button type="button" className="botao botao--secundario" onClick={testarConexao} disabled={ocupado}>
          {ocupado ? 'Aguarde...' : 'Testar conexão'}
        </button>
        {config.origemSenha === 'ArmazenamentoSeguro' && (
          <button type="button" className="botao botao--texto" onClick={removerSenha} disabled={ocupado}>
            Remover senha guardada
          </button>
        )}
      </div>
    </section>
  )
}