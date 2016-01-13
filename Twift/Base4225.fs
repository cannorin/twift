namespace Twift

open System    
open System.IO
open System.Text
open System.Collections.Generic
open System.Linq

module Base4225 =
  begin

    let base64 = List.concat[['A'..'Z']; ['a'..'z']; ['0'..'9']; ['+'; '/'; '=']]

    let b2u b1 b2 =
      let i1 = List.findIndex ((=) b1) base64 in
      let i2 = List.findIndex ((=) b2) base64 in
      let offset = i2 + (i1 * 65) in
      let a = offset / (64 * 64) in
      let b = (offset - (a * 64 * 64)) / 64 in
      let c = (offset - (a * 64 * 64) - (b * 64)) in
      let x = [0xf0; 0xa0 + a; 0x80 + b; 0x80 + c] |> List.map (byte) |> Seq.toArray |> Encoding.UTF8.GetString in
      x

    let u2b (s : string) =
      let bs = Encoding.UTF8.GetBytes(s) in
      if BitConverter.ToInt32(bs |> Seq.rev |> Seq.toArray, 0) < 0xf0a08080 then
        ""
      else
        let i = ((int)bs.[1] - 0xa0) * 64 * 64
              + ((int)bs.[2] - 0x80) * 64
              + ((int)bs.[3] - 0x80) in
        let a = i / 65 in
        let b = i - a * 65 in
        let x = [base64.[a]; base64.[b]] |> String.Concat in
        x

    let b2k (bs : byte []) =
      let b64 = Convert.ToBase64String (bs) |> Seq.toArray in
      let ln = b64.Length / 2 in
      List.map (fun i -> b2u b64.[i * 2 - 2] b64.[i * 2 - 1]) [1..ln] |> String.Concat
 
    let k2b (s : string) =
      let ln = s.Length / 2 in
      let b64 = List.map (fun i -> String([|s.[i * 2 - 2]; s.[i * 2 - 1]|]) |> u2b) [1..ln] |> String.Concat in
      Convert.FromBase64String (b64)

    let main (argv : string []) = 
      let d = argv.Contains "-d" || argv.Contains "--decode" in
      let argv = Seq.filter (fun x -> not (x.Equals "-d" || x.Equals "--decode")) argv |> Seq.toArray
      if argv.Length > 0 then
        let fn = argv.[0] in
        try
          if File.Exists fn then
            let r = File.ReadAllBytes(fn) in
            if d then
              let bs = Encoding.UTF8.GetString(r) |> k2b in
              use stdout = Console.OpenStandardOutput(bs.Length) in
                stdout.Write(bs, 0, bs.Length)
              0
            else
              let s = b2k r in
              printf "%s" s
              0
          else
            printfn "%s" ("base4225: file " + fn + " not found")
            1 
        with
          | e ->
            printfn "%s" "base4225: fatal error: "
            e.ToString () |> printfn "%s"
            1
      else
        printfn "%s" "usage: base4225 [OPTION]... [FILE]"
        printfn "%s" "Base4225 encode or decode FILE to standard output."
        Console.WriteLine ()
        printfn "%s" "OPTIONS:"
        printfn "%s" "  -d, --decode ... decode data"
        0

  end
