import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import jwt_decode from 'jwt-decode';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http: HttpClient) { }

  async login (user: any){
    const result = await this.http.post<any>(`${environment.api}/api/authentication/login`, user)
      .toPromise()
      .then(
        success => {
          window.localStorage.setItem('token', success.token);
          window.localStorage.setItem('userPublicId', success.idPublic);
          window.localStorage.setItem('userPrivateId', success.idPrivate);
          window.localStorage.setItem('userName', success.name);
          return true;
},
        error => {
          console.log(error.error.message);
          return false;
        }
    );
    return result;
  }

  async register(account: any) {
    const result = await this.http.post<any>(`${environment.api}/api/authentication/register`, account)
      .toPromise()
      .then(
        success => {
          console.log(success.message);
          return true;
        },
        error => {
          console.log(error.error.message);
          return false;
        }
      );
    return result;
  }

  clearLoginLocalStorage() {
    window.localStorage.removeItem('token');
    window.localStorage.removeItem('userPublicId');
    window.localStorage.removeItem('userPrivateId');
    window.localStorage.removeItem('userName');
  }

  getAuthorizationToken() {
    return window.localStorage.getItem('token');
  }

  getUserIds(): any {
    if (window.localStorage.hasOwnProperty('token'))
      return {
        userPublicId:  window.localStorage.getItem('userPublicId'),
        userPrivateId: window.localStorage.getItem('userPrivateId'),
      }
    else
      return {
        userPublicId:  "",
        userPrivateId: "",
      }
  }

  getUserName(): any {
    if (window.localStorage.hasOwnProperty('userName'))
      return window.localStorage.getItem('userName');
    else
      return "";
  }

  getTokenExpirationDate(token: string): Date {
    const decoded: any = jwt_decode(token);

    if (decoded.exp === undefined) {
      return new Date(0);
    }

    const date = new Date(0);
    date.setUTCSeconds(decoded.exp);
    return date;
  }

  isTokenExpired(token?: string): boolean {
    if (!token) {
      return true;
    }

    const date = this.getTokenExpirationDate(token);
    if (date === new Date(0)) {
      return true;
    }

    return !(date.valueOf() > new Date().valueOf());

  }

  isUserLoggedIn() {
    const token = this.getAuthorizationToken();
    if (!token) {
      this.clearLoginLocalStorage() // sem token valido, limpa também os ids publico e privados do usuario
      return false;
    } else if (this.isTokenExpired(token)) {
      this.clearLoginLocalStorage()
      return false;
    }

    // TODO - checar se os ids publico e privado são validos
    // cuidar para não fazer uma API muito facil, uma brecha para bruteforce

    return true;
  }
}
