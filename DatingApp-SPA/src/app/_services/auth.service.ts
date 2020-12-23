import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from '../_models/User';
import { UserToLogin } from '../_models/userToLogin';
import { BehaviorSubject } from 'rxjs';
import { DecodedToken } from '../_models/decodedToken';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = environment.apiUrl + 'auth/';

  // jwtHelper = new JwtHelperService(); // Injetei no construtor
  // decodedToken: any; // Melhorei com interface

  // Esses são atalhos para "coisas" que quero enxergar de outros pontos da aplicação
  decodedToken: any; // DecodedToken;
  currentUser: User;

  // Any to any communication fazedno uso do BehaviorSubject,que é um cara que pode tanto
  // receber alterações como gerar um Observável
  photoUrl = new BehaviorSubject<string>('../../assets/user.png'); // Aula 117. Using BehaviorSubject to add any to any communication
  currentPhotoUrl = this.photoUrl.asObservable(); // eis o observável



  constructor(private http: HttpClient, private jwtHelper: JwtHelperService) { }


  changeMemberPhoto(newPhotoUrl: string) { // Chamarei esse método para atualizar o BehaviorSubject que indica a foto
    this.photoUrl.next(newPhotoUrl); // Método next simplesmente atualiza o valor do BehaviorSubject
  }


  login(model: UserToLogin) {  // aula 40. Introduction to Angular Services
    return this.http.post(this.baseUrl + 'login', model)
      .pipe(
        map((response: any) => {
          const user = response;
          if (user) {
            // armazenagem da aplicação
            localStorage.setItem('token', user.token);
            localStorage.setItem('user', JSON.stringify(user.user));
            // armazenagem local
            this.decodedToken = this.jwtHelper.decodeToken(user.token);
            this.currentUser = user.user;
            // log
            this.changeMemberPhoto(this.currentUser.photoUrl);
            console.log(this.decodedToken);
          }
        })
      );
  }

  register(user: User) {
    return this.http.post(this.baseUrl + 'register', user);
  }

  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }

  roleMatch(allowedRoles): boolean {
    let isMatch = false;
    const userRoles = this.decodedToken.role;   // as Array<string>; // POR ALGUM MOTIVO, se eu trocar o nome para roles (ou qualquer outro) deixa de funcionar
    allowedRoles.forEach(element => {
      if (userRoles.includes(element)) {
        isMatch = true;
        return;
      }
    });
    return isMatch;
  } 

}
