namespace Tests
open System
open FSharp.Core
open FSharp.Collections 
open NUnit.Framework
open FsFinders
open FsFinders.Finders

module ScoreCalculationTests =
    [<Test>]
    let ``A single critical miss should get an automatic none score``() = 
        let score = new FinderScore(0,1,0,0,1)
        Assert.IsTrue(score.Confidence.IsNone)
    
    [<Test>]
    let ``The score should return a none confidence score by default if no tests are made``() =
        let score = new FinderScore(0,0,1,0,0)
        Assert.IsTrue(score.Confidence.IsNone)
    
    [<Test>]
    let ``The confidence score should reflect the number of standard hits divided by the total number of tests``() =
        let numberOfStandardHits = 10
        let numberOfTests = 100
        let score = new FinderScore(0, 0, numberOfStandardHits, 0, numberOfTests)
        Assert.AreEqual(Some 0.10, score.Confidence)
    
    [<Test>]
    let ``The confidence score should reflect the number of optional hits divided by the total number of tests``() =
        let numberOfOptionalHits = 42
        let numberOfTests = 100
        let score = new FinderScore(0, 0, 0, numberOfOptionalHits, numberOfTests)
        Assert.AreEqual(Some 0.42, score.Confidence)
        
    [<Test>]    
    let ``The confidence score should reflect the number of standard and optional hits divided by the total number of tests``() =
        let numberOfStandardHits = 10
        let numberOfOptionalHits = 10
        let numberOfTests = 100
        let score = new FinderScore(0, 0, numberOfStandardHits, numberOfOptionalHits, numberOfTests)
        Assert.AreEqual(Some 0.20, score.Confidence)