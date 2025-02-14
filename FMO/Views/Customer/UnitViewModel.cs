using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.ComponentModel;

namespace FMO;


/// <summary>
/// 单个项的vm，提供新旧值的
/// 需要有3个功能
/// 初始化
/// 修改
/// 保存
/// </summary>
public abstract class UnitViewModel : ObservableObject
{
    public const string UnsetValue = "-";

    /// <summary>
    /// 此项的标签
    /// </summary>
    public string? Label { get; set; }

    public abstract bool CanConfirm { get; }

    public abstract bool CanDelete { get; }

    /// <summary>
    /// 已修改未保存
    /// </summary>
    public abstract bool HasUnsavedValue { get; }

    /// <summary>
    /// 显示值
    /// </summary>
    public string? Display => DisplayOverride();

    public abstract void UpdateEntity(object obj);

    public abstract void RemoveValue(object obj);

    /// <summary>
    /// 订阅所有属性变动
    /// </summary>
    protected virtual void SubscribeChanges()
    {
        foreach (var p in GetType().GetProperties())
        {
            if (p.PropertyType.IsAssignableTo(typeof(INotifyPropertyChanged)))
                ((INotifyPropertyChanged)p.GetValue(this)!).PropertyChanged += ItemPropertyChanged;
        }
    }

    public void Init(object? param)
    {
        InitOverride(param);
        SubscribeChanges();
    }

    protected abstract void InitOverride(object? param);


    protected virtual string? DisplayOverride() => ToString();

    protected void ItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is IValueViewModel x)
        {
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(CanDelete));
            OnPropertyChanged(nameof(HasUnsavedValue));
        }
    }

    public virtual void ApplyOverride()
    {
        foreach (var p in GetType().GetProperties())
        {
            if (p.PropertyType.IsAssignableTo(typeof(IValueViewModel)))
                ((IValueViewModel)p.GetValue(this)!).Apply();
        }

        OnPropertyChanged(nameof(CanConfirm));
        OnPropertyChanged(nameof(CanDelete));
        OnPropertyChanged(nameof(HasUnsavedValue));
    }

    public void Apply()
    {
        ApplyOverride();

        OnPropertyChanged(nameof(Display));
    }
}




public partial class CustomerUnitValueViewModel<T> : UnitViewModel where T : struct
{
    public ValueViewModel<T> Data { get; } = new();

    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => throw new NotImplementedException();

    public override bool HasUnsavedValue => throw new NotImplementedException();

    public Func<object, T>? InitFunc { get; set; }

    protected override void InitOverride(object? param)
    {
        throw new NotImplementedException();
    }

    public override void RemoveValue(object obj)
    {
        throw new NotImplementedException();
    }

    public override void UpdateEntity(object obj)
    {
        throw new NotImplementedException();
    }

    protected override string? DisplayOverride()
    {
        return Data.Old.ToString();
    }
}

public partial class CustomerUnitRefrenceViewModel<T> : UnitViewModel where T : class
{
    public RefrenceViewModel<T> Data { get; } = new();

    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => Data.Old is not null;

    public override bool HasUnsavedValue => Data.IsChanged;

    public Func<IInvestor, T>? InitFunc { get; set; }

    protected override void InitOverride(object? param)
    {
        if (param is IInvestor c && InitFunc is not null)
        {
            Data.Old = InitFunc(c);
            Data.New = InitFunc(c);
        }
        else
            throw new NotImplementedException();
    }


    public override void RemoveValue(object obj)
    {
        throw new NotImplementedException();
    }

    public override void UpdateEntity(object obj)
    {
        throw new NotImplementedException();
    }


    protected override string? DisplayOverride()
    {
        return Data.Old.ToString();
    }
}