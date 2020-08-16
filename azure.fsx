#load @".paket/load/netcoreapp3.1/Farmer.fsx"

open Farmer
open Farmer.Builders
open System

open Farmer.CoreTypes
open Farmer.Arm

let servicePlan = ArmExpression.resourceId(Web.serverFarms, ResourceName "Default1", "Internal-CIT")

let app = webApp {
    name "whatnextfs"
    link_to_unmanaged_service_plan (servicePlan.Eval())
    app_insights_off
    zip_deploy "deploy"
}

let template = arm {
    location Location.NorthEurope
    add_resource app
}

template |> Deploy.execute "whatnext" []