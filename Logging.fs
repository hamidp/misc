module Logging

open System
open System.IO
open System.Text
open System.Threading

/// Log levels.
let Error = 0
let Warning = 1
let Information = 2
let Debug = 3

let LevelToString level =
  match level with
    | 0 -> "Error"
    | 1 -> "Warning"
    | 2 -> "Information"
    | 3 -> "Debug"
    | _ -> "Unknown"

/// The current log level.
let mutable current_log_level = Debug

/// The inteface loggers need to implement.
type ILogger = abstract Log : int -> Printf.StringFormat<'a,unit> -> 'a

/// Writes to console.
let ConsoleLogger = { 
  new ILogger with
    member __.Log level format =
      Printf.kprintf (printfn "[%s][%A] %s" (LevelToString level) System.DateTime.Now) format
 }

/// FileLogger that writes to a specified stream or file.
type FileLogger (stream:Stream) = class
  /// Where we write this stuff.
  let (file:Stream) = stream

  
  /// The encoding to use.
  let encoding = Encoding.Unicode

  
  /// Controls whether anything is written to the file.
  let mutable loggingEnabled = 1
  let refLoggingEnabled = ref loggingEnabled


  /// Write the message to the log file.
  let writeMessageToFile (msg:string) =
    let bytes = encoding.GetBytes(msg)
    
    // Don't write if logging is disabled.
    if (Interlocked.CompareExchange(refLoggingEnabled, 0, 0) = 1) then
      file.Write(bytes, 0, bytes.Length)
   

  /// The agent to which we post all these messages.
  let agent =
    new MailboxProcessor<string>(fun inbox ->
          let rec Loop() =
            async { let! message = inbox.Receive()
                    writeMessageToFile message
                    do! Loop () }
          Loop())
  do agent.Start()
  

  /// Posts the message to the agent.
  let log level format =
    // Don't post is locking is disabled.
    if (Interlocked.CompareExchange(refLoggingEnabled, 0, 0) = 0) then ()
    
    // Logging is still enabled, write it out.
    let msg =
      sprintf "[%s][%A] %s%s"
        (LevelToString level) System.DateTime.Now format Environment.NewLine
    agent.Post msg


  /// Creates an instance that logs to file.
  new (filePath:string) =
    let fs = File.OpenWrite(filePath)
    FileLogger(fs)
  

  interface ILogger with
    /// Log the message.
    member this.Log level format = Printf.kprintf (log level) format


  /// Stops all logging.
  member this.StopLogging () =
    if (Interlocked.Exchange(refLoggingEnabled, 0) = 0) then ()
    file.Close()
end

/// Defines which logger to use.
let mutable DefaultLogger = ConsoleLogger

/// Logs a message with the specified logger.
let logUsing (logger: ILogger) = logger.Log

/// Logs a message using the default logger.
let log level message = logUsing DefaultLogger level message