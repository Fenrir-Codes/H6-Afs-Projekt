﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevPortalen.Data;
using ElevPortalen.Models;
using ElevPortalen.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace ElevPortalenTests {
    public class StudentServiceTests {

        private readonly DbContextOptions<ElevPortalenDataDbContext> _options;
        private readonly ElevPortalenDataDbContext _context;
        private readonly StudentService _studentService;
        private readonly DbContextOptions<DataRecoveryDbContext> _recoveryOptions;
        private readonly DataRecoveryDbContext _dataRecoveryContext;

        #region constructor
        public StudentServiceTests() {
            _options = new DbContextOptionsBuilder<ElevPortalenDataDbContext>()
                .UseInMemoryDatabase(databaseName: "StudentServiceTests")
                .Options;

            //we also have to include the datarecovery db so we can test it (and fill demands of instantiating studentservice)
            _recoveryOptions = new DbContextOptionsBuilder<DataRecoveryDbContext>()
                .UseInMemoryDatabase(databaseName: "datarecoveryTest")
                .Options;

            //create DbContext using options
            _context = new ElevPortalenDataDbContext(_options);
            _dataRecoveryContext = new DataRecoveryDbContext(_recoveryOptions);

            //mock dependencies
            var _dataProtectionProviderMock = new Mock<IDataProtectionProvider>();

            //create studentService instance with mocked dependencies
            _studentService = new StudentService(_context, _dataRecoveryContext, _dataProtectionProviderMock.Object);

        }
        #endregion

        #region CreateStudent test1 - check that student is created and message displayed
        [Fact]
        public async void CreateStudent_ShouldCreateStudent() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear

            var student = new StudentModel {
                UserId = Guid.NewGuid(),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "johnDoe@mail.com",
                Address = "GadeVej1",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // ACT
            var (message, success) = await _studentService.CreateStudent(student);
            var addedStudent = await _context.Student.FindAsync(student.StudentId); // Control variable - find student in context dbset
            var result = await _studentService.ReadAllStudentData(); // Since student is visible, it should be findable with ReadAllVisibleStudentData

            // ASSERT
            Assert.True(success); // Check if student profile creation was successful
            Assert.Equal("Student Profile Created.", message); // Check if message is correct
            Assert.NotNull(addedStudent); // Assert that control variable is not null
            Assert.Equal("John", result[0].FirstName); // Assert that data can be found in our mocked student service as well
        }
        #endregion

        #region CreateStudent test2 - check that student with skillmodel is created and message displayed
        [Fact]
        public async void CreateStudent_ShouldCreateStudentWithSkillModelIfStudentHasSkills() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear

            var student = new StudentModel {
                UserId = Guid.NewGuid(),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "johnDoe@mail.com",
                Address = "GadeVej1",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Skills = new SkillModel {
                    StudentId = 1,
                    CSharp = true,
                    Java = true
                }
            };

            // ACT
            var (message, success) = await _studentService.CreateStudent(student);
            var addedStudent = await _context.Student.FindAsync(student.StudentId); // Control variable - find student in context dbset          

            // ASSERT
            Assert.True(success); // Check if student profile creation was successful
            Assert.Equal("Student Profile Created.", message); // Check if message is correct
            Assert.NotNull(addedStudent); // Assert that control variable is not null
            Assert.True(addedStudent?.Skills?.CSharp); // Assert that student has a skill
        }
        #endregion

        #region CreateStudent test3 - check that students cannot be created with the same id
        [Fact]
        public async void CreateStudent_ShouldNotCreateStudentWhenStudentAlreadyExists() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var UserGuid = Guid.NewGuid().ToString(); // Make new Guid
            var student = new StudentModel {
                UserId = Guid.Parse(UserGuid),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "johnDoe@mail.com",
                Address = "GadeVej1",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            _context.Student.Add(student);
            await _context.SaveChangesAsync();

            // ACT
            async Task action() => await _studentService.CreateStudent(student); // Attempt to create the student again

            var result = await _studentService.ReadAllStudentData();

            // ASSERT
            Assert.NotNull(result); // Result must not be null 
            Assert.Single(result); // Result must be a single student
        }
        #endregion

        #region Read Data test1 - ReadData returns all students based on Guid
        [Fact]
        public async Task ReadData_ReturnsCorrectStudents() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var UserGuid = Guid.NewGuid().ToString(); // Make new Guid
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] // Set new claim
            {
                new Claim(ClaimTypes.NameIdentifier, UserGuid)
                }));

            // Define test data
            var testData = new List<StudentModel>
            {
                new StudentModel
                {
                    UserId = Guid.Parse(UserGuid), //Create two students under the same Guid
                    StudentId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "johnDoe@mail.com",
                    Address = "GadeVej1",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new StudentModel
                {
                    UserId = Guid.Parse(UserGuid),
                    StudentId = 2,
                    FirstName = "Johnny",
                    LastName = "Doe",
                    Email = "johnnyDoe@mail.com",
                    Address = "GadeVej1",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new StudentModel
                {
                    UserId = Guid.NewGuid(), // Not belonging to the user
                    StudentId = 3,
                    FirstName = "Johnathan",
                    LastName = "Doe",
                    Email = "johnathanDoe@mail.com",
                    Address = "GadeVej1",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                }
            };

            // Add test data to the database
            _context.Student.AddRange(testData);
            await _context.SaveChangesAsync();

            // ACT
            var result = await _studentService.ReadData(user);

            // ASSERT
            Assert.NotNull(result); // Result must not be null 
            Assert.Equal(2, result.Count); // We expect 2 students belonging to the user 
            Assert.IsType<StudentModel>(result[0]); // Members of the list is StudentModel
            Assert.Equal("John", result[0].FirstName); // Assert that data is same
            Assert.NotEqual("Johnathan", result[1].FirstName); //the second should not be name created under different Guid
        }
        #endregion

        #region Read Data test2 - ReadData should not return any students if there are no students associated with the Guid
        [Fact]
        public async Task ReadData_ReturnsNoStudents_IfThereAreNoStudentsAssociatedWithTheGuid() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var UserGuid = Guid.NewGuid().ToString(); // Make new Guid
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] // Set new claim
            {
                new Claim(ClaimTypes.NameIdentifier, UserGuid)
            }));

            _context.Add(new StudentModel {
                UserId = Guid.NewGuid(), //add new student, guid not belonging to the user
                StudentId = 3,
                FirstName = "Johnathan",
                LastName = "Doe",
                Email = "johnathanDoe@mail.com",
                Address = "GadeVej1",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            // ACT
            var result = await _studentService.ReadData(user);

            // ASSERT
            Assert.Empty(result); //assert no students found
            Assert.IsType<List<StudentModel>>(result); //assert that type is list of studentmodel
        }
        #endregion

        #region ReadAllStudentData test1 - returns list of students
        [Fact]
        public async void ReadAllStudentData_ShouldReturnListOfStudents_WhenStudentsExist() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear

            _context.Student.Add(new StudentModel { // Add data to db
                UserId = Guid.NewGuid(),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "johnDoe@mail.com",
                Address = "GadeVej1",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            _context.Student.Add(new StudentModel {
                UserId = Guid.NewGuid(),
                StudentId = 2,
                FirstName = "Johnny",
                LastName = "Doe",
                Email = "johnnyDoe@mail.com",
                Address = "GadeVej1",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync(); // Save changes

            // ACT
            var result = await _studentService.ReadAllStudentData(); // Use method to read students in db

            // ASSERT
            Assert.NotNull(result);
            Assert.IsType<List<StudentModel>>(result);
            Assert.Equal(2, result.Count);
        }
        #endregion

        #region ReadAllStudentData test2 - Empty DB returns Empty list but not null
        [Fact]
        public async void ReadAllStudentData_ShouldReturnEmptyListOfStudents_WhenNoStudentsExist() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear

            // ACT
            var result = await _studentService.ReadAllStudentData(); // Use method to read students in db

            // ASSERT
            Assert.NotNull(result);
            Assert.IsType<List<StudentModel>>(result);
            Assert.Empty(result);
        }
        #endregion

        #region GetStudentCountAsync test1 - Return correct count of students
        [Fact]
        public async Task GetStudentCountAsync_ShouldReturnCorrectCount() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear

            // Add some sample students to the database for testing
            await _context.Student.AddRangeAsync(new List<StudentModel>
            {
                new StudentModel {
                    UserId = Guid.NewGuid(),
                    StudentId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "johnDoe@mail.com",
                    Address = "GadeVej1",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new StudentModel {
                    UserId = Guid.NewGuid(),
                    StudentId = 2,
                    FirstName = "Johnny",
                    LastName = "Doe",
                    Email = "johnnyDoe@mail.com",
                    Address = "GadeVej1",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                },
                new StudentModel {
                    UserId = Guid.NewGuid(),
                    StudentId = 3,
                    FirstName = "Johnathan",
                    LastName = "Doe",
                    Email = "johnathanDoe@mail.com",
                    Address = "GadeVej1",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                }
            });
            await _context.SaveChangesAsync();

            // Act
            var actualCount = await _studentService.GetStudentCountAsync();

            // Assert
            Assert.Equal(3, actualCount);
        }
        #endregion

        #region GetStudentCountAsync test2 - count is 0 when no students added
        [Fact]
        public async Task GetStudentCountAsync_ShouldReturnZeroWhenNoStudentsExist() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); // Ensure the database is empty

            // ACT
            var count = await _studentService.GetStudentCountAsync();

            // ASSERT
            Assert.Equal(0, count);
        }
        #endregion

        #region Update test1 - Update student should successfully update student
        [Fact]
        public async void UpdateStudent_ShouldUpdateStudentSuccessfully() {

            //ARRANGE
            await _context.Database.EnsureDeletedAsync(); //Ensure InMemory db is clear
            var UserGuid = Guid.NewGuid().ToString(); //make new Guid

            var studentToUpdate = new StudentModel { //Create and save a student
                UserId = Guid.Parse(UserGuid),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Address = "Address",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678
            };

            var (message, success) = await _studentService.CreateStudent(studentToUpdate); //save

            var updatedStudent = new StudentModel { //create an updated studentModel
                UserId = Guid.Parse(UserGuid),
                StudentId = 1,
                FirstName = "Johnny",
                LastName = "Doe",
                Address = "Address",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 87654321
            };

            //ACT
            var (resultMessage, isSuccess) = await _studentService.Update(updatedStudent);
            var checkResult = await _studentService.ReadAllStudentData();

            //ASSERT
            Assert.True(success); //check if initial student creation was successful
            Assert.Equal("Student Profile Created.", message); //check if message was right 
            Assert.True(isSuccess); // Check if the update was successful
            Assert.Equal("Updated successfully", resultMessage); // Assert the result message
            Assert.Equal("Johnny", checkResult[0].FirstName);

        }
        #endregion

        #region Update test 2 - Update student should return entry not found if entry is invalid
        [Fact]
        public async void UpdateStudent_ShouldReturnEntryNotFound_WhenStudentId_IsNotInDb() {

            //ARRANGE
            await _context.Database.EnsureDeletedAsync(); //Ensure InMemory db is clear

            var studentToUpdate = new StudentModel { //Create a student to update
                UserId = Guid.NewGuid(),
                StudentId = 1, // Assume this student ID does not exist in the database
                FirstName = "John",
                LastName = "Doe",
                Email = "john@mail.com",
                Address = "Address",
                Description = "Description",
                Speciality = "Programmør",
                profileImage = "image.jpg",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            //ACT
            var (resultMessage, isFalse) = await _studentService.Update(studentToUpdate);
            var checkResult = await _studentService.ReadAllStudentData();

            //ASSERT
            Assert.False(isFalse); // Check if the update was unsuccessful
            Assert.Equal("Update failed.", resultMessage); // Assert the result message
            Assert.Empty(checkResult);
        }
        #endregion

        #region Delete test1 - Delete should successfully delete student
        [Fact]
        public async void DeleteStudent_ShouldDeleteStudentSuccessfully() {

            //ARRANGE
            await _context.Database.EnsureDeletedAsync(); //Ensure InMemory db is clear

            var studentToDelete = new StudentModel { //Create and save a student
                UserId = Guid.NewGuid(),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@mail.com",
                Address = "Address",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            var (message, success) = await _studentService.CreateStudent(studentToDelete); //create a new student for deletion

            //ACT
            var deleteMessage = await _studentService.Delete(studentToDelete.StudentId);

            //ASSERT
            Assert.True(success); //check if initial student creation was successful
            Assert.Equal("Student Profile Created.", message); //check if message was right 

            Assert.Equal("The User Profile deleted Successfully.", deleteMessage); // Assert the result message
            await Assert.ThrowsAsync<InvalidOperationException>(() => //assert that an error is thrown 
                _studentService.GetStudentById(studentToDelete.StudentId));
        }
        #endregion

        #region Delete test2 - delete should return student not found if the id is not a match
        [Fact]
        public async void DeleteStudent_ShouldReturnStudentNotFound_IfIdIsNotInDb() {

            //ARRANGE
            await _context.Database.EnsureDeletedAsync(); //Ensure InMemory db is clear
            var fakeStudentId = 0;

            //ACT
            var deleteMessage = await _studentService.Delete(fakeStudentId);

            //ASSERT
            Assert.Equal("Student not found.", deleteMessage); // Assert the result message
        }
        #endregion

        #region GetStudentById test1 - Retrieve student by valid ID
        [Fact]
        public async Task GetStudentById_ShouldRetrieveStudentByValidId() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var studentId = 1;
            var expectedStudent = new StudentModel {
                UserId = Guid.NewGuid(),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Address = "Address",
                Description = "Description",
                profileImage = "image.jpg",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            _context.Student.Add(expectedStudent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentById(studentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedStudent.FirstName, result.FirstName);
            Assert.Equal(expectedStudent.LastName, result.LastName);
            // Add assertions for other properties as needed
        }
        #endregion

        #region GetStudentById test2 - Throw exception when ID does not match any student
        [Fact]
        public async Task GetStudentById_ShouldThrowExceptionForInvalidId() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var studentId = 0; // Assuming this ID does not exist in the database

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _studentService.GetStudentById(studentId));
        }
        #endregion

        #region GetStudentByIdToList test1 - Retrieve student list by valid ID
        [Fact]
        public async Task GetStudentByIdToList_ShouldRetrieveStudentListByValidId() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var studentId = 1;
            var expectedStudent = new StudentModel {
                UserId = Guid.NewGuid(),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@mail.com",
                Address = "Address",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            _context.Student.Add(expectedStudent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _studentService.GetStudentByIdToList(studentId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedStudent.FirstName, result[0].FirstName);
            Assert.IsType<List<StudentModel>>(result);

        }
        #endregion

        #region GetStudentByIdToList test2 - Retrieve student list should not retrieve a list if there is no student with the id
        [Fact]
        public async Task GetStudentByIdToList_ShouldNotRetrieveStudentList_IfIdIsNotValid() {
            // Arrange
            await _context.Database.EnsureDeletedAsync(); // Ensure InMemory db is clear
            var studentId = 0;

            // Act
            var result = await _studentService.GetStudentByIdToList(studentId);

            // Assert
            Assert.Empty(result);
            Assert.IsType<List<StudentModel>>(result);
        }
        #endregion

        #region GetStudentByGuid test1 - Get student by Guid should return the student if exists
        [Fact]
        public async Task GetStudentByGuid_ShouldReturnStudentIfExists() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); //ensure InMemory db is clear
            // Add a student with a specific GUID to the database
            var studentId = Guid.NewGuid().ToString();
            var student = new StudentModel {
                UserId = Guid.Parse(studentId),
                StudentId = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "johnDoe@mail.com",
                Address = "GadeVej1",
                Description = "Description",
                profileImage = "image.jpg",
                Speciality = "Programmør",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };
            await _context.Student.AddAsync(student);
            await _context.SaveChangesAsync();

            // ACT
            var result = await _studentService.GetStudentByGuid(Guid.Parse(studentId));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(studentId, result.UserId.ToString());
        }
        #endregion

        #region GetStudentByGuid test2 - Get student by Guid should return null if the Guid does not exist
        [Fact]
        public async Task GetStudentByGuid_ShouldReturnNullForNonExistingStudent() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); //ensure InMemory db is clear
            // Ensure that no student exists in the database with a specific GUID

            // ACT
            var result = await _studentService.GetStudentByGuid(Guid.NewGuid());

            // ASSERT
            Assert.Null(result);
        }
        #endregion

        #region GetStudentsBySpecialization test1 - GetStudentsBySpecialization should return correct list of students with specified specialization
        [Fact]
        public async Task GetStudentsBySpecialization_ShouldReturnCorrectListOfStudents() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); //ensure InMemory db is clear
            var specialization = "Programmør";
            //var studentCount = 3;
            var students = new List<StudentModel>{
                new StudentModel {
                    UserId = Guid.NewGuid(),
                    StudentId = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com",
                    Address = "Address",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now },
                new StudentModel {
                    UserId = Guid.NewGuid(),
                    StudentId = 2,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com",
                    Address = "Address",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now },
                new StudentModel {
                    UserId = Guid.NewGuid(),
                    StudentId = 3,
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john@example.com",
                    Address = "Address",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Programmør",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now },
                new StudentModel { //we add one student not a programmer
                    UserId = Guid.NewGuid(),
                    StudentId = 4,
                    FirstName = "Johnni",
                    LastName = "Doe",
                    Email = "johnni@example.com",
                    Address = "Address",
                    Description = "Description",
                    profileImage = "image.jpg",
                    Speciality = "Infrastruktur",
                    PhoneNumber = 12345678,
                    RegisteredDate = DateTime.Now,
                    UpdatedDate = DateTime.Now }
            };
            await _context.Student.AddRangeAsync(students);
            await _context.SaveChangesAsync();

            // ACT
            var result = await _studentService.GetStudentsBySpecialization(specialization);

            // ASSERT
            Assert.Equal(students.Count - 1, result.Count); // Check the count of returned students is all students minus one Infrastructure student
            foreach (var expectedStudent in students.Where(s => s.Speciality == specialization)) {
                Assert.Contains(expectedStudent, result); // Check if each expected student with the specified specialization is present in the result
            }
            foreach (var notExpectedStudent in students.Where(s => s.Speciality != specialization)) {
                Assert.DoesNotContain(notExpectedStudent, result); // Check if any student with a different specialization is not present in the result
            }
        }
        #endregion

        #region GetStudentsBySpecialization test2 - GetStudentsBySpecialization should handle case where no students with specified specialization
        [Fact]
        public async Task GetStudentsBySpecialization_ShouldHandleNoStudentsWithSpecifiedSpecialization() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); //ensure InMemory db is clear
            var specialization = "Infrastruktur";

            // ACT
            var result = await _studentService.GetStudentsBySpecialization(specialization);

            // ASSERT
            Assert.Empty(result);
            Assert.IsType<List<StudentModel>>(result);
        }
        #endregion

        #region CreateRecoveryData test1 - should create recovery data successfully
        [Fact]
        public async Task CreateRecoveryData_ShouldCreateRecoveryDataSuccessfully() {
            // ARRANGE
            await _dataRecoveryContext.Database.EnsureDeletedAsync(); // Ensure the recovery database is empty
            var deletedStudent = new StudentModel {
                UserId = Guid.NewGuid(),
                StudentId = 1,
                Title = "Mr",
                FirstName = "John",
                MiddleName = "Doe",
                LastName = "Smith",
                Email = "john@example.com",
                Address = "123 Street",
                Description = "Student Description",
                profileImage = "student.jpg",
                Speciality = "Computer Science",
                PhoneNumber = 12345678,
                RegisteredDate = DateTime.Now,
                UpdatedDate = DateTime.Now
            };

            // ACT
            var result = await _studentService.CreateRecoveryData(deletedStudent);

            // ASSERT
            Assert.Equal("Recovery Created", result);
            //ckeck the recovery database to check if student is there
            var recoveredStudent = await _dataRecoveryContext.StudentDataRecovery.FirstOrDefaultAsync(c => c.FirstName == "John");
            Assert.NotNull(recoveredStudent); // Ensure that the student exists in the recovery database

        }

        #endregion

        #region CheckRecoveryDataExist test1 - Should return true if data exists on the Guid
        [Fact]
        public async Task CheckRecoveryDataExist_ShouldReturnTrueForExistingData() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); //ensure InMemory db is clear
            // Add recovery data for a specific user to the database
            var userId = Guid.NewGuid();
            await _dataRecoveryContext.StudentDataRecovery.AddAsync(new StudentRecoveryModel { UserId = userId });
            await _dataRecoveryContext.SaveChangesAsync();

            // ACT
            var result = await _studentService.CheckRecoveryDataExist(userId);

            // ASSERT
            Assert.True(result);
        }

        #endregion

        #region CheckRecoveryDataExist test2 - Should return false if no data exists on the Guid
        [Fact]
        public async Task CheckRecoveryDataExist_ShouldReturnFalseForNonExistingData() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); //ensure InMemory db is clear
            // Ensure that no recovery data exists in the database for a specific user

            // ACT
            var result = await _studentService.CheckRecoveryDataExist(Guid.NewGuid());

            // ASSERT
            Assert.False(result);
        }

        #endregion

        #region RecoverStudentData test1 - RecoverStudentData should recover data successfully with correct Guid
        [Fact]
        public async Task RecoverStudentData_ShouldRecoverDataSuccessfully() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); //ensure InMemory db is clear
            // Add recovery data for a specific user to the recovery database
            var userId = Guid.NewGuid();
            await _dataRecoveryContext.StudentDataRecovery.AddAsync(new StudentRecoveryModel { UserId = userId });
            await _dataRecoveryContext.SaveChangesAsync();

            // ACT
            var message = await _studentService.RecoverStudentData(userId);

            // ASSERT
            Assert.Equal("Data successfully recovered.", message);

            // Check if the recovered student data exists in the main context
            var recoveredStudent = await _context.Student.FirstOrDefaultAsync(s => s.UserId == userId);
            Assert.NotNull(recoveredStudent);
        }
        #endregion

        #region RecoverStudentData test2 - RecoverStudentData should not recover data successfully without correct Guid
        [Fact]
        public async Task RecoverStudentData_ShouldReturnFailureForNonExistingData() {
            // ARRANGE
            await _context.Database.EnsureDeletedAsync(); //ensure InMemory db is clear

            var studentId = Guid.NewGuid().ToString();
            // ACT
            var message = await _studentService.RecoverStudentData(Guid.Parse(studentId));

            // ASSERT
            Assert.Equal($"No recovery data found for UserId: {studentId}.", message);
        }
        #endregion





    }
}
