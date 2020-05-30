module Funcs
    open System
    open System.Drawing
    open System.Drawing.Imaging
    open Azure.Storage.Blobs


    let tryResize (size : Size) (image : Bitmap)   =
        try
            let bitmap = new Bitmap(size.Width, size.Height)
            use g = Graphics.FromImage(bitmap :> Image)
            g.InterpolationMode <- System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic
            g.DrawImage(image, 0, 0, size.Width, size.Height)
            Some bitmap
        with 
        | ex -> printfn "%A" ex; None

    let tryResizeTo3840 (image : Bitmap) =
        let aspect = 3840.0 / (image.Size.Width |> float)
        let height = aspect * (image.Size.Height |> float)
        tryResize(Size(3840.0 |> int, height |> int)) image

    let tryResizeTo1920 (image : Bitmap) =
        let aspect = 1920.0 / (image.Size.Width |> float)
        let height = aspect * (image.Size.Height |> float)
        tryResize(Size(1920.0 |> int, height |> int)) image

    let tryResizeTo720 (image : Bitmap) =
        let aspect = 720.0 / (image.Size.Width |> float)
        let height = aspect * (image.Size.Height |> float)
        tryResize(Size(720.0 |> int, height |> int)) image

    let tryResizeTo480 (image : Bitmap) =
        let aspect = 480.0 / (image.Size.Width |> float)
        let height = aspect * (image.Size.Height |> float)
        tryResize(Size(480.0 |> int, height |> int)) image

    let tryOpenImage (filename : string)  =
        printfn "Opening %s" filename
        try
           let bitmap = new Bitmap(filename)
           Some bitmap
        with
        | ex -> printfn "%A" ex; None

    let tryParseResolution (str : string) =
        try
            match str.Split 'x' with
            | [|sx; sy; |] -> 
                let x = sx |> int
                let y = sy |> int
                Some (Size(x, y))            
            | ex -> printfn "%A" ex; None
        with
        | ex -> printfn "%A" ex; None

    let save (name:string) (image: Bitmap) =
        try
            let jpegImageCodecInfo = ImageCodecInfo.GetImageEncoders() |> Array.find (fun e -> e.MimeType.Equals("image/jpeg"))
            let encoder = Encoder.Quality
            let encoderParameters = new EncoderParameters(1)
            let encodeParameter = new EncoderParameter(encoder, 75L)
            encoderParameters.Param.[0] <- encodeParameter
            image.Save(name, jpegImageCodecInfo, encoderParameters)
            printfn "saved %s" name
            Some name
        with
        | ex -> printfn "%A" ex; None

    let upload (containerClient : BlobContainerClient) (filename : string) =
        try
            let blobClient = containerClient.GetBlobClient(IO.Path.GetFileName(filename))
            let stream = IO.File.OpenRead(filename)
            blobClient.Upload(stream, true)
            stream.Close()
            IO.Path.GetFileName(filename) |> printfn "file uploaded: %s" 
            Some ()
        with
        | ex -> printfn "file %s upload failure: %A" filename ex; None

    let getConnectionString () =
        let env = System.Environment.GetEnvironmentVariable("azazeodesign_connection_string") |> Option.ofObj
        match env with
        | Some v -> Some v
        | None ->
            try
                Some (IO.File.OpenText("config").ReadToEnd())
            with
            | ex -> printfn "no config, %A" ex; None


    let printHugoTemplate (name : string) =
        let sourceExtension = IO.Path.GetExtension(name)
        let file = IO.File.CreateText(IO.Path.ChangeExtension(name, ".md"))
        file.WriteLine "+++"
        file.WriteLine (sprintf "name = \"%s\"" (IO.Path.GetFileNameWithoutExtension(name)))
        file.WriteLine (sprintf "date = \"%d-%02d-%02d\"" DateTime.Now.Year DateTime.Now.Month DateTime.Now.Day) 
        file.WriteLine "type = \"gallery\""
        file.WriteLine (sprintf "sourceextension = \"%s\"" sourceExtension)
        file.WriteLine "+++"
        file.Close()
