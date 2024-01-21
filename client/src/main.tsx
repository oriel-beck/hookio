import React from 'react';
import ReactDOM from 'react-dom/client';
import {
  createBrowserRouter,
  RouterProvider,
} from 'react-router-dom';
import App from './App.tsx';
import Servers from './modules/servers.tsx';
import Dashboard from './modules/dashboard.tsx';
import './index.scss';
import getUser from './loaders/get-user.ts';
import getServers from './loaders/get-servers.ts';
import Login from './modules/login.tsx';

const router = createBrowserRouter([
  {
    path: "/",
    loader: getUser,
    element: <App />,
    children: [
      {
        path: "login",
        loader: getUser,
        element: <Login />
      },
      {
        path: "servers",
        loader: getServers,
        element: <Servers />
      },
      {
        path: "servers/:serverId",
        element: <Dashboard />
      }
    ]
  }
])


ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>,
)
