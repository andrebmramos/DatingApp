import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { Photo } from 'src/app/_models/photo';
import { AuthService } from 'src/app/_services/auth.service';
import { environment } from 'src/environments/environment';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  // @Output() getMemberPhotoChange = new EventEmitter<string>(); // enviará string com url da foto principal
                                              // para que a visualização seja atualizada no componente pai
                                              // posteirormente (aula 117, 7min), isso é substituído
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  currentMainPhoto: Photo;

  constructor(private authService: AuthService, private userService: UserService, private alertify: AlertifyService) { }

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }


  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024 // 10 MB
    });

    // existe um erro no exemplo: o arquivo precisa ser enviado sem credenciais, senão obtemos isso no console
    // ao fazer upload: The value of the 'Access-Control-Allow-Origin' header in the response must not be the
    // wildcard '*' when the request's credentials mode is 'include'. The credentials mode of requests initiated
    // by the XMLHttpRequest is controlled by the withCredentials attribute.
    this.uploader.onAfterAddingFile = (file) => { file.withCredentials = false; };

    // agora vamos configurar o evento para exibirmos a foto assim que ela é subida com sucesso
    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) { // garanto que há resposta
        const res: Photo = JSON.parse(response); // converte string retornada do servidor em objeto Photo
        const photo = { // monto um novo objeto de foto
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain
        };
        this.photos.push(photo);
        // this.photos.push(res);
      }
    };
  }


  setMainPhoto(photo: Photo) {
    this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id).subscribe(
      (success) => {
        this.currentMainPhoto = this.photos.filter(p => p.isMain === true)[0];
          // o filter retorna um array, portanto preciso identificar um único elemento [0]
        this.currentMainPhoto.isMain = false;
        photo.isMain = true;
        // this.getMemberPhotoChange.emit(photo.url);
        this.authService.changeMemberPhoto(photo.url);  // aula 117, substitui o evento acima, mas faltará
                                // fazer a persistência no localstorage. Sem isso, ao dar um refresh, será
                                // trazida a foto que estava antes. Vamos atualizar o currentUser em authservice
                                // e gerar uma nova representação para localstorage
        this.authService.currentUser.photoUrl = photo.url;
        localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
        console.log('Main photo updated successfully');
      },
      (error) => this.alertify.error(error)
    );
  }


  deletePhoto(photoId: number) {
    this.alertify.confirm('Você tem certeza que quer deletar a foto?',
      () => {
        this.userService.deletePhoto(this.authService.decodedToken.nameid, photoId).subscribe(
          next => {
            this.photos.splice(this.photos.findIndex(p => p.id === photoId), 1);
            this.alertify.success('Foto removida com sucesso');
          },
          error => {
            this.alertify.error('Falha ao excluir a foto');
          });
      });
  }

}
