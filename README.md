# 🏃 RaceDay - Event Management System
### PROG6212 - Programming 2B Portfolio of Evidence

[![CI Build](https://github.com/Liyema08/PROG6212-POE-Part1/actions/workflows/ci.yml/badge.svg)](https://github.com/Liyema08/PROG6212-POE-Part1/actions)

---

## 🎯 Project Overview

**RaceDay** is a comprehensive event management platform designed for the South African road running, walking, and cycling community.

This project is built across three parts:
- **Part 1:** System Planning (ERD, API Plan, SQL Script)
- **Part 2:** RESTful API (ASP.NET Core 10, EF Core, SQL Server)
- **Part 3:** MVC Web Application, Azure Blob Storage, and Docker

---

## 👥 User Roles

### 🏢 Organiser
- View a dashboard of created events
- Create, update, and delete events
- Upload event banner images to Azure Blob Storage
- Manage age/distance categories
- View participant enrolments
- Capture race results

### 🏃 Participant
- Browse upcoming events with filtering
- Enrol in events by selecting a category
- View personal enrolment history ("My Events")
- View personal race results ("My Results")

---

## 🚀 Technologies Used

- **Backend:** ASP.NET Core 10 Web API, Entity Framework Core (Code-First)
- **Frontend:** ASP.NET Core 10 MVC
- **Database:** SQL Server
- **Authentication:** Session-based with BCrypt password hashing
- **Cloud Storage:** Azure Blob Storage
- **Testing:** xUnit
- **DevOps:** GitHub Actions, Docker

---

## 🐳 Docker Setup

The MVC application is containerised using Docker. The `RaceDay.Api` must be running on the host machine to provide data to the MVC container.

### 1. Build the Docker Image
Run this command from the solution root:
```bash
docker build -f RaceDay.Mvc/Dockerfile -t raceday-mvc:part3 .
2. Run the Docker Container
Make sure the API is running locally (dotnet run --project RaceDay.API on port 5194). Then run:

bash
docker run -d -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5194 raceday-mvc:part3
3. Access the Application
Open your browser and go to:

text
http://localhost:8080
🛠️ How to Run Locally
Prerequisites
.NET 10 SDK

SQL Server (LocalDB or full instance)

Running the API
bash
dotnet run --project RaceDay.API
(Runs on http://localhost:5194)

Running the MVC App
In a new terminal:

bash
dotnet run --project RaceDay.Mvc
(Runs on http://localhost:5022)

Demo Credentials
Role	Email	Password
Organiser	organiser1@raceday.com	Test@123
Participant	testuser@raceday.com	Test@123
🧪 Testing
Run the unit tests using:

bash
dotnet test
📹 Video Presentation
https://youtu.be/64tRyv7sl8Q

👤 Student Information
Name	Student Number	Module
Liyema Dlevu	ST10495510	PROG6212
