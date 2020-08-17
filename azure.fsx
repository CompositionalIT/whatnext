#load @".paket/load/netcoreapp3.1/Farmer.fsx"

open Farmer
open Farmer.Arm
open Farmer.Builders
open Farmer.CoreTypes
open System

let storage = storageAccount {
    name "whatnextfsstore"
}

let cosmos = cosmosDb {
    account_name "citstore"
    name "whatnextdb"
    free_tier
    add_containers [
        cosmosContainer {
            name "projects"
            partition_key [ "name" ] CosmosDb.IndexKind.Hash
        }
    ]
}

// Piggy back on an existing service plan in another resource group...
let servicePlan = ArmExpression.resourceId(Web.serverFarms, ResourceName "Default1", "Internal-CIT")

let app = webApp {
    name "whatnextfs"
    link_to_unmanaged_service_plan (servicePlan.Eval())
    setting "storage_key" storage.Key
    setting "cosmos_key" cosmos.PrimaryConnectionString
    app_insights_off
    zip_deploy "deploy"
    depends_on storage
    depends_on cosmos
}

let template = arm {
    location Location.NorthEurope
    add_resources [
        storage
        cosmos
        app
    ]
}

template |> Deploy.execute "whatnext" []