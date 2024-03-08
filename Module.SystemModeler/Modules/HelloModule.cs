// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.

using Econolite.Ode.Modules;

namespace Module.SystemModeler.Modules;

public class HelloModule: IModule
{
    public IServiceCollection RegisterModule(IServiceCollection builder)
    {
        return builder;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("hello", () => "hello world");

        endpoints.MapGet("helloobj", () => new
        {
            Message = "Hello World"
        });
        
        return endpoints;
    }
}