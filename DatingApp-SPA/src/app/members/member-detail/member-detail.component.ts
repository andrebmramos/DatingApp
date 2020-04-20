import { Component, OnInit, ViewChild } from '@angular/core';
import { User } from 'src/app/_models/User';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from 'ngx-gallery';
import { TabsetComponent } from 'ngx-bootstrap';
import { AuthService } from 'src/app/_services/auth.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs') memberTabs: TabsetComponent;
  // Angular V8 : @ViewChild('memberTabs', {static: true}) memberTabs: TabsetComponent;
  user: User;

  constructor(private userService: UserService, private alertify: AlertifyService, 
    private route: ActivatedRoute, private auth: AuthService) { }

  // Sobre uso da galeria, ver usage em https://www.npmjs.com/package/ngx-gallery
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];


  ngOnInit() {
    // Carregamento dos dados por rota + resolver
    this.route.data.subscribe( data => {
      this.user = data['user']; // user é o nome da variável tipo MemberDetailResolver especificada como resolver dessa rota
    });
    // Aula 168, inserimos uma tab na query e agora queremos abri-la diretamente
    this.route.queryParams.subscribe(params => {
      const selectedTab = params['tab'];
      this.memberTabs.tabs[selectedTab > 0 ? selectedTab : 0].active = true;
    });


    // Configuração da galeria de imagens
    this.galleryOptions = [
      {
          width: '500px',
          height: '500px',
          imagePercent: 100,
          thumbnailsColumns: 4, // imagens sob a imagem principal
          imageAnimation: NgxGalleryAnimation.Slide,
          preview: false // não deixa imagem ir fullscreen se o usuário clicar nela
      }
    ];

    this.galleryImages = this.getImages();

  }

  sendLike(id: number) {
    this.userService.sendLike(this.auth.decodedToken.nameid, id).subscribe( ok => {
      this.alertify.success('Curtiu ' + this.user.knownAs);
    }, error => {
      this.alertify.error(error);
      console.log(error);
    });
  }

  getImages(): any {
    const imgs = [];
    for (let i = 0; i < this.user.photos.length; i++) {
      const img = {
        small: this.user.photos[i].url,
        medium: this.user.photos[i].url,
        big: this.user.photos[i].url,
        description: this.user.photos[i].description
      };
      imgs.push(img);
    }
    return imgs;
  }

  // Aula 168
  selectTab(tabId: number) {
    this.memberTabs.tabs[tabId].active = true;
  }


}
