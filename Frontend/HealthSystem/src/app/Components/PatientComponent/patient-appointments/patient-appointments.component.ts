import { Component, Input, OnInit } from '@angular/core';
import { PatientService } from '../../../Services/PatientServices/patient.service';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-patient-appointments',
  standalone: true,
  imports: [CommonModule, HttpClientModule, FormsModule],
  providers: [PatientService],
  templateUrl: './patient-appointments.component.html'
})
export class PatientAppointmentsComponent implements OnInit {
  @Input() userID!: string;
  appointments: any[] = [];

  constructor(private PatientService: PatientService) { }

  ngOnInit(): void {
    this.userID = localStorage.getItem('userID') || '';
    if (this.userID) {
      this.PatientService.getAppointments(this.userID).subscribe(
        (data) => {
          this.appointments = data?.$values || [];
          console.log('appointments Data:', this.appointments);
        },
        (error) => {
          console.error('Error fetching appointments:', error);
        }
      );
    }

  }


  statusFilter: string = '';
  
   

  get filteredAppointments() {
    if (!this.statusFilter) return this.appointments.map(appointment => this.updateAppointmentStatus(appointment));
  
    const now = new Date();
  
    return this.appointments.filter(appointment => {
      const appointmentDate = new Date(appointment.appointmentDate);
  
      if (this.statusFilter === 'Upcoming') {
        return appointmentDate > now;
      } else if (this.statusFilter === 'Current') {
        return appointmentDate.toDateString() === now.toDateString();
      } else if (this.statusFilter === 'Past') {
        return appointmentDate < now && appointmentDate.toDateString() !== now.toDateString();
      }
  
      return true;
    }).map(appointment => this.updateAppointmentStatus(appointment));
  }
  
  private updateAppointmentStatus(appointment: any) {
    const now = new Date();
    const appointmentDate = new Date(appointment.appointmentDate);
  
    if (appointmentDate > now) {
      appointment.statusText = 'Upcoming';
    } else if (appointmentDate.toDateString() === now.toDateString()) {
      appointment.statusText = 'Current';
    } else {
      appointment.statusText = 'Past';
    }
  
    return appointment;
    }


}
