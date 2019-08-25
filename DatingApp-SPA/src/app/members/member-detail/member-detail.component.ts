import { Component, OnInit } from '@angular/core';
import { User } from 'src/app/_models/User';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from 'ngx-gallery';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {

  user: User;

  constructor(private userService: UserService, private alertify: AlertifyService, private route: ActivatedRoute) { }

  // Sobre uso da galeria, ver usage em https://www.npmjs.com/package/ngx-gallery
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];


  ngOnInit() {
    // Carregamento dos dados por rota + resolver
    this.route.data.subscribe( data => {
      this.user = data['user']; // user é o nome da variável tipo MemberDetailResolver especificada como resolver dessa rota
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


}
