using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace FMO
{

    [AutoChangeableViewModel(typeof(DateRange))]
    public partial class DateRangeViewModel;


    public partial class SetupFlowViewModel : FlowViewModel, IChangeableEntityViewModel
    {

        /// <summary>
        /// 募集开始日期
        /// </summary>
        public ChangeableViewModel<SetupFlow, DateOnly?> RaisingStartDate { get; set; }

        /// <summary>
        /// 募集结束日期
        /// </summary>
        public ChangeableViewModel<SetupFlow, DateOnly?> RaisingEndDate { get; set; }


        public ChangeableViewModel<SetupFlow, DateRangeViewModel?> RaisingPeriod { get; set; }




        public ChangeableViewModel<SetupFlow, decimal?> InitialAsset { get; }


        public string? Capital => InitialAsset.NewValue is null ? null : NumberHelper.NumberToChinese(InitialAsset.NewValue.Value);


        /// <summary>
        /// 实缴出资
        /// </summary>
        [ObservableProperty]
        public partial FlowFileViewModel? PaidInCapitalProof { get; set; }

        /// <summary>
        /// 成立公告
        /// </summary>
        [ObservableProperty]
        public partial FlowFileViewModel? EstablishmentAnnouncement { get; set; }









        [SetsRequiredMembers]
        public SetupFlowViewModel(SetupFlow flow) : base(flow)
        {
            RaisingStartDate = new ChangeableViewModel<SetupFlow, DateOnly?>
            {
                InitFunc = x => x.RaisingStartDate == default ? null : x.RaisingStartDate,
                UpdateFunc = (x, y) => x.RaisingStartDate = y ?? default,
                ClearFunc = x => x.RaisingStartDate = default
            };
            RaisingStartDate.Init(flow);

            RaisingEndDate = new ChangeableViewModel<SetupFlow, DateOnly?>
            {
                InitFunc = x => x.RaisingEndDate == default ? null : x.RaisingEndDate,
                UpdateFunc = (x, y) => x.RaisingEndDate = y ?? default,
                ClearFunc = x => x.RaisingEndDate = default
            };
            RaisingEndDate.Init(flow);


            RaisingPeriod = new ChangeableViewModel<SetupFlow, DateRangeViewModel?>
            {
                Label = "募集期",
                InitFunc = x => x.RasingPeriod == default(DateRange) ? new() : new(x.RasingPeriod),
                UpdateFunc = (x, y) => x.RasingPeriod = y!.Build(),
                ClearFunc = x => x.RasingPeriod = default
            };
            RaisingPeriod.Init(flow);



            InitialAsset = new ChangeableViewModel<SetupFlow, decimal?>
            {
                Label = "募集规模",
                InitFunc = x => x.InitialAsset <= 0 ? null : x.InitialAsset,
                UpdateFunc = (x, y) => x.InitialAsset = y ?? 0,
                ClearFunc = x => x.InitialAsset = 0,
                DisplayFunc = x => $"{x / 10000:N2}元"
            };
            InitialAsset.Init(flow);
            InitialAsset.PropertyChanged += (s, e) => { if (e.PropertyName == "NewValue") OnPropertyChanged(nameof(Capital)); };


            PaidInCapitalProof = new(FundId, FlowId, "实缴出资证明", flow.PaidInCapitalProof?.Path, "Establish", nameof(SetupFlow.PaidInCapitalProof));
            EstablishmentAnnouncement = new(FundId, FlowId, "成立公告", flow.PaidInCapitalProof?.Path, "Announcement", nameof(SetupFlow.PaidInCapitalProof));

            Initialized = true;
        }








        [RelayCommand]
        public void Delete(IPropertyModifier unit)
        {
            if (unit is IEntityModifier<SetupFlow> entity)
            {
                using var db = DbHelper.Base();
                var v = db.GetCollection<FundFlow>().FindById(FlowId) as SetupFlow;

                if (v is not null)
                {
                    entity.RemoveValue(v);
                    entity.Init(v);
                    db.GetCollection<FundFlow>().Update(v);

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
            if (unit is IEntityModifier<SetupFlow> property)
            {
                using var db = DbHelper.Base();
                var v = db.GetCollection<FundFlow>().FindById(FlowId) as SetupFlow;

                property.UpdateEntity(v!);
            }
            unit.Apply();
        }


        [RelayCommand]
        public void Save()
        {
            var ps = GetType().GetProperties();
            foreach (var p in ps)
            {
                if (p.PropertyType.IsAssignableTo(typeof(IPropertyModifier)) && p.GetValue(this) is IPropertyModifier v && v.IsValueChanged)
                    Modify(v);
            }
            IsReadOnly = true;
        }


    }
}