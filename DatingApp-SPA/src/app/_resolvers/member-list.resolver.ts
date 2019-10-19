import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';


import { User } from '../_models/User';
import { AlertifyService } from '../_services/alertify.service'
import { UserService } from '../_services/user.service';
import { catchError } from 'rxjs/operators';




@Injectable()
export class MemberListResolver implements Resolve<User> {
    pageNumber = 1;
    pageSize = 5;

    constructor(private userService: UserService, private route: Router, private alertify: AlertifyService) {}

    resolve(route: ActivatedRouteSnapshot,
            state: import('@angular/router').RouterStateSnapshot): Observable<User> {
        return this.userService.getUsers(this.pageNumber, this.pageSize).pipe(
            catchError( error => {
                this.alertify.error('Problema na obtenção dos dados');
                this.route.navigate(['/home']);
                return of(null);
            })
        );
    }

}
