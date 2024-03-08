// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Modules;
using Microsoft.AspNetCore.Mvc;

namespace Module.SystemModeler.Modules;

public class TestModule: IModule
{
    public IServiceCollection RegisterModule(IServiceCollection builder)
    {
        builder.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        return builder;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("user", () => Results.Ok(new User("Brandon Johnson"))).Produces<User>();

        app.MapGet("special", async (HttpRequest request, HttpResponse response, CancellationToken token) =>
        {
            await response.WriteAsJsonAsync(request.Query, cancellationToken: token);
        }).Produces<IQueryCollection>();

        app.MapPost("extra/{year:int}", (int year, [FromQuery(Name = "a")]int age, [FromHeader]string accept, User user, IDateTimeProvider provider) => new
        {
            year, age, accept, user, provider.Now
        });
        return app;
    }
    
    
}

public record User(string FullName);