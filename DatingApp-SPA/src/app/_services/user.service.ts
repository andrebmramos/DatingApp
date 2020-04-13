import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { Message } from '../_models/message';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
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

  getUsers_OLD(): Observable<User[]> {
    // exemplo de como mandar o header com token manualmente. Agora estamos usando Jwt para fazer isso
    // (ver app.module.ts)
    // return this.http.get<User[]>(this.baseUrl + 'users', httpOptions);
    return this.http.get<User[]>(this.baseUrl + 'users');
  }

  getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

    let params = new HttpParams();

    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    if (userParams != null) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    if (likesParam === 'Likers') {
      params = params.append('likers', 'true');
    }

    if (likesParam === 'Likees') {
      params = params.append('likees', 'true');
    }


    return this.http.get<User[]>(this.baseUrl + 'users', { observe: 'response', params} )
      // notar a chamada com { observe: 'response' }. Por padrão, o método get trata apenas o body,
      // mas aqui quero tratar a resposta completa, pois a API manda os dados (usuários) no body
      // e a informação de paginação no header
      .pipe(
        map( response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') != null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }
          return paginatedResult;
        })
      );
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

  alreadyLikeUser(id: number, recipientId: number): Observable<boolean> {
    return this.http.get<boolean>(this.baseUrl + 'users/' + id + '/alreadylike/' + recipientId, {});
  }

  sendLike(id: number, recipientId: number) {
    return this.http.post(this.baseUrl + 'users/' + id + '/like/' + recipientId, {});
  }

  sendDisLike(id: number, recipientId: number) {
    return this.http.post(this.baseUrl + 'users/' + id + '/dislike/' + recipientId, {});
  }

  getMessages(id: number, page?, itemsPerPage?, messageContainer?) {
    const paginatedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();

    let params = new HttpParams();

    params = params.append('MessageContainer', messageContainer);

    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages',
      {
        observe: 'response',
        params
      })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') !== null) {
            paginatedResult.pagination = JSON.parse(
              response.headers.get('Pagination')
            );
          }

          return paginatedResult;
        })
      );
  }

  getMessageThread(id: number, recipientId: number) {
    return this.http.get<Message[]>(
      this.baseUrl + 'users/' + id + '/messages/thread/' + recipientId
    );
  }

  sendMessage(id: number, message: Message) {
    return this.http.post(this.baseUrl + 'users/' + id + '/messages', message);
  }

  deleteMessage(id: number, userId: number) {
    return this.http.post(
      this.baseUrl + 'users/' + userId + '/messages/' + id,
      {}
    );
  }

  markAsRead(userId: number, messageId: number) {
    this.http
      .post(
        this.baseUrl + 'users/' + userId + '/messages/' + messageId + '/read',
        {}
      )
      .subscribe();
  }

}
