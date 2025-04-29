using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.TPL;
using FMO.Utilities;
using Microsoft.Win32;
using Serilog;
using System.IO;

namespace FMO;


public partial class FutureAccountViewModel : ObservableObject
{
    public FutureAccountViewModel(FutureAccount v)
    {
        Company = v.Company;
        Id = v.Id;

        Common = new(v.Id, v.Company, v.Common);
    }

    public int Id { get; set; }

    public string? Company { get; set; }


    public BasicAccountViewModel Common { get; set; }






    public partial class BasicAccountViewModel : ObservableObject
    {
        public BasicAccountViewModel(int id, string? company, OpenAccountEvent? common)
        {
            Id = id;
            Company = company;
            if (common is not null)
            {
                IsReadOnly = true;
                Name = common.Name;
                Account = common.Account;
                TradePassword = common.TradePassword;
                CapitalPassword = common.CapitalPassword;
                BankLetter = new FileViewModel<OpenAccountEvent>
                {
                    Label = "银期",
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
        public partial FileViewModel<OpenAccountEvent>? BankLetter { get; set; }
        public int Id { get; }
        public string? Company { get; }

        [RelayCommand]
        public void SetFile(IFileSelector obj)
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

            string hash = fi.ComputeHash()!;

            // 保存副本
            var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "future", Id.ToString(), Name));

            var tar = FileHelper.CopyFile2(fi, dir.FullName);
            if (tar is null)
            {
                Log.Error($"保存文件出错，{fi.Name}");
                HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
                return;
            }

            var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

            using var db = DbHelper.Base();
            var obj = db.GetCollection<FutureAccount>().FindById(Id);

            if (Name == obj.Common?.Name)
            {
                obj.Common.BankLetter = new FileStorageInfo { Name = "银期", Hash = hash, Path = path, Time = fi.LastWriteTime };
                db.GetCollection<FutureAccount>().Update(obj);
            }

            v.File = fi;

        }


        [RelayCommand]
        public void OpenRawFolder()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "future", Id.ToString(), Name, "原始文件");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = folder, UseShellExecute = true }); } catch { }
        }


        [RelayCommand]
        public void OpenSealFolder()
        {
            if (string.IsNullOrWhiteSpace(Name)) return;

            var folder = Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "future", Id.ToString(), Name, "用印文件");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = folder, UseShellExecute = true }); } catch { }
        }


        [RelayCommand]
        public void GenerateOpenAccountFiles()
        {
            try
            {
                // 模板文件
                var tplPath = @$"{Company}.docx";
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "future", Id.ToString(), Name, "原始文件");

                using var db = DbHelper.Base();
                var m = db.GetCollection<Manager>().FindOne(x => x.IsMaster);

                // 数据
                var obj = new
                {
                    Manager = new
                    {
                        Name = m.Name,
                        EnglishName = m.EnglishName,
                        LegalPerson = m.LegalAgent?.Name ?? m.ArtificialPerson,
                        ArtificialPerson = m.ArtificialPerson,
                        SetupDate = m.SetupDate,
                        ExpireDate = m.ExpireDate,
                        RegisterAddress = m.RegisterAddress,
                        OfficeAddress = m.OfficeAddress,
                        RegisterCapital = m.RegisterCapital,
                        RealCapital = m.RealCapital,
                        BusinessScope = m.BusinessScope,
                        Telephone = m.Telephone,
                    },
                    LegalPerson = new
                    {
                        Name = m.LegalAgent?.Name,
                        IDType = m.LegalAgent?.IDType switch { IDType.IdentityCard or null => "身份证", var x => EnumDescriptionTypeConverter.GetEnumDescription(x)},
                        Id = m.LegalAgent?.Id,
                        Phone = m.LegalAgent?.Phone,
                        Address = m.LegalAgent?.Address,
                    },

                };



                WordTpl.GenerateFromTemplate(Path.Combine(folder, "开户材料.docx"), tplPath, obj);
            }
            catch (Exception e)
            {
                Log.Error($"生成开户材料失败，{e}");
            }
        }



        [RelayCommand]
        public void Save()
        {
            using var db = DbHelper.Base();
            var obj = db.GetCollection<FutureAccount>().FindById(Id);

            if (Name == obj.Common?.Name)
            {
                obj.Common!.Account = Account;
                obj.Common!.TradePassword = TradePassword;
                obj.Common!.CapitalPassword = CapitalPassword;

                db.GetCollection<FutureAccount>().Update(obj);
            }


            IsReadOnly = true;
        }

        [RelayCommand]
        public void DeleteFile(IFileSelector file)
        {
            if (file is FileViewModel<OpenAccountEvent> v)
            {
                if (v.File is null) return;
                try
                {

                    using var db = DbHelper.Base();
                    var obj = db.GetCollection<FutureAccount>().FindById(Id);

                    if (Name == obj.Common?.Name)
                    {
                        obj.Common!.BankLetter = null;
                        db.GetCollection<FutureAccount>().Update(obj);
                    }
                    File.Delete(v.File.FullName);
                    v.File = null;
                }
                catch (Exception e)
                {
                    Log.Error($"删除账户银行关联文件失败 {e.Message}");
                }
            }
        }
    }


}
