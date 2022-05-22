using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NaszePrzepisy.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NaszePrzepisy.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class AddRecipeViewModel : ViewModelBase
    {
        private int wybranyPrzepisDoEdycji;
        public int WybranyPrzepisDoEdycji
        {
            get { return wybranyPrzepisDoEdycji; }
            set { Set(ref wybranyPrzepisDoEdycji, value); FillFields(); }
        }

        private ObservableCollection<string> mealTimes;
        public ObservableCollection<string> MealTimes
        {
            get { return mealTimes; }
            set { Set(ref mealTimes, value); }
        }

        private string recipeName;
        public string RecipeName
        {
            get { return recipeName; }
            set {Set(ref recipeName, value); }
        }

        private string selectedMealTime;
        public string SelectedMealTime
        {
            get { return selectedMealTime; }
            set { Set(ref selectedMealTime, value); }
        }

        private string newIngridient;
        public string NewIngridient
        {
            get { return newIngridient; }
            set { Set(ref newIngridient, value); }
        }

        private ObservableCollection<string> ingridients;
        public ObservableCollection<string> Ingridients
        {
            get { return ingridients; }
            set {Set(ref ingridients,value); }
        }

        private int selectedIngridient;
        public int SelectedIngridient
        {
            get { return selectedIngridient; }
            set { Set(ref selectedIngridient, value); }
        }

        private int przepisId;
        public int PrzepisId
        {
            get { return przepisId; }
            set { Set(ref przepisId, value); }
        }

        private string preparationInstruction;
        public string PreparationInstruction
        {
            get { return preparationInstruction; }
            set { Set(ref preparationInstruction, value); }
        }

        public ICommand AddButton { get; set; }
        public ICommand RemoveButton { get; set; }
        public ICommand OKClick { get; set; }
        public ICommand CancelClick { get; set; }


        public AddRecipeViewModel()
        {
            Messenger.Default.Register<int>(this, "wybranyPrzepisEdit", message => WybranyPrzepisDoEdycji = message);
            Ingridients = new ObservableCollection<string>();
            SetCommands();
            MealTimes = Model.Statics.Methods.ConvertArrayToObsCollection();
        }

        private void SetCommands()
        {
            AddButton = new RelayCommand(AddButtonExecute);
            RemoveButton = new RelayCommand(RemoveButtonExecute);
            OKClick = new RelayCommand(OkClickExecute);
            CancelClick = new RelayCommand(CancelClickExecute);
        }

        private void CancelClickExecute()
        {
            var window = Application.Current.Windows.Cast<Window>().Single(w => w.DataContext == this);
            window.DialogResult = false;
            window.Close();
        }

        private void OkClickExecute()
        {
            bool everythingOk = true;
            if (string.IsNullOrEmpty(SelectedMealTime))
            {
                MessageBox.Show("Nie została uzupełniona pora posiłku", "Ups", MessageBoxButton.OK, MessageBoxImage.Warning);
                everythingOk = false;
            }
            if (string.IsNullOrEmpty(RecipeName))
            {
                MessageBox.Show("Nie została uzupełniona nazwa posiłku", "Ups", MessageBoxButton.OK, MessageBoxImage.Warning);
                everythingOk = false;
            }
            if (Ingridients.Count == 0)
            {
                MessageBox.Show("Nie zostały uzupełnione składniki posiłku", "Ups", MessageBoxButton.OK, MessageBoxImage.Warning);
                everythingOk = false;
            }
            if (string.IsNullOrEmpty(PreparationInstruction))
            {
                MessageBox.Show("Nie został uzupełniony opis przyrządzenia", "Ups", MessageBoxButton.OK, MessageBoxImage.Warning);
                everythingOk = false;
            }
            if (everythingOk)
            {
                if (WybranyPrzepisDoEdycji==-2)
                    Model.Statics.Methods.UpdateXML(new Przepis(SelectedMealTime, RecipeName, Model.Statics.Methods.ConvertToList(Ingridients), PreparationInstruction, Model.Statics.Methods.GetPrzepisID()),true);
                else
                    Model.Statics.Methods.UpdateXML(new Przepis(SelectedMealTime, RecipeName, Model.Statics.Methods.ConvertToList(Ingridients), PreparationInstruction, WybranyPrzepisDoEdycji),false);

                SelectedMealTime = string.Empty;
                RecipeName = string.Empty;
                Ingridients = new ObservableCollection<string>();
                PreparationInstruction = string.Empty;
                var window = Application.Current.Windows.Cast<Window>().Single(w => w.DataContext == this);
                window.DialogResult = true;
                window.Close();
            }
        }

        private void RemoveButtonExecute()
        {
            Ingridients.Remove(Ingridients[SelectedIngridient]);
        }

        private void AddButtonExecute()
        {
            if (!string.IsNullOrEmpty(NewIngridient))
                Ingridients.Add(NewIngridient);
            NewIngridient = string.Empty;
        }

        private void FillFields()
        {
            if (WybranyPrzepisDoEdycji == -2)
            {
                SelectedMealTime = string.Empty;
                RecipeName = string.Empty;
                Ingridients = new ObservableCollection<string>();
                PreparationInstruction = string.Empty;
            }
            else
            {
                Przepis przepis = GetPrzepis(WybranyPrzepisDoEdycji);
                SelectedMealTime = przepis.Posilek;
                RecipeName = przepis.Nazwa;
                Ingridients = ToObsColl(przepis.Skladniki);
                PreparationInstruction = przepis.Przygotowanie;
            }
        }

        private Przepis GetPrzepis(int wybranyPrzepisDoEdycji)
        {
            foreach (var poraPosilku in Model.Statics.Methods.slownikPrzepisow)
            {
                foreach (var przepis in poraPosilku.Value.Where(x => x.Id == wybranyPrzepisDoEdycji))
                    return przepis;
            }
            return null;
        }

        private ObservableCollection<string> ToObsColl(List<string> skladniki)
        {
            ObservableCollection<string> result = new ObservableCollection<string>();
            foreach (var skladnik in skladniki)
                result.Add(skladnik);
            return result;
        }
    }
}