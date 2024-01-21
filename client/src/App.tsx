import { Outlet, redirect, useLoaderData } from 'react-router-dom'
import './App.scss'
import Header from './components/header'

function App() {
  const user = useLoaderData();
  console.log(user);
  if (!user) redirect("/login");
  return (
    <>
      <Header />
      <main>
        <Outlet/>
      </main>
    </>
  )
}

export default App
