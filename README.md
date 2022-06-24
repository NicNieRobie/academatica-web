# Academatica Web Services
Web services for Academatica - a school mathematics learning application for iOS, intended for use by school students of all ages.

## Main features
- Interactive math lessons, including theory and problems for solving
- Custom practice mode with topics chosen by user
- Progress tracking and achievement system
- Leaderboards and leagues with rewards for progress

## Architecture and technology stack
Web services are built on ASP.NET Core for request handling endpoints implementation and utilize PostgreSQL and Redis databases for data storage and management.
The microservice architecture is used to separate independent implementation modules, with communication between services implemented via gRPC and RabbitMQ.

## Developers
- **NicNieRobie** - back-end services development & deployment, database design
