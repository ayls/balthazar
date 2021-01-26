namespace API

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.Extensions.Logging
open AzureFunctions.Extensions.Swashbuckle.Attribute
open AzureFunctions.Extensions.Swashbuckle
open System.Net.Http

module Swagger =
    [<SwaggerIgnore()>]
    [<FunctionName("Swagger")>]
    let Run ([<HttpTrigger(AuthorizationLevel.Anonymous, [|"get"|])>] req: HttpRequestMessage) ([<SwashBuckleClient()>] swashBuckleClient: ISwashBuckleClient) (log: ILogger) = 
        swashBuckleClient.CreateSwaggerDocumentResponse(req, "BalthazarAPI")