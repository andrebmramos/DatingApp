import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MessagesComponent } from './messages/messages.component';
import { ListsComponent } from './lists/lists.component';
import { AuthGuard } from './_guards/auth.guard';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberDetailResolver } from './_resolvers/member-detail.resolver';
import { MemberListResolver } from './_resolvers/member-list.resolver';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MemberEditResolver } from './_resolvers/member-edit.resolver';
import { PreventUnsavedChanges } from './_guards/prevent-unsaved-changes.guard';
import { ListsResolver } from './_resolvers/lists.resolver';
import { MessagesResolver } from './_resolvers/messages.resolver';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';

 // Roteamento, aula 61 cria este arquivo, depois vai incrementando
export const appRoute: Routes = [
    { path: '', component: HomeComponent },
    {
        // Jeito de fazer um path com subpaths de modo aplicar o guarda uma única vez e fazer valer para todos
        path: '', // (*)
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        children: [
            { path: 'members', component: MemberListComponent, resolve: { users: MemberListResolver }}, // o path '' + 'members', (*)

            { path: 'members/:id', component: MemberDetailComponent,
                                   resolve: { user: MemberDetailResolver }},

            { path: 'member/edit', component: MemberEditComponent,
                                   resolve: { user: MemberEditResolver },
                                   canDeactivate: [ PreventUnsavedChanges ]},

            { path: 'messages', component: MessagesComponent,
                                resolve: {messages: MessagesResolver}},

            { path: 'lists', component: ListsComponent, resolve: { usersFromRoute: ListsResolver } },

            { path: 'admin', component: AdminPanelComponent, data: { roles: [ 'Admin', 'Moderator'] } },
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
