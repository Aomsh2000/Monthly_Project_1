import { Component, Input, OnInit } from '@angular/core';
import { PatientService } from '../../../Services/PatientServices/patient.service';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http'; 


@Component({
  selector: 'app-patient-summary',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  providers: [PatientService],
  styleUrls: ['./patient-summary.component.css'],
  templateUrl: './patient-summary.component.html' 
})

export class PatientSummaryComponent implements OnInit {
  @Input() userID!: string;
  patientData: any;

  constructor(private patientService: PatientService) {}

  ngOnInit(): void {
    this.userID = localStorage.getItem('userID') || '';
//use GET patient data
    if (this.userID) {
      this.patientService.getPatientData(this.userID).subscribe({
        next: (data) => {
          this.patientData = data;
          console.log('Patient Data:', data);
        },
        error: (error) => {
          console.error('Error fetching patient data:', error);
        }
      });
  }
    
  }
}
