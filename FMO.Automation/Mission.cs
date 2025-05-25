using CommunityToolkit.Mvvm.Messaging;
using MimeKit;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace FMO.Schedule;

/// <summary>
/// 任务
/// </summary>
public abstract class Mission
{
    public int Id { get; set; }

    public DateTime? LastRun { get; set; }

    public DateTime? NextRun { get; set; }


    bool _isEnabled;
    public bool IsEnabled { get => _isEnabled; set { _isEnabled = value; if (value) SetNextRun(); } }

    public bool IsWorking { get; private set; }


    private string? _log;
    public string? WorkLog { get => _log; protected set { _log = value; WeakReferenceMessenger.Default.Send(new MissionWorkMessage(Id, value ?? "")); } }

    public void OnTime(DateTime time)
    {
        if (!IsEnabled || IsWorking || NextRun is null) return;

        if (time < NextRun) return;

        if (LastRun is not null && NextRun < LastRun)
        {
            SetNextRun();
            if (NextRun < LastRun)
            {
                IsEnabled = false;
                return;
            }
        }

        // 设为永不执行
        NextRun = DateTime.MaxValue;
        IsWorking = true;
        Task.Run(() => Work());
    }

    public bool Work()
    {
        IsWorking = true;
        WeakReferenceMessenger.Default.Send(new MissionMessage { Id = Id, IsWorking = true });
        var r = false;
        try
        {
            WorkLog = "";
            try { r = WorkOverride(); LastRun = DateTime.Now; if (r) SetNextRun(); } catch (Exception e) { }

            using (var db = new MissionDatabase())
                db.GetCollection<Mission>().Upsert(this);

        }
        catch (Exception e) { }

        IsWorking = false;
        WeakReferenceMessenger.Default.Send(new MissionMessage { Id = Id, IsWorking = false, LastRun = LastRun, NextRun = NextRun });
        return r;
    }


    protected virtual bool WorkOverride()
    {
        return true;
    }

    protected virtual void SetNextRun()
    {
        NextRun = DateTime.MaxValue;
    }

    public virtual void Init()
    {
        SetNextRun();
    }


    protected void SendLog(string log) { Debug.WriteLine(log); WeakReferenceMessenger.Default.Send(new MissionWorkMessage(Id, log)); }

}

public class MissionTitleAttribute : Attribute
{
    [SetsRequiredMembers]
    public MissionTitleAttribute(string v)
    {
        Title = v;
    }

    public required string Title { get; set; }
}




public class MailMission : Mission
{

    public string? MailName { get => field; set { field = value; _collection = value is null ? null : BitConverter.ToString(SHA256.HashData(Encoding.Default.GetBytes(value))).Replace("-", "").ToLowerInvariant(); } }


    protected string? _collection;

    public virtual MailCategory DetermineCategory(MimeMessage? message)
    {
        if (message is null) return MailCategory.Unk;
        MailCategory category = MailCategory.Unk;


        if (message.Subject.Contains("估值表"))
            category |= MailCategory.ValueSheet;

        if (message.Subject.Contains("对账单") || message.TextBody.Contains("对账单"))        
            category |= MailCategory.Statement;
        
        if (message.Subject.Contains("交易确认") || message.TextBody.Contains("交易确认"))        
            category |= MailCategory.TA;
         
        using var db = new MissionDatabase();
        db.GetCollection<MailCategoryInfo>(_collection).Upsert(new MailCategoryInfo(message.MessageId, message.Subject, MailCategory.ValueSheet));
        return category;
    }
}
