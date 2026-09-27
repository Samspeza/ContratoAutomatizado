export interface ResponsavelAssinatura {
  nome: string
  cpf: string
}

export interface DadosContrato {
  percentualHonorarios: number
  quantidadeParcelas: number
  dataContrato: string // formato yyyy-MM-dd, vindo do <input type="date">
  responsaveis: ResponsavelAssinatura[]
}