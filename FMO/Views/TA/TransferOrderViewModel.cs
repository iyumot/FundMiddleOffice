using FMO.Models;
using FMO.Shared;

namespace FMO;

[AutoChangeableViewModel(typeof(TransferOrder))]
partial class TransferOrderViewModel
{
    public bool IsConfirmed { get => field; set { field = value; OnPropertyChanged(nameof(IsConfirmed)); } }

    /// <summary>
    /// 是否已申请
    /// </summary>
    public bool IsApplyed { get =>field; set { field = value; OnPropertyChanged(nameof(IsApplyed)); } }
}
