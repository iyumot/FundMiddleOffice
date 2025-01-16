using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FMO
{
    public partial class SetupFlowViewModel:FlowViewModel
    {

        /// <summary>
        /// 定稿合同
        /// </summary>
        [ObservableProperty]
        public partial FileInfo? PaidInCapitalProof { get; set; }

        [SetsRequiredMembers]
        public SetupFlowViewModel(SetupFlow flow) : base(flow)
        {
            if (!string.IsNullOrWhiteSpace(flow.PaidInCapitalProof?.Path))
                PaidInCapitalProof = new FileInfo(flow.PaidInCapitalProof.Path);

        }
    }
}