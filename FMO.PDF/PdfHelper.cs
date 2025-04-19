using PDFiumSharp;
using PDFiumSharp.Enums;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FMO.PDF;

public static class PdfHelper
{


    public static (string text, byte[] page)[]? GetSecurityAccounts(Stream s)
    {
        List<(string text, byte[] page)> result = new();


        {
            List<string> strings = new List<string>();

            byte[] buf = new byte[s.Length];
            s.Seek(0, SeekOrigin.Begin);
#pragma warning disable CA2022 // 避免使用 "Stream.Read" 进行不准确读取
            s.Read(buf, 0, buf.Length);
#pragma warning restore CA2022 // 避免使用 "Stream.Read" 进行不准确读取
            var d = PDFium.FPDF_LoadDocument(buf);
            var cnt = PDFium.FPDF_GetPageCount(d);


            for (int i = 0; i < cnt; i++)
            {
                StringBuilder sb = new();
                var page = PDFium.FPDF_LoadPage(d, i);
                var textpage = PDFium.FPDFText_LoadPage(page);
                var charcnt = PDFium.FPDFText_CountChars(textpage);
                var rotate = PDFium.FPDFPage_GetRotation(page);

                var str = PDFium.FPDFText_GetText(textpage, 0, charcnt);

                double lb = 0, lr = 0, width = 0; int chcnt = 0;
                double[] rect = [0, 0, 0, 0];

                for (int j = 0; j < charcnt; j++)
                {
                    var ok = PDFium.FPDFText_GetCharBox(textpage, j, out double l, out double r, out double b, out double t);
                    if (!ok) break;

                    Rotate(rotate, ref l, ref t, ref r, ref b);
                    width += r - l;
                    ++chcnt;

                    if (j > 5 && str[j - 2] == '\r' && str[j - 1] == '\n' && Math.Abs(b - lb) < Math.Abs(t - b) / 2)
                        sb.Remove(sb.Length - 2, 2);

                    //Debug.WriteLine($"{str[j]}       {l:n2},{t:n2},{r:n2},{b:n2}        {l - lr:n2} {t - lb:n2}");

                    if (str[j]!=' ' && lr > 0 && lb > 0 && ((l - lr < 0 && t - lb > 3) || (l - lr > width / chcnt * 8)))
                    {
                        sb.Append('\n');
                        rect = [0, 0, 0, 0];
                        width = 0;
                        chcnt = 0;
                    }

                    if (str[j] != ' ')
                    { lr = r; lb = b; }

                    var value = str[j];
                    sb.Append(value);

                }

                PDFium.FPDFText_ClosePage(textpage);
                PDFium.FPDF_ClosePage(page);

                var ss = sb.ToString();

                var newdoc = PDFium.FPDF_CreateNewDocument();
                PDFium.FPDF_ImportPages(newdoc, d, 0, [i]);
                using var ms = new MemoryStream();
                PDFium.FPDF_SaveAsCopy(newdoc, ms, SaveFlags.None);
                PDFium.FPDF_CloseDocument(newdoc);

                result.Add((ss, ms.ToArray()));
            }


            PDFium.FPDF_CloseDocument(d);
        }


        return result.ToArray();//doc.GetPages().Select(p => Regex.Replace( string.Join('\n', p.GetWords().Select(x => x.Text)),"\n([^:：]+\n|$)","$1" )).ToArray();
    }










    private static void Rotate(PageOrientations o, ref double l, ref double t, ref double r, ref double b)
    {
        switch (o)
        {
            case PageOrientations.Rotated90CW:
                {
                    var tmp = b;
                    b = r;
                    r = t;
                    t = l;
                    l = tmp;
                }
                break;
            case PageOrientations.Rotated180:
                {
                    var tmp = l;
                    l = r;
                    r = tmp;
                    tmp = t;
                    t = b;
                    b = tmp;
                }
                break;
            case PageOrientations.Rotated90CCW:
                {
                    var tmp = l;
                    l = t;
                    t = r;
                    r = b;
                    b = tmp;
                }
                break;
        }
    }


    internal static WriteableBitmap Render(string path)
    {
        using PDFiumSharp.PdfDocument doc = new PDFiumSharp.PdfDocument(path);
        int pagecnt = doc.Pages.Count;

        var rects = doc.Pages.Select(x => new Rect(0, 0, x.Width, x.Height)).ToArray();
        for (int i = 1; i < rects.Length; i++)
            rects[i].Offset(0, rects[i - 1].Bottom);

        var wb = new WriteableBitmap((int)rects.Max(x => x.Width), (int)rects[^1].Bottom, 72, 72, PixelFormats.Bgr32, null);
        int stride = wb.Format.BitsPerPixel / 8;
        var buf = new byte[wb.PixelWidth * wb.PixelHeight * stride];
        Array.Fill<byte>(buf, 0xff);
        wb.WritePixels(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight), buf, wb.PixelWidth * stride, 0);


        for (int i = 0; i < rects.Length; i++)
        {
            var page = doc.Pages[i];
            page.Render(wb, rects[i], PDFiumSharp.Enums.PageOrientations.Normal, PDFiumSharp.Enums.RenderingFlags.None);
        }
        return wb;
    }


    public static MemoryStream ExportToPdf(string[] images)
    {
        var doc = PDFium.FPDF_CreateNewDocument();
        int index = 0;
        List<PDFiumSharp.Types.FPDF_PAGE> pages = new();
        foreach (var f in images)
        {
            using var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var img = PDFium.FPDFPageObj_NewImageObj(doc);
            var page = PDFium.FPDFPage_New(doc, index++, 595, 841);
            pages.Add(page);
            PDFium.FPDFImageObj_LoadJpegFile(pages.ToArray(), img, fs);
            PDFium.FPDFImageObj_GetImagePixelSize(img, out uint width, out uint height);
            PDFium.FPDFImageObj_SetMatrix(img, 595, 0, 0, 841, 0, 0);
            PDFium.FPDFPage_InsertObject(page, ref img);
            PDFium.FPDFPage_GenerateContent(page);
        }
        var ms = new MemoryStream();
        PDFium.FPDF_SaveAsCopy(doc, ms, PDFiumSharp.Enums.SaveFlags.None);

        PDFium.FPDF_CloseDocument(doc);
        return ms;
    }

    internal static MemoryStream? GenThumbnail(MemoryStream? stream)
    {
        if (stream is null) return null;
        var pdf = new PdfDocument(stream.ToArray());
        var wb = new WriteableBitmap(200, 283, 96, 96, PixelFormats.Bgr32, null);
        pdf.Pages[0].Render(wb);
        wb.Freeze();

        var encoder = new JpegBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(wb));
        var ms = new MemoryStream();
        encoder.Save(ms);
        return ms;
    }


    /// <summary>
    /// Renders the page to a <see cref="WriteableBitmap"/>
    /// </summary>
    /// <param name="page">The page which is to be rendered.</param>
    /// <param name="renderTarget">The bitmap to which the page is to be rendered.</param>
    /// <param name="rectDest">The destination rectangle in <paramref name="renderTarget"/>.</param>
    /// <param name="orientation">The orientation at which the page is to be rendered.</param>
    /// <param name="flags">The flags specifying how the page is to be rendered.</param>
    public static void Render(this PdfPage page, WriteableBitmap renderTarget, (int left, int top, int width, int height) rectDest, PageOrientations orientation = PageOrientations.Normal, RenderingFlags flags = RenderingFlags.None)
    {
        if (renderTarget == null)
            throw new ArgumentNullException(nameof(renderTarget));

        if (rectDest.left >= renderTarget.PixelWidth || rectDest.top >= renderTarget.PixelHeight)
            return;

        var bitmapFormat = GetBitmapFormat(renderTarget.Format);
        renderTarget.Lock();
        using (var tmpBitmap = new PDFiumBitmap(renderTarget.PixelWidth, renderTarget.PixelHeight, bitmapFormat, renderTarget.BackBuffer, renderTarget.BackBufferStride))
        {
            page.Render(tmpBitmap, rectDest, orientation, flags);
        }

        if (rectDest.left < 0)
        {
            rectDest.width += rectDest.left;
            rectDest.left = 0;
        }
        if (rectDest.top < 0)
        {
            rectDest.height += rectDest.top;
            rectDest.top = 0;
        }
        rectDest.width = Math.Min(rectDest.width, renderTarget.PixelWidth);
        rectDest.height = Math.Min(rectDest.height, renderTarget.PixelHeight);
        renderTarget.AddDirtyRect(new Int32Rect(rectDest.left, rectDest.top, rectDest.width, rectDest.height));
        renderTarget.Unlock();
    }

    /// <summary>
    /// Renders the page to a <see cref="WriteableBitmap"/>
    /// </summary>
    /// <param name="page">The page which is to be rendered.</param>
    /// <param name="renderTarget">The bitmap to which the page is to be rendered.</param>
    /// <param name="orientation">The orientation at which the page is to be rendered.</param>
    /// <param name="flags">The flags specifying how the page is to be rendered.</param>
    public static void Render(this PdfPage page, WriteableBitmap renderTarget, PageOrientations orientation = PageOrientations.Normal, RenderingFlags flags = RenderingFlags.None)
    {
        page.Render(renderTarget, (0, 0, renderTarget.PixelWidth, renderTarget.PixelHeight), orientation, flags);
    }

    public static void Render(this PdfPage page, WriteableBitmap renderTarget, Rect rect, PageOrientations orientation = PageOrientations.Normal, RenderingFlags flags = RenderingFlags.None)
    {
        page.Render(renderTarget, ((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height), orientation, flags);
    }

    static BitmapFormats GetBitmapFormat(PixelFormat pixelFormat)
    {
        if (pixelFormat == PixelFormats.Bgra32)
            return BitmapFormats.BGRA;
        if (pixelFormat == PixelFormats.Bgr32)
            return BitmapFormats.BGRx;
        if (pixelFormat == PixelFormats.Bgr24)
            return BitmapFormats.BGR;
        if (pixelFormat == PixelFormats.Gray8)
            return BitmapFormats.Gray;
        throw new NotSupportedException($"Pixel format {pixelFormat} is not supported.");
    }
}