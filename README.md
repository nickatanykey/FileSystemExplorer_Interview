# Filesystem Browsing Application

## Welcome to My Interview Project

Hello and welcome to the filesystem browsing application for my interview! This project is designed to provide a web interface for browsing and managing files on a server. This document outlines the technical and design approaches taken in the application.

## Technical Overview

### Backend - ASP.NET Web API

- **`IFileService` Interface**: Abstracts file system operations for modularity and testability.
- **`FileService` Implementation**: Manages file system interactions, using `System.IO.Abstractions`.
- **Controller Layer**: RESTful endpoints for file operations (browsing, uploading, deleting).
- **Asynchronous Programming**: Used extensively for IO-bound operations.

### Frontend - JavaScript/HTML/CSS

- **Single Page Application (SPA)**: For a seamless user experience.
- **Dynamic Content Rendering**: File and directory information rendered using JavaScript.
- **Event-Driven Updates**: UI updates in response to user actions without full page reloads.
- **Deep Linking**: Enables bookmarking and direct navigation to specific application states.

### Design Approach

- **Modularity**: Structured into distinct layers (presentation, business logic, data access).
- **Testability**: Dependency injection and abstraction of file system interactions.
- **Security**: Validation and sanitization to prevent common vulnerabilities.
- **Performance**: async operations and batched DOM updates for increased performance.

## Running the Application

- Ensure you have Visual Studio Installed
- Ensure you have .NET 4.8 installed
- clone the repo, open up the solution, and mash F5

---

Thank you for reviewing my application. I look forward to discussing it with you and receiving your feedback!
