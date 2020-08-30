namespace API

open AzureFunctions.Extensions.Swashbuckle
open System.Reflection
open Microsoft.Azure.WebJobs.Hosting
open Microsoft.Azure.Functions.Extensions.DependencyInjection
open AzureFunctions.Extensions.Swashbuckle.Settings
open System

module Startup =
    type WebJobsExtensionStartup () =
        inherit FunctionsStartup ()
            override __.Configure(builder: IFunctionsHostBuilder ) =
                let swaggerDocument = new SwaggerDocument (Title = "Balthazar API" )                    
                let configureAction (options: SwaggerDocOptions) = 
                    options.Documents <- [ swaggerDocument ]
                builder.AddSwashBuckle(Assembly.GetExecutingAssembly(), Action<SwaggerDocOptions> configureAction)
                |> ignore

    [<assembly: WebJobsStartup(typeof<WebJobsExtensionStartup>)>]
    do ()