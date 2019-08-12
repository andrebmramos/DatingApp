import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};  // <==

  constructor(public authService: AuthService, private alertify: AlertifyService) { }

  ngOnInit() {
  }

  login() {
    console.log(this.model);
    this.authService.login(this.model).subscribe( next => {
      // console.log('Logged in successfully');
      this.alertify.success('Logged in successfully');
    }, error => {
      // console.log('Failed to login');
      // console.log(error);
      this.alertify.error(error);
    });
  }


  loggedIn() {
    // Primeira forma que fizermos, apenas verifica se tem algo em localstorage (lá estávamos guardando o token)
    // sem fazer validação (nem confirmar se é um token de fato)
    // Não era problema de segurança, mas certamente era tosco. Vamos passar a administra o Token por meio de
    // ferramena específica: auth0/angular2-jwt -- npm install @auth0/angular-jwt
    // const token = localStorage.getItem('token');
    // return !!token; // se há algo no token, operador !! retorna true, ou seja, há token ativo, usuário está logado

    // colocamos o JwtHelperService no nosso AuthService. Lá o token será verificado. Chamamos aquela função daqui de dentro
    // pra não ter que mexer no código
    return this.authService.loggedIn();

  }


  logout() {
    localStorage.removeItem('token');
    // console.log('logged out');
    this.alertify.message('logged out');
  }

}
