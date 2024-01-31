import { Outlet, Await, useLoaderData } from 'react-router-dom'
import './App.scss'
import Header from './components/header'
import { Suspense } from 'react'
import { User } from './types/types';

function App() {
  const data = useLoaderData() as { user: Promise<User> };
  return (
    <Suspense
      fallback={
        <>
          <Header showLogin={false} user={null} />
          <main>
            <div className='flex items-center justify-center h-screen bg-gray-600'>
              <div
                className="inline-block h-52 w-52 animate-spin rounded-full border-4 border-solid border-current border-r-transparent align-[-0.125em] motion-reduce:animate-[spin_1.5s_linear_infinite]"
                role="status">
                <span
                  className="!absolute !-m-px !h-px !w-px !overflow-hidden !whitespace-nowrap !border-0 !p-0 ![clip:rect(0,0,0,0)]"
                >Loading...</span
                >
              </div>
            </div>
          </main>
        </>
      }
    >
      <Await
        resolve={data.user}
      >
        {(user) => (
          <>
            <Header user={user} />
            <main className='bg-gray-600 p-10 h-full'>
              <Outlet context={user} />
            </main>
          </>
        )}
      </Await>
    </Suspense>
  )
}

export default App
