﻿module PredicateLanguageInteropTests

open Barb.Compiler

open Xunit

type DudeRecord = { Name: string; Sex: char }

type ParentWithObject = { State: string; Data: obj }

type DudeRecordWithInt = { Name: string; Age: int }
    with member t.GetAge() = t.Age

[<Fact>]
let ``predicate language should support dynamic property lookup on unknown types`` () =
    let childRec = { Name = "Dude Duderson"; Age = 20 }
    let parentRec = { State = "Washington"; Data = childRec :> obj }
    let dudePredicate = buildExpr<ParentWithObject,bool> "Data.Name = \"Dude Duderson\" and Data.Age < 30"
    let result = dudePredicate parentRec
    Assert.True(result)

[<Fact>]
let ``predicate language should support dynamic method lookup on unknown types`` () =
    let childRec = { Name = "Dude Duderson"; Age = 20 }
    let parentRec = { State = "Washington"; Data = childRec :> obj }
    let dudePredicate = buildExpr<ParentWithObject,bool> "Data.Name = \"Dude Duderson\" and Data.GetAge() < 30"
    let result = dudePredicate parentRec
    Assert.True(result)

[<Fact>] 
let ``predicate language should support no argument methods`` () =
    let testRecord = { Name = " Dude Duderson "; Age = 20 }
    let dudePredicate = buildExpr<DudeRecordWithInt,bool> "Name.Trim() = \"Dude Duderson\""
    let result = dudePredicate testRecord
    Assert.True(result)

[<Fact>] 
let ``predicate language should support no argument methods on passed in constructs`` () =
    let testRecord = { Name = " Dude Duderson "; Age = 20 }
    let dudePredicate = buildExpr<DudeRecordWithInt,bool> "GetAge() = 20"
    let result = dudePredicate testRecord
    Assert.True(result)

[<Fact>] 
let ``predicate language should support single argument methods`` () =
    let testRecord = { Name = "Dude Duderson"; Age = 20 }
    let dudePredicate = buildExpr<DudeRecordWithInt,bool> "Name.Contains(\"Dude Duderson\")"
    let result = dudePredicate testRecord
    Assert.True(result)

[<Fact>] 
let ``predicate language should support multi-argument methods`` () =
    let testRecord = { Name = "Dude Duderson"; Age = 20 }
    let dudePredicate = buildExpr<DudeRecordWithInt,bool> "Name.Substring(0, 4) = \"Dude\""
    let result = dudePredicate testRecord
    Assert.True(result)

[<Fact>]
let ``predicate language should support invoking a method on the results of a method call`` () =
    let testRecord = { Name = "Dude Duderson"; Age = 20 }
    let dudePredicate = buildExpr<DudeRecordWithInt,bool> "Name.Substring(0, 4).Length = 4"
    let result = dudePredicate testRecord
    Assert.True(result)

[<Fact>]
let ``predicate language should support invoking a method on the results of a subexpression`` () =
    let testRecord = { Name = "Dude Duderson"; Age = 20 }
    let dudePredicate = buildExpr<DudeRecordWithInt,bool> "(Name.Substring(0, 4)).Length = 4"
    let result = dudePredicate testRecord
    Assert.True(result)

type PropIndexerTester<'a,'b when 'a : comparison> (map: Map<'a,'b>) = 
    member this.Item
        with get(indexer: 'a) : 'b = map |> Map.find indexer

type IndexerRecord<'a,'b when 'a : comparison> = 
    {
        Name: string
        Table: PropIndexerTester<'a,'b>
    }

[<Fact>]
let ``predicate language should support property indexers`` () = 
    let testRecord = { Name = "Dude Duderson"; Table = new PropIndexerTester<int,int>([0..5] |> List.map (fun i -> i, i) |> Map.ofList) }
    let dudePredicate = buildExpr<IndexerRecord<int,int>,bool> "Table.Item[0] = 0"
    let result = dudePredicate testRecord
    Assert.True(result)    

[<Fact>]
let ``predicate language should support property indexers with strings`` () = 
    let testRecord = { Name = "Dude Duderson"; Table = new PropIndexerTester<string,string>(["one"; "two"; "three"] |> List.map (fun i -> i, i) |> Map.ofList) }
    let dudePredicate = buildExpr<IndexerRecord<string,string>,bool> "Table.Item[\"two\"] = \"two\""
    let result = dudePredicate testRecord
    Assert.True(result)    