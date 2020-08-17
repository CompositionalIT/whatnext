namespace Shared

open System

type Contact =
    | Twitter of handle:string
    | Email of address:string

type Skill = Skill of string

type Difficulty =
    | Beginner | Average | Expert
    static member Parse = function
        | "Beginner" -> Beginner
        | "Average" -> Average
        | "Expert" -> Expert
        | v -> failwithf "Unknown difficulty %s" v

type Project =
    { Name : string
      Description : string
      Repository : string
      Contact : Contact
      Difficulty : Difficulty
      Skills : Skill list }

type Todo =
    { Id : Guid
      Description : string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type IProjectApi =
    { getProjects : unit -> Project list Async

    }
