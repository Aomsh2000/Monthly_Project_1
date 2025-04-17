import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InformationComponent } from '../information/information.component';
import { AppointmentsComponent } from '../appointments/appointments.component';
import { SignInService } from '../../../Services/SignInServices/sign-in/sign-in.service';  
import { HttpClientModule } from '@angular/common/http'; 
import { RouterOutlet, Router } from '@angular/router';
import { RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { DoctorService } from '../../../Services/DoctorServices/doctor.service';

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [CommonModule,RouterModule, InformationComponent, AppointmentsComponent],
  templateUrl: './doctor-dashboard.component.html',
  styleUrl: './doctor-dashboard.component.css'
})
export class DoctorDashboardComponent implements OnInit {
  userID: string = '';  
  doctorName:string='';
  constructor(private router: Router, private route: ActivatedRoute,private doctorService:DoctorService) {}

  ngOnInit() {
 
      this.userID = localStorage.getItem('userID') || '';
      console.log('hello from Doctor com : User ID from localStorage:', this.userID);

      if (this.userID) {
        this.doctorService.getDoctorData(this.userID).subscribe(
          (data) => {
            this.doctorName = `${data.user.firstName}`;  
          },
          (error) => {
            console.error('Error fetching doctor info:', error);
          }
        );
      }
    
  }


  // Navigate to Doctor Info Page
  showInformation() {
    this.router.navigate(['information'], { relativeTo: this.route });
  }

  // Navigate to Appointments Page
  showAppointments() {
    this.router.navigate(['appointments'], { relativeTo: this.route });
  }


  // Navigate to Login Page
  showLogin() {
    localStorage.removeItem('userID');
    localStorage.removeItem('token');
    localStorage.removeItem('jwtToken');
    this.router.navigate(['/signin']);
  }

  
}







