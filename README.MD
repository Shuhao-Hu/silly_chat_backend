# Silly Chat - .NET Backend

Welcome to the Silly Chat backend! This is the backend service for the Silly Chat app, which provides real-time messaging, user authentication, and message storage. The backend is built with .NET and uses PostgreSQL as the database.

## Features

- **User Authentication**: JWT-based authentication (Access and Refresh tokens)
- **Real-Time Messaging**: WebSocket support for real-time chat
- **Message Storage**: Stores messages in a PostgreSQL database
- **Offline Message Delivery**: Ensures messages are delivered when the user is online

## Prerequisites

Before running the backend, make sure you have the following installed:

- [.NET SDK 8.0 or higher](https://dotnet.microsoft.com/download/dotnet)
- [PostgreSQL](https://www.postgresql.org/download/)