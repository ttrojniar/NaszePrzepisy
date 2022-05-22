using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NaszePrzepisy.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;

namespace NaszePrzepisy.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainUserControlViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainUserControlViewModel class.
        /// </summary>
        ///
        private string wybranyPosilek;
        public string WybranyPosilek
        {
            get { return wybranyPosilek; }
            set
            {
                Set(ref wybranyPosilek, value);
                Recipes = Model.Statics.Methods.FillRecipes(value);
            }
        }

        private ObservableCollection<string> recipes;
        public ObservableCollection<string> Recipes
        {
            get { return recipes; }
            set { Set(ref recipes, value); }
        }

        private int selectedRecipe;
        public int SelectedRecipe
        {
            get { return selectedRecipe; }
            set
            {
                Set(ref selectedRecipe,value);
                if (value >= 0 && !string.IsNullOrEmpty(WybranyPosilek))
                    SelectedPrzepisId = Model.Statics.Methods.slownikPrzepisow[WybranyPosilek][value].Id;
                else
                    SelectedPrzepisId = 666666;
                SetEnabler(value);
                SelectedRecipeIngredients = Model.Statics.Methods.GetIngridients(value,WybranyPosilek);
                SelectedRecipeInstruction = Model.Statics.Methods.GetInstruction(value,WybranyPosilek);
            }
        }

        private string selectedRecipeInstruction;
        public string SelectedRecipeInstruction
        {
            get { return selectedRecipeInstruction; }
            set { Set(ref selectedRecipeInstruction, value); }
        }


        private ObservableCollection<string> selectedRecipeIngredients;
        public ObservableCollection<string> SelectedRecipeIngredients
        {
            get { return selectedRecipeIngredients; }
            set { Set(ref selectedRecipeIngredients,value); }
        }

        private bool isWindowEnabled;
        public bool IsWindowEnabled
        {
            get { return isWindowEnabled; }
            set { Set(ref isWindowEnabled,value); }
        }

        private bool enabler;
        public bool Enabler
        {
            get { return enabler; }
            set { Set(ref enabler, value); }
        }

        private int selectedPrzepisId;

        public int SelectedPrzepisId
        {
            get { return selectedPrzepisId; }
            set {Set(ref selectedPrzepisId, value); }
        }


        public ICommand AddRecipe { get; set; }
        public ICommand EditRecipe { get; set; }
        public ICommand RemoveRecipe { get; set; }
        public ICommand UploadAll { get; set; }

        public MainUserControlViewModel()
        {
            IsWindowEnabled = true ;
            Enabler = false;
            SetCommands();
            SelectedRecipe = 0;
            Messenger.Default.Register<string>(this, "wybranyposilek", message => WybranyPosilek = message);
            Messenger.Default.Register<string>(this, "dodanoprzepis", message => WybranyPosilek = message);
        }

        private void SetCommands()
        {
            AddRecipe = new RelayCommand(AddRecipeExecute);
            EditRecipe = new RelayCommand(EditRecipeExecute);
            RemoveRecipe = new RelayCommand(RemoveRecipeExecute);
            UploadAll = new RelayCommand(UploadAllRecipeExecute);
        }

        private void UploadAllRecipeExecute()
        {
            Model.Statics.Methods.UploadRecipes();
        }

        private void RemoveRecipeExecute()
        {
            Model.Statics.Methods.RemoveSelectedRecipe(SelectedPrzepisId,WybranyPosilek);
        }

        private void EditRecipeExecute()
        {            
            Views.AddRecipe window = new Views.AddRecipe();
            Messenger.Default.Send(SelectedPrzepisId, "wybranyPrzepisEdit");
            var dialog = window.ShowDialog();
            RaisePropertyChanged(() => Recipes);
        }

        private void AddRecipeExecute()
        {
            Messenger.Default.Send(string.Empty, "wybranyPosilekEdit");
            Messenger.Default.Send(-2, "wybranyPrzepisEdit");
            Views.AddRecipe window = new Views.AddRecipe();
            var dialog = window.ShowDialog();
            RaisePropertyChanged(() => Recipes);

        }

        private void SetEnabler(int value)
        {
            var test = Model.Statics.Methods.slownikPrzepisow;
            if (WybranyPosilek == null || Model.Statics.Methods.slownikPrzepisow == null || value < 0 || Model.Statics.Methods.slownikPrzepisow[WybranyPosilek][value] == null)
                Enabler = false;
            else
                Enabler = true;
        }

    }
}