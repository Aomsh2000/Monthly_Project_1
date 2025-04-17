import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HttpHeaders } from '@angular/common/http';
@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = 'https://localhost:7021/api/admin';

  constructor(private http: HttpClient) { }

  // ------- Admin & statistics -------

  getBarChartData(): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Token not found');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    return this.http.get(`${this.apiUrl}/graph/barChart`, { headers });
  }

  getPieChartData(): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Token not found');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    return this.http.get(`${this.apiUrl}/graph/piechart`, { headers });
  }

  // ------- Admin & Patient -------

  createPatient(patientData: any): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Token not found');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    return this.http.post(`${this.apiUrl}/create-patient`, patientData, { headers });
  }

  // ------- Admin & Doctor -------
  createDoctor(doctorData: any): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Token not found');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    return this.http.post(`${this.apiUrl}/create-doctor`, doctorData, { headers });
  }

  //method to get all doctors
  getDoctors(): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Token not found');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    return this.http.get(`${this.apiUrl}/doctors`, { headers });
  }


  downloadDoctorExcel(doctorId: string): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Token not found');
    }
    const headers = new HttpHeaders({
      'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'Authorization': `Bearer ${token}`
    });

    return this.http.get(`${this.apiUrl}/download-excel/${doctorId}`, {
      headers: headers,
      responseType: 'arraybuffer'
    });
  }

  // Get available time slots
  getAvailableAppointments(date: string, clinic: number): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Token not found');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    const url = `${this.apiUrl}/getAllAvailablAappointments?date=${date}&clinic=${clinic}`;
    return this.http.get<any>(url, { headers });
  }

  // Create new appointment
  createAppointment(request: any): Observable<any> {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('Token not found');
    }
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    return this.http.post(`${this.apiUrl}/appointments/create`, request, { headers });
  }
}