using FMO.Models;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public class ElementItemViewModel<T> : ElementItemViewModel
{
    public ValueViewModel<T> Data { get; } = new();


    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => Data.New is not null;

    [SetsRequiredMembers]
    public ElementItemViewModel(FundElements elements, string property, int flowid, string label)
    {
        Property = property;
        var obj = elements.GetType().GetProperty(property)?.GetValue(elements);

        if (obj is Mutable<T> mutable)
        {
            Label = label;
            (int fid, T? dec) = mutable.GetValue(flowid);
            Data.Old = fid == flowid ? dec : fid == -1 ? default : dec;
            Data.New = fid == flowid ? dec : default;
        }
        else if (obj is Mutable<T?> mutable2)
        {
            Label = label;
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



public class ElementItemWithEnumViewModel<TEnum, TValue> : ElementItemViewModel where TEnum : struct, Enum where TValue : struct
{
    public ValueViewModel<TEnum> Type { get; } = new();

    public ValueViewModel<TValue?> Data { get; } = new();

    public ValueViewModel<string> Extra { get; } = new();



    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => 



    [SetsRequiredMembers]
    public ElementItemWithEnumViewModel(FundElements elements, string property, int flowid, string label) //: base(elements, property, flowid, label)
    {
        Property = property;
        var obj = elements.GetType().GetProperty(property)?.GetValue(elements) as dynamic;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
        {
            Label = label;
            (int fid, ValueWithEnum<TEnum, TValue>? dec) = mutable.GetValue(flowid);

            Type.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Type : default;
            Type.New = Type.Old;

            Data.Old = fid == -1 ? null : fid == flowid && dec is not null ? dec.Value : null;
            Data.New = Data.Old;

            Extra.Old = fid == -1 ? null : fid == flowid && dec is not null ? dec.Extra : null;
            Extra.New = Extra.Old;

        }
        else if (obj is Mutable<ValueWithEnum<TEnum, TValue?>> mutable2)
        {
            Label = label;
            (int fid, ValueWithEnum<TEnum, TValue?>? dec) = mutable2.GetValue(flowid);

            Type.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Type : default;
            Type.New = Type.Old;

            Data.Old = fid == -1 ? null : fid == flowid && dec is not null ? dec.Value : null;
            Data.New = Data.Old;

            Extra.Old = fid == -1 ? null : fid == flowid && dec is not null ? dec.Extra : null;
            Extra.New = Extra.Old;
        }
        else
            throw new NotImplementedException();

        Type.PropertyChanged += ItemPropertyChanged;
        Data.PropertyChanged += ItemPropertyChanged;
        Extra.PropertyChanged += ItemPropertyChanged;
    }

    public override void UpdateEntity(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.SetValue(new ValueWithEnum<TEnum, TValue> { Type = Type.New, Value = Data.New.Value, Extra = Extra.New }, flowid);

        //else if (obj is Mutable<T?> mutable2)
        //    mutable.SetValue(new ValueWithEnum<TEnum, TValue> { Type = Type.New, Value = Data.New, Extra = Extra.New }, flowid);

        else throw new NotImplementedException();
    }

    public override void Init(FundElements elements, int flowid)
    {
        throw new NotImplementedException();
    }

    public override void Apply()
    {
        Type.Apply();
        Data.Apply();
        Extra.Apply();
    }
}




public class ElementRefItemWithEnumViewModel<TEnum, TValue> : ElementItemViewModel where TEnum : struct, Enum where TValue : class
{
    public ValueViewModel<TEnum> Type { get; } = new();

    public ValueViewModel<TValue?> Data { get; } = new();

    public ValueViewModel<string> Extra { get; } = new();



    public override bool CanConfirm => Data.IsChanged;




    [SetsRequiredMembers]
    public ElementRefItemWithEnumViewModel(FundElements elements, string property, int flowid, string label) //: base(elements, property, flowid, label)
    {
        Property = property;
        var obj = elements.GetType().GetProperty(property)?.GetValue(elements) as dynamic;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
        {
            Label = label;
            (int fid, ValueWithEnum<TEnum, TValue>? dec) = mutable.GetValue(flowid);

            Type.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Type : default;
            Type.New = Type.Old;

            Data.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Value : default;
            Data.New = Data.Old;

            Extra.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Extra : default;
            Extra.New = Extra.Old;

        }
        else if (obj is Mutable<ValueWithEnum<TEnum, TValue?>> mutable2)
        {
            Label = label;
            (int fid, ValueWithEnum<TEnum, TValue?>? dec) = mutable2.GetValue(flowid);

            Type.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Type : default;
            Type.New = Type.Old;

            Data.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Value : default;
            Data.New = Data.Old;

            Extra.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Extra : default;
            Extra.New = Extra.Old;
        }
        else
            throw new NotImplementedException();

        Type.PropertyChanged += ItemPropertyChanged;
        Data.PropertyChanged += ItemPropertyChanged;
        Extra.PropertyChanged += ItemPropertyChanged;
    }

    public override void UpdateEntity(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.SetValue(new ValueWithEnum<TEnum, TValue> { Type = Type.New, Value = Data.New, Extra = Extra.New }, flowid);

        //else if (obj is Mutable<T?> mutable2)
        //    mutable.SetValue(new ValueWithEnum<TEnum, TValue> { Type = Type.New, Value = Data.New, Extra = Extra.New }, flowid);

        else throw new NotImplementedException();
    }

    public override void Init(FundElements elements, int flowid)
    {
        throw new NotImplementedException();
    }

    public override void Apply()
    {
        Type.Apply();
        Data.Apply();
        Extra.Apply();
    }
}



public class ElementWithBooleanViewModel<T> : ElementItemViewModel where T : struct
{
    /// <summary>
    /// 是否采用
    /// </summary>
    public ValueViewModel<bool> IsAdopted { get; } = new();

    public ValueViewModel<T?> Data { get; } = new();



    public override bool CanConfirm => (IsAdopted.New && Data.IsChanged) || (!IsAdopted.New && IsAdopted.Old);




    [SetsRequiredMembers]
    public ElementWithBooleanViewModel(FundElements elements, string property, int flowid, string label) //: base(elements, property, flowid, label)
    {
        Property = property;
        var obj = elements.GetType().GetProperty(property)?.GetValue(elements);

        if (obj is Mutable<ValueWithBoolean<T>> mutable)
        {
            Label = label;
            (int fid, ValueWithBoolean<T>? dec) = mutable.GetValue(flowid);

            IsAdopted.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.IsAdopted : default;
            IsAdopted.New = IsAdopted.Old;

            Data.Old = fid == -1 ? null : fid == flowid && dec is not null ? dec.Value : null;
            Data.New = Data.Old;

        }
        else if (obj is Mutable<ValueWithBoolean<T?>> mutable2)
        {
            Label = label;
            (int fid, ValueWithBoolean<T?>? dec) = mutable2.GetValue(flowid);

            IsAdopted.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.IsAdopted : default;
            IsAdopted.New = IsAdopted.Old;

            Data.Old = fid == -1 ? null : fid == flowid && dec is not null ? dec.Value : null;
            Data.New = Data.Old;
        }
        else
            throw new NotImplementedException();

        IsAdopted.PropertyChanged += ItemPropertyChanged;
        Data.PropertyChanged += ItemPropertyChanged;
    }

    public override void UpdateEntity(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<ValueWithBoolean<T>> mutable)
            mutable.SetValue(new ValueWithBoolean<T> { IsAdopted = IsAdopted.New, Value = Data.New.Value }, flowid);
        else if (obj is Mutable<ValueWithBoolean<T?>> mutable2)
            mutable2.SetValue(new ValueWithBoolean<T?> { IsAdopted = IsAdopted.New, Value = Data.New.Value }, flowid);
        else throw new NotImplementedException();
    }

    public override void Init(FundElements elements, int flowid)
    {
        throw new NotImplementedException();
    }

    public override void Apply()
    {
        IsAdopted.Apply();
        Data.Apply();

    }

}


public class PortionElementItemWithEnumViewModel<TEnum, TValue> : ElementItemViewModel where TEnum : struct, Enum
{
    public ValueViewModel<TEnum> Type { get; } = new();

    public ValueViewModel<TValue> Data { get; } = new();

    public ValueViewModel<string> Extra { get; } = new();



    public override bool CanConfirm => Data.IsChanged;

    public string Share { get; }


    [SetsRequiredMembers]
    public PortionElementItemWithEnumViewModel(FundElements elements, string share, string property, int flowid, string label) //: base(elements, property, flowid, label)
    {
        Property = property;
        var obj = elements.GetType().GetProperty(property)?.GetValue(elements);

        if (obj is PortionMutable<ValueWithEnum<TEnum, TValue>> mutable)
        {
            Label = label;
            (int fid, ValueWithEnum<TEnum, TValue>? dec) = mutable.GetValue(share, flowid);

            Type.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Type : default;
            Type.New = Type.Old;

            Data.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Value : default;
            Data.New = Data.Old;

            Extra.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Extra : default;
            Extra.New = Extra.Old;

        }
        else if (obj is PortionMutable<ValueWithEnum<TEnum, TValue>> mutable2)
        {
            Label = label;
            (int fid, ValueWithEnum<TEnum, TValue>? dec) = mutable2.GetValue(share, flowid);

            Type.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Type : default;
            Type.New = Type.Old;

            Data.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Value : default;
            Data.New = Data.Old;

            Extra.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Extra : default;
            Extra.New = Extra.Old;
        }
        else
            throw new NotImplementedException();

        Type.PropertyChanged += ItemPropertyChanged;
        Data.PropertyChanged += ItemPropertyChanged;
        Extra.PropertyChanged += ItemPropertyChanged;
        Share = share;
    }


    [SetsRequiredMembers]
    public PortionElementItemWithEnumViewModel(FundElements elements, string property, int flowid, string label) //: base(elements, property, flowid, label)
    {
        Property = property;
        var obj = elements.GetType().GetProperty(property)?.GetValue(elements) as dynamic;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
        {
            Label = label;
            (int fid, ValueWithEnum<TEnum, TValue>? dec) = mutable.GetValue(flowid);

            Type.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Type : default;
            Type.New = Type.Old;

            Data.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Value : default;
            Data.New = Data.Old;

            Extra.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Extra : default;
            Extra.New = Extra.Old;

        }
        else if (obj is Mutable<ValueWithEnum<TEnum, TValue?>> mutable2)
        {
            Label = label;
            (int fid, ValueWithEnum<TEnum, TValue?>? dec) = mutable2.GetValue(flowid);

            Type.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Type : default;
            Type.New = Type.Old;

            Data.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Value : default;
            Data.New = Data.Old;

            Extra.Old = fid == -1 ? default : fid == flowid && dec is not null ? dec.Extra : default;
            Extra.New = Extra.Old;
        }
        else
            throw new NotImplementedException();

        Type.PropertyChanged += ItemPropertyChanged;
        Data.PropertyChanged += ItemPropertyChanged;
        Extra.PropertyChanged += ItemPropertyChanged;
        Share = FundElements.SingleShareKey;
    }

    public override void UpdateEntity(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is PortionMutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.SetValue(Share, new ValueWithEnum<TEnum, TValue> { Type = Type.New, Value = Data.New, Extra = Extra.New }, flowid);

        else if (obj is PortionMutable<ValueWithEnum<TEnum, TValue>?> mutable2)
            mutable2.SetValue(Share, new ValueWithEnum<TEnum, TValue> { Type = Type.New, Value = Data.New, Extra = Extra.New }, flowid);

        else throw new NotImplementedException();
    }

    public override void Init(FundElements elements, int flowid)
    {
        throw new NotImplementedException();
    }

    public override void Apply()
    {
        Type.Apply();
        Data.Apply();
        Extra.Apply();
    }
}
