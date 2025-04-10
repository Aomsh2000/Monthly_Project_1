using HealthSystem.Controllers;
using HealthSystem.Data;
using HealthSystem.Models;
using HealthSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Xunit;

namespace HealthSystem.Tests.Controllers
{
    public class AdminControllerTests : IDisposable
    {
        private readonly AdminController _controller;
        private readonly AppDbContext _context;
        private readonly ITwilioService _twilioService;
        public AdminControllerTests()
        {
            // Create a new in-memory database and controller instance for each test
            _context = GetFakeDbContext();
            _controller = new AdminController(_context, _twilioService);
        }


        // Create an in-memory database for testing
        private AppDbContext GetFakeDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppDbContext(options);

            SeedTestData(context);

            return context;
        }


        // Seed test data for the in-memory database
        private void SeedTestData(AppDbContext context)
        {
            // Clear existing data
            context.Users.RemoveRange(context.Users);
            context.Patients.RemoveRange(context.Patients);
            context.Doctors.RemoveRange(context.Doctors);
            context.SaveChanges();

            // Create test users with explicit role assignments
            var patientUser1 = new User
            {
                UserID = Guid.NewGuid(),
                FirstName = "John",
                MiddleName = "A",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                Password = "password123",
                Role = UserRole.Patient
            };

            var patientUser2 = new User
            {
                UserID = Guid.NewGuid(),
                FirstName = "Jane",
                MiddleName = "B",
                LastName = "Doe",
                Email = "jane@example.com",
                PhoneNumber = "0987654321",
                Password = "password456",
                Role = UserRole.Patient
            };

            var doctorUser = new User
            {
                UserID = Guid.NewGuid(),
                FirstName = "Dr. Smith",
                MiddleName = "C",
                LastName = "Smith",
                Email = "smith@example.com",
                PhoneNumber = "5551234567",
                Password = "password789",
                Role = UserRole.Doctor
            };

            context.Users.AddRange(patientUser1, patientUser2, doctorUser);

            // Create test patients
            var patient1 = new Patient
            {
                UserID = patientUser1.UserID,
                NationalID = "12345",
                Gender = Gender.Male,
                BloodType = BloodType.A_Positive,
                DateOfBirth = new DateTime(1980, 1, 1),
                Allergies = "None",
                ChronicDiseases = "None"
            };

            var patient2 = new Patient
            {
                UserID = patientUser2.UserID,
                NationalID = "67890",
                Gender = Gender.Female,
                BloodType = BloodType.B_Negative,
                DateOfBirth = new DateTime(1985, 1, 1),
                Allergies = "Pollen",
                ChronicDiseases = "None"
            };

            context.Patients.AddRange(patient1, patient2);


            // Create test doctocr
            var doctocr1 = new Doctor
            {
                UserID = doctorUser.UserID,
                Gender = Gender.Male,
                Specialization = "Cardiology",
                Clinic = ClinicType.Cardiology
            };
            context.Doctors.Add(doctocr1);

            context.SaveChanges();

            // Verify test data counts
            var actualPatients = context.Users.Count(u => u.Role == UserRole.Patient);
            var actualDoctors = context.Users.Count(u => u.Role == UserRole.Doctor);

            if (actualPatients != 2 || actualDoctors != 1)
            {
                throw new Exception($"Test data verification failed. Patients: {actualPatients}, Doctors: {actualDoctors}");
            }
        }

        //------ End of the test setup ------



        //------ Start of the tests ------
        // Test 1: Verify that the Barchart method returns the correct patient and doctor counts
        #region BarChart Tests
        [Fact]
        public async Task Barchart_ReturnsCorrectCounts()
        {
            // check the expected number of patients and doctors
            var expectedPatients = await _context.Users.CountAsync(u => u.Role == UserRole.Patient);
            var expectedDoctors = await _context.Users.CountAsync(u => u.Role == UserRole.Doctor);

            // Act
            var result = await _controller.Barchart();

            // Assert
            // Verify the response is an OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verify the response value is dynamic
            var response = okResult.Value;

            // Handle dynamic response
            // divide the response into patients and doctors
            var responseType = response.GetType();
            var patientsProp = responseType.GetProperty("patients");
            var doctorsProp = responseType.GetProperty("doctors");

            //count the number of patients and doctors
            var patientsCount = (int)patientsProp.GetValue(response);
            var doctorsCount = (int)doctorsProp.GetValue(response);

            // Verify the counts
            Assert.NotNull(patientsProp);
            Assert.Equal(expectedPatients, patientsCount);

            Assert.NotNull(doctorsProp);
            Assert.Equal(expectedDoctors, doctorsCount);
        }
        #endregion


        // Test 2: 
        #region PieChart Tests
        [Fact]
        public async Task Piechart_ReturnsCorrectGenderCounts()
        {
            // check the expected number of male and female patients
            var expectedMaleCount = await _context.Patients.CountAsync(p => p.Gender == Gender.Male);
            var expectedFemaleCount = await _context.Patients.CountAsync(p => p.Gender == Gender.Female);

            // Act
            var result = await _controller.Piechart();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as IEnumerable<dynamic>;
            var dataList = response.ToList();

            Assert.Equal(2, dataList.Count);

            var maleData = dataList.FirstOrDefault(item =>
                item.GetType().GetProperty("name").GetValue(item).ToString() == "Male");
            var femaleData = dataList.FirstOrDefault(item =>
                item.GetType().GetProperty("name").GetValue(item).ToString() == "Female");

            Assert.NotNull(maleData);
            Assert.Equal(expectedMaleCount, (int)maleData.GetType().GetProperty("value").GetValue(maleData));

            Assert.NotNull(femaleData);
            Assert.Equal(expectedFemaleCount, (int)femaleData.GetType().GetProperty("value").GetValue(femaleData));
        }
        #endregion


        // Test 3: Verify the CreatePatient method
        #region CreatePatient Tests
        [Fact]
        public async Task CreatePatient_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new PatientCreateRequest
            {
                user = new UserRequest
                {
                    firstName = "New",
                    middleName = "D",
                    lastName = "Patient",
                    email = "new@example.com",
                    phoneNumber = "1234567890",
                    password = "password123"
                },
                nationalID = "11111",
                dateOfBirth = "2000-01-01",
                gender = "Male",
                bloodType = "A+",
                allergies = "None",
                chronicDiseases = "None"
            };

            // Act
            var result = await _controller.CreatePatient(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;

            var messageProp = response.GetType().GetProperty("message");
            Assert.NotNull(messageProp);
            Assert.Equal("Patient created successfully", messageProp.GetValue(response).ToString());

            var userIdProp = response.GetType().GetProperty("userId");
            Assert.NotNull(userIdProp);

            // Verify the new patient and user were added to the database
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.NationalID == "11111");
            // Verify the patient and user exist
            Assert.NotNull(patient);

            // Verify the user exists
            var user = await _context.Users.FindAsync((Guid)userIdProp.GetValue(response));
            Assert.NotNull(user);
        }

        [Fact]
        public async Task CreatePatient_DuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var request = new PatientCreateRequest
            {
                user = new UserRequest
                {
                    firstName = "New",
                    middleName = "D",
                    lastName = "Patient",
                    email = "john@example.com", // Duplicate from test data
                    phoneNumber = "1234567890",
                    password = "password123"
                },
                nationalID = "11111",
                dateOfBirth = "2000-01-01",
                gender = "Male",
                bloodType = "A+",
                allergies = "None",
                chronicDiseases = "None"
            };

            // Act
            var result = await _controller.CreatePatient(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Email already exists.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreatePatient_DuplicateNationalID_ReturnsBadRequest()
        {
            // Arrange
            var request = new PatientCreateRequest
            {
                user = new UserRequest
                {
                    firstName = "New",
                    middleName = "D",
                    lastName = "Patient",
                    email = "new@example.com",
                    phoneNumber = "1234567890",
                    password = "password123"
                },
                nationalID = "12345", // Duplicate from test data
                dateOfBirth = "2000-01-01",
                gender = "Male",
                bloodType = "A+",
                allergies = "None",
                chronicDiseases = "None"
            };

            // Act
            var result = await _controller.CreatePatient(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("National ID already exists.", badRequestResult.Value);
        }

        [Fact]
        public async Task CreatePatient_InvalidDate_ReturnsBadRequest()
        {
            // Arrange
            var request = new PatientCreateRequest
            {
                user = new UserRequest
                {
                    firstName = "New",
                    middleName = "D",
                    lastName = "Patient",
                    email = "new@example.com",
                    phoneNumber = "1234567890",
                    password = "password123"
                },
                nationalID = "11111",
                dateOfBirth = "invalid-date", // invalide date format
                gender = "Male",
                bloodType = "A+",
                allergies = "None",
                chronicDiseases = "None"
            };

            // Act
            var result = await _controller.CreatePatient(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid date format", badRequestResult.Value.ToString());
        }

        [Fact]
        public async Task CreatePatient_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var request = new PatientCreateRequest
            {
                user = new UserRequest
                {
                    firstName = "", // empty name
                    middleName = "D",
                    lastName = "Patient",
                    email = "new@example.com",
                    phoneNumber = "1234567890",
                    password = "password123"
                },
                nationalID = "11111",
                dateOfBirth = "2000-01-01",
                gender = "Male",
                bloodType = "A+",
                allergies = "None",
                chronicDiseases = "None"
            };

            // Force model validation
            _controller.ModelState.AddModelError("user.firstName", "First name is required");

            // Act
            var result = await _controller.CreatePatient(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }
        #endregion

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}