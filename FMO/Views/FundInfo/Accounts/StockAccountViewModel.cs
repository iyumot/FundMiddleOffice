using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;

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
                BankLetter = new FileViewModel<OpenAccountEvent>
                {
                    Label = "银证",
                    SaveFolder = Path.Combine("files", "accounts", "stock", Id.ToString(), Name),
                    GetProperty = x => x.BankLetter,
                    SetProperty = (x, y) => x.BankLetter = y,
                };

                BankLetter.Init(common);


                ServiceAgreement = new FileViewModel<OpenAccountEvent>
                {
                    Label = "经服",
                    SaveFolder = Path.Combine("files", "accounts", "stock", Id.ToString(), Name),
                    GetProperty = x => x.ServiceAgreement,
                    SetProperty = (x, y) => x.ServiceAgreement = y,
                };

                ServiceAgreement.Init(common);
            }

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
        public FileViewModel<OpenAccountEvent>? BankLetter { get; }


        /// <summary>
        /// 经服
        /// </summary>
        public FileViewModel<OpenAccountEvent>? ServiceAgreement { get; }

        public int Id { get; }


        [RelayCommand]
        public void ChooseFile(IFileSelector obj)
        {
            if (obj is not FileViewModel<OpenAccountEvent> v) return;

            var fd = new OpenFileDialog();
            fd.Filter = v.Filter;
            if (fd.ShowDialog() != true)
                return;

            var old = v.File;

            var fi = new FileInfo(fd.FileName);
            if (fi is not null)
                SetFile(v, fi);

            try { if (old is not null) File.Delete(old.FullName); } catch { }
        }

        private void SetFile(FileViewModel<OpenAccountEvent> v, FileInfo fi)
        {
            if (Id == 0 || string.IsNullOrWhiteSpace(Name))
            {
                return;
            }


            var s = v.Build(fi.FullName);

            using var db = DbHelper.Base();
            var obj = db.GetCollection<StockAccount>().FindById(Id);

            if (Name == obj.Common?.Name)
            {
                v.SetProperty(obj.Common, s);
                db.GetCollection<StockAccount>().Update(obj);
            }
            else if (Name == obj.Credit?.Name)
            {
                v.SetProperty(obj.Credit, s);
                db.GetCollection<StockAccount>().Update(obj);
            }

            v.File = s?.Path is null ? null : new FileInfo(s.Path);
        }


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

        [RelayCommand]
        public void Clear(IFileSelector file)
        {
            if (file is FileViewModel<OpenAccountEvent> v)
            {
                if (v.File is null) return;
                try
                {

                    using var db = DbHelper.Base();
                    var obj = db.GetCollection<StockAccount>().FindById(Id);

                    if (Name == obj.Common?.Name)
                    {
                        obj.Common!.BankLetter = null;
                        db.GetCollection<StockAccount>().Update(obj);
                    }
                    else if (Name == obj.Credit?.Name)
                    {
                        obj.Credit!.BankLetter = null;
                        db.GetCollection<StockAccount>().Update(obj);
                    }
                    File.Delete(v.File.FullName);
                    v.File = null;
                }
                catch (Exception e)
                {
                    Log.Error($"删除股票账户银行关联文件失败 {e.Message}");
                }
            }
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
