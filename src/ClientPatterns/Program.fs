// Learn more about F# at http://fsharp.org

open System
open System.Net.Http
open Newtonsoft.Json
open System.Text

[<CLIMutable>]
type User =
    {
        Username : string
        Password : string
    }

let getAsync (client:HttpClient) (url:string) = 
    async {
        let! response = client.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

let postAsync (client:HttpClient) (url:string) (user:User) =
    async {
        let json = JsonConvert.SerializeObject(user)
        use content = new StringContent(json, Encoding.UTF8, "application/json")
        let! response = client.PostAsync(url, content) |> Async.AwaitTask
        let! body = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return match response.IsSuccessStatusCode with 
        | true -> Ok body
        | false -> Error body
    }

let runExamples =
    async {
        let url = "http://localhost:5000/users"
        use httpClient = new System.Net.Http.HttpClient()
        let! user = 
            getAsync httpClient url
        printfn "Returned User: %s" user

        let newUser = { Username = "Username2"; Password = "SecretSecret" }
        let! userResult = postAsync httpClient url newUser

        match userResult with
        | Ok username -> printfn "Created Username: %s" username
        | Error err -> printfn "Failed request: %s" err
    }
    

[<EntryPoint>]
let main argv =
    runExamples
    |> Async.RunSynchronously

    0
