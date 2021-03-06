namespace EventStore.PublicTypes

open System
open FsToolkit.ErrorHandling
open EventStore.PrivateTypes

type Stream = {
    StreamId : string
    Version : int32
    Name : string
    CreatedAt : DateTimeOffset
    UpdatedAt : DateTimeOffset Nullable }

[<RequireQualifiedAccess>]
module Stream =

    let fromEntity (entity : EventStore.DataAccess.Stream) = {
        StreamId = entity.StreamId
        Name = entity.Name
        Version = entity.Version
        CreatedAt = entity.CreatedAt
        UpdatedAt = entity.UpdatedAt
    }

type Event = {
    EventId : string
    StreamId : string
    Version : int32
    Data : string
    Type : string    
    CreatedAt : DateTimeOffset }

[<RequireQualifiedAccess>]
module Event =

    let fromEntity (entity : EventStore.DataAccess.Event) = {
        EventId = entity.EventId
        StreamId = entity.StreamId
        Version = entity.Version
        Type = entity.Type
        Data = entity.Data
        CreatedAt = entity.CreatedAt
    }

type Snapshot = {
    SnapshotId : string
    StreamId : string
    Version : int32
    Data : string
    Description : string    
    CreatedAt : DateTimeOffset }

[<RequireQualifiedAccess>]
module Snapshot =

    let fromEntity (entity : EventStore.DataAccess.Snapshot) = {
        SnapshotId = entity.SnapshotId
        StreamId = entity.StreamId
        Version = entity.Version
        Description = entity.Description
        Data = entity.Data
        CreatedAt = entity.CreatedAt
    }

type UnvalidatedNewEvent = {
    Data : string
    Type : string }

[<RequireQualifiedAccess>]
module UnvalidatedNewEvent =

    let validate (model : UnvalidatedNewEvent) : Result<NewEvent, string> = result {
        let! eventType =
            String256.tryCreate model.Type
            |> Result.requireSome "Event type is required and it must be at most 256 characters"
        
        let! data = 
            StringMax.tryCreate model.Data
            |> Result.requireSome "Event data is required"

        return { Type = eventType; Data = data }
    }

type UnvalidatedAppendEvents = {
    ExpectedVersion : int32
    StreamName : string
    Events : UnvalidatedNewEvent list }

[<RequireQualifiedAccess>]
module UnvalidatedAppendEvents =

    let validate (model : UnvalidatedAppendEvents) : Result<AppendEvents, string> = result {        
        let! streamName = 
            String256.tryCreate model.StreamName
            |> Result.requireSome "Stream name is required and it must be at most 256 characters"
        
        let! version =
            NonNegativeInt.tryCreate model.ExpectedVersion
            |> Result.requireSome "Invalid stream version"

        do! model.Events |> Result.requireNotEmpty "Cannot append to stream without events" 

        let! events = 
            model.Events 
            |> List.traverseResultM UnvalidatedNewEvent.validate
            
        return { StreamName = streamName; Events = events; ExpectedVersion = version }
    }

type UnvalidatedCreateSnapshot = {
    StreamName : string
    Description : string
    Data : string }

[<RequireQualifiedAccess>]
module UnvalidatedCreateSnapshot =

    let validate (model : UnvalidatedCreateSnapshot) : Result<CreateSnapshot, string> = result {
        let! streamName = 
            String256.tryCreate model.StreamName
            |> Result.requireSome "Stream name is required and it must be at most 256 characters"

        let! description =
            String256.tryCreate model.Description
            |> Result.requireSome "Snapshot description is required and it must be at most 256 characters"
        
        let! data = 
            StringMax.tryCreate model.Data
            |> Result.requireSome "Snapshot data is required"

        return { StreamName = streamName; Description = description; Data = data }
    }

type UnvalidatedStreamQuery = { StreamName : string }    


[<RequireQualifiedAccess>]
module UnvalidatedStreamQuery =

    let validate (model : UnvalidatedStreamQuery) : Result<StreamQuery, string> = result {
        let! streamName = 
            String256.tryCreate model.StreamName
            |> Result.requireSome "Stream name is required and it must be at most 256 characters"

        return { StreamName = streamName }
    }

type UnvalidatedSnapshotsQuery = { StreamName : string }

[<RequireQualifiedAccess>]
module UnvalidatedSnapshotsQuery =

    let validate (model : UnvalidatedSnapshotsQuery) : Result<SnapshotsQuery, string> = result {
        let! streamName = 
            String256.tryCreate model.StreamName
            |> Result.requireSome "Stream name is required and it must be at most 256 characters"

        return { StreamName = streamName }
    }

type UnvalidatedEventsQuery = { 
    StreamName : string
    StartAtVersion : int32 }

[<RequireQualifiedAccess>]
module UnvalidatedEventsQuery =

    let validate (model : UnvalidatedEventsQuery) : Result<EventsQuery, string> = result {
        let! streamName = 
            String256.tryCreate model.StreamName
            |> Result.requireSome "Stream name is required and it must be at most 256 characters"

        let! version =
            NonNegativeInt.tryCreate model.StartAtVersion
            |> Result.requireSome "Invalid stream version"

        return { StreamName = streamName; StartAtVersion = version }
    }