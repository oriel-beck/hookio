import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', loadComponent: () => import('./modules/main/main.component').then(c => c.MainComponent) },
    { path: 'guilds', loadChildren: () => import('./modules/guilds/guilds.routes') },
    { path: '**', loadComponent: () => import('./modules/not-found/not-found.component').then(c => c.NotFoundComponent) }
];
