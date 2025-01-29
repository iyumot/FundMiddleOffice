using FMO.Models;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public class ElementValueViewModel<T> : ElementItemViewModel where T : struct
{
    public ValueViewModel<T> Data { get; } = new();


    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => !CanConfirm && Data.New is not null;

    [SetsRequiredMembers]
    public ElementValueViewModel(FundElements elements, string property, int flowid, string label)
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
            mutable.SetValue(Data.New.Value, flowid);
    }


    public override void RemoveValue(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<T> mutable)
            mutable.RemoveValue(flowid);

        Data.New = null;
    }


    public override void Apply()
    {
        Data.Apply();
        OnPropertyChanged(nameof(CanConfirm));
    }
}


public class ElementRefrenceViewModel<T> : ElementItemViewModel where T : class
{
    public RefrenceViewModel<T> Data { get; } = new();


    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => !CanConfirm && Data.New is not null;


    [SetsRequiredMembers]
    public ElementRefrenceViewModel(FundElements elements, string property, int flowid, string label)
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

        Init();
    }

    public override void UpdateEntity(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<T> mutable)
            mutable.SetValue(Data.New, flowid);
    }

    public override void RemoveValue(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<T> mutable)
            mutable.RemoveValue(flowid);


        Data.New = null;
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

    public ValueViewModel<TValue> Data { get; } = new();

    public RefrenceViewModel<string> Extra { get; } = new();



    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => !CanConfirm && (Data.New is not null || Type.New is not null);



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

        if (Data.New is null || Type.New is null) return;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.SetValue(new ValueWithEnum<TEnum, TValue> { Type = Type.New.Value, Value = Data.New.Value, Extra = Extra.New }, flowid);

        //else if (obj is Mutable<T?> mutable2)
        //    mutable.SetValue(new ValueWithEnum<TEnum, TValue> { Type = Type.New, Value = Data.New, Extra = Extra.New }, flowid);

        else throw new NotImplementedException();
    }


    public override void RemoveValue(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.RemoveValue(flowid);


        Type.New = null;
        Data.New = null;
        Extra.New = null;
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

    public RefrenceViewModel<TValue> Data { get; } = new();

    public RefrenceViewModel<string> Extra { get; } = new();



    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => !CanConfirm && (Data.New is not null || Type.New is not null);


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

        if (Data.New is null || Type.New is null) return;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.SetValue(new ValueWithEnum<TEnum, TValue> { Type = Type.New.Value, Value = Data.New, Extra = Extra.New }, flowid);

        //else if (obj is Mutable<T?> mutable2)
        //    mutable.SetValue(new ValueWithEnum<TEnum, TValue> { Type = Type.New, Value = Data.New, Extra = Extra.New }, flowid);

        else throw new NotImplementedException();
    }

    public override void RemoveValue(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.RemoveValue(flowid);


        Type.New = null;
        Data.New = null;
        Extra.New = null;
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

    public ValueViewModel<T> Data { get; } = new();



    public override bool CanConfirm => (IsAdopted.New.HasValue && IsAdopted.New.Value && Data.IsChanged) || (!(IsAdopted.New ?? false) && (IsAdopted.Old ?? false));

    /// <summary>
    /// 当不采用或者采用且有值时
    /// </summary>
    public override bool CanDelete => !CanConfirm && ((!IsAdopted.New ?? false) || ((IsAdopted.New ?? false) && Data.New.HasValue));


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

        if (Data.New is null || IsAdopted.New is null) return;

        if (obj is Mutable<ValueWithBoolean<T>> mutable)
            mutable.SetValue(new ValueWithBoolean<T> { IsAdopted = IsAdopted.New.Value, Value = Data.New.Value }, flowid);
        else if (obj is Mutable<ValueWithBoolean<T?>> mutable2)
            mutable2.SetValue(new ValueWithBoolean<T?> { IsAdopted = IsAdopted.New.Value, Value = Data.New.Value }, flowid);
        else throw new NotImplementedException();
    }

    public override void RemoveValue(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is Mutable<ValueWithBoolean<T>> mutable)
            mutable.RemoveValue(flowid);


        Data.New = null;
        IsAdopted.New = null;
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


public class PortionElementItemWithEnumViewModel<TEnum, TValue> : ElementItemViewModel where TEnum : struct, Enum where TValue : struct
{
    public ValueViewModel<TEnum> Type { get; } = new();

    public ValueViewModel<TValue> Data { get; } = new();

    public RefrenceViewModel<string> Extra { get; } = new();



    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => !CanConfirm && (Type.New is not null || Data.New is not null);

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

        if (Data.New is null || Type.New is null) return;

        if (obj is PortionMutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.SetValue(Share, new ValueWithEnum<TEnum, TValue> { Type = Type.New.Value, Value = Data.New.Value, Extra = Extra.New }, flowid);

        // else if (obj is PortionMutable<ValueWithEnum<TEnum, TValue>?> mutable2)
        //     mutable2.SetValue(Share, new ValueWithEnum<TEnum, TValue> { Type = Type.New.Value, Value = Data.New.Value, Extra = Extra.New }, flowid);

        else throw new NotImplementedException();
    }

    public override void RemoveValue(FundElements elements, int flowid)
    {
        var obj = elements.GetType().GetProperty(Property)?.GetValue(elements);

        if (Data.New is null) return;

        if (obj is PortionMutable<ValueWithEnum<TEnum, TValue>> mutable)
            mutable.RemoveValue(flowid);


        Type.New = null;
        Data.New = null;
        Extra.New = null;
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
