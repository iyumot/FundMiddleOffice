using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;

namespace FMO
{
    public partial class SetupFlowViewModel:FlowViewModel
    {

        /// <summary>
        /// 实缴出资
        /// </summary>
        [ObservableProperty]
        public partial PredefinedFileViewModel? PaidInCapitalProof { get; set; }

        /// <summary>
        /// 成立公告
        /// </summary>
        [ObservableProperty]
        public partial PredefinedFileViewModel? EstablishmentAnnouncement { get; set; }









        [SetsRequiredMembers]
        public SetupFlowViewModel(SetupFlow flow) : base(flow)
        {
            PaidInCapitalProof = new(FundId, FlowId, "实缴出资证明", flow.PaidInCapitalProof?.Path, "Establish", nameof(SetupFlow.PaidInCapitalProof));
            EstablishmentAnnouncement = new(FundId, FlowId, "成立公告", flow.PaidInCapitalProof?.Path, "Announcement", nameof(SetupFlow.PaidInCapitalProof));

            Initialized = true;
        }
    }
}