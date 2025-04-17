// create-patient.component.ts
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AdminService } from '../../../Services/AdminServices/admin.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import Bugsnag from '@bugsnag/js';

@Component({
  selector: 'app-create-patient',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, HttpClientModule],
  templateUrl: './create-patient.component.html',
  styleUrls: ['./create-patient.component.css']
})
export class CreatePatientComponent {
  patientForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private adminService: AdminService,
    private router: Router
  ) {
    // form validations
    this.patientForm = this.fb.group({
      user: this.fb.group({
        firstName: ['', [Validators.required, Validators.minLength(2)]],
        middleName: [''],
        lastName: ['', [Validators.required, Validators.minLength(2)]],
        email: ['', [Validators.required, Validators.email]],
        phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
        password: ['', [Validators.required, Validators.minLength(8)]]
      }),
      nationalID: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]],
      dateOfBirth: ['', [Validators.required, Validators.pattern(/^\d{4}-\d{2}-\d{2}$/)]],
      gender: ['', Validators.required],
      bloodType: ['', Validators.required],
      allergies: [''],
      chronicDiseases: ['']
    });
  }

  onSubmit() {
    if (this.patientForm.invalid) {
      this.markFormGroupTouched(this.patientForm);
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formData = this.patientForm.value;

    // Log the API request JSON to console
    console.log('API Request JSON:', JSON.stringify(formData, null, 2));

    this.adminService.createPatient(formData).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.successMessage = 'Patient created successfully!';
      },
      error: (error) => {
        this.isLoading = false;
        if (error.error && typeof error.error === 'string') {
          this.errorMessage = error.error;
        } else {
          this.errorMessage = 'An error occurred while creating the patient.';
        }
        console.error('Error creating patient:', error);
      }
    });
  }

  // Helper method to mark all form controls as touched
  private markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
        Bugsnag.notify(new Error('Validation failed on Create Patient form'), event => {
          event.context = 'CreatePatientComponent.formValidation';
          event.addMetadata('formErrors', this.getFormValidationErrors());
        });
      }
    });
  }



  private getFormValidationErrors() {
    const errors: any = {};
    Object.keys(this.patientForm.controls).forEach(key => {
      const control = this.patientForm.get(key);
      if (control instanceof FormGroup) {
        errors[key] = this.getGroupErrors(control);
      } else if (control && control.invalid) {
        errors[key] = control.errors;
      }
    });
    return errors;
  }

  private getGroupErrors(group: FormGroup) {
    const groupErrors: any = {};
    Object.keys(group.controls).forEach(key => {
      const control = group.get(key);
      if (control && control.invalid) {
        groupErrors[key] = control.errors;
      }
    });
    return groupErrors;
  }


}