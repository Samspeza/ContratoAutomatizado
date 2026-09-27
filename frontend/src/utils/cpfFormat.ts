export function apenasDigitosCpf(valor: string): string {
  return valor.replace(/\D/g, '')
}

export function formatarCpf(valorDigitado: string): string {
  const digitos = apenasDigitosCpf(valorDigitado).slice(0, 11)
  let resultado = digitos

  if (digitos.length > 3) resultado = `${digitos.slice(0, 3)}.${digitos.slice(3)}`
  if (digitos.length > 6) resultado = `${digitos.slice(0, 3)}.${digitos.slice(3, 6)}.${digitos.slice(6)}`
  if (digitos.length > 9) resultado = `${digitos.slice(0, 3)}.${digitos.slice(3, 6)}.${digitos.slice(6, 9)}-${digitos.slice(9)}`

  return resultado
}

export function cpfValido(valor: string): boolean {
  const digitos = apenasDigitosCpf(valor)

  if (digitos.length !== 11) return false
  if (new Set(digitos).size === 1) return false

  const calcularDigito = (base: string, fatorInicial: number): number => {
    let soma = 0
    for (let i = 0; i < base.length; i++) {
      soma += Number(base[i]) * (fatorInicial - i)
    }
    const resto = (soma * 10) % 11
    return resto === 10 ? 0 : resto
  }

  const primeiroDigito = calcularDigito(digitos.slice(0, 9), 10)
  const segundoDigito = calcularDigito(digitos.slice(0, 9) + primeiroDigito, 11)

  return Number(digitos[9]) === primeiroDigito && Number(digitos[10]) === segundoDigito
}