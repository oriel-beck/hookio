import { Route } from "@angular/router";
import dashboardRoutes from '../dashboard/dashboard.routes';

const routes: Route[] = [
    { path: '', loadComponent: () => import('./guilds.component').then(c => c.GuildsComponent) },
    { path: ':guildId', loadChildren: () => dashboardRoutes }
]

export default routes;