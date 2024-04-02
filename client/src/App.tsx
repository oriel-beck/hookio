import { Await, useLoaderData } from 'react-router-dom'
import './App.scss'
import { Suspense } from 'react'
import { User } from './types/types';
import Loader from './components/loader';
import Layout from './modules/layout';
import { useMetaTags } from 'react-metatags-hook';

function App() {
  const data = useLoaderData() as { user: Promise<User> };
  useMetaTags({
    title: "Hookio",
    description: "A service to forward YouTube, Twitch, and other services to Discord Webhooks.",
    charset: "utf8",
    lang: "en",
  }, []);
  
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
