import type { Empresa } from '../types/empresa'
import type { DadosContrato } from '../types/dadosContrato'
import { formatarDataBr, formatarEnderecoResumo } from '../utils/formatarRevisao'

interface RevisaoContratoProps {
  empresa: Empresa
  dados: DadosContrato
  gerando: boolean
  onConfirmar: () => void
  onVoltar: () => void
}

export function RevisaoContrato({ empresa, dados, gerando, onConfirmar, onVoltar }: RevisaoContratoProps) {
  return (
    <section className="cartao">
      <h2 className="cartao__titulo">Revisão do contrato</h2>
      <p className="cartao__descricao">Confira todos os dados antes de gerar o contrato definitivo.</p>

      <dl className="dados-lista">
        <dt>EMPRESA</dt>
        <dd>{empresa.razaoSocial}</dd>

        <dt>CNPJ</dt>
        <dd>{empresa.cnpj}</dd>

        <dt>ENDEREÇO</dt>
        <dd>
          {formatarEnderecoResumo(empresa.logradouro, empresa.numero, empresa.bairro, empresa.municipio, empresa.uf, empresa.cep)}
        </dd>

        <dt>HONORÁRIOS</dt>
        <dd>{dados.percentualHonorarios}%</dd>

        <dt>PARCELAS</dt>
        <dd>{dados.quantidadeParcelas}</dd>

        <dt>DATA</dt>
        <dd>{formatarDataBr(dados.dataContrato)}</dd>

        <dt>{dados.responsaveis.length > 1 ? 'RESPONSÁVEIS' : 'RESPONSÁVEL'}</dt>
        <dd>
          {dados.responsaveis.map((responsavel, indice) => (
            <div key={indice}>
              {responsavel.nome}
              {responsavel.cpf ? ` — CPF ${responsavel.cpf}` : ''}
            </div>
          ))}
        </dd>
      </dl>

      <div className="acoes">
        <button type="button" className="botao botao--secundario" onClick={onVoltar} disabled={gerando}>
          Voltar e corrigir
        </button>
        <button type="button" className="botao botao--primario" onClick={onConfirmar} disabled={gerando}>
          {gerando ? 'Gerando...' : 'Gerar contrato'}
        </button>
      </div>
    </section>
  )
}