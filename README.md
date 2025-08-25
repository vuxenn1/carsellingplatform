# Car Selling Platform

A fullstack web application designed for listing, browsing, and managing used car sales. This project features a complete user authentication system, detailed car listings with image galleries, a user-to-user offer system, notifications, and a comprehensive admin panel for site management.

The application is built with a C# and ASP.NET Core backend API, an Oracle database, and a vanilla JavaScript single-page application (SPA) style frontend.

---

## Key Features

* **User Authentication:** Secure user registration and login system with password hashing (BCrypt).
* **Car Listings:** Users can create, edit, and manage their car listings with detailed specifications.
* **Image Uploads:** Support for uploading multiple images for each car listing, stored securely.
* **Offer System:** Users can make, accept, or reject offers on car listings.
* **Notifications:** Automatic notifications are sent to users for key events, such as offer responses.
* **Favorites:** Users can save their favorite car listings for easy access.
* **Admin Panel:** A dedicated dashboard for administrators to manage all users and car listings on the platform.
* **Activity Logging:** Detailed logging for user, car, and offer activities, viewable by administrators.
* **File-Based Logging:** A separate, file-based logging system for monitoring application startup, shutdown, and security events.

---

## Technology Stack

### Backend

* **Framework:** ASP.NET Core 8
* **Language:** C#
* **Database:** Oracle Database 23ai
* **ORM (Object Relational Mapping):** Entity Framework Core
* **Authentication:** Password Hashing with BCrypt

### Frontend

* **Language:** JavaScript
* **Styling:** HTML and CSS (No frameworks)

---

## Project Structure

The repository is organized into three main directories:

* **/backend/**: Contains the C# ASP.NET Core all related project files (`.csproj`, controllers, services, models).
* **/frontend/**: Contains all the static web files, including HTML, CSS, and JavaScript for the client-side application.
* **/database/**: Contains the `.sql` scripts for creating the database tables, views, sequences, and triggers.

---

## Setup and Installation

To run this project locally, you will need the following prerequisites:

* .NET 8 SDK
* An Oracle Database instance
* Node.js and npm (for the frontend development server)
* A code editor like Visual Studio or VS Code

### 1. Database Setup

1. Connect to your Oracle database instance.
2. Execute the SQL scripts located in the `/database/` folder to create the necessary tables, views, and other database objects.

### 2. Backend Setup

1. Navigate to the `/backend/` directory.
2. Create a new file named `appsettings.Development.json`. This file will hold your secret credentials and will be ignored by Git.
3. Copy the content from `appsettings.json` into your new `appsettings.Development.json` file.
4. In `appsettings.Development.json`, fill in your actual Oracle **connection string** and a long, random **JWT secret key**.
5. In the terminal, run the following commands:
    ```bash
    dotnet restore
    dotnet run
    ```
6. The backend API should now be running on `http://localhost:2525`.

### 3. Frontend Setup

1. Navigate to the `/frontend/` directory.
2. If you are using a development server like `live-server`, simply start it from this directory.
    ```bash
    # If you don't have live-server, install it globally:
    # npm install -g live-server
    
    live-server --port=5500
    ```
3. Open your web browser and navigate to `http://127.0.0.1:5500` (or whichever port you are using).

The application should now be fully functional.
