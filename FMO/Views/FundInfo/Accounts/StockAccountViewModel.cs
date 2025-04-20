using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.IO;

namespace FMO;

/// <summary>
/// 股票户
/// </summary>
public partial class StockAccountViewModel : ObservableObject
{
    public StockAccountViewModel(StockAccount v)
    {
        Company = v.Company;

        Common = new(v.Id, v.Common);
        Credit = new(v.Id, v.Credit);
    }

    public string? Company { get; set; }


    public BasicAccountViewModel Common { get; set; }


    public BasicAccountViewModel Credit { get; set; }




    public partial class BasicAccountViewModel : ObservableObject
    {
        public BasicAccountViewModel(int id, BasicAccountEvent? common)
        {
            Id = id;

            if (common is not null)
            {
                IsReadOnly = true;
                Name = common.Name;
                Account = common.Account;
                TradePassword = common.TradePassword;
                CapitalPassword = common.CapitalPassword;
                BankLetter = new FileViewModel<BasicAccountEvent>
                {
                    Label = "银证",
                    GetProperty = x => x.BankLetter,
                    SetProperty = (x, y) => x.BankLetter = y,
                };

                BankLetter.Init(common);
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
        [ObservableProperty]
        public partial FileViewModel<BasicAccountEvent>? BankLetter { get; set; }
        public int Id { get; }


        [RelayCommand]
        public void SetFile(IFileSelector obj)
        {
            if (obj is not FileViewModel<BasicAccountEvent> v) return;

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

        private void SetFile(FileViewModel<BasicAccountEvent> v, FileInfo fi)
        {
            if (Id == 0)
            {
                return;
            }

            string hash = fi.ComputeHash()!;

            // 保存副本
            var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "stock", Id.ToString(), Name));

            var tar = FileHelper.CopyFile2(fi, dir.FullName);
            if (tar is null)
            {
                Log.Error($"保存文件出错，{fi.Name}");
                HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
                return;
            }

            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

            using var db = DbHelper.Base();
            var obj = db.GetCollection<StockAccount>().FindById(Id);

            if (Name == obj.Common?.Name)
            {
                obj.Common.BankLetter = new FileStorageInfo { Name = "银证", Hash = hash, Path = path, Time = fi.LastWriteTime };
                db.GetCollection<StockAccount>().Update(obj);
            }
            else if (Name == obj.Credit?.Name)
            {
                obj.Credit.BankLetter = new FileStorageInfo { Name = "银信", Hash = hash, Path = path, Time = fi.LastWriteTime };
                db.GetCollection<StockAccount>().Update(obj);
            }

            v.File = fi;

        }


        [RelayCommand]
        public void OpenRawFolder()
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "stock", Id.ToString(), Name, "原始文件");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = folder, UseShellExecute = true }); } catch { }
        }


        [RelayCommand]
        public void OpenSealFolder()
        {
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
        public void DeleteFile(IFileSelector file)
        {
            if (file is FileViewModel<BasicAccountEvent> v)
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


}


