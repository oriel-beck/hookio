import { Await, useLoaderData } from 'react-router-dom'
import './App.scss'
import { lazy, Suspense } from 'react'
import { User } from './types/types';
import Loader from './components/loader';
import MetaTags from './components/meta-tags';
const LazyLayout = lazy(() => import("./modules/layout"));

function App() {
  const data = useLoaderData() as { user: Promise<User> };

  return (
    <>
      <MetaTags />
      <Suspense
        fallback={<Loader />}
      >
        <Await
          resolve={data.user}
        >
          {(user) => <LazyLayout user={user} />}
        </Await>
      </Suspense>
    </>
  )
}

export default App
