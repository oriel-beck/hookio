import ReactDOM from 'react-dom/client';
import {
  createBrowserRouter,
  RouterProvider,
} from 'react-router-dom';
import App from './App.tsx';
import Guilds from './modules/guilds.tsx';
import Dashboard from './modules/dashboard.tsx';
import getUser from './loaders/get-user.ts';
import Home from './modules/home.tsx';
import './index.scss';
import LoginGuard from './modules/guard.tsx';

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
        element: <LoginGuard><Dashboard /></LoginGuard>
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
