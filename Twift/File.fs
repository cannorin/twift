namespace Twift

open System    
open System.IO
open System.Text
open System.Collections.Generic
open System.Linq
open CoreTweet
open Twift.Base4225

module FileTransfer =
  begin
    
    let toLines (bs : byte []) i : string seq =
      if !Settings.isVerbose then printfn "Preparing %i bytes..." bs.Length
      let s = b2k bs in
      let ln = s.Length / 2 in
      let cs = List.map (fun i -> String([|s.[i * 2 - 2]; s.[i * 2 - 1]|])) [1..ln] in
      Seq.chunkBySize i cs |> Seq.map String.Concat

    let mapv f (s : string seq) =
      let l = s.Count () in
      Seq.mapi (fun i x -> 
        if !Settings.isVerbose then printfn "Sending %i/%i ..." (i + 1) l
        f x
      ) s
          
    let sendByTweet (t : Tokens) bs =
      let ls = toLines bs (140 - 7) in
      let is = mapv (fun s -> (t.Statuses.Update(status = "@tw1ft " + s)).Id) ls |> Seq.toArray in
      (Seq.head is, Seq.last is)

    let sendByDm (t : Tokens) (scrName : string) bs =
      let ls = toLines bs 10000 in
      let is = mapv (fun s -> (t.DirectMessages.New(scrName, s)).Id) ls |> Seq.toArray in
      (Seq.head is, Seq.last is)

    let genParams ids =
      let (id1, id2) = ids in
      let d = new Dictionary<string, obj> () in
      d.Add ("since_id", id1 - 1L)
      d.Add ("max_id", id2)
      d.Add ("count", 200)
      d
    
    let receiveByDm (t : Tokens) (ids : (int64 * int64)) =
      let (id1, id2) = ids in
      let u = t.DirectMessages.Show(id1).Sender.Id in
      let p = genParams ids in
      p.Add ("full_text", true)
      if !Settings.isVerbose then printfn "Fetching dms..."
      let dms = t.DirectMessages.Received(p)
                |> Enumerable.Reverse
                |> Seq.filter (fun d -> d.Sender.Id = u) 
                |> Seq.map (fun d -> d.Text)
                |> String.Concat in
      if !Settings.isVerbose then printfn "Decoding..."
      k2b dms

    let receiveByTweet (t : Tokens) (ids : (int64 * int64)) =
      let (id1, id2) = ids in
      let u = t.Statuses.Show(id1).User.Id in
      let p = genParams ids in
      p.Add("user_id", u.Value)
      if !Settings.isVerbose then printfn "Fetching tweets..."
      let tws = t.Statuses.UserTimeline(p)
                |> Enumerable.Reverse
                |> Seq.map (fun s -> s.Text.Replace("@tw1ft ", ""))
                |> String.Concat in
      if !Settings.isVerbose then printfn "Decoding..."
      k2b tws
  end
