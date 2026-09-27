export function formatarDataBr(dataIso: string): string {
  const [ano, mes, dia] = dataIso.split('-')
  return `${dia}/${mes}/${ano}`
}

export function formatarEnderecoResumo(logradouro: string, numero: string | null, bairro: string, municipio: string, uf: string, cep: string): string {
  const linha1 = numero ? `${logradouro}, ${numero}, ${bairro},` : `${logradouro}, ${bairro},`
  return `${linha1} ${municipio}/${uf}, CEP ${cep}`
}