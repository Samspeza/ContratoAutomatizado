import type { Retorno } from '../types/configuracoes'

interface MensagemRetornoProps {
  retorno: Retorno
}

export function MensagemRetorno({ retorno }: MensagemRetornoProps) {
  if (!retorno) return null

  return (
    <p
      className={retorno.sucesso ? 'mensagem mensagem--sucesso' : 'mensagem mensagem--erro'}
      role={retorno.sucesso ? 'status' : 'alert'}
    >
      {retorno.texto}
    </p>
  )
}