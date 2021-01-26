namespace API

open AzureFunctions.Extensions.Swashbuckle
open System.Reflection
open Microsoft.Azure.Functions.Extensions.DependencyInjection
open AzureFunctions.Extensions.Swashbuckle.Settings
open System

module Startup =
    type Startup () =
        inherit FunctionsStartup ()
            override __.Configure(builder: IFunctionsHostBuilder ) =
                let swaggerDocument = new SwaggerDocument (Name = "BalthazarAPI", Title = "Balthazar API" )                    
                let configureAction (options: SwaggerDocOptions) = 
                    options.Documents <- [ swaggerDocument ]
                builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), Action<SwaggerDocOptions> configureAction)
                |> ignore

    [<assembly: FunctionsStartup(typeof<Startup>)>]
    do ()