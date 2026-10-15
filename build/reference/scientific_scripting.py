"""Script IO/plotting contracts and ownership, not a browser/portable promise."""
from common import A, SOURCE

def scripting(name,args,sig):
    url=SOURCE+'Computator.NET.Core/Functions/ScriptingFunctions.cs'
    if name=='setWorkingDirectory':
        return A('Set the process working directory','Creates the supplied directory if needed, then changes the process’s current directory.',
                 '', 'path is a filesystem path subject to current user permissions.',
                 'This affects subsequent relative paths process-wide, not just a variable inside one function. Scripts have the host user’s file access; this is not a sandbox.',url=url,category='Script files')
    if name in ('File','FileReader','FileWriter'):
        chooser=not args
        d={'File':'Creates Computator’s file wrapper, opening separate read and append-write streams.',
           'FileReader':'Opens a .NET StreamReader for text input.', 'FileWriter':'Opens a .NET StreamWriter for text output.'}[name]
        note={'File':'Writing through this wrapper appends. Call close() when finished; clear() truncates the file. The chooser overload falls back to file.txt if its dialog is cancelled.',
              'FileReader':'The file must exist. Close or dispose the returned reader. Cancelling the chooser can leave an invalid filename and raise an exception.',
              'FileWriter':'Opening this writer normally CREATES OR TRUNCATES the file; it is not append mode. Close/dispose to flush buffered output. Cancelling the chooser can raise an exception.'}[name]
        return A(name+' text-file opener',d,'',
                 'Uses a desktop file-selection dialog; requires an interactive Windows UI.' if chooser else 'Explicit path; relative paths use the process working directory. File permissions and encoding follow the underlying stream implementation.',
                 note+(' Interactive chooser calls are not supported as browser dialogs in API workers.' if chooser else ''),url=url,category='Script files')
    if name=='convert':
        return A('Convert an input string to a script scalar','Converts text using the application’s legacy string/real/complex conversion helper.',
                 '', 'Use T=string, real (double), or complex; numeric parsing uses the invariant decimal point.',
                 'The numeric branch boxes a double and casts it to T, so other numeric T such as integer can fail rather than perform a general numeric conversion. Empty/cancelled text can also fail for value types; this is not a TryParse API.',url=url,category='Script input')
    if name in ('read','readln'):
        fromfile='file' in args;out='output' in args;generic='<T>' in sig
        line=name=='readln'
        return A(('Read one line' if line else 'Read remaining text')+(' from a file' if fromfile else ' from a desktop dialog'),
                 ('Reads up to the next line ending, advancing the wrapper’s reader.' if line and fromfile else 'Reads all remaining text from the current file position.' if fromfile else 'Displays a modal desktop input dialog and converts its result.'),
                 '', ('file is a Computator File object, not a pathname or StreamReader.' if fromfile else 'Optional caption labels the dialog. This requires an interactive Windows UI and is not browser/CLI standard input.')+
                 (' output is an out argument assigned by the method.' if out else ' The result is returned to the caller.'),
                 ('Typed overloads use convert<T>; prefer string, real or complex and handle invalid/empty input. ' if generic else '')+
                 ('readln can return null at end of file; read returns an empty string when no text remains.' if fromfile else 'Cancellation may produce empty text and conversion errors; no value should be assumed without handling that outcome.'),url=url,category='Script input')
    if name in ('write','writeln','show'):
        if name=='show':
            return A('Show a desktop message','Formats the supplied object and displays it in a modal message box.',
                     '', 'o is any supported display object; caption optionally labels the dialog.',
                     'Desktop-only interaction; it does not append to the script output stream or open a browser popup. Use write/writeln for captured output.',url=url,category='Script output')
        newline=name=='writeln';where=args.split(',')[0] if ',' in args else 'console'
        d={'console':'Formats o and appends it to the current script-output callback.',
           'file':'Formats o and writes through an existing Computator File wrapper.',
           'writer':'Formats o and writes through an existing .NET StreamWriter.',
           'path':'Opens the pathname, writes the formatted object and closes the newly created stream.'}[where]
        note={'console':'The executing host must supply an output callback. API/portable hosts impose output limits; output is not a native console requirement.',
              'file':'The File wrapper writes in append mode and flushes. The caller still owns its lifetime.',
              'writer':'The caller owns flushing/disposal; this helper does not close the writer.',
              'path':'This path overload TRUNCATES any existing file on each call. It is not append logging; use an explicitly opened append-capable stream or File wrapper for repeated writes.'}[where]
        return A(('Write line' if newline else 'Write text')+(' to script output' if where=='console' else ' to '+where),d,
                 'Adds the platform newline after the object.' if newline else 'No newline is appended.',
                 'Real/complex numbers and matrices use the application’s display formatting; null is formatted as empty text.',note,url=url,category='Script output')
    if name=='plot':
        if args=='x,y':d='Displays a 2D plot from paired x/y coordinate sequences; supply equally sized finite sequences.'
        elif args=='x,y,z':d='Displays 3D points from equally sized x/y/z coordinate sequences.'
        elif args.startswith('fx,xys'):d='Displays a real function with additional data series assembled from successive adjacent sequence pairs.'
        elif args.startswith('fxs'):d='Displays several real one-variable functions on a shared 2D chart.'
        elif args.startswith('fxys'):d='Displays one or more real two-variable functions in the desktop 3D chart.'
        elif args.startswith('fxy,'):d='Displays the surface z=fxy(x,y) in the desktop 3D chart.'
        elif args.startswith('fz,'):d='Displays a complex function using the application’s complex-chart renderer.'
        elif args.startswith('fx,'):d='Displays a real one-variable function in a desktop 2D chart.'
        else:d='Displays a precompiled Function in the renderer selected by its FunctionType.'
        return A('Interactive desktop plot',d,'',
                 'Explicit bounds use XMin<XMax and YMin<YMax; quality is multiplied by 100 before being sent to the chart. Default bounds are −5…5 and default quality 0.5 where supplied.',
                 'This opens a modal Windows plot dialog; it does not return an image or implement server-side rendering. Browser users should use the Chart screen. Data-series orientation follows the historical AddDataPoints adapters, and mismatched sequence lengths can fail.',url=url,category='Script plotting')
    raise KeyError('Script '+sig)
