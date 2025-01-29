using FMO.Models;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public class ElementItemFundModeViewModel : ElementItemViewModel
{
    public ValueViewModel<FundMode?> Data { get; } = new();

    public ValueViewModel<string?> Extra { get; } = new();

    public override bool CanConfirm => Data.New switch { FundMode.Other => Extra.IsChanged && !string.IsNullOrWhiteSpace(Extra.New), _ => Data.IsChanged };



    [SetsRequiredMembers]
    public ElementItemFundModeViewModel(FundElements elements, string property, int flowid, string label)
    {
        Property = property;
        var mutable = elements.FundModeInfo!;

        Label = label;// mutable.Description!;
        (int fid, var dec) = mutable.GetValue(flowid);
        Data.Old = fid == flowid ? dec?.Data : fid == -1 ? null : dec?.Data;
        Data.New = fid == flowid ? dec?.Data : fid == -1 ? null : dec?.Data;

        Extra.Old = fid == flowid ? dec?.Extra : fid == -1 ? null : dec?.Extra;
        Extra.New = fid == flowid ? dec?.Extra : fid == -1 ? null : dec?.Extra;

        Data.PropertyChanged += ItemPropertyChanged;
        Extra.PropertyChanged += ItemPropertyChanged;
    }

    public override void UpdateEntity(FundElements elements, int fid)
    {
        DataExtra<FundMode>? val = Data.New is null ? null : new DataExtra<FundMode> { Data = Data.New.Value, Extra = Extra.New };
        if (val is null)
            elements.FundModeInfo!.RemoveValue(fid);
        else
            elements.FundModeInfo!.SetValue(val, fid);
    }

    public override void Init(FundElements elements, int flowid)
    {
        (int fid, var obj) = elements.FundModeInfo!.GetValue(flowid);
        if (obj is null) return;

        Data.Old = obj.Data;
        Data.New = obj.Data;

        Extra.Old = obj.Extra;
        Extra.New = obj.Extra;
    }
}
