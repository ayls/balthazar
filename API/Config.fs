namespace API

open System

module Config = 
    let getConnectionString =
        let connectionString = Environment.GetEnvironmentVariable("BookmarkCollectionConnectionString")
        connectionString
    let getPartitionKey =
        let partitionKey = Environment.GetEnvironmentVariable("BookmarkCollectionPartitionKey")
        partitionKey


