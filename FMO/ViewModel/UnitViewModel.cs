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

    protected virtual string? DisplayOverride() => ToString();

    protected void ItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is IModifiableValue x)
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
            if (p.PropertyType.IsAssignableTo(typeof(IModifiableValue)))
                ((IModifiableValue)p.GetValue(this)!).Apply();
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

public interface IEntityViewModel<T> where T : notnull
{
    public void Init(T param);

    public void UpdateEntity(T obj);

    public Action<T>? ClearFunc { get; set; }

    public void RemoveValue(T obj)
    {
        foreach (var p in obj.GetType().GetProperties())
        {
            if (p.PropertyType.IsAssignableTo(typeof(IModifiableValue)))
             ((IModifiableValue)p.GetValue(this)!).Clear();
        }

        if (obj is not null && ClearFunc is not null)
            ClearFunc(obj);
    }
}

public partial class EntityPropertyViewModel<T, TProperty> : UnitViewModel, IEntityViewModel<T> where TProperty : notnull where T : notnull
{
    public PropertyViewModel<TProperty> Data { get; } = new();

    public override bool CanConfirm => Data.IsChanged;

    public override bool CanDelete => !HasUnsavedValue && Data.Old is not null;

    public override bool HasUnsavedValue => Data.IsChanged;

    public Func<T, TProperty?>? InitFunc { get; set; }

    public Action<T, TProperty>? UpdateFunc { get; set; }

    public Action<T>? ClearFunc { get; set; }

    protected override string? DisplayOverride()
    {
        return Data.Old switch { Enum e => EnumDescriptionTypeConverter.GetEnumDescription(e), null => null, var a => a.ToString() };
    }

    public void Init(T param)
    {
        if (param is not null && InitFunc is not null)
        {
            Data.Old = InitFunc(param);
            Data.New = InitFunc(param);
        }
        SubscribeChanges();
    }

    public void UpdateEntity(T obj)
    {
        if (obj is not null && UpdateFunc is not null && Data.New is not null)
            UpdateFunc(obj, Data.New);
    }

    //public void RemoveValue(T obj)
    //{
    //    Data.New = default;
    //    Data.Old = default;

    //    if (obj is not null && ClearFunc is not null)
    //        ClearFunc(obj);
    //}
}

