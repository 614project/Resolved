using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Resolved.Scripts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Controls
{
    public sealed partial class UserSearch : UserControl
    {
        public UserSearch()
        {
            this.InitializeComponent();
            this.SizeChanged += this.UserSearch_SizeChanged;
        }

        private void UserSearch_SizeChanged(object sender , SizeChangedEventArgs e)
        {
            BackgroundImage.Width = e.NewSize.Width;
            Canvas.SetTop(Profile , BackgroundImage.ActualHeight);
            Canvas.SetTop(Handle , Profile.ActualOffset.Y);
        }

        private ObservableCollection<string> SuggestKeywords { get; } = new();
        private async void HandleTextChanged(AutoSuggestBox sender , AutoSuggestBoxTextChangedEventArgs args)
        {
            string text = sender.Text;
            if (string.IsNullOrWhiteSpace(text) || !ResolvedInfo.HasCredentials)
                return;
            try
            {
                //사용자명 접두사 검색 (2자 미만이면 결과 없음)
                (var suggest, _) = await ResolvedInfo.API.GetSearchUserAsync(text);
                if (suggest != null)
                {
                    SuggestKeywords.Clear();
                    foreach (var user in suggest.Items)
                    {
                        SuggestKeywords.Add(user.Handle);
                    }
                }

                (var info, _) = await ResolvedInfo.API.GetUserAsync(text);
                if (info != null)
                {
                    DispatcherQueue.TryEnqueue(() => {
                        if (info.ProfileImageUrl == null)
                            Profile.ProfilePicture = null;
                        else
                            Profile.ProfilePicture = new BitmapImage(new Uri(info.ProfileImageUrl));

                        Tier.Text = $"{info.GetTierName} {info.Rating}";
                        Bio.Text = info.Bio ?? string.Empty;

                        SolidColorBrush color = new(info.GetTierColor.ToColor());
                        Tier.Foreground = color;
                        RatingBar.Foreground = color;

                        //API v4에서는 배경 정보가 사용자 정보에 함께 온다.
                        string? background = info.Background?.BackgroundImageUrl;
                        if (string.IsNullOrEmpty(background))
                            BackgroundImage.Source = null;
                        else
                            BackgroundImage.Source = new BitmapImage(new Uri(background));
                    });
                }
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}
