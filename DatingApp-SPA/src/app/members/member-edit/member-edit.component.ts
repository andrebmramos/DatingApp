import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';


import { User } from 'src/app/_models/User';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';


@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {

  user: User;

  // Atenção, no ANGULAR V8 exige assim:  @ViewChild('editForm', { static: true }) editFormAngular8: NgForm;
  @ViewChild('editForm') editForm: NgForm; // permite "enxergar" o formulário aqui no código ts

  // Aqui interceptaremos o fechamento da janela
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    if (this.editForm.dirty) {   // quando form estiver dirty...
      $event.returnValue = true; // ...browser notificará antes de permitir fechamento
    }
  }



  constructor(private route: ActivatedRoute, private alertify: AlertifyService,
              private userService: UserService, private authService: AuthService) { }

  ngOnInit() {
    this.route.data.subscribe( data => {
      this.user = data['user'];
    });
  }

  updateUser() {
    const id = this.authService.decodedToken.nameid;
    this.userService.updateUser(id, this.user).subscribe( next => {
      this.alertify.success('Perfil atualizado com sucesso');
      this.editForm.reset(this.user); // Para remover flag dirty mantendo dados atuais do form conforme user
    }, error => {
      this.alertify.error(error);
    });
  }

}
