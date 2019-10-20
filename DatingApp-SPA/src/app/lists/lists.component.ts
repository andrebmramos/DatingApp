import { Component, OnInit } from '@angular/core';
import { User } from '../_models/User';
import { Pagination, PaginatedResult } from '../_models/pagination';
import { AuthService } from '../_services/auth.service';
import { UserService } from '../_services/user.service';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
  users: User[];
  pagination: Pagination;
  likesParam: string;

  constructor(private auth: AuthService, private userService: UserService,
    private route: ActivatedRoute, private alertify: AlertifyService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data['usersFromRoute'].result;
      this.pagination = data['usersFromRoute'].pagination;
    });
    this.likesParam = 'Likers';
  }

  pageChanged(event: any): void { // aula 142
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }


  loadUsers() { // aula 142
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, null, this.likesParam).subscribe(
       (res: PaginatedResult<User[]>) => {
         this.users = res.result;
         this.pagination = res.pagination;
       },
       error => this.alertify.error(error),
       () => console.log('Usuários carregados: ' + this.users.length)
    );
  }

}
