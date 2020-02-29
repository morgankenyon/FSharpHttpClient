// Learn more about F# at http://fsharp.org

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http

[<CLIMutable>]
type User =
    {
        Username : string
        Password : string
    }

let getUser : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let user = { Username = "Test"; Password = "secret" }

            return! Successful.OK user next ctx
        }

let addUser =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let! user = ctx.BindJsonAsync<User>();

            return! Successful.OK user.Username next ctx
        }

let webApp =
    choose [
        GET >=>
            choose [
                route "/users" >=> getUser
            ]
        POST >=>
            choose [
                route "/users" >=> addUser
            ]
        setStatusCode 404 >=> text "Not Found" ]

let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    WebHostBuilder()
        .UseKestrel()
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .Build()
        .Run()
    0