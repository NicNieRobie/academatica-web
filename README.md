# Academatica
School mathematics learning application for iOS, intended for use by school students of all ages.

## Main features (in progress)
- Interactive math lessons, including theory and problems for solving
- Custom practice mode with topics chosen by user
- Progress tracking and achievement system
- Leaderboards and leagues with rewards for progress

## Architecture and technology stack
The front-end is built in Swift according to the Clean Architecture & MVVM guidelines.

The back-end is build on ASP.NET Core for request handling endpoints implementation and utilizes PostgreSQL and Redis databases for data storage and management.
The microservice architecture is used to separate independent implementation modules, with communication between services implemented via gRPC and RabbitMQ.

## Developers
- **RomKhan** - front-end iOS client development
- **NicNieRobie** - back-end services development & database design
