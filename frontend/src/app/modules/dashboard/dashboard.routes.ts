import { Route } from "@angular/router";

const routes: Route[] = [
    { path: '', loadComponent: () => import('./dashboard.component').then(c => c.DashboardComponent) }
]

export default routes;