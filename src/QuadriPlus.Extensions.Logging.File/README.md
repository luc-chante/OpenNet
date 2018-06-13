# File logging extension

## Configuration

Add a subsection *File* into *Logging* section from *appsettings.json* file.
ex:
```json
{
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    },
    "File": {
      "Path": "~/service.log",
      "Pattern": "%date [%lvl] %name - %message",
      "Behaviour": "Backup",
      "BackupMode": "Startup, Size, Age",
      "MaxSize": "1M",
      "MaxAge":  "1.00:00:00"
    }
  }
}
```

 - **IncludeScopes** and **LogLevel** : see Logging configuration
 - **Path** : string, mandatory
   > If `path` starts with **~**, it is resolved relatively to application directory.
 - **Pattern** : string, optional (default: "%date [%lvl] %name - ")
   > Format: *%[alignment]{var}*, *alignment* is optional and the same as in `string.Format()`. *var* can be :  
   > *%date* : `DateTime.Now`  
   > *%Level* : Full log level string (Trace, Debug, ...)  
   > *%level* : Full log level string in lower case (trace, debug, information, ...)  
   > *%Lvl* : Short log level string (Trce, Dbug, ...)  
   > *%lvl* : Short log level string in lower case (trce, dbug, info, ...)  
   > *%name* : The category of the logger  
   > *%message* : The message to log
 - **Behaviour** : FileLoggerBehaviour, optional (default: FileLoggerBehaviour.Append)
   > *Append* : Open log file in append mode  
   > *Override* : Open file in erase mode  
   > *Backup* : Do backup file, depending on *BackupMode* value
 - **BackupMode** : FileLoggerBackupMode, optional (default, FileLoggerBackupMode.Default)
   > This enum is a flag, value can be mixed
   > 
   > *Default* : Same as FileLoggerBackupMode.Startup  
   > *Startup* : Do a backup when application starts  
   > *Size* : Do a backup when file exceed *MaxSize*  
   > *Age* : Do a backup when file exceed *MaxAge*
 - **MaxSize** : string, mandatory for FileLoggerBackupMode.Size
   > The max size of the file. Can be a number or a size (ex: 1M, 230k, ...).  
   > The size can be exceeded if the last written message is big enough.
 - **MaxAge** : TimeSpan, mandatory for FileLoggerBackupMode.Age
   > The max age of the file.  
   > ex: 1 day => `"1.00:00:00"`, half hour => `"00:30:00"`

## Initilisation

Use the extension `ILoggingBuilder.AddFile()`.

```c#
using QuadriPlus.Extensions.Logging;

public class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Run();
    }

    public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .ConfigureLogging((hostingContext, builder) =>
            {
                builder.AddFile();
            })
            .UseStartup<Startup>()
            .Build();
}
```