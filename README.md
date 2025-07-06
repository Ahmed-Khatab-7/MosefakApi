# MosefakApi - Comprehensive Healthcare API

## Overview

The MosefakApi is a robust and scalable backend solution designed to manage various aspects of a healthcare or clinic management system. It provides a comprehensive set of APIs for user authentication, appointment scheduling, doctor and patient management, payment processing, and more. Built with modern .NET technologies, it aims to offer a secure, efficient, and flexible foundation for healthcare applications.




## Key Features

- **User Authentication & Authorization**: Secure user registration, login, and role-based access control (RBAC) for different user types (patients, doctors, admins).
- **Doctor Management**: Comprehensive profiles for doctors, including specializations, clinics, working hours, awards, education, and experience. Search and filter doctors based on various criteria.
- **Patient Management**: Functionality to manage patient information and their interactions within the system.
- **Appointment Scheduling**: Flexible appointment booking system with features for creating, viewing, approving, rejecting, rescheduling, and canceling appointments. Includes payment integration for appointments.
- **Payment Processing**: Integration with payment gateways (e.g., Stripe) for secure and efficient handling of appointment payments.
- **Notification System**: Mechanisms for sending notifications related to appointments, profile updates, and other system events.
- **Clinic Management**: Doctors can manage multiple clinics, including their addresses and working times.
- **Review and Rating System**: Patients can leave reviews and ratings for doctors.
- **ID Protection**: Sensitive IDs are protected using a custom ID protection service.
- **Rate Limiting**: Implemented to prevent abuse and ensure API stability.



## Entity Relationships

The MosefakApi leverages a well-structured relational database schema to manage its data. Key entities and their relationships include:

- **`AppUser` (Identity)**: Represents a user in the system. This is the base identity for both `Doctor` and `Patient`.
- **`Doctor`**: Extends `AppUser` and contains specific information about medical practitioners, such as `LicenseNumber`, `AboutMe`, and collections of `Specializations`, `AppointmentTypes`, `Awards`, `Experiences`, `Educations`, and `Clinics`.
  - A `Doctor` can have many `Specializations` (one-to-many).
  - A `Doctor` can define many `AppointmentTypes` (one-to-many).
  - A `Doctor` can have many `Awards` (one-to-many).
  - A `Doctor` can have many `Experiences` (one-to-many).
  - A `Doctor` can have many `Educations` (one-to-many).
  - A `Doctor` can manage multiple `Clinics` (one-to-many).
  - A `Doctor` can have many `Reviews` from patients (one-to-many).
  - A `Doctor` can have many `Appointments` (one-to-many).
- **`Patient`**: Also extends `AppUser` (implicitly, as `PatientId` in `Appointment` refers to `AppUser`).
- **`Clinic`**: Represents a medical clinic or practice location. Each `Clinic` belongs to a `Doctor` and has associated `WorkingTimes`.
  - A `Clinic` has many `WorkingTimes` (one-to-many).
- **`WorkingTime`**: Defines the working hours for a `Clinic` on specific days, including `Periods` (time slots).
  - A `WorkingTime` has many `Period`s (one-to-many).
- **`Appointment`**: Represents a scheduled appointment between a `Doctor` and a `Patient`. It includes details like `StartDate`, `EndDate`, `AppointmentType`, `ProblemDescription`, `AppointmentStatus`, and `PaymentStatus`.
  - An `Appointment` is linked to one `Doctor` (many-to-one).
  - An `Appointment` is linked to one `Patient` (many-to-one).
  - An `Appointment` has one `AppointmentType` (many-to-one).
  - An `Appointment` can have one `Payment` (one-to-one).
- **`Payment`**: Stores details about payments made for appointments.
- **`Review`**: Represents a review given by a patient to a doctor, including a rating and comments.
- **`Specialization`**: Defines a medical specialization (e.g., Cardiology, Pediatrics).
- **`AppointmentType`**: Defines the type of appointment (e.g., Consultation, Follow-up).
- **`Award`**: Represents an award or recognition received by a doctor.
- **`Experience`**: Details a doctor's professional experience.
- **`Education`**: Details a doctor's educational background.




## Technologies Used

The MosefakApi is built using a modern and robust technology stack, primarily focused on the .NET ecosystem:

- **.NET 8**: The core framework for building the API, providing high performance and cross-platform capabilities.
- **ASP.NET Core**: Used for building the web API, offering features like MVC, routing, and middleware.
- **Entity Framework Core**: An object-relational mapper (ORM) that simplifies database interactions, allowing for data access using .NET objects.
- **Microsoft SQL Server**: The relational database management system used for data persistence.
- **ASP.NET Core Identity**: For managing user authentication, authorization, and user profiles.
- **JWT (JSON Web Tokens)**: Used for secure API authentication and authorization.
- **Stripe API**: Integrated for handling payment processing.
- **Firebase**: Potentially used for real-time features or notifications (based on `AddFirebaseFieldsToAppUser` migration).
- **Docker**: For containerization, enabling consistent deployment across different environments.
- **AWS Elastic Beanstalk**: Used for deployment and scaling of the application on Amazon Web Services.
- **Rate Limiting Middleware**: Custom or third-party middleware for controlling the rate of requests to the API.
- **AutoMapper**: For object-to-object mapping, reducing boilerplate code.
- **FluentValidation**: For building strong-typed validation rules.




## Technical Highlights

- **Clean Architecture**: The project appears to follow principles of clean architecture, separating concerns into distinct layers such as `MosefakApi.Business`, `MosefakApp.API`, `MosefakApp.Core`, `MosefakApp.Domains`, `MosefakApp.Infrastructure`, and `MosefakApp.Shared`. This promotes maintainability, testability, and scalability.
- **ID Protection**: Implementation of `IIdProtectorService` ensures that sensitive internal IDs are not exposed directly through the API, enhancing security by obfuscating primary keys in API responses and requests.
- **Role-Based Access Control (RBAC)**: The use of `[RequiredPermission]` attributes on API endpoints indicates a granular RBAC system, allowing precise control over which users can access specific functionalities.
- **Rate Limiting**: The `[EnableRateLimiting]` attribute suggests the implementation of rate limiting, which helps protect the API from abuse, denial-of-service attacks, and ensures fair usage among clients.
- **Asynchronous Programming**: Extensive use of `async` and `await` keywords throughout the codebase ensures non-blocking operations, improving the responsiveness and scalability of the API, especially for I/O-bound tasks like database access and external API calls.
- **Centralized Error Handling**: The presence of `ErrorsController.cs` and custom exception types in `MosefakApp.Shared/Exceptions` indicates a structured approach to error handling, providing consistent and informative error responses to API consumers.
- **Dependency Injection**: The project heavily utilizes ASP.NET Core's built-in dependency injection container, promoting loose coupling and making the codebase more modular and testable.
- **Database Migrations**: The `MosefakApp.Infrastructure/Migrations` folder confirms the use of Entity Framework Core migrations for managing database schema changes, enabling smooth evolution of the database alongside application development.
- **Seed Data**: The `MosefakApp.Infrastructure/Seed` directory suggests the inclusion of seed data, which is useful for populating the database with initial or test data, facilitating development and deployment.
- **Global Usings**: The `globalUsings.cs` files indicate the use of C# 10's global `using` directives, which reduce boilerplate and improve code readability by centralizing common `using` statements.




## Key Use Cases

The MosefakApi can be utilized in various scenarios within the healthcare domain:

- **Patient Portal/Mobile App**: A patient-facing application can use the API for user registration, login, searching for doctors, booking appointments, viewing appointment history, managing payments, and leaving reviews.
- **Doctor Dashboard/Clinic Management System**: Doctors or clinic administrators can use the API to manage their profiles, working hours, clinics, view upcoming and past appointments, approve/reject appointments, manage specializations, awards, education, and experiences.
- **Admin Panel**: An administrative interface can leverage the API for comprehensive user management (doctors, patients), system configuration, and monitoring.
- **Telemedicine Platforms**: The API can serve as the backend for telemedicine solutions, facilitating virtual consultations and managing related appointments and patient data.
- **Healthcare Integration**: It can be integrated with other healthcare systems (e.g., EHR/EMR) to synchronize patient and appointment data.
- **Reporting and Analytics**: Data exposed through the API can be used to generate reports on appointments, doctor performance, earnings, and patient engagement.




## API Documentation

This section provides detailed documentation for the various API endpoints available in MosefakApi. Each endpoint description includes its purpose, HTTP method, URL, request parameters (if any), and expected responses.

### 1. Account Management (`AccountController`)

Base URL: `/api/account`

- **`GET /api/account/google-login`**
  - **Description**: Initiates the Google OAuth 2.0 login process. This endpoint redirects the user to Google's authentication page.
  - **Authentication**: Anonymous
  - **Request Parameters**:
    - `returnUrl` (query, optional): The URL to redirect to after successful Google login. Defaults to `/`.
  - **Responses**:
    - `302 Redirect`: Redirects to Google's authentication page.

- **`GET /api/account/google-callback`**
  - **Description**: Callback endpoint for Google OAuth 2.0. Handles the response from Google after user authentication, creates a new user if one doesn't exist, and generates a JWT token.
  - **Authentication**: Anonymous
  - **Request Parameters**:
    - `returnUrl` (query, optional): The URL to redirect to after successful Google login. Defaults to `/`.
  - **Responses**:
    - `200 OK`: Returns a JSON object containing the JWT token.
      ```json
      {
        "token": "<JWT_TOKEN_STRING>"
      }
      ```
    - `400 Bad Request`: If external authentication fails or email is not found in the Google account.
      ```json
      {
        "error": "External authentication failed."
      }
      ```
      or
      ```json
      {
        "error": "Email not found in Google account."
      }
      ```
      or
      ```json
      {
        "error": "Failed to create user."
      }
      ```




### 2. Authentication Management (`AuthenticationController`)

Base URL: `/api/authentication`

- **`POST /api/authentication/Login`**
  - **Description**: Authenticates a user and returns a JWT token upon successful login.
  - **Authentication**: Anonymous
  - **Request Body**: `LoginRequest`
    ```json
    {
      "email": "string",
      "password": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Returns a `LoginResponse` object containing the JWT token.
      ```json
      {
        "token": "<JWT_TOKEN_STRING>",
        "userId": "<PROTECTED_USER_ID>",
        "userName": "string",
        "email": "string",
        "roles": [
          "string"
        ]
      }
      ```
    - `400 Bad Request`: If login credentials are invalid.
      ```json
      {
        "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        "title": "Bad Request",
        "status": 400,
        "traceId": "string",
        "errors": {
          "": [
            "Invalid Credentials"
          ]
        }
      }
      ```

- **`POST /api/authentication/Register`**
  - **Description**: Registers a new user in the system.
  - **Authentication**: Anonymous
  - **Request Body**: `RegisterRequest`
    ```json
    {
      "email": "string",
      "password": "string",
      "confirmPassword": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: User registered successfully.
    - `400 Bad Request`: If registration fails due to invalid input or existing user.

- **`POST /api/authentication/confirm-email`**
  - **Description**: Confirms a user's email address using a provided token.
  - **Authentication**: Anonymous
  - **Request Body**: `ConfirmEmailRequest`
    ```json
    {
      "userId": "string",
      "token": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Email confirmed successfully, returns protected user ID.
    - `400 Bad Request`: If confirmation fails.

- **`POST /api/authentication/resend-confirmation-email`**
  - **Description**: Resends the email confirmation link to the user.
  - **Authentication**: Anonymous
  - **Request Body**: `ResendConfirmationEmailRequest`
    ```json
    {
      "email": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Confirmation email sent successfully.
    - `400 Bad Request`: If email is not found or other issues.

- **`POST /api/authentication/forget-password`**
  - **Description**: Initiates the password reset process by sending a reset code to the user's email.
  - **Authentication**: Anonymous
  - **Request Body**: `ForgetPasswordRequest`
    ```json
    {
      "email": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: If email is registered, a password reset code will be sent.
    - `400 Bad Request`: If the request is invalid.

- **`POST /api/authentication/verify-reset-code`**
  - **Description**: Verifies the password reset code sent to the user's email.
  - **Authentication**: Anonymous
  - **Request Body**: `VerifyResetCodeRequest`
    ```json
    {
      "email": "string",
      "code": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Verification successful. User can now set a new password.
    - `400 Bad Request`: If the code is invalid or expired.

- **`POST /api/authentication/reset-password`**
  - **Description**: Resets the user's password using the verified reset code.
  - **Authentication**: Anonymous
  - **Request Body**: `ResetPasswordRequest`
    ```json
    {
      "email": "string",
      "code": "string",
      "newPassword": "string",
      "confirmNewPassword": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Password reset successfully.
    - `400 Bad Request`: If the request is invalid or the code is incorrect.




### 3. Appointment Management (`AppointmentsController`)

Base URL: `/api/appointments`

- **`GET /api/appointments/patient`**
  - **Description**: Retrieves a paginated list of appointments for the authenticated patient.
  - **Authentication**: Required (Patient)
  - **Permissions**: `Appointments.ViewPatientAppointments`
  - **Request Parameters**:
    - `status` (query, optional): Filter appointments by status (e.g., `PendingApproval`, `Confirmed`, `Cancelled`, `Completed`).
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentResponse>`.
      ```json
      {
        "data": [
          {
            "id": "<PROTECTED_APPOINTMENT_ID>",
            "doctorId": "<PROTECTED_DOCTOR_ID>",
            "patientId": "<PROTECTED_PATIENT_ID>",
            "startDate": "2025-07-06T10:00:00Z",
            "endDate": "2025-07-06T11:00:00Z",
            "appointmentTypeId": "<PROTECTED_APPOINTMENT_TYPE_ID>",
            "appointmentType": {
              "id": "<PROTECTED_APPOINTMENT_TYPE_ID>",
              "name": "string"
            },
            "problemDescription": "string",
            "appointmentStatus": "PendingApproval",
            "cancellationReason": "string",
            "paymentStatus": "Pending",
            "payment": null,
            "paymentDueTime": "2025-07-06T09:00:00Z",
            "confirmedAt": null,
            "cancelledAt": null,
            "completedAt": null,
            "approvedByDoctor": false,
            "serviceProvided": false,
            "doctorSpecialization": [
              {
                "id": "<PROTECTED_SPECIALIZATION_ID>",
                "name": "string"
              }
            ]
          }
        ],
        "totalPages": 1,
        "currentPage": 1,
        "pageSize": 10
      }
      ```

- **`GET /api/appointments/{appointmentId}`**
  - **Description**: Retrieves a specific appointment by its protected ID.
  - **Authentication**: Required
  - **Permissions**: `Appointments.View`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment.
  - **Responses**:
    - `200 OK`: Returns an `AppointmentResponse` object.
    - `400 Bad Request`: If `appointmentId` is invalid.
    - `404 Not Found`: If the appointment is not found.

- **`GET /api/appointments/range`**
  - **Description**: Retrieves a paginated list of appointments for the authenticated patient within a specified date range.
  - **Authentication**: Required (Patient)
  - **Permissions**: `Appointments.ViewInRange`
  - **Request Parameters**:
    - `startDate` (query): Start date and time of the range (e.g., `2025-01-01T00:00:00Z`).
    - `endDate` (query): End date and time of the range (e.g., `2025-12-31T23:59:59Z`).
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentResponse>`.

- **`PUT /api/appointments/{appointmentId}/approve`**
  - **Description**: Approves a pending appointment by a doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Appointments.Approve`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment to approve.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `appointmentId` is invalid.

- **`PUT /api/appointments/{appointmentId}/reject`**
  - **Description**: Rejects a pending appointment by a doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Appointments.Reject`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment to reject.
  - **Request Body**: `RejectAppointmentRequest`
    ```json
    {
      "rejectionReason": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `appointmentId` is invalid.

- **`PUT /api/appointments/{appointmentId}/complete`**
  - **Description**: Marks an appointment as completed by a doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Appointments.MarkAsCompleted`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment to mark as completed.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `appointmentId` is invalid.

- **`DELETE /api/appointments/{appointmentId}/doctor-cancel`**
  - **Description**: Cancels an appointment by a doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Appointments.CancelByDoctor`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment to cancel.
  - **Request Body**: `CancelAppointmentRequest`
    ```json
    {
      "cancelationReason": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `appointmentId` is invalid.

- **`PUT /api/appointments/{appointmentId}/patient-cancel`**
  - **Description**: Cancels an appointment by a patient.
  - **Authentication**: Required (Patient)
  - **Permissions**: `Appointments.CancelByPatient`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment to cancel.
  - **Request Body**: `CancelAppointmentRequest`
    ```json
    {
      "cancelationReason": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `appointmentId` is invalid.

- **`POST /api/appointments/create-payment-intent/{appointmentId}`**
  - **Description**: Creates a payment intent for a given appointment, typically used for Stripe integration.
  - **Authentication**: Required
  - **Permissions**: `Appointments.CreatePaymentIntent`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment.
  - **Responses**:
    - `200 OK`: Returns the client secret for the payment intent.
    - `400 Bad Request`: If `appointmentId` is invalid.

- **`POST /api/appointments/confirm-appointment-payment/{appointmentId}`**
  - **Description**: Confirms the payment for an appointment. This can be used if webhooks are not utilized.
  - **Authentication**: Required
  - **Permissions**: `Appointments.ConfirmPayment`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment.
  - **Responses**:
    - `200 OK`: Returns `true` if payment is confirmed successfully.
    - `400 Bad Request`: If `appointmentId` is invalid.

- **`PUT /api/appointments/reschedule`**
  - **Description**: Reschedules an existing appointment.
  - **Authentication**: Required
  - **Permissions**: `Appointments.Reschedule`
  - **Request Body**: `RescheduleAppointmentRequest`
    ```json
    {
      "appointmentId": "<PROTECTED_APPOINTMENT_ID>",
      "selectedDate": "2025-07-06T00:00:00Z",
      "newTimeSlot": "string" // e.g., "14:00-15:00"
    }
    ```
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `appointmentId` is invalid or rescheduling fails.

- **`POST /api/appointments/book`**
  - **Description**: Books a new appointment.
  - **Authentication**: Required (Patient)
  - **Permissions**: `Appointments.Book`
  - **Request Body**: `BookAppointmentRequest`
    ```json
    {
      "doctorId": "<PROTECTED_DOCTOR_ID>",
      "appointmentTypeId": "<PROTECTED_APPOINTMENT_TYPE_ID>",
      "startDate": "2025-07-06T10:00:00Z",
      "endDate": "2025-07-06T11:00:00Z",
      "problemDescription": "string"
    }
    ```
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If any provided ID is invalid or booking fails.

- **`GET /api/appointments/{appointmentId}/status`**
  - **Description**: Retrieves the status of a specific appointment.
  - **Authentication**: Required
  - **Permissions**: `Appointments.ViewStatus`
  - **Request Parameters**:
    - `appointmentId` (path): The protected ID of the appointment.
  - **Responses**:
    - `200 OK`: Returns the `AppointmentStatus` enum value.
    - `400 Bad Request`: If `appointmentId` is invalid.

- **`GET /api/appointments/doctor`**
  - **Description**: Retrieves a paginated list of appointments for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Appointments.ViewDoctorAppointments`
  - **Request Parameters**:
    - `status` (query, optional): Filter appointments by status.
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentResponse>`.

- **`GET /api/appointments/doctor/patient-data`**
  - **Description**: Retrieves a paginated list of appointments for a specific doctor, including patient data.
  - **Authentication**: Required
  - **Permissions**: `Appointments.ViewDoctorAppointments`
  - **Request Parameters**:
    - `doctorId` (query): The protected ID of the doctor.
    - `status` (query, optional): Filter appointments by status.
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentPatientResponse>`.
    - `400 Bad Request`: If `doctorId` is invalid.

- **`GET /api/appointments/pending`**
  - **Description**: Retrieves a paginated list of pending appointments for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Appointments.ViewPendingForDoctor`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentResponse>`.

- **`GET /api/appointments/doctor/range`**
  - **Description**: Retrieves a paginated list of appointments for the authenticated doctor within a specified date range.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Appointments.ViewInRangeForDoctor`
  - **Request Parameters**:
    - `startDate` (query): Start date and time of the range.
    - `endDate` (query): End date and time of the range.
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentResponse>`.

- **`GET /api/appointments/Payments`**
  - **Description**: Retrieves a paginated list of payments related to appointments.
  - **Authentication**: Required
  - **Permissions**: `Appointments.Payments`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<PaymentResponse>`.

- **`DELETE /api/appointments/Payments/{paymentId}`**
  - **Description**: Deletes a specific payment record.
  - **Authentication**: Required
  - **Permissions**: `Appointments.DeletePayment`
  - **Request Parameters**:
    - `paymentId` (path): The protected ID of the payment to delete.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `paymentId` is invalid.




### 4. Doctor Management (`DoctorsController`)

Base URL: `/api/doctors`

- **`GET /api/doctors`**
  - **Description**: Retrieves a paginated list of all doctors.
  - **Authentication**: Required
  - **Permissions**: `Doctors.View`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<DoctorResponse>`.

- **`GET /api/doctors/{doctorId}`**
  - **Description**: Retrieves detailed information about a specific doctor by their protected ID.
  - **Authentication**: Required
  - **Permissions**: `Doctors.ViewById`
  - **Request Parameters**:
    - `doctorId` (path): The protected ID of the doctor.
  - **Responses**:
    - `200 OK`: Returns a `DoctorDetail` object.
    - `400 Bad Request`: If `doctorId` is invalid.

- **`POST /api/doctors/search`**
  - **Description**: Searches for doctors based on various unified filter criteria.
  - **Authentication**: Required
  - **Permissions**: `Doctors.Search`
  - **Request Body**: `DoctorUnifiedSearchFilter` (details of this DTO would be needed for full documentation).
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<DoctorResponse>`.

- **`GET /api/doctors/profile`**
  - **Description**: Retrieves the profile of the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.ViewProfile`
  - **Responses**:
    - `200 OK`: Returns a `DoctorProfileResponse` object.

- **`GET /api/doctors/top-ten`**
  - **Description**: Retrieves a list of the top 10 doctors.
  - **Authentication**: Required
  - **Permissions**: `Doctors.ViewTopTen`
  - **Responses**:
    - `200 OK`: Returns a list of `DoctorResponse` objects.

- **`GET /api/doctors/appointments/upcoming`**
  - **Description**: Retrieves a paginated list of upcoming appointments for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.ViewUpcomingAppointments`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentDto>`.

- **`GET /api/doctors/appointments/today`**
  - **Description**: Retrieves a paginated list of today's appointments for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.GetToDayAppointments`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentPatinetDetail>`.

- **`GET /api/doctors/appointments/past`**
  - **Description**: Retrieves a paginated list of past appointments for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.ViewPastAppointments`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AppointmentDto>`.

- **`GET /api/doctors/appointments/total`**
  - **Description**: Retrieves the total number of appointments for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.GetTotalAppointments`
  - **Responses**:
    - `200 OK`: Returns a `long` representing the total count.

- **`POST /api/doctors/profile/image`**
  - **Description**: Uploads a profile image for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.UploadProfileImage`
  - **Request Body**: `IFormFile` (image file).
  - **Responses**:
    - `200 OK`: Returns `true` if successful.

- **`GET /api/doctors/specializations`**
  - **Description**: Retrieves a paginated list of specializations associated with the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Specializations.View`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<SpecializationResponse>`.

- **`GET /api/doctors/specializations/admin`**
  - **Description**: Retrieves a paginated list of all specializations (for admin use).
  - **Authentication**: Required (Admin)
  - **Permissions**: `Specializations.ViewForAdmin`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<SpecializationResponse>`.

- **`POST /api/doctors/specializations`**
  - **Description**: Adds a new specialization to the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Specializations.Create`
  - **Request Body**: `SpecializationRequest`
  - **Responses**:
    - `200 OK`: Returns `true` if successful.

- **`PUT /api/doctors/specializations/{specializationId}`**
  - **Description**: Edits an existing specialization for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Specializations.Edit`
  - **Request Parameters**:
    - `specializationId` (path): The protected ID of the specialization to edit.
  - **Request Body**: `SpecializationRequest`
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `specializationId` is invalid.

- **`DELETE /api/doctors/specializations/{specializationId}`**
  - **Description**: Removes a specialization from the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Specializations.Remove`
  - **Request Parameters**:
    - `specializationId` (path): The protected ID of the specialization to remove.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `specializationId` is invalid.

- **`POST /api/doctors`**
  - **Description**: Adds a new doctor to the system (Admin only).
  - **Authentication**: Required (Admin)
  - **Permissions**: `Doctors.Create`
  - **Request Body**: `DoctorRequest`
  - **Responses**:
    - `201 Created`: Doctor created successfully.

- **`POST /api/doctors/profile/complete`**
  - **Description**: Completes the profile for a newly registered doctor.
  - **Authentication**: Anonymous
  - **Request Parameters**:
    - `userId` (query): The protected ID of the user (doctor).
  - **Request Body**: `CompleteDoctorProfileRequest` (form data).
  - **Responses**:
    - `200 OK`: Profile completed successfully.
    - `400 Bad Request`: If `userId` is invalid.

- **`PUT /api/doctors/update-working-times/{clinicId}`**
  - **Description**: Updates the working times for a specific clinic of the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.UpdateWorkingTimesAsync`
  - **Request Parameters**:
    - `clinicId` (path): The protected ID of the clinic.
  - **Request Body**: `IEnumerable<WorkingTimeRequest>`.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `clinicId` is invalid.

- **`PUT /api/doctors/profile/update`**
  - **Description**: Updates the authenticated doctor's profile information.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.EditProfile`
  - **Request Body**: `DoctorProfileUpdateRequest`.
  - **Responses**:
    - `200 OK`: Profile updated successfully.

- **`DELETE /api/doctors/{doctorId}`**
  - **Description**: Deletes a doctor from the system (Admin only).
  - **Authentication**: Required (Admin)
  - **Permissions**: `Doctors.Delete`
  - **Request Parameters**:
    - `doctorId` (path): The protected ID of the doctor to delete.
  - **Responses**:
    - `204 No Content`: Doctor deleted successfully.
    - `400 Bad Request`: If `doctorId` is invalid.

- **`GET /api/doctors/{doctorId}/clinics/{clinicId}/appointments/{appointmentTypeId}/available-slots`**
  - **Description**: Retrieves available time slots for a doctor at a specific clinic and appointment type on a given day.
  - **Authentication**: Required
  - **Permissions**: `Doctors.ViewAvailableTimeSlots`
  - **Request Parameters**:
    - `doctorId` (path): Protected ID of the doctor.
    - `clinicId` (path): Protected ID of the clinic.
    - `appointmentTypeId` (path): Protected ID of the appointment type.
    - `selectedDay` (query): Day of the week (e.g., `Monday`, `Tuesday`).
  - **Responses**:
    - `200 OK`: Returns a list of `TimeSlot` objects.
    - `400 Bad Request`: If any provided ID is invalid.

- **`GET /api/doctors/awards`**
  - **Description**: Retrieves a paginated list of awards for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Awards.View`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<AwardResponse>`.

- **`POST /api/doctors/awards`**
  - **Description**: Adds a new award to the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Awards.Create`
  - **Request Body**: `AwardRequest`.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.

- **`PUT /api/doctors/awards/{awardId}`**
  - **Description**: Edits an existing award for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Awards.Edit`
  - **Request Parameters**:
    - `awardId` (path): The protected ID of the award to edit.
  - **Request Body**: `AwardRequest`.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `awardId` is invalid.

- **`DELETE /api/doctors/awards/{awardId}`**
  - **Description**: Removes an award from the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Awards.Remove`
  - **Request Parameters**:
    - `awardId` (path): The protected ID of the award to remove.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `awardId` is invalid.

- **`GET /api/doctors/educations`**
  - **Description**: Retrieves a paginated list of educational entries for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Educations.View`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<EducationResponse>`.

- **`POST /api/doctors/education`**
  - **Description**: Adds a new educational entry to the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Educations.Create`
  - **Request Body**: `EducationRequest` (form data).
  - **Responses**:
    - `200 OK`: Returns `true` if successful.

- **`PUT /api/doctors/education/{educationId}`**
  - **Description**: Edits an existing educational entry for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Educations.Edit`
  - **Request Parameters**:
    - `educationId` (path): The protected ID of the education entry to edit.
  - **Request Body**: `EducationRequest` (form data).
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `educationId` is invalid.

- **`DELETE /api/doctors/education/{educationId}`**
  - **Description**: Removes an educational entry from the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Educations.Remove`
  - **Request Parameters**:
    - `educationId` (path): The protected ID of the education entry to remove.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `educationId` is invalid.

- **`GET /api/doctors/experiences`**
  - **Description**: Retrieves a paginated list of experience entries for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Experiences.View`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<ExperienceResponse>`.

- **`POST /api/doctors/experience`**
  - **Description**: Adds a new experience entry to the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Experiences.Create`
  - **Request Body**: `ExperienceRequest` (form data).
  - **Responses**:
    - `200 OK`: Returns `true` if successful.

- **`PUT /api/doctors/experience/{experienceId}`**
  - **Description**: Edits an existing experience entry for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Experiences.Edit`
  - **Request Parameters**:
    - `experienceId` (path): The protected ID of the experience entry to edit.
  - **Request Body**: `ExperienceRequest` (form data).
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `experienceId` is invalid.

- **`DELETE /api/doctors/experience/{experienceId}`**
  - **Description**: Removes an experience entry from the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Experiences.Remove`
  - **Request Parameters**:
    - `experienceId` (path): The protected ID of the experience entry to remove.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `experienceId` is invalid.

- **`POST /api/doctors/clinics`**
  - **Description**: Adds a new clinic for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Clinics.Create`
  - **Request Body**: `ClinicRequest` (form data).
  - **Responses**:
    - `200 OK`: Returns `true` if successful.

- **`PUT /api/doctors/clinics/{clinicId}`**
  - **Description**: Edits an existing clinic for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Clinics.Edit`
  - **Request Parameters**:
    - `clinicId` (path): The protected ID of the clinic to edit.
  - **Request Body**: `ClinicRequest` (form data).
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `clinicId` is invalid.

- **`DELETE /api/doctors/clinics/{clinicId}`**
  - **Description**: Removes a clinic from the authenticated doctor's profile.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Clinics.Remove`
  - **Request Parameters**:
    - `clinicId` (path): The protected ID of the clinic to remove.
  - **Responses**:
    - `200 OK`: Returns `true` if successful.
    - `400 Bad Request`: If `clinicId` is invalid.

- **`GET /api/doctors/{doctorId}/clinics`**
  - **Description**: Retrieves a paginated list of clinics for a specific doctor (for patient view).
  - **Authentication**: Required
  - **Permissions**: `Clinics.View`
  - **Request Parameters**:
    - `doctorId` (path): The protected ID of the doctor.
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<ClinicResponse>`.
    - `400 Bad Request`: If `doctorId` is invalid.

- **`GET /api/doctors/clinics`**
  - **Description**: Retrieves a paginated list of clinics for the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Clinics.View`
  - **Request Parameters**:
    - `pageNumber` (query, optional): Page number for pagination (default: 1).
    - `pageSize` (query, optional): Number of items per page (default: 10).
  - **Responses**:
    - `200 OK`: Returns a `PaginatedResponse<ClinicResponse>`.

- **`GET /api/doctors/reviews/{doctorId}/average-rating`**
  - **Description**: Retrieves the average rating for a specific doctor.
  - **Authentication**: Required
  - **Permissions**: `Doctors.ViewAverageRating`
  - **Request Parameters**:
    - `doctorId` (path): The protected ID of the doctor.
  - **Responses**:
    - `200 OK`: Returns a `double` representing the average rating.
    - `400 Bad Request`: If `doctorId` is invalid.

- **`GET /api/doctors/analytics/total-patients`**
  - **Description**: Retrieves the total number of patients served by the authenticated doctor.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.ViewTotalPatientsServed`
  - **Responses**:
    - `200 OK`: Returns a `long` representing the total patient count.

- **`GET /api/doctors/earnings-report`**
  - **Description**: Retrieves an earnings report for the authenticated doctor within a specified date range.
  - **Authentication**: Required (Doctor)
  - **Permissions**: `Doctors.ViewEarningsReport`
  - **Request Parameters**:
    - `startDate` (query): Start date for the report.
    - `endDate` (query): End date for the report.
  - **Responses**:
    - `200 OK`: Returns a `DoctorEarningsResponse` object.



