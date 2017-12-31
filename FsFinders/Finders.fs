namespace FsFinders

type FuzzyItem<'a>(item : 'a, score: float) = 
    member this.Item = item 
    member this.Score = score