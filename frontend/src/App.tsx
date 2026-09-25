import { useState } from 'react'
import type { Empresa } from './types/empresa'
import { ConsultaCnpj } from './components/ConsultaCnpj'
import './App.css'

function App() {
  const [, setEmpresaSelecionada] = useState<Empresa | null>(null)

  return (
    <div style={{ fontFamily: 'sans-serif', padding: '2rem', maxWidth: '600px', margin: '0 auto' }}>
      <h1>Contrato Automatizado</h1>
      <ConsultaCnpj onEmpresaEncontrada={setEmpresaSelecionada} />
    </div>
  )
}

export default App