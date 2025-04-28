using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;

namespace FMO.OCR;

public static class OCRWorker
{
    public static void Init()
    {
        FullOcrModel model = FullOcrModel.FromDirectory("", "","","", ModelVersion.V4);// new FullOcrModel(DetectionModel.FromDirectory(""ModelVersion.V4))




        using (PaddleOcrAll all = new PaddleOcrAll(model, PaddleDevice.Mkldnn())
        {
            AllowRotateDetection = true, /* 允许识别有角度的文字 */
            Enable180Classification = false, /* 允许识别旋转角度大于90度的文字 */
        })
        {
            // Load local file by following code: 
            using (Mat src = Cv2.ImDecode(sampleImageData, ImreadModes.Color))
            {
                PaddleOcrResult result = all.Run(src);
                Console.WriteLine("Detected all texts: \n" + result.Text);
                foreach (PaddleOcrResultRegion region in result.Regions)
                {
                    Console.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
                }
            }
        }
    }
}
