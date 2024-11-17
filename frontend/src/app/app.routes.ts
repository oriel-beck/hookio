import { Routes } from '@angular/router';
import { loggedInGuard } from './guards/logged-in/logged-in.guard';

export const routes: Routes = [
    { path: '', loadComponent: () => import('./modules/main/main.component').then(c => c.MainComponent) },
    { path: 'servers', loadChildren: () => import('./modules/guilds/guilds.routes'), canActivate: [loggedInGuard] },
    { path: '**', loadComponent: () => import('./modules/not-found/not-found.component').then(c => c.NotFoundComponent) }
];
