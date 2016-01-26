namespace Twift

open System
open System.Linq
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Runtime.Serialization
open System.Xml
open CoreTweet
open Twift.FileTransfer
open Mono.Options


module Main =
  begin
    
    let getTokens () =
      let unix = (Environment.OSVersion.Platform = PlatformID.Unix || Environment.OSVersion.Platform = PlatformID.MacOSX)
      let tf = if unix then ".twtokens" else "twtokens.xml"
      let home = if unix then Environment.GetEnvironmentVariable ("HOME") else Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), "csharp")
      let tpath = Path.Combine (home, tf)
      Directory.CreateDirectory home |> ignore
      let x = new DataContractSerializer (typeof<string[]>)
  
      if (File.Exists tpath) then
        use y = XmlReader.Create tpath in
          let ss = x.ReadObject y :?> string [] in
          Tokens.Create(ss.[0], ss.[1], ss.[2], ss.[3])
      else
        let se = OAuth.Authorize ("JuTftntaYp9ey92DDYmZNBwYy", "Tlk25t18SYj9Ib9aYgIiRsC32CbO2lljJiv91g4HdeyhgP9olm") in
        Console.WriteLine ("Open: " + se.AuthorizeUri.ToString ())
        Console.Write "PIN> "
        let g = se.GetTokens (Console.ReadLine()) in
        let s = XmlWriterSettings () in
        s.Encoding <- System.Text.UTF8Encoding false
        use y = XmlWriter.Create (tpath, s) in
            x.WriteObject (y, [| g.ConsumerKey; g.ConsumerSecret; g.AccessToken; g.AccessTokenSecret |])
        g

    let (|Regex|_|) pattern input =
      let m = Regex.Match(input, pattern)
      if m.Success then Some(List.tail [ for g in m.Groups -> g.Value ])
      else None

    let main argv =
      let o = new OptionSet () in
      o.Add("d=|dm=", "Send or receive file by DM to/from specified user.", (fun s -> 
        Settings.isDm := true; Settings.dmTarget := s
      )) |> ignore
      o.Add("r=|receive=", "Receive file specified by two ids and write to FILE. (two comma-separated ids, no spaces)", (fun s -> 
        Settings.isReceive := true; 
        Settings.receiveIds := match s with
                                 | Regex "(\d+),(\d+)" [i; j] ->
                                    let is = [Int64.Parse i; Int64.Parse j] |> List.sort in
                                    (is.[0], is.[1])
                                 | _ -> Exception "Ids must be comma-separated and no spaces." |> raise
      )) |> ignore
      o.Add("q|quiet", "DOES NOT show detailed log.", (fun _ -> Settings.isVerbose := false)) |> ignore
      o.Add("n|noshare", "DOES NOT tweet or DM two ids to specify the sent file.", (fun _ -> Settings.doesShare := false)) |> ignore
      o.Add("t|setup", "Setup Twitter account.", (fun _ -> getTokens () |> ignore)) |> ignore

      let fs = o.Parse argv in

      if fs.Count = 0 then
        printfn "Usage: twift [OPTIONS] FILE"
        printfn ""
        printfn "Share FILE by Tweet. Filename will be omitted to a.[extension] when -d is not specified."
        printfn ""
        printfn "Options:"
        o.WriteOptionDescriptions(Console.Out);
        0
      else 
        let f = Seq.head fs in
        if (!Settings.isReceive || File.Exists f) |> not then
          Exception (sprintf "twift: file %s not found" f) |> raise
        let t = getTokens () in
        match (!Settings.isReceive, !Settings.isDm) with
          | (true, true) ->
            let bs = receiveByDm t !Settings.receiveIds in
            File.WriteAllBytes(f, bs)
            printfn "%i bytes written: %s" bs.Length f

          | (false, true) ->
            let bs = File.ReadAllBytes f in
            let (i1, i2) = sendByDm t !Settings.dmTarget bs in
            if !Settings.doesShare then
              let f = Path.GetFileName f in
              let un = t.Account.VerifyCredentials().ScreenName in
              t.DirectMessages.New(!Settings.dmTarget, (sprintf "Just shared %s by @tw1ft! $ twift -d %s -r %i,%i %s to receive." f un i1 i2 f)) |> ignore 
            printfn "Completed: %i,%i" i1 i2

          | (_, false) -> 
            printfn "Sorry, this feature is disabled until 10000 charactors tweets become available.";
            printfn "Use -d instead."

          | (true, false) ->
            let bs = receiveByTweet t !Settings.receiveIds in
            File.WriteAllBytes(f, bs)
            printfn "%i bytes written: %s" bs.Length f
           
          | (false, false) ->
            let bs = File.ReadAllBytes f in
            let (i1, i2) = sendByTweet t bs in
            if !Settings.doesShare then
              let f = Path.GetExtension f in
              t.Statuses.Update(status= sprintf "Just shared a%s by @tw1ft! $ twift -r %i,%i a.%s to receive." f i1 i2 f) |> ignore 
            printfn "Completed: %i,%i" i1 i2

        0

    [<EntryPoint>]
    let ep argv =
      //main argv
      try main argv with | e -> printfn "%s" e.Message; -1

end