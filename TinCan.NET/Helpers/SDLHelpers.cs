using Avalonia.Input;
using Silk.NET.SDL;

namespace TinCan.NET.Helpers;

public class SDLHelpers
{
    public static Scancode ToSDLScancode(Key key)
    {
        return key switch
        {
            Key.None => Scancode.ScancodeUnknown,
            Key.Cancel => Scancode.ScancodeCancel,
            Key.Back => Scancode.ScancodeBackspace,
            Key.Tab => Scancode.ScancodeTab,
            Key.LineFeed => Scancode.ScancodeReturn2,
            Key.Clear => Scancode.ScancodeClear,
            Key.Return => Scancode.ScancodeReturn,
            Key.Pause => Scancode.ScancodePause,
            Key.CapsLock => Scancode.ScancodeCapslock,

            Key.HangulMode => Scancode.ScancodeLang1,
            Key.JunjaMode => Scancode.ScancodeUnknown,
            Key.FinalMode => Scancode.ScancodeUnknown,
            Key.KanjiMode => Scancode.ScancodeLang2,

            Key.Escape => Scancode.ScancodeEscape,

            Key.ImeConvert => Scancode.ScancodeUnknown,
            Key.ImeNonConvert => Scancode.ScancodeUnknown,
            Key.ImeAccept => Scancode.ScancodeUnknown,
            Key.ImeModeChange => Scancode.ScancodeUnknown,

            Key.Space => Scancode.ScancodeSpace,

            Key.PageUp => Scancode.ScancodePageup,
            Key.PageDown => Scancode.ScancodePagedown,
            Key.End => Scancode.ScancodeEnd,
            Key.Home => Scancode.ScancodeHome,
            Key.Left => Scancode.ScancodeLeft,
            Key.Up => Scancode.ScancodeUp,
            Key.Right => Scancode.ScancodeRight,
            Key.Down => Scancode.ScancodeDown,

            Key.Select => Scancode.ScancodeSelect,
            Key.Print => Scancode.ScancodePrintscreen,
            Key.Execute => Scancode.ScancodeExecute,
            Key.PrintScreen => Scancode.ScancodePrintscreen,
            Key.Insert => Scancode.ScancodeInsert,
            Key.Delete => Scancode.ScancodeDelete,
            Key.Help => Scancode.ScancodeHelp,

            Key.D0 => Scancode.Scancode0,
            Key.D1 => Scancode.Scancode1,
            Key.D2 => Scancode.Scancode2,
            Key.D3 => Scancode.Scancode3,
            Key.D4 => Scancode.Scancode4,
            Key.D5 => Scancode.Scancode5,
            Key.D6 => Scancode.Scancode6,
            Key.D7 => Scancode.Scancode7,
            Key.D8 => Scancode.Scancode8,
            Key.D9 => Scancode.Scancode9,

            Key.A => Scancode.ScancodeA,
            Key.B => Scancode.ScancodeB,
            Key.C => Scancode.ScancodeC,
            Key.D => Scancode.ScancodeD,
            Key.E => Scancode.ScancodeE,
            Key.F => Scancode.ScancodeF,
            Key.G => Scancode.ScancodeG,
            Key.H => Scancode.ScancodeH,
            Key.I => Scancode.ScancodeI,
            Key.J => Scancode.ScancodeJ,
            Key.K => Scancode.ScancodeK,
            Key.L => Scancode.ScancodeL,
            Key.M => Scancode.ScancodeM,
            Key.N => Scancode.ScancodeN,
            Key.O => Scancode.ScancodeO,
            Key.P => Scancode.ScancodeP,
            Key.Q => Scancode.ScancodeQ,
            Key.R => Scancode.ScancodeR,
            Key.S => Scancode.ScancodeS,
            Key.T => Scancode.ScancodeT,
            Key.U => Scancode.ScancodeU,
            Key.V => Scancode.ScancodeV,
            Key.W => Scancode.ScancodeW,
            Key.X => Scancode.ScancodeX,
            Key.Y => Scancode.ScancodeY,
            Key.Z => Scancode.ScancodeZ,

            Key.LWin => Scancode.ScancodeLgui,
            Key.RWin => Scancode.ScancodeRgui,
            Key.Apps => Scancode.ScancodeApplication,
            Key.Sleep => Scancode.ScancodeSleep,

            Key.NumPad0 => Scancode.ScancodeKP0,
            Key.NumPad1 => Scancode.ScancodeKP1,
            Key.NumPad2 => Scancode.ScancodeKP2,
            Key.NumPad3 => Scancode.ScancodeKP3,
            Key.NumPad4 => Scancode.ScancodeKP4,
            Key.NumPad5 => Scancode.ScancodeKP5,
            Key.NumPad6 => Scancode.ScancodeKP6,
            Key.NumPad7 => Scancode.ScancodeKP7,
            Key.NumPad8 => Scancode.ScancodeKP8,
            Key.NumPad9 => Scancode.ScancodeKP9,
            Key.Multiply => Scancode.ScancodeKPMultiply,
            Key.Add => Scancode.ScancodeKPPlus,
            Key.Separator => Scancode.ScancodeSeparator,
            Key.Subtract => Scancode.ScancodeKPMinus,
            Key.Decimal => Scancode.ScancodeKPDecimal,
            Key.Divide => Scancode.ScancodeKPDivide,
            
            Key.F1 => Scancode.ScancodeF1,
            Key.F2 => Scancode.ScancodeF2,
            Key.F3 => Scancode.ScancodeF3,
            Key.F4 => Scancode.ScancodeF4,
            Key.F5 => Scancode.ScancodeF5,
            Key.F6 => Scancode.ScancodeF6,
            Key.F7 => Scancode.ScancodeF7,
            Key.F8 => Scancode.ScancodeF8,
            Key.F9 => Scancode.ScancodeF9,
            Key.F10 => Scancode.ScancodeF10,
            Key.F11 => Scancode.ScancodeF11,
            Key.F12 => Scancode.ScancodeF12,
            Key.F13 => Scancode.ScancodeF13,
            Key.F14 => Scancode.ScancodeF14,
            Key.F15 => Scancode.ScancodeF15,
            Key.F16 => Scancode.ScancodeF16,
            Key.F17 => Scancode.ScancodeF17,
            Key.F18 => Scancode.ScancodeF18,
            Key.F19 => Scancode.ScancodeF19,
            Key.F20 => Scancode.ScancodeF20,
            Key.F21 => Scancode.ScancodeF21,
            Key.F22 => Scancode.ScancodeF22,
            Key.F23 => Scancode.ScancodeF23,
            Key.F24 => Scancode.ScancodeF24,
            
            Key.NumLock => Scancode.ScancodeNumlockclear,
            Key.Scroll => Scancode.ScancodeScrolllock,
            Key.LeftShift => Scancode.ScancodeLshift,
            Key.RightShift => Scancode.ScancodeRshift,
            Key.LeftCtrl => Scancode.ScancodeLctrl,
            Key.RightCtrl => Scancode.ScancodeRctrl,
            Key.LeftAlt => Scancode.ScancodeLalt,
            Key.RightAlt => Scancode.ScancodeRalt,
            
            Key.BrowserBack => Scancode.ScancodeACBack,
            Key.BrowserForward => Scancode.ScancodeACForward,
            Key.BrowserRefresh => Scancode.ScancodeACRefresh,
            Key.BrowserStop => Scancode.ScancodeACStop,
            Key.BrowserSearch => Scancode.ScancodeACSearch,
            Key.BrowserFavorites => Scancode.ScancodeACBookmarks,
            Key.BrowserHome => Scancode.ScancodeACHome,
            
            Key.VolumeMute => Scancode.ScancodeMute,
            Key.VolumeDown => Scancode.ScancodeVolumedown,
            Key.VolumeUp => Scancode.ScancodeVolumeup,
            
            Key.MediaNextTrack => Scancode.ScancodeAudionext,
            Key.MediaPreviousTrack => Scancode.ScancodeAudioprev,
            Key.MediaStop => Scancode.ScancodeAudiostop,
            Key.MediaPlayPause => Scancode.ScancodeAudioplay,
            
            Key.LaunchMail => Scancode.ScancodeMail,
            Key.SelectMedia => Scancode.ScancodeMediaselect,
            Key.LaunchApplication1 => Scancode.ScancodeApp1,
            Key.LaunchApplication2 => Scancode.ScancodeApp2,
            

            Key.OemSemicolon => Scancode.ScancodeSemicolon,
            Key.OemPlus => Scancode.ScancodeEquals,
            Key.OemComma => Scancode.ScancodeComma,
            Key.OemMinus => Scancode.ScancodeMinus,
            Key.OemPeriod => Scancode.ScancodePeriod,
            Key.OemQuestion => Scancode.ScancodeSlash,
            Key.OemTilde => Scancode.ScancodeGrave,
            Key.AbntC1 => Scancode.ScancodeUnknown,
            Key.AbntC2 => Scancode.ScancodeUnknown,
            Key.OemOpenBrackets => Scancode.ScancodeLeftbracket,
            Key.OemPipe => Scancode.ScancodeBackslash,
            Key.OemCloseBrackets => Scancode.ScancodeRightbracket,
            Key.OemQuotes => Scancode.ScancodeApostrophe,
            Key.Oem8 => Scancode.ScancodeRctrl,
            Key.OemBackslash => Scancode.ScancodeNonusbackslash,
            
            Key.ImeProcessed => Scancode.ScancodeUnknown,
            Key.System => Scancode.ScancodeUnknown,
            
            Key.OemAttn => Scancode.ScancodeSysreq,
            
            // Key.OemFinish => Scancode.ScancodeUnknown,
            // Key.OemCopy => Scancode.ScancodeUnknown,
            // Key.OemAuto => Scancode.ScancodeUnknown,
            // Key.OemEnlw => Scancode.ScancodeUnknown,
            // Key.OemBackTab => Scancode.ScancodeUnknown,
            // Key.DbeNoRoman => Scancode.ScancodeUnknown,
            
            Key.CrSel => Scancode.ScancodeCrsel,
            Key.ExSel => Scancode.ScancodeExsel,
            
            // Key.EraseEof => Scancode.ScancodeUnknown,
            // Key.DbeCodeInput => Scancode.ScancodeUnknown,
            // Key.DbeNoCodeInput => Scancode.ScancodeUnknown,
            // Key.DbeDetermineString => Scancode.ScancodeUnknown,
            // Key.Pa1 => Scancode.ScancodeUnknown,
            // Key.OemClear => Scancode.ScancodeUnknown,
            
            Key.DeadCharProcessed => Scancode.ScancodeUnknown,
            
            Key.FnLeftArrow => Scancode.ScancodeLeft,
            Key.FnRightArrow => Scancode.ScancodeRight,
            Key.FnUpArrow => Scancode.ScancodeUp,
            Key.FnDownArrow => Scancode.ScancodeDown,

            _ => Scancode.ScancodeUnknown
        };
    }
    
    public static Key ToAvaloniaKey(Scancode key)
    {
        return key switch
        {
            Scancode.ScancodeUnknown => Key.None,
            Scancode.ScancodeCancel => Key.Cancel,
            Scancode.ScancodeBackspace => Key.Back,
            Scancode.ScancodeTab => Key.Tab,
            Scancode.ScancodeReturn2 => Key.LineFeed,
            Scancode.ScancodeClear => Key.Clear,
            Scancode.ScancodeReturn => Key.Return,
            Scancode.ScancodePause => Key.Pause,
            Scancode.ScancodeCapslock => Key.CapsLock,

            Scancode.ScancodeLang1 => Key.HangulMode,
            Scancode.ScancodeLang2 => Key.KanjiMode,

            Scancode.ScancodeEscape => Key.Escape,

            Scancode.ScancodeSpace => Key.Space,

            Scancode.ScancodePageup => Key.PageUp,
            Scancode.ScancodePagedown => Key.PageDown,
            Scancode.ScancodeEnd => Key.End,
            Scancode.ScancodeHome => Key.Home,
            Scancode.ScancodeLeft => Key.Left,
            Scancode.ScancodeUp => Key.Up,
            Scancode.ScancodeRight => Key.Right,
            Scancode.ScancodeDown => Key.Down,

            Scancode.ScancodeSelect => Key.Select,
            Scancode.ScancodeExecute => Key.Execute,
            Scancode.ScancodePrintscreen => Key.PrintScreen,
            Scancode.ScancodeInsert => Key.Insert,
            Scancode.ScancodeDelete => Key.Delete,
            Scancode.ScancodeHelp => Key.Help,

            Scancode.Scancode0 => Key.D0,
            Scancode.Scancode1 => Key.D1,
            Scancode.Scancode2 => Key.D2,
            Scancode.Scancode3 => Key.D3,
            Scancode.Scancode4 => Key.D4,
            Scancode.Scancode5 => Key.D5,
            Scancode.Scancode6 => Key.D6,
            Scancode.Scancode7 => Key.D7,
            Scancode.Scancode8 => Key.D8,
            Scancode.Scancode9 => Key.D9,

            Scancode.ScancodeA => Key.A,
            Scancode.ScancodeB => Key.B,
            Scancode.ScancodeC => Key.C,
            Scancode.ScancodeD => Key.D,
            Scancode.ScancodeE => Key.E,
            Scancode.ScancodeF => Key.F,
            Scancode.ScancodeG => Key.G,
            Scancode.ScancodeH => Key.H,
            Scancode.ScancodeI => Key.I,
            Scancode.ScancodeJ => Key.J,
            Scancode.ScancodeK => Key.K,
            Scancode.ScancodeL => Key.L,
            Scancode.ScancodeM => Key.M,
            Scancode.ScancodeN => Key.N,
            Scancode.ScancodeO => Key.O,
            Scancode.ScancodeP => Key.P,
            Scancode.ScancodeQ => Key.Q,
            Scancode.ScancodeR => Key.R,
            Scancode.ScancodeS => Key.S,
            Scancode.ScancodeT => Key.T,
            Scancode.ScancodeU => Key.U,
            Scancode.ScancodeV => Key.V,
            Scancode.ScancodeW => Key.W,
            Scancode.ScancodeX => Key.X,
            Scancode.ScancodeY => Key.Y,
            Scancode.ScancodeZ => Key.Z,

            Scancode.ScancodeLgui => Key.LWin,
            Scancode.ScancodeRgui => Key.RWin,
            Scancode.ScancodeApplication => Key.Apps,
            Scancode.ScancodeSleep => Key.Sleep,

            Scancode.ScancodeKP0 => Key.NumPad0,
            Scancode.ScancodeKP1 => Key.NumPad1,
            Scancode.ScancodeKP2 => Key.NumPad2,
            Scancode.ScancodeKP3 => Key.NumPad3,
            Scancode.ScancodeKP4 => Key.NumPad4,
            Scancode.ScancodeKP5 => Key.NumPad5,
            Scancode.ScancodeKP6 => Key.NumPad6,
            Scancode.ScancodeKP7 => Key.NumPad7,
            Scancode.ScancodeKP8 => Key.NumPad8,
            Scancode.ScancodeKP9 => Key.NumPad9,
            Scancode.ScancodeKPMultiply => Key.Multiply,
            Scancode.ScancodeKPPlus => Key.Add,
            Scancode.ScancodeSeparator => Key.Separator,
            Scancode.ScancodeKPMinus => Key.Subtract,
            Scancode.ScancodeKPDecimal => Key.Decimal,
            Scancode.ScancodeKPDivide => Key.Divide,
            
            Scancode.ScancodeF1 => Key.F1,
            Scancode.ScancodeF2 => Key.F2,
            Scancode.ScancodeF3 => Key.F3,
            Scancode.ScancodeF4 => Key.F4,
            Scancode.ScancodeF5 => Key.F5,
            Scancode.ScancodeF6 => Key.F6,
            Scancode.ScancodeF7 => Key.F7,
            Scancode.ScancodeF8 => Key.F8,
            Scancode.ScancodeF9 => Key.F9,
            Scancode.ScancodeF10 => Key.F10,
            Scancode.ScancodeF11 => Key.F11,
            Scancode.ScancodeF12 => Key.F12,
            Scancode.ScancodeF13 => Key.F13,
            Scancode.ScancodeF14 => Key.F14,
            Scancode.ScancodeF15 => Key.F15,
            Scancode.ScancodeF16 => Key.F16,
            Scancode.ScancodeF17 => Key.F17,
            Scancode.ScancodeF18 => Key.F18,
            Scancode.ScancodeF19 => Key.F19,
            Scancode.ScancodeF20 => Key.F20,
            Scancode.ScancodeF21 => Key.F21,
            Scancode.ScancodeF22 => Key.F22,
            Scancode.ScancodeF23 => Key.F23,
            Scancode.ScancodeF24 => Key.F24,
            
            Scancode.ScancodeNumlockclear => Key.NumLock,
            Scancode.ScancodeScrolllock => Key.Scroll,
            Scancode.ScancodeLshift => Key.LeftShift,
            Scancode.ScancodeRshift => Key.RightShift,
            Scancode.ScancodeLctrl => Key.LeftCtrl,
            Scancode.ScancodeRctrl => Key.RightCtrl,
            Scancode.ScancodeLalt => Key.LeftAlt,
            Scancode.ScancodeRalt => Key.RightAlt,
            
            Scancode.ScancodeACBack => Key.BrowserBack,
            Scancode.ScancodeACForward => Key.BrowserForward,
            Scancode.ScancodeACRefresh => Key.BrowserRefresh,
            Scancode.ScancodeACStop => Key.BrowserStop,
            Scancode.ScancodeACSearch => Key.BrowserSearch,
            Scancode.ScancodeACBookmarks => Key.BrowserFavorites,
            Scancode.ScancodeACHome => Key.BrowserHome,
            
            Scancode.ScancodeMute => Key.VolumeMute,
            Scancode.ScancodeVolumedown => Key.VolumeDown,
            Scancode.ScancodeVolumeup => Key.VolumeUp,
            
            Scancode.ScancodeAudionext => Key.MediaNextTrack,
            Scancode.ScancodeAudioprev => Key.MediaPreviousTrack,
            Scancode.ScancodeAudiostop => Key.MediaStop,
            Scancode.ScancodeAudioplay => Key.MediaPlayPause,
            
            Scancode.ScancodeMail => Key.LaunchMail,
            Scancode.ScancodeMediaselect => Key.SelectMedia,
            Scancode.ScancodeApp1 => Key.LaunchApplication1,
            Scancode.ScancodeApp2 => Key.LaunchApplication2,
            

            Scancode.ScancodeSemicolon => Key.OemSemicolon,
            Scancode.ScancodeEquals => Key.OemPlus,
            Scancode.ScancodeComma => Key.OemComma,
            Scancode.ScancodeMinus => Key.OemMinus,
            Scancode.ScancodePeriod => Key.OemPeriod,
            Scancode.ScancodeSlash => Key.OemQuestion,
            Scancode.ScancodeGrave => Key.OemTilde,
            Scancode.ScancodeLeftbracket => Key.OemOpenBrackets,
            Scancode.ScancodeBackslash => Key.OemPipe,
            Scancode.ScancodeRightbracket => Key.OemCloseBrackets,
            Scancode.ScancodeApostrophe => Key.OemQuotes,
            Scancode.ScancodeNonusbackslash => Key.OemBackslash,
            
            Scancode.ScancodeSysreq => Key.OemAttn,
            
            // Scancode.ScancodeUnknown => Key.OemFinish,
            // Scancode.ScancodeUnknown => Key.OemCopy,
            // Scancode.ScancodeUnknown => Key.OemAuto,
            // Scancode.ScancodeUnknown => Key.OemEnlw,
            // Scancode.ScancodeUnknown => Key.OemBackTab,
            // Scancode.ScancodeUnknown => Key.DbeNoRoman,
            
            Scancode.ScancodeCrsel => Key.CrSel,
            Scancode.ScancodeExsel => Key.ExSel,
            
            // Scancode.ScancodeUnknown => Key.EraseEof,
            // Scancode.ScancodeUnknown => Key.DbeCodeInput,
            // Scancode.ScancodeUnknown => Key.DbeNoCodeInput,
            // Scancode.ScancodeUnknown => Key.DbeDetermineString,
            // Scancode.ScancodeUnknown => Key.Pa1,
            // Scancode.ScancodeUnknown => Key.OemClear,

            _ => Key.None
        };
    }
    
    public static Keymod ToSDLKeymod(KeyModifiers mod)
    {
        Keymod res = 0;
        res |= (mod & KeyModifiers.Alt) != 0 ? Keymod.Alt : 0;
        res |= (mod & KeyModifiers.Control) != 0 ? Keymod.Ctrl : 0;
        res |= (mod & KeyModifiers.Shift) != 0 ? Keymod.Shift : 0;
        res |= (mod & KeyModifiers.Meta) != 0 ? Keymod.Gui : 0;
        return res;
    }

    public static RawInputModifiers ToAvaloniaInputModifiers(Keymod mod)
    {
        RawInputModifiers res = 0;
        res |= (mod & Keymod.Ctrl) != 0 ? RawInputModifiers.Control : 0;
        res |= (mod & Keymod.Shift) != 0 ? RawInputModifiers.Shift : 0;
        res |= (mod & Keymod.Alt) != 0 ? RawInputModifiers.Alt : 0;
        res |= (mod & Keymod.Gui) != 0 ? RawInputModifiers.Meta : 0;
        return res;
    }
}