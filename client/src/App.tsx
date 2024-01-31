import { Await, useLoaderData } from 'react-router-dom'
import './App.scss'
import { Suspense } from 'react'
import { User } from './types/types';
import Loader from './components/loader';
import Layout from './modules/layout';

function App() {
  const data = useLoaderData() as { user: Promise<User> };
  return (
    <Suspense
      fallback={<Loader />}
    >
      <Await
        resolve={data.user}
      >
        {(user) => <Layout user={user} />}
      </Await>
    </Suspense>
  )
}

export default App
