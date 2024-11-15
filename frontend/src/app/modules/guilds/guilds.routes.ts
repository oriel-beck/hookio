import { Route } from "@angular/router";

const routes: Route[] = [
    { path: '', loadComponent: () => import('./guilds.component').then(c => c.GuildsComponent) },
    // { path: ':guildId', loadChildren: () => ... }
]

export default routes;