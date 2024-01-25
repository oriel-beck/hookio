import { Outlet } from 'react-router-dom'
import './App.scss'
import Header from './components/header'

function App() {
  return (
    <>
      <Header />
      <main className='bg-gray-600 p-10 h-full'>
        <Outlet/>
      </main>
    </>
  )
}

export default App
