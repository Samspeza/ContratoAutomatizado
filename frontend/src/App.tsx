import { useEffect, useState } from 'react'

function App() {
  const [status, setStatus] = useState<'checking' | 'ok' | 'error'>('checking')

  useEffect(() => {
    fetch('/api/health')
      .then((res) => {
        if (!res.ok) throw new Error('Resposta inválida')
        return res.json()
      })
      .then(() => setStatus('ok'))
      .catch(() => setStatus('error'))
  }, [])

  return (
    <div style={{ fontFamily: 'sans-serif', padding: '2rem' }}>
      <h1>Contrato Automatizado</h1>
      {status === 'checking' && <p>Verificando conexão com o backend...</p>}
      {status === 'ok' && <p style={{ color: 'green' }}>Backend conectado com sucesso.</p>}
      {status === 'error' && <p style={{ color: 'red' }}>Não foi possível conectar ao backend.</p>}
    </div>
  )
}

export default App