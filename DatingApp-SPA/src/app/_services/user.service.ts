import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/User';
// import { environment } from 'src/environments/environment.prod';


// Implementação temporária de construir um header que carregará o token
// e será mandado junto com as requisições get na parte de options
// const httpOptions = {
//  headers: new HttpHeaders({
//    'Authorization': 'Bearer ' + localStorage.getItem('token')
//  })
// };


@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  getUsers(): Observable<User[]> {
    // exemplo de como mandar o header com token manualmente. Agora estamos usando Jwt para fazer isso
    // (ver app.module.ts)
    // return this.http.get<User[]>(this.baseUrl + 'users', httpOptions);
    return this.http.get<User[]>(this.baseUrl + 'users');
  }

  getUser(id: number): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'users/' + id, user);
  }

  setMainPhoto(userId: number, id: number) {
    return this.http.post(this.baseUrl + 'users/' + userId + '/photos/' + id + '/setMain', {});
        // objeto vazio só para satistazer método post
  }

  deletePhoto(userId: number, id: number) { // Aula 118, esse método será usado no photo-editor.component.ts
    return this.http.delete(this.baseUrl + 'users/' + userId + '/photos/' + id);
  }

}
