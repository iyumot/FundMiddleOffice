using FMO.Models;
using FMO.Utilities;

namespace FMO.TPL.Tests
{
    [TestClass()]
    public class WordTplTests
    {
        [TestMethod()]
        public void GenerateRegisterAnounceTest()
        {
            Directory.SetCurrentDirectory("E:\\funds");

            WordTpl.GenerateRegisterAnounce(new Models.Fund { Name = "ffff", Code = "abcdes" }, "承诺.docx");

        }

        [TestMethod()]
        public void GenerateFile()
        {
            Directory.SetCurrentDirectory("E:\\fmo");

            var Company = "国海良时期货";
            var Id = 1;
            var Name = "基本账户";

            // 模板文件
            var tplPath = @$"{Company}.docx";
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "files", "accounts", "future", Id.ToString(), Name, "原始文件");

            using var db = DbHelper.Base();
            var m = db.GetCollection<Models.Manager>().FindOne(x => x.IsMaster);
            var fund = db.GetCollection<Fund>().FindOne(x => x.Name.Contains("吉祥"));
            var daily = db.GetDailyCollection(fund.Id).Find(x => x.NetAsset > 0).MaxBy(x => x.Date)!;
            var ele = db.GetCollection<FundElements>().FindById(fund.Id);
            var pe = new
            {
                Name = "石俊",
                IdType = "身份证",
                Id = "342623",
                Phone = "18550110512",
                Address = m.OfficeAddress
            };

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
                    RegisterCapital = $"{m.RegisterCapital}万",
                    RealCapital = $"{m.RealCapital}万",
                    BusinessScope = m.BusinessScope,
                    Telephone = m.Telephone,
                },
                Fund = new
                {
                    Name = fund.Name,
                    NetAsset = $"{daily.NetAsset / 10000:N0}万",
                    CustodyAccount = ele.CustodyAccount.Value,
                    Duration = $"{ele.DurationInMonths}个月"
                }
                ,
                LegalPerson = new
                {
                    Name = m.LegalAgent?.Name,
                    IdType = m.LegalAgent?.IDType switch { IDType.IdentityCard or IDType.Unknown or null => "身份证", var x => EnumDescriptionTypeConverter.GetEnumDescription(x) },
                    Id = m.LegalAgent?.Id,
                    Phone = m.LegalAgent?.Phone,
                    Address = m.LegalAgent?.Address,
                },
                InvestmentManager = new
                {
                    Name = "杨博涵",
                    IdType = "身份证",
                    Id = "342623",
                    Phone = "18550110512",
                    Address = m.OfficeAddress
                },
                ResponsiblePerson = pe,
                OpenAgent = pe,
                OrderPlacer = pe,
                FundTransferor = pe,
                ConfirmationPerson = pe
            };



            WordTpl.GenerateFromTemplate(Path.Combine(folder, "开户材料2.docx"), tplPath, obj);
        }
    }
}