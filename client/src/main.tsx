import ReactDOM from 'react-dom/client';
import {
  createBrowserRouter,
  RouterProvider,
} from 'react-router-dom';
import App from './App.tsx';
import Guilds from './modules/guilds.tsx';
import getUser from './loaders/get-user.ts';
import Home from './modules/home.tsx';
import './index.scss';
import LoginGuard from './modules/guard.tsx';
import ProviderSelection from './modules/provider-selection.tsx';
import SubscriptionsManager from './modules/subscriptions-manager.tsx';
import getAllSubscriptions from './loaders/get-all-subscriptions.ts';

const router = createBrowserRouter([
  {
    path: "/",
    loader: getUser,
    id: "root",
    element: <App />,
    children: [
      {
        path: "",
        element: <Home />
      },
      {
        path: "servers",
        element: <LoginGuard><Guilds /></LoginGuard>
      },
      {
        path: "servers/:serverId",
        element: <LoginGuard><ProviderSelection /></LoginGuard>
      },
      {
        path: "servers/:serverId/:provider",
        loader: getAllSubscriptions,
        element: <LoginGuard><SubscriptionsManager /></LoginGuard>
      }
    ]
  }
])


ReactDOM.createRoot(document.getElementById('root')!).render(
  // Due to discord only allowing code exchange once this causes issues
  // <React.StrictMode>
  <RouterProvider router={router} />
  // </React.StrictMode>,
)
