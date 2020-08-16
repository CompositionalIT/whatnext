module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Fable.FontAwesome

type Model =
    { Projects: Project list
      Input: string }

type Msg =
    | UpdateModel of (Model -> Model)
    | SetInput of string

let projectApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IProjectApi>

let init() =
    let model =
        { Projects = []
          Input = "" }
    let cmd = Cmd.OfAsync.perform projectApi.getProjects () (fun projects model -> { model with Projects = projects })
    model, Cmd.map UpdateModel cmd

let update msg model =
    match msg with
    | UpdateModel update ->
        update model, Cmd.none
    | SetInput value ->
        { model with Input = value }, Cmd.none

open Fable.React
open Fable.React.Props
open Fulma

let navBrand =
    Navbar.Brand.div [ ] [
        Navbar.Item.a [
            Navbar.Item.Props [ Href "https://safe-stack.github.io/" ]
            Navbar.Item.IsActive true
        ] [
            img [
                Src "/favicon.png"
                Alt "Logo"
            ]
        ]
    ]

let containerBox model dispatch =
    Box.box' [ ] [
        Content.content [ ] [
            Control.div [ Control.HasIconLeft ] [
                Input.search [ Input.Placeholder "Search for a project!" ]
                Icon.icon [ Icon.IsLeft ] [
                    Fa.i [ Fa.Solid.Search ] []
                ]
            ]
            Table.table [ ] [
                thead [] [
                    tr [] [
                        th [ Style [ Width 150 ] ] [ str "Project" ]
                        th [] [ str "Level" ]
                        th [] [ str "Skills" ]
                        th [ Style [ ] ] [ str "Description" ]
                        th [] [ str "Contact" ]
                    ]
                ]
                tbody [] [
                    for project in model.Projects do
                        tr [] [
                            td [] [ strong [] [ a [ Href project.Repository ] [ str project.Name ] ] ]
                            td [] [
                                Tag.tag [
                                    let color =
                                        match project.Difficulty with
                                        | Beginner -> Color.IsSuccess
                                        | Average -> Color.IsWarning
                                        | Expert -> Color.IsDanger
                                    Tag.Color color
                                    ] [
                                    str (string project.Difficulty) ]
                                ]
                            td [] [
                                Tag.list [] [
                                    for (Skill skill) in project.Skills do
                                        Tag.tag [ Tag.Color Color.IsInfo ] [ str skill ]
                                ]
                            ]
                            td [] [ str project.Description ]
                            td [] [
                                match project.Contact with
                                | Twitter account ->
                                    a [ Href ("https://twitter.com/" + account) ] [
                                        span [] [ Fa.i [ Fa.Brand.Twitter ] [] ]
                                    ]
                                | Email account ->
                                    a [ Href ("mailto:" + account) ] [
                                        span [] [ Fa.i [ Fa.Solid.Envelope ] [] ]
                                    ]
                            ]
                        ]
                ]
            ]
        ]
    ]

let view (model : Model) (dispatch : Msg -> unit) =
    Hero.hero [
        Hero.Color IsPrimary
        Hero.IsFullHeight
        Hero.Props [
            Style [
                Background """linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.5)), url("https://unsplash.it/1200/900?random") no-repeat center center fixed"""
                BackgroundSize "cover"
            ]
        ]
    ] [
        Hero.head [ ] [
            Navbar.navbar [ ] [
                Container.container [ ] [ navBrand ]
            ]
        ]

        Hero.body [ ] [
            Container.container [ ] [
                Heading.p [ Heading.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ] [ str "whatnext" ]
                containerBox model dispatch
            ]
        ]
    ]

