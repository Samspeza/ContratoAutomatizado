export const CONFIG_CONTRATO = {
  responsaveis: {
    minimo: 1,
    maximo: 2
  },
  honorarios: {
    minimoPercentual: 0.01,
    maximoPercentual: 100,
    valorPadrao: 25
  },
  parcelas: {
    minimo: 1,
    maximo: 60,
    valorPadrao: 3
  }
} as const