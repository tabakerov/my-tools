#r @"process_and_upload_images\System.Drawing.Common.dll"
#r @"process_and_upload_images\Azure.Storage.Blobs.dll"
#r @"process_and_upload_images\Azure.Storage.Common.dll"
#r @"process_and_upload_images\Azure.Core.dll"

#load @"process_and_upload_images\Funcs.fs"

open System
open System.Drawing
open System.Drawing.Imaging
open Azure.Storage.Blobs
open Funcs

let argv = Environment.GetCommandLineArgs()

let processFile (filename : IO.FileInfo) =
    printfn "Processing %A" filename

    // COPY INPUT FILE WITH A NEW RANDOM NAME
    let newName = IO.Path.Combine(filename.Directory.ToString(),
                                  IO.Path.ChangeExtension(
                                      String.Join("", IO.Path.GetRandomFileName(), IO.Path.GetRandomFileName()).Replace(".", ""), 
                                      filename.Extension))
    filename.CopyTo(newName) |> ignore
    
    Funcs.printHugoTemplate newName

    // GET NAMES FOR PREVIEW (SMALLER JPG IMAGE) AND THUMBNAIL (THE SMALLEST JPG)
    let name3840 = IO.Path.ChangeExtension(newName, ".3840.jpg")
    let name1920 = IO.Path.ChangeExtension(newName, ".1920.jpg")
    let name720 = IO.Path.ChangeExtension(newName, ".720.jpg")
    let name480 = IO.Path.ChangeExtension(newName, ".480.jpg")

    let save3840 = Funcs.save name3840
    let save1920 = Funcs.save name1920
    let save720 = Funcs.save name720
    let save480 = Funcs.save name480

    let image = Funcs.tryOpenImage newName
    let image3840 = Option.bind Funcs.tryResizeTo3840 image
    let image1920 = Option.bind Funcs.tryResizeTo1920 image
    let image720 = Option.bind Funcs.tryResizeTo720 image
    let image480 = Option.bind Funcs.tryResizeTo480 image

    let saved3840 = Option.bind save3840 image3840
    let saved1920 = Option.bind save1920 image1920
    let saved720 = Option.bind save720 image720
    let saved480 = Option.bind save480 image480

    match Funcs.getConnectionString() with
    | Some s -> 
        let blobServiceClient = BlobServiceClient(s)
        let containerName = "images"
        let containerClient = blobServiceClient.GetBlobContainerClient(containerName)
        let uploadWithClient = Funcs.upload containerClient

        uploadWithClient newName |> ignore
        Option.bind uploadWithClient saved3840 |> ignore
        Option.bind uploadWithClient saved1920 |> ignore
        Option.bind uploadWithClient saved720 |> ignore
        Option.bind uploadWithClient saved480 |> ignore
        
    | None -> printfn "No connection string provided"

let filterExtension = match argv.Length with
                        | 3 -> None
                        | _ -> Some argv.[3..]

printfn "%A" filterExtension

let filterFile (extensions : string [] option) (f : IO.FileInfo) =
    let s = f.ToString()
    match extensions with
    | Some exts ->
        (exts |> Array.filter(fun e -> s.EndsWith(e)) |> Array.length) > 0
    | _ -> true
    

match IO.File.GetAttributes(argv.[2]).HasFlag(IO.FileAttributes.Directory) with
| false ->
    let inputFilename = IO.FileInfo(argv.[2])
    processFile inputFilename
    printfn "done"
| true ->
    match IO.Directory.Exists(argv.[2]) with
    | false -> printfn "Nothing found"
    | true ->
        IO.Directory.GetFiles(argv.[2]) 
        |> Array.map (fun f -> IO.FileInfo(f))
        |> Array.filter (filterFile filterExtension)
        |> Array.iter processFile
        //|> Array.iter (fun f -> printfn "enumerating files: %A" f)
        |> ignore
        printfn "done"


Console.ReadLine() |> ignore
0 // return an integer exit code
