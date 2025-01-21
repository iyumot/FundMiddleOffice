using FMO.Models;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public class ElementItemViewModelSealing : ElementItemViewModel
{
    public ValueViewModel<SealingType?> Type { get; } = new();

    public ValueViewModel<int> Month { get; } = new();

    public ValueViewModel<string?> Extra { get; } = new();

    public override void UpdateEntity(FundElements elements, int fid)
    {
        SealingRule? sealingRule = Type.New is null ? null : new SealingRule { Type = Type.New.Value, Month = Month.New, Extra = Extra.New };
        if (sealingRule is null)
            elements.SealingRule!.RemoveValue(fid);
        else
            elements.SealingRule!.SetValue(sealingRule, fid);
    }


    public override bool CanConfirm => Type.New switch { SealingType.No => Type.New != Type.Old, SealingType.Has => Month.New > 0 && Month.New != Month.Old, SealingType.Other => !string.IsNullOrWhiteSpace(Extra.New) && Extra.New != Extra.Old, _ => false };

    [SetsRequiredMembers]
    public ElementItemViewModelSealing(FundElements elements, string property, int flowid)
    {
        Property = property;
        var mutable = property switch { nameof(FundElements.SealingRule) => elements.SealingRule!, nameof(FundElements.LockingRule) => elements.LockingRule!, _ => throw new Exception() };

        Label = mutable.Description!;
        (int fid, var dec) = mutable.GetValue(flowid);
        Type.Old = fid == flowid ? dec?.Type : fid == -1 ? null : dec?.Type;
        Type.New = fid == flowid ? dec?.Type : fid == -1 ? null : dec?.Type;

        Month.Old = fid == flowid ? dec?.Month ?? 0 : 0;
        Month.New = Month.Old;

        Extra.Old = fid == flowid ? dec?.Extra : fid == -1 ? null : dec?.Extra;
        Extra.New = fid == flowid ? dec?.Extra : fid == -1 ? null : dec?.Extra;

        Type.PropertyChanged += ItemPropertyChanged;
        Extra.PropertyChanged += ItemPropertyChanged;
    }

    public override void Init(FundElements elements, int flowid)
    {
        (int fid, var obj) = elements.SealingRule!.GetValue(flowid);
        if (obj is null) return;

        Type.Old = obj.Type;
        Type.New = obj.Type;

        Month.Old = obj.Month;
        Month.New = obj.Month;

        Extra.Old = obj.Extra;
        Extra.New = obj.Extra;
    }
}
