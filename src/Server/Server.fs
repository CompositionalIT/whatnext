module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
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

let projectsApi =
    { getProjects = fun () -> async { return [
        makeProject "What next"
                    "This is a SAFE Stack data entry app that records what projects people could use help with."
                    "https://github.com/compositionalit/whatnext"
                    (Twitter "@compositionalit")
                    Beginner
                    [ "SAFE Stack" ]
        makeProject "Farmer"
                    "A library to generate ARM templates for Azure deployments using an F# DSL."
                    "https://github.com/compositionalit/farmer"
                    (Email "isaac@compositional-it.com")
                    Average
                    [ "F#"; "Azure"; "API design" ]
        makeProject "SAFE Template"
                    "The actual template for the SAFE Stack."
                    "https://github.com/safe-stack/safe-template"
                    (Twitter "@safe_stack")
                    Expert
                    [ "dotnet"; "SAFE Stack"; "webpack"; "FAKE" ]
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
