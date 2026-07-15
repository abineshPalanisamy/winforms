// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Buffers.Binary;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Windows.Forms;

/// <summary>
///  Represents the image used to paint the mouse pointer. Different cursor shapes are used to inform the user
///  what operation the mouse will have.
/// </summary>
[TypeConverter(typeof(CursorConverter))]
[Editor($"System.Drawing.Design.CursorEditor, {(Assemblies.SystemDrawingDesign)}", typeof(UITypeEditor))]
public sealed class Cursor : IDisposable, ISerializable, IHandle<HICON>, IHandle<HANDLE>, IHandle<HCURSOR>
{
    private static Size s_cursorSize = Size.Empty;

    private readonly byte[]? _cursorData;
    private HCURSOR _handle;
    private readonly bool _freeHandle;

    /// <summary>
    ///  If created by the <see cref="Cursors"/> class, this is the property name that created it.
    /// </summary>
    internal string? CursorsProperty { get; }

    internal unsafe Cursor(PCWSTR nResourceId, string cursorsProperty)
    {
        GC.SuppressFinalize(this);
        _freeHandle = false;
        CursorsProperty = cursorsProperty;
        _handle = PInvoke.LoadCursor(HINSTANCE.Null, nResourceId);
        if (_handle.IsNull)
        {
            throw new Win32Exception(string.Format(SR.FailedToLoadCursor, Marshal.GetLastWin32Error()));
        }
    }

    internal Cursor(string resource, string cursorsProperty)
        : this(typeof(Cursors).Assembly.GetManifestResourceStream(typeof(Cursor), resource).OrThrowIfNull())
    {
        GC.SuppressFinalize(this);
        CursorsProperty = cursorsProperty;
        _freeHandle = false;
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="Cursor"/> class from the specified <paramref name="handle"/>.
    /// </summary>
    public Cursor(IntPtr handle)
    {
        GC.SuppressFinalize(this);
        if (handle == 0)
        {
            throw new ArgumentException(string.Format(SR.InvalidGDIHandle, (typeof(Cursor)).Name), nameof(handle));
        }

        _freeHandle = false;
        _handle = (HCURSOR)handle;
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="Cursor"/> class with the specified <paramref name="fileName"/>.
    /// </summary>
    public Cursor(string fileName)
    {
        _cursorData = File.ReadAllBytes(fileName);
        _freeHandle = true;

        // Prefer letting the OS load .cur/.ani files directly from the file: it natively understands animated
        // cursors (which LoadCursorFromResourceData cannot parse at all) and correctly picks the hotspot, without
        // the manual ICONDIR parsing below. Since a real file already exists on disk, this requires no extra temp
        // file or memory copy. This is intentionally NOT used for .ico: unlike Windows' own cursor loader, the
        // legacy OLE IPicture path this replaces always used the *first* image entry in a multi-resolution .ico
        // file (no size-based matching), and LoadCursorFromFile does its own OS size selection instead, which
        // would silently change the hotspot/image picked for such files. Fall back to the manual, in-memory
        // parse for .ico files and for anything the OS loader rejects.
        string extension = Path.GetExtension(fileName);
        bool preferNativeLoad = extension.Equals(".cur", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ani", StringComparison.OrdinalIgnoreCase);

        if (!preferNativeLoad || !TryLoadCursorFromFile(fileName))
        {
            LoadCursorFromResourceData(_cursorData, nameof(fileName));
        }
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="Cursor"/> class from the specified <paramref name="resource"/>.
    /// </summary>
    public Cursor(Type type, string resource)
        : this(type.OrThrowIfNull().Module.Assembly.GetManifestResourceStream(type, resource)!)
    {
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="Cursor"/> class from the
    ///  specified data <paramref name="stream"/>.
    /// </summary>
    public Cursor(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using MemoryStream memoryStream = new();

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        stream.CopyTo(memoryStream);
        _cursorData = memoryStream.ToArray();
        _freeHandle = true;

        LoadCursorFromResourceData(_cursorData, nameof(stream));
    }

    /// <summary>
    ///  Gets or sets a <see cref="Rectangle"/> that represents the current clipping
    ///  rectangle for this <see cref="Cursor"/> in screen coordinates.
    /// </summary>
    public static unsafe Rectangle Clip
    {
        get
        {
            PInvoke.GetClipCursor(out RECT rect);
            return rect;
        }
        set
        {
            if (value.IsEmpty)
            {
                PInvoke.ClipCursor((RECT*)null);
            }
            else
            {
                RECT rect = value;
                PInvoke.ClipCursor(&rect);
            }
        }
    }

    /// <summary>
    ///  Gets or sets a <see cref="Cursor"/> that represents the current mouse cursor.
    ///  The value is <see langword="null"/> if the current mouse cursor is not visible.
    /// </summary>
    public static Cursor? Current
    {
        get
        {
            HCURSOR cursor = PInvoke.GetCursor();
            return cursor.IsNull ? null : new Cursor(cursor);
        }
        set => PInvoke.SetCursor(value?._handle ?? HCURSOR.Null);
    }

    /// <summary>
    ///  Gets the Win32 handle for this <see cref="Cursor"/>.
    /// </summary>
    public IntPtr Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle.IsNull, this);
            return (nint)_handle;
        }
    }

    /// <summary>
    ///  Returns the "hot" location of the cursor.
    /// </summary>
    public Point HotSpot
    {
        get
        {
            using ICONINFO info = PInvokeCore.GetIconInfo(this);
            return new Point((int)info.xHotspot, (int)info.yHotspot);
        }
    }

    /// <summary>
    ///  Gets or sets a <see cref="Point"/> that specifies the current cursor position in screen coordinates.
    /// </summary>
    public static Point Position
    {
        get
        {
            PInvoke.GetCursorPos(out Point p);
            return p;
        }
        set => PInvoke.SetCursorPos(value.X, value.Y);
    }

    /// <summary>
    ///  Gets the size of this <see cref="Cursor"/> object.
    /// </summary>
    public Size Size
    {
        get
        {
            if (s_cursorSize.IsEmpty)
            {
                s_cursorSize = SystemInformation.CursorSize;
            }

            return s_cursorSize;
        }
    }

    [SRCategory(nameof(SR.CatData))]
    [Localizable(false)]
    [Bindable(true)]
    [SRDescription(nameof(SR.ControlTagDescr))]
    [DefaultValue(null)]
    [TypeConverter(typeof(StringConverter))]
    public object? Tag { get; set; }

    HICON IHandle<HICON>.Handle => (HICON)Handle;

    HANDLE IHandle<HANDLE>.Handle => (HANDLE)Handle;
    HCURSOR IHandle<HCURSOR>.Handle => _handle;

    /// <summary>
    ///  Duplicates this the Win32 handle of this <see cref="Cursor"/>.
    /// </summary>
    public IntPtr CopyHandle()
    {
        Size sz = Size;
        return (nint)PInvokeCore.CopyCursor(this, sz.Width, sz.Height, IMAGE_FLAGS.LR_DEFAULTCOLOR);
    }

    /// <summary>
    ///  Cleans up the resources allocated by this object. Once called, the cursor object is no longer useful.
    /// </summary>
    public void Dispose()
    {
        if (!_handle.IsNull && _freeHandle)
        {
            PInvoke.DestroyCursor(_handle);
            _handle = HCURSOR.Null;
        }

        GC.SuppressFinalize(this);
    }

    private bool TryLoadCursorFromStream(byte[] cursorData)
    {
        string? extension = GetNativeCursorExtension(cursorData);

        if (extension is null)
        {
            return false;
        }

        string tempFile = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}{extension}");

        try
        {
            File.WriteAllBytes(tempFile, cursorData);

            HCURSOR cursor = LoadCursorFromFile(tempFile);

            if (cursor.IsNull)
            {
                return false;
            }

            _handle = cursor;
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            try
            {
                File.Delete(tempFile);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }

    private static string? GetNativeCursorExtension(byte[] data)
    {
        if (IsCursorFile(data))
        {
            return ".cur";
        }

        if (IsAnimatedCursorFile(data))
        {
            return ".ani";
        }

        return null;
    }

    private static bool IsCursorFile(byte[] data)
    {
        // CUR header:
        // WORD reserved = 0
        // WORD type     = 2
        // WORD count    > 0
        return data.Length >= 6
            && BitConverter.ToUInt16(data, 0) == 0
            && BitConverter.ToUInt16(data, 2) == 2
            && BitConverter.ToUInt16(data, 4) > 0;
    }

    private static bool IsAnimatedCursorFile(byte[] data)
    {
        // ANI header:
        // RIFF .... ACON
        return data.Length >= 12
            && data[0] == (byte)'R'
            && data[1] == (byte)'I'
            && data[2] == (byte)'F'
            && data[3] == (byte)'F'
            && data[8] == (byte)'A'
            && data[9] == (byte)'C'
            && data[10] == (byte)'O'
            && data[11] == (byte)'N';
    }

    private static bool ShouldUseNativeCursorLoader(string fileName)
    {
        string extension = Path.GetExtension(fileName);

        return extension.Equals(".cur", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ani", StringComparison.OrdinalIgnoreCase);
    }

    private static unsafe HCURSOR LoadCursorFromFile(string fileName)
    {
        fixed (char* fileNamePtr = fileName)
        {
            return PInvoke.LoadCursorFromFile((PCWSTR)fileNamePtr);
        }
    }

    /// <summary>
    ///  Draws this image to a graphics object. The drawing command originates on the graphics
    ///  object, but a graphics object generally has no idea how to render a given image. So,
    ///  it passes the call to the actual image. This version crops the image to the given
    ///  dimensions and allows the user to specify a rectangle within the image to draw.
    /// </summary>
    private void DrawImageCore(Graphics graphics, Rectangle imageRect, Rectangle targetRect, bool stretch)
    {
        ArgumentNullException.ThrowIfNull(graphics);

        // Support GDI+ Translate method
        targetRect.X += (int)graphics.Transform.OffsetX;
        targetRect.Y += (int)graphics.Transform.OffsetY;

        using DeviceContextHdcScope dc = new(graphics, applyGraphicsState: false);

        int imageX = 0;
        int imageY = 0;
        int imageWidth;
        int imageHeight;
        int targetX = 0;
        int targetY = 0;
        int targetWidth = 0;
        int targetHeight = 0;

        Size cursorSize = Size;

        // compute the dimensions of the icon, if needed
        if (!imageRect.IsEmpty)
        {
            imageX = imageRect.X;
            imageY = imageRect.Y;
            imageWidth = imageRect.Width;
            imageHeight = imageRect.Height;
        }
        else
        {
            imageWidth = cursorSize.Width;
            imageHeight = cursorSize.Height;
        }

        if (!targetRect.IsEmpty)
        {
            targetX = targetRect.X;
            targetY = targetRect.Y;
            targetWidth = targetRect.Width;
            targetHeight = targetRect.Height;
        }
        else
        {
            targetWidth = cursorSize.Width;
            targetHeight = cursorSize.Height;
        }

        int drawWidth, drawHeight;
        int clipWidth, clipHeight;

        if (stretch)
        {
            // Short circuit the simple case of blasting an icon to the screen
            if (targetWidth == imageWidth && targetHeight == imageHeight
                && imageX == 0 && imageY == 0
                && imageWidth == cursorSize.Width && imageHeight == cursorSize.Height)
            {
                PInvokeCore.DrawIcon(dc, targetX, targetY, this);
                return;
            }

            drawWidth = cursorSize.Width * targetWidth / imageWidth;
            drawHeight = cursorSize.Height * targetHeight / imageHeight;
            clipWidth = targetWidth;
            clipHeight = targetHeight;
        }
        else
        {
            // Short circuit the simple case of blasting an icon to the screen
            if (imageX == 0 && imageY == 0
                && cursorSize.Width <= targetWidth && cursorSize.Height <= targetHeight
                && cursorSize.Width == imageWidth && cursorSize.Height == imageHeight)
            {
                PInvokeCore.DrawIcon(dc, targetX, targetY, this);
                return;
            }

            drawWidth = cursorSize.Width;
            drawHeight = cursorSize.Height;
            clipWidth = targetWidth < imageWidth ? targetWidth : imageWidth;
            clipHeight = targetHeight < imageHeight ? targetHeight : imageHeight;
        }

        // The ROP is SRCCOPY, so we can be simple here and take advantage of clipping regions.
        // Drawing the cursor is merely a matter of offsetting and clipping.
        PInvoke.IntersectClipRect(dc, targetX, targetY, targetX + clipWidth, targetY + clipHeight);
        PInvokeCore.DrawIconEx(
            (HDC)dc,
            targetX - imageX,
            targetY - imageY,
            this,
            drawWidth,
            drawHeight);

        // Let GDI+ restore clipping
        return;
    }

    /// <summary>
    ///  Draws this <see cref="Cursor"/> to a <see cref="Graphics"/>.
    /// </summary>
    public void Draw(Graphics g, Rectangle targetRect)
    {
        DrawImageCore(g, Rectangle.Empty, targetRect, stretch: false);
    }

    /// <summary>
    ///  Draws this <see cref="Cursor"/> to a <see cref="Graphics"/>.
    /// </summary>
    public void DrawStretched(Graphics g, Rectangle targetRect)
    {
        DrawImageCore(g, Rectangle.Empty, targetRect, stretch: true);
    }

    ~Cursor() => Dispose();

    void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
    {
        throw new PlatformNotSupportedException();
    }

    /// <summary>
    ///  Hides the cursor. For every call to Cursor.hide() there must be a balancing call to Cursor.show().
    /// </summary>
    public static void Hide() => PInvoke.ShowCursor(bShow: false);

    /// <summary>
    ///  Attempts to load the cursor directly from <paramref name="fileName"/> using the OS's own cursor loader.
    /// </summary>
    /// <returns>
    ///  <see langword="true"/> if the cursor was loaded successfully; otherwise <see langword="false"/>.
    /// </returns>
    private unsafe bool TryLoadCursorFromFile(string fileName)
    {
        fixed (char* lpFileName = fileName)
        {
            HCURSOR cursor = PInvoke.LoadCursorFromFile(lpFileName);
            if (cursor.IsNull)
            {
                return false;
            }

            _handle = cursor;
            return true;
        }
    }

    /// <summary>
    ///  Loads the cursor image directly from the raw <paramref name="cursorData"/> bytes of a .cur or .ico file.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///   This manually parses the ICONDIR/ICONDIRENTRY structures (the .cur format is identical to .ico,
    ///   other than <c>idType</c> and the fact that the per-entry <c>wPlanes</c>/<c>wBitCount</c> fields are
    ///   reused to store the hotspot) and hands the raw resource bytes for the best-matching image directly
    ///   to <c>PInvoke.CreateIconFromResourceEx</c>. This mirrors <c>Icon.Initialize</c> and, unlike the legacy OLE
    ///   <c>IPicture</c>/<see cref="PInvokeCore.CopyImage(HANDLE, GDI_IMAGE_TYPE, int, int, IMAGE_FLAGS)"/>
    ///   pipeline this replaces, correctly preserves the alpha channel of modern 32-bit cursors.
    ///  </para>
    /// </remarks>
    private unsafe void LoadCursorFromResourceData(byte[] cursorData, string paramName)
    {
        try
        {
            SpanReader<byte> reader = new(cursorData);

            // .cur files use idType 2. Plain .ico files (idType 1) are also historically accepted here (they
            // were previously loaded via the OLE IPicture/PICTYPE_ICON path) and are treated as icons, getting
            // an OS-centered hotspot.
            if (!reader.TryRead(out ICONDIR dir)
                || dir.idReserved != 0
                || (dir.idType != 1 && dir.idType != 2)
                || dir.idCount == 0
                || !reader.TryRead(dir.idCount, out ReadOnlySpan<ICONDIRENTRY> entries))
            {
                throw new ArgumentException(string.Format(SR.InvalidPictureType, nameof(cursorData), nameof(Cursor)), paramName);
            }

            bool isIcon = dir.idType == 1;

            uint bestImageOffset;
            uint bestBytesInRes;
            ushort bestHotspotX = 0;
            ushort bestHotspotY = 0;

            if (isIcon)
            {
                // Historically, loading an .ico file as a Cursor went through the OLE IPicture pipeline, which
                // only ever surfaces the first image in the file (it has no notion of picking a "best" size).
                // Preserve that behavior for back-compat rather than doing size-based matching like Icon does.
                ICONDIRENTRY entry = entries[0];
                bestImageOffset = entry.dwImageOffset;
                bestBytesInRes = entry.dwBytesInRes;
            }
            else
            {
                int desiredWidth = PInvokeCore.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXCURSOR);
                int desiredHeight = PInvokeCore.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYCURSOR);

                bestImageOffset = 0;
                bestBytesInRes = 0;
                int bestDelta = int.MaxValue;

                // Cursor files can contain multiple resolutions of the same cursor. Pick the entry whose size
                // is closest to the system's desired cursor size, same as Windows does when loading cursors.
                // Note: the wPlanes/wBitCount fields (which hold color plane/bit depth information for icons)
                // are repurposed by the .cur format to store the hotspot x/y coordinates.
                foreach (ICONDIRENTRY entry in entries)
                {
                    int entryWidth = entry.bWidth == 0 ? 256 : entry.bWidth;
                    int entryHeight = entry.bHeight == 0 ? 256 : entry.bHeight;
                    int delta = Math.Abs(entryWidth - desiredWidth) + Math.Abs(entryHeight - desiredHeight);

                    if (delta < bestDelta)
                    {
                        bestDelta = delta;
                        bestImageOffset = entry.dwImageOffset;
                        bestBytesInRes = entry.dwBytesInRes;
                        bestHotspotX = entry.wPlanes;
                        bestHotspotY = entry.wBitCount;
                    }
                }
            }

            if (bestImageOffset > int.MaxValue || bestBytesInRes > int.MaxValue)
            {
                throw new Win32Exception((int)WIN32_ERROR.ERROR_INVALID_PARAMETER);
            }

            uint endOffset = checked(bestImageOffset + bestBytesInRes);
            if (endOffset > cursorData.Length)
            {
                throw new ArgumentException(string.Format(SR.InvalidPictureType, nameof(cursorData), nameof(Cursor)), paramName);
            }

            ReadOnlySpan<byte> bestImage = reader.Span.Slice((int)bestImageOffset, (int)bestBytesInRes);

            if (isIcon)
            {
                // Icon resource data is passed to CreateIconFromResourceEx as-is; the OS centers the hotspot.
                fixed (byte* b = bestImage)
                {
                    _handle = (HCURSOR)PInvoke.CreateIconFromResourceEx(b, (uint)bestImage.Length, fIcon: true, 0x00030000, 0, 0, 0).Value;
                }
            }
            else
            {
                // Unlike icon resource data, cursor resource data passed to CreateIconFromResourceEx must be
                // prefixed with the hotspot as two little-endian WORDs, which is not part of the .cur file's
                // per-image data block (the hotspot only lives in the ICONDIRENTRY there). Build that buffer here.
                using BufferScope<byte> imageBuffer = new(sizeof(ushort) * 2 + (int)bestBytesInRes);
                Span<byte> imageSpan = imageBuffer.AsSpan();
                BinaryPrimitives.WriteUInt16LittleEndian(imageSpan, bestHotspotX);
                BinaryPrimitives.WriteUInt16LittleEndian(imageSpan[2..], bestHotspotY);
                bestImage.CopyTo(imageSpan[4..]);

                fixed (byte* b = imageBuffer)
                {
                    _handle = (HCURSOR)PInvoke.CreateIconFromResourceEx(b, (uint)imageSpan.Length, fIcon: false, 0x00030000, 0, 0, 0).Value;
                }
            }

            if (_handle.IsNull)
            {
                throw new Win32Exception(string.Format(SR.FailedToLoadCursor, Marshal.GetLastWin32Error()));
            }
        }
        catch (COMException e)
        {
            throw new ArgumentException(SR.InvalidPictureFormat, paramName, e);
        }
    }

    /// <summary>
    ///  Saves a picture from the requested stream.
    /// </summary>
    internal unsafe byte[] GetData()
    {
        if (_cursorData is null)
        {
            throw CursorsProperty is null
                ? new InvalidOperationException(SR.InvalidPictureFormat)
                : new FormatException(SR.CursorCannotCovertToBytes);
        }

        return (byte[])_cursorData.Clone();
    }

    /// <summary>
    ///  Displays the cursor. For every call to Cursor.show() there must have been
    ///  a previous call to Cursor.hide().
    /// </summary>
    public static void Show() => PInvoke.ShowCursor(bShow: true);

    /// <summary>
    ///  Retrieves a human readable string representing this <see cref="Cursor"/>.
    /// </summary>
    public override string ToString() => $"[Cursor: {CursorsProperty ?? base.ToString()}]";

    public static bool operator ==(Cursor? left, Cursor? right)
    {
        return right is null || left is null ? left is null && right is null : left._handle == right._handle;
    }

    public static bool operator !=(Cursor? left, Cursor? right) => !(left == right);

    public override unsafe int GetHashCode() => (int)_handle.Value;

    public override bool Equals(object? obj) => obj is Cursor cursor && this == cursor;
}
