import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './member-list/member-list.component';
import { MessagesComponent } from './messages/messages.component';
import { ListsComponent } from './lists/lists.component';
import { AuthGuard } from './_guards/auth.guard';


export const appRoute: Routes = [
    { path: '', component: HomeComponent },
    {
        // Jeito de fazer um path com subpaths de modo aplicr o guarda uma única vez e fazer valer para todos
        path: '', // (*)
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        children: [
            { path: 'members', component: MemberListComponent, canActivate: [AuthGuard] }, // o path de fato é '' + 'members', (*)
            { path: 'messages', component: MessagesComponent },
            { path: 'lists', component: ListsComponent },
        ]
    },
    /*
    // Jeito de fazer aplicando guarda individualmente, claro que é mais fácil nesse caso simples
    { path: 'members', component: MemberListComponent, canActivate: [AuthGuard] },
    { path: 'messages', component: MessagesComponent, canActivate: [AuthGuard]  },
    { path: 'lists', component: ListsComponent, canActivate: [AuthGuard]  },
    */
    { path: '**', redirectTo: '', pathMatch: 'full' },
];
