import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/User';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  users: User[];

  constructor(private userService: UserService, private alertify: AlertifyService, private route: ActivatedRoute) { }

  ngOnInit() {
    // this.loadUsers(); // Modo ruim, migrando para rotas e resolvers:

    // Vamos carregar os dados diretamente da rota fazendouso do resolver
    // - o resolver para essa rota foi especificado no routes.ts
    // - o resolver propriamente foi criado na pasta resolvers
    // - agora posso retirar os ? de user em cada chamada do html sem causar erro no console ({{ user.city }}, {{ user.country }}, etc
    this.route.data.subscribe( data => {
      this.users = data['users']; // user é o nome da variável tipo MemberDetailResolver especificada como resolver dessa rota
    });
  }


  // loadUsers() {
  //   this.userService.getUsers().subscribe(
  //     (users: User[]) => this.users = users,
  //     error => this.alertify.error(error),
  //     () => console.log('Usuários carregados: ' + this.users.length)
  //   );
  // }



}
