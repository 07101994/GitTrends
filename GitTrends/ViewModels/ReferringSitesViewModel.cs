﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using GitTrends.Shared;
using HtmlAgilityPack;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace GitTrends
{
    public class ReferringSitesViewModel : BaseViewModel
    {
        readonly GitHubApiV3Service _gitHubApiV3Service;

        bool _isRefreshing;

        public ReferringSitesViewModel(GitHubApiV3Service gitHubApiV3Service)
        {
            _gitHubApiV3Service = gitHubApiV3Service;
            RefreshCommand = new AsyncCommand<(string Owner, string Repository)>(repo => ExecuteRefreshCommand(repo.Owner, repo.Repository));

            //https://codetraveler.io/2019/09/11/using-observablecollection-in-a-multi-threaded-xamarin-forms-application/
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization(ReferringSitesCollection, null, ObservableCollectionCallback);
        }

        public ObservableCollection<MobileReferringSiteModel> ReferringSitesCollection { get; } = new ObservableCollection<MobileReferringSiteModel>();

        public ICommand RefreshCommand { get; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        async Task ExecuteRefreshCommand(string owner, string repository)
        {
            ReferringSitesCollection.Clear();

            try
            {
                var referringSitesList = new List<MobileReferringSiteModel>();

                await foreach (var site in _gitHubApiV3Service.GetReferingSites(owner, repository).ConfigureAwait(false))
                {
                    referringSitesList.Add(site);
                }

                foreach (var site in referringSitesList.OrderByDescending(x => x.TotalCount))
                    ReferringSitesCollection.Add(site);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        //https://codetraveler.io/2019/09/11/using-observablecollection-in-a-multi-threaded-xamarin-forms-application/
        void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess)
        {
            lock (collection)
            {
                accessMethod?.Invoke();
            }
        }
    }
}
