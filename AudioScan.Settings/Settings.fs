namespace AudioScan


open FSharp.Configuration
open AudioScan.Common
type SettingsType = YamlConfig<"Settings.yaml", ReadOnly=true>
type MutableSettings = YamlConfig<"Settings.yaml">

module Settings =
    open System
    open System.IO

    let executingAssemblyDir() =
        let codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase
        let uri = UriBuilder(codeBase)
        let path = Uri.UnescapeDataString(uri.Path)
        IO.FileInfo(path).Directory
        
    let getSettingsFiles () =
      let currentDir = executingAssemblyDir()
      [ Some currentDir.FullName
        Some currentDir.Parent.FullName // <root>\out\[Debug|Release]\
        (try Some(Path.Combine(currentDir.Parent.Parent.Parent.FullName, "AudioScan.Settings")) 
         with _ -> None)
        (try Some(Path.Combine(currentDir.Parent.Parent.Parent.Parent.FullName, "src/AudioScan.Settings"))  
         with _ -> None) ]
      |> List.choose id
      |> List.map (fun dir -> Path.Combine(dir, "Settings.yaml"))
      |> List.filter File.Exists
    let location =
        let files = getSettingsFiles()
        match files with
        | [] -> failwithf "Cannot find any of the setting files: %A" files
        | file :: _ ->
            file |> Path.GetFullPath
            
    let private settings =
        let settings = SettingsType()
        settings.LoadAndWatch location |> ignore
        settings
        
    let logger =        
        let logs = LoggingInstaller.Configure()
        let minlevel = NLog.LogLevel.FromString(settings.Logs.MinLevel)
        if (Environment.UserInteractive) then
            logs.WriteToConsole(apply = true, minLevel = minlevel) |> ignore

        if (String.IsNullOrWhiteSpace(settings.Logs.Directory) |> not) then
            logs.WriteToFolder(settings.Logs.Directory.Trim(), apply = true, minLevel = minlevel) |> ignore

        (fun x -> NLog.LogManager.GetLogger(x))

    let Default =

        settings |> ignore
        (logger "init").Trace "initialized logging"
        
        settings