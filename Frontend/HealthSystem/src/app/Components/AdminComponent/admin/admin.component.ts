import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router'; 

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.css']
})
export class AdminComponent {

  constructor(
    private router: Router
  ) { }

  // Navigate to Login Page
  showLogin() {
    localStorage.removeItem('userID');
    localStorage.removeItem('token');
    localStorage.removeItem('jwtToken');
    this.router.navigate(['/signin']);
  }
}
