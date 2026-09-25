export function apenasDigitos(valor: string): string {
  return valor.replace(/\D/g, '')
}

export function formatarCnpj(valorDigitado: string): string {
  const digitos = apenasDigitos(valorDigitado).slice(0, 14)
  let resultado = digitos

  if (digitos.length > 2) resultado = `${digitos.slice(0, 2)}.${digitos.slice(2)}`
  if (digitos.length > 5) resultado = `${digitos.slice(0, 2)}.${digitos.slice(2, 5)}.${digitos.slice(5)}`
  if (digitos.length > 8) resultado = `${digitos.slice(0, 2)}.${digitos.slice(2, 5)}.${digitos.slice(5, 8)}/${digitos.slice(8)}`
  if (digitos.length > 12) resultado = `${digitos.slice(0, 2)}.${digitos.slice(2, 5)}.${digitos.slice(5, 8)}/${digitos.slice(8, 12)}-${digitos.slice(12)}`

  return resultado
}