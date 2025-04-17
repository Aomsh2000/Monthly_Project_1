import { Component, Input, OnInit } from '@angular/core';
import { DoctorService } from '../../../Services/DoctorServices/doctor.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './appointments.component.html',
  styleUrls: ['./appointments.component.css']
})
export class AppointmentsComponent implements OnInit {
  // Doctor user ID (received as input or from local storage)
  @Input() userID!: string;
  // List of appointments
  appointments: any[] = [];
// Status filter for appointments
  statusFilter: string = '';

  // Modal details
  isModalOpen: boolean = false;
  note: string = '';
  selectedAppointmentId!: number;

  constructor(private DoctorService: DoctorService) { }
 // On component initialization, fetch appointments for the doctor
  ngOnInit(): void {
    this.userID = localStorage.getItem('userID') || '';
    if (this.userID) {
      this.DoctorService.getAppointments(this.userID).subscribe(
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


// Getter to filter appointments based on status
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
    console.log("app", appointment)

    if (appointmentDate > now) {
      appointment.statusText = 'Upcoming';

    } else if (appointmentDate.toDateString() === now.toDateString()) {
      appointment.statusText = 'Current';
    } else {
      appointment.statusText = 'Past';
    }

    return appointment;
  }





  //Open the module to add a note.
  openPopup(appointmentId: number) {
    this.selectedAppointmentId = appointmentId;
    this.isModalOpen = true;
  }

  // Close the module
  closeModal() {
    this.isModalOpen = false;
    this.note = '';
  }

  //Save note
  saveNote() {
    if (!this.note.trim()) {
      alert('Please write a note.');
      return;
    }

    this.DoctorService.updateNote(this.selectedAppointmentId, this.note).subscribe({
      next: () => {
        alert('Note saved successfully!');
        this.closeModal();
      },
      error: () => {
        console.error('Error saving note');
        alert('Failed to save the note. Please try again.');
      }
    });
  }










}



































