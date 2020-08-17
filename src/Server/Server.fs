module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.Azure.Cosmos.Table
open Saturn

open Shared
open System

let makeProject name description repo contact difficulty skills =
    { Name = name
      Description = description
      Repository = repo
      Contact = contact
      Difficulty = difficulty
      Skills = List.map Skill skills }

//TODO: Replace with managed identity...
let storage =
    match Option.ofObj (System.Environment.GetEnvironmentVariable "storage_key") with
    | None -> CloudStorageAccount.DevelopmentStorageAccount
    | Some key -> CloudStorageAccount.Parse key
let projects =
    let projects =
        let tableClient = storage.CreateCloudTableClient()
        tableClient.GetTableReference "Projects"
    projects.CreateIfNotExists() |> ignore
    projects

let projectsApi =
    let (|Field|_|) field (row:DynamicTableEntity) =
        match row.Properties.TryGetValue field with
        | true, v -> Some (Field v.StringValue)
        | false, _ -> None

    { getProjects = fun () -> async { return [
        let query = projects.ExecuteQuery(TableQuery()) |> Seq.toArray
        for row in query do
            makeProject row.Properties.["Name"].StringValue
                        row.Properties.["Description"].StringValue
                        row.Properties.["Repository"].StringValue
                        (match row with
                        | Field "Twitter" v -> Twitter v
                        | Field "Email" v -> Email v
                        | _ -> failwith "Missing a contact method")
                        (Difficulty.Parse row.Properties.["Difficulty"].StringValue)
                        (row.Properties.["Skills"].StringValue.Split ',' |> Array.toList)
      ]}
    }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue projectsApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
