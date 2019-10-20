import { Component, OnInit, Input } from '@angular/core';
import { User } from '../../_models/User';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  @Input() user: User;

  constructor(private userService: UserService, private auth: AuthService, private alert: AlertifyService) { }

  ngOnInit() {
  }


  sendLike(id: number) {
    this.userService.sendLike(this.auth.decodedToken.nameid, id).subscribe( ok => {
      this.alert.success('Curtiu ' + this.user.knownAs);
    }, error => {
      this.alert.error(error);
      console.log(error);
    });
  }

}
