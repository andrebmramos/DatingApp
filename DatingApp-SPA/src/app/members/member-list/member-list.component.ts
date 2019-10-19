import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/User';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';
import { e } from '@angular/core/src/render3';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {

  // Usuários e informações de paginação
  users: User[];
  pagination: Pagination;

  // Parâmetros de filtragem (carga inicial dos componentes)
  user: User = JSON.parse(localStorage.getItem('user'));
  genderList = [{value: 'male', display: 'Homens'}, {value: 'female', display: 'Mulheres'}];
  userParams: any = {}; // construído no ngOnInit 


  constructor(private userService: UserService, private alertify: AlertifyService, private route: ActivatedRoute) { }

  ngOnInit() {
    // Vamos carregar os dados diretamente da rota fazendouso do resolver
    // - o resolver para essa rota foi especificado no routes.ts
    // - o resolver propriamente foi criado na pasta resolvers
    // - agora posso retirar os ? de user em cada chamada do html sem causar erro no console ({{ user.city }}, {{ user.country }}, etc
    this.route.data.subscribe( data => {
      this.users = data['users'].result; // user é o nome da variável tipo MemberDetailResolver especificada como resolver dessa rota
                                         // .result acrescentado na aula 141, ao criar paginação fazendo uso da interface PaginatedResult
      this.pagination = data['users'].pagination; // aula 142
    });

    // Parâmetgros iniciais do filtro
    this.userParams.gender = this.user.gender === 'male' ? 'female' : 'male';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';

  }

  resetFilters() {
    this.userParams.gender = this.user.gender === 'male' ? 'female' : 'male';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';
    this.loadUsers();
  }


  pageChanged(event: any): void { // aula 142
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }


  loadUsers() { // aula 142
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams).subscribe(
       (res: PaginatedResult<User[]>) => {
         this.users = res.result;
         this.pagination = res.pagination;
       },
       error => this.alertify.error(error),
       () => console.log('Usuários carregados: ' + this.users.length)
    );
  }



}
