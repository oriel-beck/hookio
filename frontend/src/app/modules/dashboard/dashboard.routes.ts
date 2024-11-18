import { Route } from "@angular/router";

const routes: Route[] = [
    { path: '', loadComponent: () => import('./dashboard.component').then(c => c.DashboardComponent) },
    { path: ':subscriptionId', loadComponent: () => import('../editor/editor.component').then(c => c.EditorComponent) }
]

export default routes;