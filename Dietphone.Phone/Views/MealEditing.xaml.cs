﻿using System;
using System.Windows;
using Dietphone.ViewModels;
using Telerik.Windows.Controls;
using Dietphone.Tools;
using System.Windows.Navigation;
using System.Windows.Controls;

namespace Dietphone.Views
{
    public partial class MealEditing : StateProviderPage
    {
        public new MealEditingViewModel ViewModel { get { return (MealEditingViewModel)base.ViewModel; } }
        private const string TOP_ITEM_INDEX = "TOP_ITEM_INDEX";

        public MealEditing()
        {
            InitializeComponent();
        }

        protected override void OnInitializePage()
        {
            ViewModel.StateProvider = this;
            ViewModel.ItemEditing = ItemEditing.ViewModel;
            ViewModel.IsDirtyChanged += ViewModel_IsDirtyChanged;
            ViewModel.CannotSave += ViewModel_CannotSave;
            ViewModel.InvalidateItems += ViewModel_InvalidateItems;
            Scores.ScoreClick += Scores_ScoreClick;
            InteractionEffectManager.AllowedTypes.Add(typeof(RadDataBoundListBoxItem));
            Save = this.GetIcon(0);
            TranslateApplicationBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel.Navigator == null)
            {
                var navigator = new NavigatorImpl(new NavigationServiceImpl(NavigationService));
                ViewModel.Navigator = navigator;
                ViewModel.Load();
                Scores.DataContext = ViewModel.Subject.Scores;
            }
            else
            {
                ViewModel.ReturnedFromNavigation();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode != NavigationMode.Back)
            {
                ViewModel.Tombstone();
                TombstoneTopItem();
            }
        }

        private void AddMealName_Click(object sender, RoutedEventArgs e)
        {
            MealName.QuicklyCollapse();
            var input = new XnaInputBox(this)
            {
                Title = Translations.AddName,
                Description = Translations.Name
            };
            input.Show();
            input.Confirmed += delegate
            {
                ViewModel.AddAndSetName(input.Text);
                MealName.ForceRefresh(ProgressBar);
            };
        }

        private void EditMealName_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CanEditName())
            {
                EditMealNameDo();
            }
            else
            {
                MessageBox.Show(ViewModel.NameOfName, Translations.CannotEditThisName,
                    MessageBoxButton.OK);
            }
        }

        private void EditMealNameDo()
        {
            MealName.QuicklyCollapse();
            var input = new XnaInputBox(this)
            {
                Title = Translations.EditName,
                Description = Translations.Name,
                Text = ViewModel.NameOfName
            };
            input.Show();
            input.Confirmed += delegate
            {
                ViewModel.NameOfName = input.Text;
                MealName.ForceRefresh(ProgressBar);
            };
        }

        private void DeleteMealName_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.CanDeleteName())
            {
                DeleteMealNameDo();
            }
            else
            {
                MessageBox.Show(ViewModel.NameOfName, Translations.CannotDeleteThisName,
                    MessageBoxButton.OK);
            }
        }

        private void DeleteMealNameDo()
        {
            if (MessageBox.Show(
                String.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisName,
                ViewModel.NameOfName),
                Translations.DeleteName, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Save.IsEnabled = false;
                MealName.QuicklyCollapse();
                Dispatcher.BeginInvoke(() =>
                {
                    ViewModel.DeleteName();
                    MealName.ForceRefresh(ProgressBar);
                    Save.IsEnabled = ViewModel.IsDirty;
                });
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Focus();
            Dispatcher.BeginInvoke(() =>
            {
                if (ViewModel.CanSave())
                {
                    ViewModel.SaveWithUpdatedTimeAndReturn();
                }
            });
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            ViewModel.CancelAndReturn();
        }

        private void Insulin_Click(object sender, EventArgs e)
        {
            ViewModel.GoToInsulinEditing();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                String.Format(Translations.AreYouSureYouWantToPermanentlyDeleteThisMeal,
                ViewModel.IdentifiableName),
                Translations.DeleteMeal, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                ViewModel.DeleteAndSaveAndReturn();
            }
        }

        private void ViewModel_IsDirtyChanged(object sender, EventArgs e)
        {
            Save.IsEnabled = ViewModel.IsDirty;
        }

        private void ViewModel_CannotSave(object sender, CannotSaveEventArgs e)
        {
            e.Ignore = (MessageBox.Show(e.Reason, Translations.AreYouSureYouWantToSaveThisMeal,
                MessageBoxButton.OKCancel) == MessageBoxResult.OK);
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.AddItem();
        }

        private void Items_ItemTap(object sender, ListBoxItemTapEventArgs e)
        {
            var item = Items.SelectedItem as MealItemViewModel;
            if (item != null)
            {
                ViewModel.EditItem(item);
            }
            Items.SelectedItem = null;
        }

        private void ItemGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var itemGrid = (Grid)sender;
            var meal = ViewModel.Subject;
            var scores = meal.Scores;
            if (!scores.FirstExists)
            {
                itemGrid.HideColumnWithIndex(1);
            }
            if (!scores.SecondExists)
            {
                itemGrid.HideColumnWithIndex(2);
            }
            if (!scores.ThirdExists)
            {
                itemGrid.HideColumnWithIndex(3);
            }
            if (!scores.FourthExists)
            {
                itemGrid.HideColumnWithIndex(4);
            }
        }

        private void Items_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsOpened)
            {
                if (ViewModel.NeedsScrollingItemsDown)
                {
                    ViewModel.NeedsScrollingItemsDown = false;
                    ScrollItemsDown();
                }
                else
                {
                    UntombstoneTopItem();
                }
            }
            Dispatcher.BeginInvoke(() =>
            {
                if (IsOpened)
                {
                    ViewModel.UiRendered();
                }
            });
        }

        private void ViewModel_InvalidateItems(object sender, EventArgs e)
        {
            Items.ForceInvalidate();
        }

        protected void Scores_ScoreClick(object sender, EventArgs e)
        {
            ViewModel.OpenScoresSettings();
        }

        private void TranslateApplicationBar()
        {
            Save.Text = Translations.Save;
            this.GetIcon(1).Text = Translations.Cancel;
            this.GetIcon(2).Text = Translations.Insulin;
            this.GetMenuItem(0).Text = Translations.Delete;
        }

        private void TombstoneTopItem()
        {
            var topItemSource = Items.TopVisibleItem;
            if (topItemSource != null)
            {
                var topItem = topItemSource as MealItemViewModel;
                if (topItem != null)
                {
                    var meal = ViewModel.Subject;
                    var items = meal.Items;
                    var topItemIndex = items.IndexOf(topItem);
                    State[TOP_ITEM_INDEX] = topItemIndex;
                }
            }
        }

        private void UntombstoneTopItem()
        {
            if (State.ContainsKey(TOP_ITEM_INDEX))
            {
                var topItemIndex = (int)State[TOP_ITEM_INDEX];
                var meal = ViewModel.Subject;
                var items = meal.Items;
                if (topItemIndex > -1 && topItemIndex < items.Count)
                {
                    var topItem = items[topItemIndex];
                    Items.BringIntoView(topItem);
                }
            }
        }

        private void ScrollItemsDown()
        {
            var meal = ViewModel.Subject;
            var items = meal.Items;
            if (items.Count > 0)
            {
                var bottomItem = items[items.Count - 1];
                Items.BringIntoView(bottomItem);
            }
        }
    }
}