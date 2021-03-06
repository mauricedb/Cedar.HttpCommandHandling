# Cedar HttpCommandHandling
[![Build status](https://ci.appveyor.com/api/projects/status/2p0cc1foi56t84jx/branch/master)](https://ci.appveyor.com/project/damianh/cedar-httpcommandhandling) [![NuGet Status](http://img.shields.io/nuget/v/Cedar.HttpCommandHandling.svg?style=flat)](https://www.nuget.org/packages/Cedar.HttpCommandHandling/) [![NuGet Status](http://img.shields.io/nuget/v/Cedar.HttpCommandHandling.Client.svg?style=flat)](https://www.nuget.org/packages/Cedar.HttpCommandHandling.Client/)

Owin Middleware for handling commands, typically used in CQRS applications. Commands are treated as resources in their own right and are PUT (i.e. `PUT http://example.com/commands/c9e714c8-c9c1-433b-bcc6-de971b384a03`) to encourage idempotent handling.

### Features
1. Simple way to wire up handlers for commands.
2. Easy way to create handler pipelines.
3. Strategies to support command versioning using Content-Type.
4. An optional .NET client library to facilitate simple command invocation.
5. Supports IETF HTTP Problem Details for errors (json only). Problem details are extendable and exceptions are re-raised when using .NET client. 
6. Simple to test without any enviromental dependencies.
7. Commands can be invoked embedded, in-mem and in-proc allowing the same pipeline to be invoked remote or embedded.
8. No dependencies!

### Getting started.

```CSharp
public class MyCommand {}

public class MyCommandModule : CommandHandlerModule
{
    public CommandModule()
    {
        For<MyCommand>()
            .Handle(commandMessage => /* handle */);
    }
}

public class Server
{
    static void Main()
    {
        var resolver = new CommandHandlerResolver(new CommandModule());
        var settings = new CommandHandlingSettings(resolver);
        var middleware = CommandHandlingMiddleware.HandleCommands(settings);
        
        Action<IAppBuilder> startup = (app) => app.Use(middleware);
        
        using(WebApp.Start("http://localhost:8080", startup))
        {
            Console.WriteLine("Press any key to exit");
        }
    }
}

/* In client code */
using(var client = new HttpClient(){ BaseAddress = new Uri("http://localhost:8080") })
{
    client.PutCommand(new MyCommand(), Guid.NewGuid());
}
```

See the [examples](https://github.com/damianh/Cedar.HttpCommandHandling/tree/master/src/Cedar.HttpCommandHandling.Example) for more advanced scenarios.
