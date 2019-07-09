import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};  // <==

  constructor(private authService: AuthService) { }

  ngOnInit() {
  }

  login() {
    console.log(this.model);
    this.authService.login(this.model).subscribe( next => {
      console.log('Logged in successfully');
    }, error => {
      console.log('Failed to login');
    });
  }


  loggedIn() {
    const token = localStorage.getItem('token');
    return !!token; // se há algo no token, operador !! retorna true, ou seja, há token ativo, usuário está logado
  }


  logout() {
    localStorage.removeItem('token');
    console.log('logged out');
  }

}
