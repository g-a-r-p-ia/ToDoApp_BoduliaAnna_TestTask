import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = 'http://localhost:5000/api/auth';
  private readonly tokenKey = 'jwt_token';
  private readonly authenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());

  constructor(private http: HttpClient) {}

  login(email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, { email, password }).pipe(
      tap((response) => this.handleAuthSuccess(response))
    );
  }

  register(firstName: string, lastName: string, email: string, password: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, { firstName, lastName, email, password }).pipe(
      tap((response) => this.handleAuthSuccess(response))
    );
  }

  loginWithGoogle(googleToken: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/google-login`, JSON.stringify(googleToken), {
      headers: { 'Content-Type': 'application/json' }
    }).pipe(
      tap((response) => this.handleAuthSuccess(response))
    );
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.authenticatedSubject.next(false);
  }

  isLoggedIn(): boolean {
    return this.hasToken();
  }

  isAuthenticated(): Observable<boolean> {
    return this.authenticatedSubject.asObservable();
  }

  private handleAuthSuccess(response: any): void {
    localStorage.setItem(this.tokenKey, response.token);
    this.authenticatedSubject.next(true);
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(this.tokenKey);
  }
}
