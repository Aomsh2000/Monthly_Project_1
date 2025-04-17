import { Injectable } from '@angular/core';
import { HttpClient ,HttpHeaders} from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap,map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class DoctorService {
  private apiUrl = 'https://localhost:7021/api/doctor'; 

  constructor(private http: HttpClient) {}




// Get doctor data by user ID (includes authorization header)
getDoctorData(userID: string): Observable<any> {
  const token = localStorage.getItem('token');
  if (!token) {
    throw new Error('Token not found');
  }
  const headers = new HttpHeaders({
    'Authorization': `Bearer ${token}`
  });
  return this.http.get(`${this.apiUrl}/${userID}`, { headers });
}

    
// Get appointments for a specific doctor (by user ID), with logging
getAppointments(userID: string): Observable<any> {
  const token = localStorage.getItem('token');
  if (!token) {
    throw new Error('Token not found');
  }
  const headers = new HttpHeaders({
    'Authorization': `Bearer ${token}`
  });
  return this.http.get<any>(`${this.apiUrl}/${userID}/appointments`,{ headers }).pipe(
    tap((data) => console.log('Appointments:', data))
  );
}
// Update note for a specific appointment (by appointment ID), includes authorization
 updateNote(appointmentId: number, note: string): Observable<any> {
  const token = localStorage.getItem('token');
  if (!token) {
    throw new Error('Token not found');
  }
  const headers = new HttpHeaders({
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  });
    const url = `${this.apiUrl}/appointments/${appointmentId}/notes`; 
    return this.http.put(url, JSON.stringify( note) , { headers });


}


}
