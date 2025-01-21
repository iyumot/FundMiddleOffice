using FMO.Models;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public class ElementItemViewModel<T> : ElementItemViewModel
{
    public ValueViewModel<T> Data { get; } = new();


    public override bool CanConfirm => Data.IsChanged;


    [SetsRequiredMembers]
    public ElementItemViewModel(FundElements elements, string property, int flowid)
    {
        Property = property;
        var obj = elements.GetType().GetProperty(property)?.GetValue(elements);

        if (obj is Mutable<T> mutable)
        {
            Label = mutable.Description!;
            (int fid, T? dec) = mutable.GetValue(flowid);
            Data.Old = fid == flowid ? dec : fid == -1 ? default : dec;
            Data.New = fid == flowid ? dec : default;
        }
        else if (obj is Mutable<T?> mutable2)
        {
            Label = mutable2.Description!;
            (int fid, T? dec) = mutable2.GetValue(flowid);
            Data.Old = fid == flowid ? dec : fid == -1 ? default : dec;
            Data.New = fid == flowid ? dec : default;
        }
        else
            throw new NotImplementedException();

        Data.PropertyChanged += ItemPropertyChanged;
    }


    public override void Init(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (obj is Mutable<T> mutable)
        {
            (int fid, T? dec) = mutable.GetValue(flowid);
            Data.Old = fid == flowid ? dec : fid == -1 ? default : dec;
            Data.New = fid == flowid ? dec : default;
        }
        else if (obj is Mutable<T?> mutable2)
        {
            (int fid, T? dec) = mutable2.GetValue(flowid);
            Data.Old = fid == flowid ? dec : fid == -1 ? default : dec;
            Data.New = fid == flowid ? dec : default;
        }

        Init();
    }

    public override void UpdateEntity(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<T> mutable)
            mutable.SetValue(Data.New, flowid);

        else if (obj is Mutable<T?> mutable2)
            mutable2.SetValue(Data.New, flowid);
    }

    public override void Apply()
    {
        Data.Apply();
        OnPropertyChanged(nameof(CanConfirm));
    }
}
