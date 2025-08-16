﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
using FMO.TPL;
using FMO.Utilities;
using LiteDB;
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
        FundId = v.FundId;

        Common = new(v.Id, FundId, v.Company, v.Common);
    }

    public int Id { get; set; }

    public int FundId { get; }
    public string? Company { get; set; }


    public BasicAccountViewModel Common { get; set; }






    public partial class BasicAccountViewModel : ObservableObject
    {
        public BasicAccountViewModel(int id, int fundId, string? company, OpenAccountEvent? common)
        {
            Id = id;
            FundId = fundId;
            Company = company;
            if (common is not null)
            {
                IsReadOnly = true;
                Name = common.Name;
                Account = common.Account;
                TradePassword = common.TradePassword;
                CapitalPassword = common.CapitalPassword;


                BankLetter = new(common.BankLetter);
                BankLetter.FileChanged += f => UpdateFile(new { BankLetter = f });
                 

                ServiceAgreement = new (common.ServiceAgreement);
                ServiceAgreement.FileChanged += f => UpdateFile(new { ServiceAgreement = f }); 

                AccountLetter = new(common.AccountLetter);
                AccountLetter.FileChanged += f => UpdateFile(new { AccountLetter = f });
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


        /// <summary>
        /// 账户信息
        /// </summary>
        public SimpleFileViewModel? AccountLetter { get; }



        public int Id { get; }
        public int FundId { get; }
        public string? Company { get; }

 


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
            // 验证数据 
            //1 有托管
            {
                using var db = DbHelper.Base();
                var ele = db.GetCollection<FundElements>().FindById(FundId);
                if (ele is null || ele.TrusteeInfo.Value is null)
                {
                    HandyControl.Controls.Growl.Warning("请先【要素】中设置 托管信息");
                    return;
                }
                var im = db.GetCollection<FundInvestmentManager>().Find(x => x.FundId == FundId).ToArray();
                if (im.Length == 0)
                {
                    HandyControl.Controls.Growl.Warning("请先在【策略】中设置 投资经理");
                    return;
                }

                var per = db.GetCollection<Participant>().FindAll().ToArray();
                if (!per.Any(x => x.Role.HasFlag(PersonRole.Agent)) ||
                    !per.Any(x => x.Role.HasFlag(PersonRole.OrderPlacer)) ||
                    !per.Any(x => x.Role.HasFlag(PersonRole.FundTransferor)) ||
                    !per.Any(x => x.Role.HasFlag(PersonRole.ConfirmationPerson)))
                {
                    HandyControl.Controls.Growl.Warning("请先在【管理人】 【成员】中设置 开户代理人、指定下单人、资金划转人、结算单确认人等");
                    return;
                }




            }



            var wnd = new FutureOpenFilesGeneratorWindow();
            wnd.Owner = App.Current.MainWindow;
            wnd.DataContext = new FutureOpenFilesGeneratorWindowViewModel
            {
                FundId = FundId,
                Company = Company!,
                TemplatePath = @$"files\tpl\{Company}.docx",
                TargetFolder = Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "future", Id.ToString(), Name!, "原始文件")
            };
            wnd.ShowDialog();

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

     
    }


}
