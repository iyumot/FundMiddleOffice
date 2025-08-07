﻿using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// LogView.xaml 的交互逻辑
/// </summary>
public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
    }
}


public partial class LogViewModel : ObservableObject
{

    [ObservableProperty]
    public partial IEnumerable<LogMessage>? CommonLogs { get; set; }

    public LogViewModel()
    {
        using var db = new LiteDatabase($@"FileName=logs.db;Connection=Shared");

        CommonLogs = db.GetCollection("log").Query().OrderByDescending(x => x["_t"].AsDateTime)
                    .Limit(1000).ToList().Select(x => new LogMessage(x["_t"].AsDateTime, x["_m"].AsString));

    }

    public record LogMessage(DateTime Time, string Message);
}

