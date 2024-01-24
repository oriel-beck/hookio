import ReactDOM from 'react-dom/client';
import {
  createBrowserRouter,
  Outlet,
  RouterProvider,
} from 'react-router-dom';
import App from './App.tsx';
import Servers from './modules/servers.tsx';
import Dashboard from './modules/dashboard.tsx';
import getUser from './loaders/get-user.ts';
import Login from './modules/login.tsx';
import './index.scss';

const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        path: "login",
        loader: getUser,
        element: <Login />
      },
      {
        path: "",
        loader: getUser,
        element: <Outlet />,
        children: [
          {
            path: "servers",
            element: <Servers />
          },
          {
            path: "servers/:serverId",
            element: <Dashboard />
          }
        ]
      },
    ]
  }
])


ReactDOM.createRoot(document.getElementById('root')!).render(
  // Due to discord only allowing code exchange once this causes issues
  // <React.StrictMode>
  <RouterProvider router={router} />
  // </React.StrictMode>,
)
