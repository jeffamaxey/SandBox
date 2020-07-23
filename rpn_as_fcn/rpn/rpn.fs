namespace RPN

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Microsoft.Extensions.Logging

open PolishCalculator


// $ curl -X POST http://localhost:7071/api/rpn -H 'Content-Type: application/json' -d '{"name":"3 4 + 2 *"}'

module rpn =
    // Define a nullable container to deserialize into.
    [<AllowNullLiteral>]
    type InputContainer() =
        member val Input = "" with get, set

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let Name = "input"

    let stack = EMPTY

    [<FunctionName("rpn")>]
    let run ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>]req: HttpRequest) (log: ILogger) =
        async {
            log.LogInformation("F# HTTP trigger function processed a request.")

            let inputOpt = 
                if req.Query.ContainsKey(Name) then
                    Some(req.Query.[Name].[0])
                else
                    None


            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() |> Async.AwaitTask

            let data = JsonConvert.DeserializeObject<InputContainer>(reqBody)

            let input =
                match inputOpt with
                | Some n -> n
                | None ->
                   match data with
                   | null -> ""
                   | nc -> nc.Input

            let responseMessage =             
                if (String.IsNullOrWhiteSpace(input)) then
                    """HTTP triggered function executed successfully. Pass a input in the query string as input=4 3 4 * / 2 -
                    or post json as
                    $ curl -X POST http://localhost:7071/api/rpn -H 'Content-Type: application/json' -d '{"input":"3 4 2 + * 5 /"}'"""
                else // PolishCalculator RPN
                    let stringList = words input
                    let result = RPN stringList stack
                    let x,_ = pop result
                    input + " = " + string x + ". HTTP triggered function executed successfully."

            return OkObjectResult(responseMessage) :> IActionResult
        } |> Async.StartAsTask