namespace FsFinders

module Finders = 
    open System
    open System.Collections
    open System.Collections.Generic
    type FinderScore(criticalHits : int, criticalMisses : int, normalHits : int, optionalHits : int, totalNumberOfTests) = 
        member this.CriticalHits = criticalHits
        member this.CriticalMisses = criticalMisses 
        member this.StandardHits = normalHits
        member this.OptionalHits = optionalHits
        member this.NumberOfTests = totalNumberOfTests
        
        member this.AddCriticalHits(numberOfHits : int) =
            new FinderScore(this.CriticalHits + numberOfHits, this.CriticalMisses, this.StandardHits, this.OptionalHits, this.NumberOfTests + numberOfHits)        
                        
        member this.AddNormalHits(numberOfHits : int) = 
            new FinderScore(this.CriticalHits, this.CriticalMisses, this.StandardHits + numberOfHits, this.OptionalHits, this.NumberOfTests + numberOfHits)
            
        member this.AddOptionalHits(numberOfHits : int) = 
            new FinderScore(this.CriticalHits, this.CriticalMisses, this.StandardHits, this.OptionalHits + numberOfHits, this.NumberOfTests + numberOfHits)
        
        member this.AddCriticalMiss(numberOfMisses : int) =            
            new FinderScore(this.CriticalHits, this.CriticalMisses + numberOfMisses, this.StandardHits, this.OptionalHits, this.NumberOfTests + numberOfMisses)
        
        member this.Confidence = 
            if this.CriticalMisses > 0 || this.NumberOfTests = 0 then
                None
            else
                let standardHits = float this.StandardHits
                let optionalHits = float this.OptionalHits
                let numberOfTests = float this.NumberOfTests
                (standardHits + optionalHits) / numberOfTests |> Some       
            
    type FuzzyItem<'a>(item : 'a, score : FinderScore option) = 
        member this.Item = item
        member this.Score = match score with
                                | Some s -> s
                                | _ -> new FinderScore(0,0,0,0,0) 
    
    let ToWeightedList<'a>(items : 'a seq) = items |> Seq.map (fun item -> new FuzzyItem<_>(item, None))
    let GetScore<'a>(item : FuzzyItem<'a>) = 
        item.Score
        
    let Unwrap<'a>(items : FuzzyItem<'a> seq) =
        items |> Seq.map(fun item->item.Item)             
        
module Criteria =
    let Critical<'a>(test : 'a -> bool) (item : Finders.FuzzyItem<'a>) = 
        let score = item.Score                                   
        if test(item.Item) then            
            new Finders.FuzzyItem<_>(item.Item, score.AddCriticalHits(1) |> Some)        
        else
            let newScore = score.AddCriticalMiss(1) |> Some
            new Finders.FuzzyItem<_>(item.Item, newScore)
            
    let Standard<'a>(test : 'a -> bool) (item : Finders.FuzzyItem<'a>) =
        let score = item.Score                   
        
        if test(item.Item) then            
            new Finders.FuzzyItem<_>(item.Item, score.AddNormalHits(1) |> Some)        
        else
            let newScore = new Finders.FinderScore(score.CriticalHits, score.CriticalMisses, score.StandardHits, score.OptionalHits, score.NumberOfTests + 1) |> Some
            new Finders.FuzzyItem<_>(item.Item, newScore)
            
    let Optional<'a>(test : 'a -> bool) (item : Finders.FuzzyItem<'a>) = 
        let score = item.Score                                   
        if test(item.Item) then            
            new Finders.FuzzyItem<_>(item.Item, score.AddOptionalHits(1) |> Some)        
        else
            item

module FilterBy =
    let Critical<'a>(test : 'a -> bool) (items : Finders.FuzzyItem<'a> seq) = 
            items |> Seq.map(fun item -> Criteria.Critical<_> test item)
            
    let Standard<'a>(test : 'a -> bool) (items : Finders.FuzzyItem<'a> seq) = 
        items |> Seq.map(fun item -> Criteria.Standard<_> test item)
    
    let Optional<'a>(test : 'a -> bool) (items : Finders.FuzzyItem<'a> seq) = 
            items |> Seq.map(fun item -> Criteria.Optional<_> test item)
    
    let Confidence<'a>(threshold : float) (items : Finders.FuzzyItem<'a> seq) = 
        let getConfidence (item : Finders.FuzzyItem<'a>) =
            let score = item.Score
            let confidence = match score.Confidence with
                                | Some c -> c
                                | None -> 0.0
            confidence
            
        items |> Seq.filter(fun item->getConfidence item >= threshold)