using FMO.ESigning.MeiShi;

namespace FMO.ESigning;

public static class SigningGalley
{
    public static ESignViewModelBase[] ViewModels { get; set; }

    public static ESigningWorker Worker { get; }

    static SigningGalley()
    {
        ViewModels = [new MeiShiViewModel()];

        foreach (var item in ViewModels)
            item.Load();

        Worker = new ESigningWorker(ViewModels.Select(x => x.Signing));

    }

    /// <summary>
    /// 放到首页中
    /// </summary>
    public static void Initialize()
    {
#if !DEBUG
        Worker.Start();
#endif
    }
}
