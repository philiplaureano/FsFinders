namespace Tests
open System
open FSharp.Core
open FSharp.Collections 
open NUnit.Framework
open FsFinders
open FsFinders.Finders

module FinderTests = 
    
    [<Test>]
    let ``The score count should increase by one for normal matching criteria``() =
        let item = new FuzzyItem<_>(12345, None)
        let score = item |> Criteria.Standard(fun item->true) |> Finders.GetScore   
        
        Assert.AreEqual(1, score.StandardHits)
        Assert.AreEqual(1, score.NumberOfTests)

    [<Test>]
    let ``The score count should increase by one for optional matching criteria``() =
        let item = new FuzzyItem<_>(12345, None)
        let score = item |> Criteria.Optional(fun item->true) |> Finders.GetScore   
        
        Assert.AreEqual(1, score.OptionalHits)
        Assert.AreEqual(1, score.NumberOfTests)
        
    [<Test>]
    let ``The number of tests should not increase if an optional criteria fails``() =
        let item = new FuzzyItem<_>(12345, None)
        let score = item |> Criteria.Optional(fun item->false) |> Finders.GetScore   
        
        Assert.AreEqual(0, score.OptionalHits)
        Assert.AreEqual(0, score.StandardHits)
        Assert.AreEqual(0, score.CriticalHits)
        Assert.AreEqual(0, score.CriticalMisses)
        Assert.AreEqual(0, score.NumberOfTests)
                
    [<Test>]
    let ``The score count should increase by one for critical matching criteria``() =
        let item = new FuzzyItem<_>(12345, None)
        let score = item |> Criteria.Critical(fun item->true) |> Finders.GetScore  
        
        Assert.AreEqual(1, score.CriticalHits)
        Assert.AreEqual(1, score.NumberOfTests)
        
    [<Test>]
    let ``It should record a critical miss if a given critical criteria filter fails``() = 
        let item = new FuzzyItem<_>(12345, None)
        let score = item |> Criteria.Critical(fun item->false) |> Finders.GetScore  
        
        Assert.AreEqual(1, score.CriticalMisses)
        Assert.AreEqual(1, score.NumberOfTests)
        
    [<Test>]
    let ``Should be able to convert any given list into a fuzzy item list``() =
        let intList = [|1 .. 100|]
        
        let mustBeFuzzyItem item =
            Assert.IsInstanceOf(typeof<FuzzyItem<int>>, item) 
        
        let convertedList = intList |> ToFuzzyList
        convertedList |> Seq.toArray |> Array.iter(mustBeFuzzyItem)                                          
    
