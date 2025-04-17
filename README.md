# 🏥 Health Information System

   A full-stack Health Information System designed to streamline clinic operations. The application provides secure access to patients, doctors, and admin users with role-based functionalities.

# 🌐 Tech Stack

**Frontend**: Angular, Bootstrap, FontAwesome, Angular Material, ngx-echarts
**Backend**: ASP.NET Core Web API, Entity Framework Core
**Database**: MySQL
**Authentication**: JWT Token Authentication
Others: **Twilio** (SMS Notifications), **BugSnag** (Monitoring), **EPPlus** (Excel Export)

# Features by User Role:

# # 🔒 Admin
- Add new Patients and Doctors
- View Clinic Statistics with graphs
- Schedule Appointments
- Export Employee Data as Excel files

# # 🩺 Doctor
- View Personal Profile
- View Upcoming & Past Appointments
- Add Notes for Appointments

# #👤 Patient
- View Personal Profile
- View Upcoming & Past Appointments

# ⚙️ Setup Instructions
🔽 Backend Setup:

1. **Install Dependencies**:
   Run the following in your ASP.NET Core project directory:

           dotnet add package Microsoft.EntityFrameworkCore --version 8.0.0
           dotnet add package MySql.EntityFrameworkCore --version 8.0.33
           dotnet add package Microsoft.EntityFrameworkCore.Tools
           dotnet add package Pomelo.EntityFrameworkCore.MySql
           dotnet add package EPPlus
           dotnet add package Swashbuckle.AspNetCore
           dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
           dotnet add package Twilio
           dotnet add package Bugsnag.AspNet.Core

2. **Database**
   Configure MySQL connection in appsettings.json
   Apply EF Core migrations:

            dotnet ef migrations add InitialCreate
            dotnet ef database update

3. **Run the API**

            dotnet run

# 🖥️ Frontend Setup (Angular)

1. **Install Dependencies**
            npm install echarts
            npm install ngx-echarts -S
            npm install bootstrap @fortawesome/fontawesome-free
            npm install @angular/material @angular/cdk file-saver
            npm install --save-dev @types/file-saver

2. **Serve Angular App**
            ng serve


# Git Contribution Workflow

## For The First Time-> Fork & Clone the Repo

https://github.com/Razan-Alahmadi/Monthly_Project_1

## create branch
git checkout -b AR-feature/Edit-Readme-File

## save it
git add .
git commit -m "[numberOfPush] nameOfFeature - Short Description"

## push your feature
git push origin feature/<feature-name>

## up to date your main:
git checkout main
git pull origin main
git checkout feature/user-login
git merge main

## After the PR is merged into the main repository, sync your fork 

