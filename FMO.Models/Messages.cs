using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;

public record FundDailyUpdateMessage(int FundId, DailyValue Daily);

//public record FundStatusChangedMessage(int FundId, FundStatus Status);

public record OpenFundMessage(int Id);

public record OpenPageMessage(string Page);

public record PageTAMessage(int TabIndex, string Search);


public record FundAccountChangedMessage(int FundId, FundAccountType Type);

/// <summary>
/// 要素在后台被修改，通知到UI页
/// </summary>
/// <param name="FundId"></param>
/// <param name="FlowId"></param>
public record ElementChangedBackgroundMessage(int FundId, int FlowId);


public enum LogLevel
{
    Info,
    Warning, 
    Error,
    Success
}

/// <summary>
/// 
/// </summary>
public record ToastMessage(LogLevel Level, string Message);

public record EntityDeleted<T>(T Value);


public record EntityDeleted(Type Type, object Id);


public record EntityChangedMessage<TEntity,TProperty>(TEntity Entity, string PropertyName, TProperty Value);


public record EntityChanged<TEntity, TProperty>(TEntity Entity, string PropertyName, TProperty Old, TProperty New);


public record EntityChanged<TProperty>(Type Type, object Id, string PropertyName, TProperty Old, TProperty New);


/// <summary>
/// 交易确认关联订单
/// </summary>
/// <param name="RecordId"></param>
/// <param name="OrderId"></param>
public record TransferRecordLinkOrderMessage(int RecordId, int OrderId);


public record TipChangeMessage(TipType Type);

public record TrusteeRunMessage(string Name, bool IsRunning);