namespace TinCan.NET;

public enum MupenError
{
    Success = 0,
    NotInit, /* Function is disallowed before InitMupen64Plus() is called */
    AlreadyInit, /* InitMupen64Plus() was called twice */
    Incompatible, /* API versions between components are incompatible */
    InputAssert, /* Invalid parameters for function call, such as ParamValue=NULL for GetCoreParameter() */
    InputInvalid, /* Invalid input data, such as ParamValue="maybe" for SetCoreParameter() to set a BOOL-type value */
    InputNotFound, /* The input parameter(s) specified a particular item which was not found */
    NoMemory, /* Memory allocation failed */
    Files, /* Error opening, creating, reading, or writing to a file */
    Internal, /* Internal error */
    InvalidState, /* Current program state does not allow operation */
    PluginFail, /* A plugin function returned a fatal error */
    SystemFail, /* A system function call, such as an SDL or file operation, failed */
    Unsupported, /* Function call is not supported (ie, core not built with debugger) */
    WrongType
}

public enum MupenPluginType
{
    Null = 0,
    RSP = 1,
    Graphics,
    Audio,
    Input,
    Core
}

public enum MupenMsgLevel
{
    Error = 1,
    Warning,
    Info,
    Status,
    Verbose
}