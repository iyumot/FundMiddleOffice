using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Online;
using System;

namespace FMO.OCR;

public static class OCRWorker
{
    public static async Task<PaddleOcrResult> VerifyCode(byte[] buf)
    {
        var model = await OnlineFullModels.EnglishV4.DownloadAsync();

        using PaddleOcrAll all = new PaddleOcrAll(model, PaddleDevice.Mkldnn())
        {
            AllowRotateDetection = true, /* 允许识别有角度的文字 */
            Enable180Classification = false, /* 允许识别旋转角度大于90度的文字 */
        };

        using (Mat src = Cv2.ImDecode(buf, ImreadModes.Color))
        {
            return all.Run(src);
        }

    }
}
