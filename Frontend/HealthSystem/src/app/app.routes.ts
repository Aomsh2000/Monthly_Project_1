

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppointmentsComponent } from './Components/DoctorComponent/appointments/appointments.component';
import { InformationComponent } from './Components/DoctorComponent/information/information.component';
import { SigninComponent } from './Components/SignInComponent/sign-in/sign-in.component';
import { DoctorDashboardComponent } from './Components/DoctorComponent/doctor-dashboard/doctor-dashboard.component';
import { AdminComponent } from './Components/AdminComponent/admin/admin.component';
import { CreatePatientComponent } from './Components/AdminComponent/create-patient/create-patient.component';
import { AppComponent } from './app.component';
import { AdminDashboardComponent } from './Components/AdminComponent/admin-dashboard/admin-dashboard.component';
import { CreateDoctorComponent } from './Components/AdminComponent/create-doctor/create-doctor.component';
import { DoctorListComponent } from './Components/AdminComponent/doctor-list/doctor-list.component';
import { AppointmentCreateComponent } from './Components/AdminComponent/appointment-create/appointment-create.component';
import { PatientDashboardComponent } from './Components/PatientComponent/patient-dashboard/patient-dashboard.component';
export const routes: Routes = [
  {
    path: 'admin',
    component: AdminComponent, // يحتوي على sidebar
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'createPatient', component: CreatePatientComponent },
      { path: 'create-doctor', component: CreateDoctorComponent },
      { path: 'doctor-list', component: DoctorListComponent },
      { path: 'appointments/create', component: AppointmentCreateComponent },
    ],
  },
  { path: '', redirectTo: '/signin', pathMatch: 'full' },
  { path: 'signin', component: SigninComponent },
  {
    path: 'doctor-dashboard',
    component: DoctorDashboardComponent,
    children: [
      { path: 'information', component: InformationComponent },
      { path: 'appointments', component: AppointmentsComponent },
      { path: '', redirectTo: 'information', pathMatch: 'full' },
    ],
  },
  {
    path: 'patient-dashboard',
    component: PatientDashboardComponent,

  }


];


@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }




