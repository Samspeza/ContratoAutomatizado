import type { Empresa } from '../types/empresa'

interface DadosEmpresaProps {
  empresa: Empresa
}

export function DadosEmpresa({ empresa }: DadosEmpresaProps) {
  const enderecoIncompleto = !empresa.logradouro || !empresa.cep

  return (
    <div>
      <h3>Dados da empresa</h3>

      {enderecoIncompleto && (
        <p role="alert" style={{ color: '#8a6d00' }}>
          Os dados da empresa foram encontrados, mas o endereço não foi retornado pela fonte consultada.
        </p>
      )}

      <dl>
        <dt>CNPJ</dt>
        <dd>{empresa.cnpj}</dd>

        <dt>Razão Social</dt>
        <dd>{empresa.razaoSocial}</dd>

        <dt>Endereço</dt>
        <dd>
          {empresa.logradouro}
          {empresa.numero ? `, ${empresa.numero}` : ''}
          {empresa.complemento ? `, ${empresa.complemento}` : ''}
        </dd>

        <dt>Bairro</dt>
        <dd>{empresa.bairro}</dd>

        <dt>Cidade</dt>
        <dd>{empresa.municipio}</dd>

        <dt>UF</dt>
        <dd>{empresa.uf}</dd>

        <dt>CEP</dt>
        <dd>{empresa.cep}</dd>
      </dl>
    </div>
  )
}