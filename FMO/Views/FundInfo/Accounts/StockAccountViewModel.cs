using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using LiteDB;
using Microsoft.Win32;
using Serilog;
using System.IO;
using System.Windows.Media;

namespace FMO;

/// <summary>
/// 股票户
/// </summary>
public partial class StockAccountViewModel : ObservableObject
{
    public StockAccountViewModel(StockAccount v)
    {
        Company = v.Company;
        Id = v.Id;
        Group = v.Group;

        Common = new(v.Id, v.Common);
        Credit = new(v.Id, v.Credit);
    }

    public int Id { get; set; }

    public string? Company { get; set; }


    public BasicAccountViewModel Common { get; set; }


    public BasicAccountViewModel Credit { get; set; }



    [ObservableProperty] public partial bool ShowGroupPop { get; set; }

    [ObservableProperty] public partial int Group { get; set; }

    [ObservableProperty]
    public partial SolidColorBrush? GroupBrush { get; set; }


    public partial class BasicAccountViewModel : ObservableObject
    {
        public BasicAccountViewModel(int id, OpenAccountEvent? common)
        {
            Id = id;
            
            if (common is not null)
            {
                IsReadOnly = true;
                Name = common.Name;
                Account = common.Account;
                TradePassword = common.TradePassword;
                CapitalPassword = common.CapitalPassword;


                BankLetter = new(common.BankLetter);
                BankLetter.FileChanged += f => UpdateFile(new { BankLetter = f });


                ServiceAgreement = new(common.ServiceAgreement);
                ServiceAgreement.FileChanged += f => UpdateFile(new { ServiceAgreement = f });
                 
            }

        }

        private void UpdateFile<T>(T f)
        {
            if (Id == 0) return; // 新建时不保存
            using var db = DbHelper.Base();
            db.GetCollection<FundAnnouncement>().UpdateMany(BsonMapper.Global.ToDocument(f).ToString(), $"_id={Id}");
        }

        [ObservableProperty]
        public partial bool IsReadOnly { get; set; }

        [ObservableProperty]
        public partial string? Name { get; set; }

        /// <summary>
        /// 资金账号
        /// </summary>
        [ObservableProperty]
        public partial string? Account { get; set; }

        /// <summary>
        /// 交易密码
        /// </summary>
        [ObservableProperty]
        public partial string? TradePassword { get; set; }

        /// <summary>
        /// 资金密码
        /// </summary>
        [ObservableProperty]
        public partial string? CapitalPassword { get; set; }

        /// <summary>
        /// 银证、银期等
        /// </summary>
        public SimpleFileViewModel? BankLetter { get; }



        public SimpleFileViewModel? ServiceAgreement { get; }
         

        public int Id { get; }



        [RelayCommand]
        public void OpenRawFolder()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "stock", Id.ToString(), Name, "原始文件");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = folder, UseShellExecute = true }); } catch { }
        }


        [RelayCommand]
        public void OpenSealFolder()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "stock", Id.ToString(), Name, "用印文件");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = folder, UseShellExecute = true }); } catch { }
        }





        [RelayCommand]
        public void Save()
        {
            using var db = DbHelper.Base();
            var obj = db.GetCollection<StockAccount>().FindById(Id);

            if (Name == obj.Common?.Name)
            {
                obj.Common!.Account = Account;
                obj.Common!.TradePassword = TradePassword;
                obj.Common!.CapitalPassword = CapitalPassword;

                db.GetCollection<StockAccount>().Update(obj);
            }
            else if (Name == obj.Credit?.Name)
            {
                obj.Credit!.Account = Account;
                obj.Credit!.TradePassword = TradePassword;
                obj.Credit!.CapitalPassword = CapitalPassword;

                db.GetCollection<StockAccount>().Update(obj);
            }

            IsReadOnly = true;
        }

    }


    partial void OnGroupChanged(int value)
    {
        using var db = DbHelper.Base();
        var obj = db.GetCollection<StockAccount>().FindById(Id);
        obj.Group = value;
        db.GetCollection<StockAccount>().Update(obj);
        ShowGroupPop = false;
    }


    [RelayCommand]
    public void SetGroup()
    {
        ShowGroupPop = true;
    }
}
