import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignInService {
  private apiUrl = 'https://localhost:7021/api/Auth/signin';

  constructor(private http: HttpClient) { }

  // Method to sign in and get the token
  signIn(Email: string, password: string): Observable<any> {
    return this.http.post<any>(this.apiUrl, { Email, password });
  }

  // Store the token in localStorage (or sessionStorage, depending on your preference)
  storeToken(token: string): void {
    localStorage.setItem('jwtToken', token);
  }

  // storeID(ID: string): void {
  //   localStorage.setItem('userID', ID);
  // }

  // Get the stored token (for use in requests)
  getToken(): string | null {
    return localStorage.getItem('jwtToken');
  }

  // Check if the user is authenticated (i.e., token exists)
  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  // Log out the user by removing the token
  logout(): void {
    localStorage.removeItem('jwtToken');
  }

  // Get the user ID (this would require an endpoint that returns user details based on the token)
  /*getUserDetails(): Observable<any> {
    const token = this.getToken();
    if (token) {
      // Assuming there's an API endpoint that provides user details using the token
      return this.http.get<any>('http://localhost:5187/api/Auth/user-details', {
        headers: { Authorization: `Bearer ${token}` }
      });
    }
    return new Observable(observer => observer.error('No token found'));
  }*/
}
