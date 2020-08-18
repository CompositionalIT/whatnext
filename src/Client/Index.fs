module Index

open Elmish
open Fable.Remoting.Client
open Shared
open Fable.FontAwesome

type Model =
    { AllProjects: Project list
      DisplayedProjects : Project list
      Input: string }

type SearchTerm = Wildcard | UserTerm of string
type Msg =
    | LoadedProjects of Project list
    | Search of SearchTerm

let projectApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IProjectApi>

let init() =
    let model =
        { AllProjects = []
          DisplayedProjects = []
          Input = "" }
    model, Cmd.OfAsync.perform projectApi.getProjects () LoadedProjects

let searchProjects term projects =
    match term with
    | Wildcard ->
        projects
    | UserTerm term ->
        let terms = term.ToLower().Split ' ' |> List.ofArray
        let isMatch project term =
            project.Name.ToLower().Contains term
            || project.Description.ToLower().Contains term
            || (string project.Difficulty).ToLower().Contains term
            || project.Skills |> List.exists(fun skill -> (string skill).ToLower().Contains term)
        projects
        |> List.filter(fun project ->
            terms
            |> List.forall(fun term -> isMatch project term))
    |> List.sortBy(fun r -> r.Name)
    |> List.truncate 10

let update msg model =
    match msg with
    | LoadedProjects projects ->
        { model with AllProjects = projects }, Cmd.ofMsg (Search Wildcard)
    | Search value ->
        { model with
            DisplayedProjects = model.AllProjects |> searchProjects value }, Cmd.none

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
                Input.search [ Input.Placeholder "Search for a project!"; Input.OnChange (fun e -> e.Value |> UserTerm |> Search |> dispatch) ]
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
                    for project in model.DisplayedProjects do
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

