﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FMO.Schedule
{
    /// <summary>
    /// GatherDailyFromMailView.xaml 的交互逻辑
    /// </summary>
    public partial class GatherDailyFromMailView : UserControl
    {
        public GatherDailyFromMailView()
        {
            InitializeComponent();
        }

        private void pMail_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is GatherDailyFromMailViewModel vm && sender is PasswordBox pb)
                vm.MailPassword = pb.Password;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GatherDailyFromMailViewModel vm)
                pMail.Password = vm.MailPassword;
        }
    }
}
