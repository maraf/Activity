﻿using Neptuo;
using Neptuo.Observables;
using Neptuo.Observables.Collections;
using Neptuo.Observables.Commands;
using Neptuo.Productivity.ActivityLog.Services;
using Neptuo.Productivity.ActivityLog.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Neptuo.Productivity.ActivityLog.ViewModels
{
    public class CategoryListViewModel : ObservableObject
    {
        private readonly INavigator navigator;

        public ObservableCollection<CategoryViewModel> Items { get; private set; }

        public ICommand Create { get; private set; }
        public ICommand Edit { get; private set; }
        public ICommand Remove { get; private set; }

        public CategoryListViewModel(INavigator navigator)
        {
            Ensure.NotNull(navigator, "navigator");
            this.navigator = navigator;

            Items = new ObservableCollection<CategoryViewModel>();
            Create = new DelegateCommand(OnCreate);
            Edit = new DelegateCommand<CategoryViewModel>(OnEdit);
            Remove = new DelegateCommand<CategoryViewModel>(vm => Items.Remove(vm));
        }

        private async void OnEdit(CategoryViewModel source)
        {
            ICategory target = await navigator.EditCategory(source);
            if (target != null)
            {
                source.Color = target.Color;
                source.Name = target.Name;
            }
        }

        private async void OnCreate()
        {
            ICategory category = await navigator.NewCategory();
            if (category != null)
            {
                Items.Add(new CategoryViewModel()
                {
                    Color = category.Color,
                    Name = category.Name
                });
            }
        }
    }
}
