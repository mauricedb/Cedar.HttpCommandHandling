# Cedar HttpCommandHandling
[![Build status](https://ci.appveyor.com/api/projects/status/2p0cc1foi56t84jx/branch/master)](https://ci.appveyor.com/project/damianh/cedar-httpcommandhandling) [![NuGet Status](http://img.shields.io/nuget/v/Cedar.HttpCommandHandling.svg?style=flat)](https://www.nuget.org/packages/Cedar.HttpCommandHandling/) [![NuGet Status](http://img.shields.io/nuget/v/Cedar.HttpCommandHandling.Client.svg?style=flat)](https://www.nuget.org/packages/Cedar.HttpCommandHandling.Client/)

Owin Middleware for handling commands, typically used in CQRS applications.

### Features
 1. Simple way to wire up handlers for commands.
 2. Easy way to create handler pipelines.
 3. Commands are PUT encouraging idempotent handling.
 4. Strategies to support command versioning using Content-Type.
 5. An optional .NET client library to facilitate simple command invocation.
 6. Suppot for IETF HTTP Problem Details for exception handling
 7. Simple to test.
