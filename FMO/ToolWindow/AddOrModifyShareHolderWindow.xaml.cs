using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Windows;

namespace FMO;

/// <summary>
/// AddOrModifyShareHolderWindow.xaml 的交互逻辑
/// </summary>
public partial class AddOrModifyShareHolderWindow : Window
{
    public AddOrModifyShareHolderWindow()
    {
        InitializeComponent();
    }
}



public partial class AddOrModifyShareHolderWindowViewModel : ObservableObject
{
    public AddOrModifyShareHolderWindowViewModel()
    {
        using var db = DbHelper.Base();
        var per = db.GetCollection<Person>().FindAll().ToArray();
        People = new(per);

        var ins = db.GetCollection<Institution>().FindAll().ToArray();
        Institutions = new(ins);

    }

    public ObservableCollection<Person> People { get; }

    public ObservableCollection<Institution> Institutions { get; }

    [ObservableProperty]
    public partial IEntity? Holder { get; set; }






}