﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Dietphone.ViewModels;
using Dietphone.Models;

namespace Dietphone.Views
{
    public partial class MealEditing : PhoneApplicationPage
    {
        public List<string> MealNames { get; set; }
        public List<MealItemViewModel> Items { get; set; }
        public MealEditing()
        {
            InitializeComponent();
            MealNames = new List<string>();
            MealNames.Add("Bez nazwy");
            MealNames.Add("Śniadanie");
            MealNames.Add("Obiad");
            MealNames.Add("Kolacja");

            Items = new List<MealItemViewModel>();
            var mealItem = new MealItem();
            mealItem.Owner = App.Factories;
            mealItem.ProductId = App.Factories.Products[50].Id;
            mealItem.Value = 50;
            Items.Add(new MealItemViewModel(mealItem));
            mealItem = new MealItem();
            mealItem.Owner = App.Factories;
            mealItem.ProductId = App.Factories.Products[100].Id;
            mealItem.Value = 100;
            Items.Add(new MealItemViewModel(mealItem));

            DataContext = this;
        }

        private void AddMealName_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditMealName_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteMealName_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Save_Click(object sender, EventArgs e)
        {

        }

        private void Cancel_Click(object sender, EventArgs e)
        {

        }

        private void Delete_Click(object sender, EventArgs e)
        {

        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}