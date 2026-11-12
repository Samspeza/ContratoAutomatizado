import { useEffect, useState } from 'react'
import type { Configuracoes, ItemVerificacao } from '../types/configuracoes'
import { obterConfiguracoes, obterVerificacao } from '../services/configuracoesService'
import { VerificacaoSistema } from './VerificacaoSistema'
import { ConfigEmail } from './ConfigEmail'
import { ConfigMensagemEmail } from './ConfigMensagemEmail'
import { ConfigArmazenamento } from './ConfigArmazenamento'
import { ConfigAvancada } from './ConfigAvancada'

interface AreaConfiguracoesProps {
  ativa: boolean
}

export function AreaConfiguracoes({ ativa }: AreaConfiguracoesProps) {
  const [configuracoes, setConfiguracoes] = useState<Configuracoes | null>(null)
  const [verificacao, setVerificacao] = useState<ItemVerificacao[]>([])
  const [erro, setErro] = useState('')
  const [chaveAtualizacao, setChaveAtualizacao] = useState(0)

  // Recarrega quando a aba é aberta e depois de cada alteração.
  useEffect(() => {
    if (!ativa) return

    let cancelado = false

    Promise.all([obterConfiguracoes(), obterVerificacao()]).then(([resultadoConfiguracoes, resultadoVerificacao]) => {
      if (cancelado) return

      if (resultadoConfiguracoes.sucesso) {
        setConfiguracoes(resultadoConfiguracoes.dados)
        setErro('')
      } else {
        setErro(resultadoConfiguracoes.mensagem)
      }

      if (resultadoVerificacao.sucesso) setVerificacao(resultadoVerificacao.dados)
    })

    return () => {
      cancelado = true
    }
  }, [ativa, chaveAtualizacao])

  const recarregar = () => setChaveAtualizacao((chave) => chave + 1)

  return (
    <>
      {erro && <p className="mensagem mensagem--erro" role="alert">{erro}</p>}

      {configuracoes && (
        <>
          <VerificacaoSistema itens={verificacao} onVerificar={recarregar} />
          <ConfigEmail config={configuracoes.email} onAlterado={recarregar} />
          <ConfigMensagemEmail ativa={ativa} />
          <ConfigArmazenamento config={configuracoes.armazenamento} onAlterado={recarregar} />
          <ConfigAvancada configuracoes={configuracoes} onAlterado={recarregar} />
        </>
      )}
    </>
  )
}