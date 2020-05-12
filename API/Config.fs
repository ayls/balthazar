namespace API

open System

module Config = 
    let getConnectionString =
        let connectionString = Environment.GetEnvironmentVariable("BookmarkCollectionConnectionString")
        connectionString
    let getPartitionKey =
        "30a70b65-bb88-4de7-82ad-6067d9acaf82" // using a hardcoded partition key, since the table does not have good candidates for a partition key


