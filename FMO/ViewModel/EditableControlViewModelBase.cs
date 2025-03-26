using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Shared;
using FMO.Utilities;
using System.Reflection;

namespace FMO;

public partial class EditableControlViewModelBase<T> : ObservableObject where T : class, new()
{

    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;

    public int Id { get; protected set; }

    [RelayCommand]
    public void Delete(IPropertyModifier unit)
    {

        if (unit is IEntityModifier<T> entity)
        {
            using var db = DbHelper.Base();
            var v = db.GetCollection<T>().FindById(Id);

            if (v is not null)
            {
                entity.RemoveValue(v);
                entity.Init(v);
                db.GetCollection<T>().Upsert(v);

                WeakReferenceMessenger.Default.Send(v);
            }
        }
    }

    [RelayCommand]
    public void Reset(IPropertyModifier unit)
    {
        unit.Reset();
    }



    [RelayCommand]
    public void Modify(IPropertyModifier unit)
    {
        if (unit is IEntityModifier<T> property)
        {
            T? v = default;
            if (Id == 0)
            {
                v = new();
                InitNewEntity(v);
            }
            else
            {
                using var db = DbHelper.Base();
                v = db.GetCollection<T>().FindById(Id);
                if (v is null)
                {
                    v = new();
                    InitNewEntity(v);
                }
            }

            property.UpdateEntity(v);

            if (v is not null)
            {
                using var db = DbHelper.Base();
                db.GetCollection<T>().Upsert(v);
                if (Id == 0 && v.GetType().GetProperty("Id") is PropertyInfo pi && pi.PropertyType == typeof(int))
                {
                    Id = (int)pi.GetValue(v)!;
                }
                //WeakReferenceMessenger.Default.Send(v);
            }
        }
        unit.Apply();
    }

    [RelayCommand]
    public void Save()
    {
        var ps = GetType().GetProperties();
        foreach (var p in ps)
        {
            if (p.PropertyType.IsAssignableTo(typeof(IPropertyModifier)) && p.GetValue(this) is IPropertyModifier v)
                Modify(v);
        }

        IsReadOnly = true;
    }


    protected virtual void InitNewEntity(T v)
    {
    }
}
