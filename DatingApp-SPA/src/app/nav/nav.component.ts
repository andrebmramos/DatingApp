import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';
import { UserToLogin } from '../_models/userToLogin';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  // model: any = {};      // Funciona, mas acho tosco
  // model: UserToLogin;   // Cria problema com undefineds no html, então vamos inicializar
  model: UserToLogin = {
    username: '',
    password: '',
  };

  photoUrl: string;


  constructor(public authService: AuthService,
              private alertify: AlertifyService,
              private router: Router) { }

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(next => this.photoUrl = next);
  }

  login() { // aula 41. Injecting the Angular services in our Components
    // console.log(this.model); // Cuidado! Vai mostrar senha no console!
    this.authService.login(this.model).subscribe( next => {
      this.alertify.success('Logged in successfully');
    }, error => {
      this.alertify.error(error);
    }, () => {
      this.router.navigate(['/members']);
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
    // remove da armazenagem do app
    localStorage.removeItem('token');
    localStorage.removeItem('user');

    // remove de AuthService
    this.authService.decodedToken = null;
    this.authService.currentUser = null;

    // notifica
    this.alertify.message('logged out');

    // navega para página inicial
    this.router.navigate(['/home']);
  }

}
