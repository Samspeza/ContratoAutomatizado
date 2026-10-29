import type { VarianteSelo } from '../utils/rotuloStatus'

interface SeloProps {
  texto: string
  variante: VarianteSelo
}

export function Selo({ texto, variante }: SeloProps) {
  if (texto === '—') return <span>—</span>
  return <span className={`selo selo--${variante}`}>{texto}</span>
}