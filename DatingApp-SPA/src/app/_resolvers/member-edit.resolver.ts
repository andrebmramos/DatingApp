import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';


import { User } from '../_models/User';
import { AlertifyService } from '../_services/alertify.service'
import { UserService } from '../_services/user.service';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';




@Injectable()
export class MemberEditResolver implements Resolve<User> {

    constructor(private userService: UserService,
                private route: Router,
                private alertify: AlertifyService,
                private authService: AuthService) {}

    resolve(route: ActivatedRouteSnapshot, state: import('@angular/router').RouterStateSnapshot): Observable<User> {
        // no MemberDetailResolver usávamos o route.params['id']. Aqui, vamos extrair id do token
        const id = this.authService.decodedToken.nameid;
        // console.log('Buscando usuário: ' + id);
        return this.userService.getUser(id).pipe(
            catchError( error => {
                this.alertify.error('Problema na obtenção dos dados do usuário');
                this.route.navigate(['/members']);
                return of(null);
            })
        );
    }

}
