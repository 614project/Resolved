﻿using CommunityToolkit.WinUI.Helpers;
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
            Background.Width = e.NewSize.Width;
            Canvas.SetTop(Profile , Background.ActualHeight);
            Canvas.SetTop(Handle , Profile.ActualOffset.Y);
        }

        private ObservableCollection<string> SuggestKeywords { get; } = new();
        private async void HandleTextChanged(AutoSuggestBox sender , AutoSuggestBoxTextChangedEventArgs args)
        {
            (var suggest, _) = await ResolvedInfo.API.GetSearchAutoCompleteAsync(sender.Text);
            if (suggest != null)
            {
                SuggestKeywords.Clear();
                foreach(var user in suggest.Users)
                {
                    SuggestKeywords.Add(user.Handle);
                }
            }

            (var info, _) = await ResolvedInfo.API.GetUserAsync(Handle.Text.ToLower());
            if (info != null)
            {
                DispatcherQueue.TryEnqueue(() => {
                    if (info.ProfileImageUrl == null)
                        Profile.ProfilePicture = null;
                    else
                        Profile.ProfilePicture = new BitmapImage(new Uri(info.ProfileImageUrl));

                    Tier.Text = $"{info.GetTierName} {info.Rating}";
                    Bio.Text = info.Bio;

                    SolidColorBrush color = new((info.GetTierColor ?? "#000000").ToColor());
                    Tier.Foreground = color;
                    RatingBar.Foreground = color;
                });

                (var background, _) = await ResolvedInfo.API.GetBackgroundAsync(info.BackgroundId);
                DispatcherQueue.TryEnqueue(() => {
                    if (background == null)
                        Background.Source = null;
                    else
                        Background.Source = new BitmapImage(new Uri(background.BackgroundImageUrl));
                });
            }
        }
    }
}
