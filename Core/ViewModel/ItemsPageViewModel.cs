using Android.OS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Diary.Core.ViewModel
{
    public class ItemsPageViewModel : BaseViewModel
    {
        public ItemsPageViewModel()
        {
            Title = $"Дневник {DateTime.Now.ToShortDateString()}";
            Items = new ObservableCollection<DiaryClass>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            AddCommand = new Command(Add);
            SwitchCommand = new Command(Sw);
            ItemTapped = new Command<DiaryClass>(Tapped);
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        #region Properties

        public ObservableCollection<DiaryClass> Items { get; }

        private string switcher = "📍";
        public string Switcher
        {
            get => switcher;
            set
            {
                SetProperty(ref switcher, value);
            }
        }

        #endregion


        #region Commands

        public Command<DiaryClass> ItemTapped { get; }
        public Command LoadItemsCommand { get; }
        public Command AddCommand { get; }
        public Command SwitchCommand { get; }

        #endregion

        async Task ExecuteLoadItemsCommand()
        {
            if (App.StartSwitch)
            {
                string password;
                Debug.WriteLine("d");
                do
                {
                    Debug.WriteLine("d");
                    password = await Shell.Current.DisplayPromptAsync($"Вход в систему", $"Введте пароль", maxLength: 5);

                } while (password != "6");

                App.StartSwitch = false;
                await Reminder();
            }

            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await App.DataBase.GetObjectsAsync();
                if (items.Count == 0)
                {
                    return;
                }
                else
                {
                    items = items.OrderByDescending(x => x.SaveDate).ToList();
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }
                }
            }
            catch
            {

            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task Refresh()
        {
            Items.Clear();
            var items = await App.DataBase.GetObjectsAsync();
            if (items.Count == 0)
            {
                return;
            }
            else
            {
                items = items.OrderByDescending(x => x.SaveDate).ToList();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
        }

        private async void Add(object obj)
        {

            string _type = await Shell.Current.DisplayActionSheet("Выбор типа нового елемента", "Cancel", null, App.Types.ToArray());
            if (String.IsNullOrWhiteSpace(_type) | _type == "Cancel")
            {
                await Shell.Current.DisplayAlert("Выбор нового елемента", $"Не выбран тип", "OK");
                return;
            }

            string _name;
            var names = await App.DataBase.GetNamesFromObjectsAsync(_type);
            if (names == null)
            {
                _name = await Shell.Current.DisplayPromptAsync($"Выбор нового елемента", $"Введите название", maxLength: 25);
                if (String.IsNullOrWhiteSpace(_name))
                {
                    await Shell.Current.DisplayAlert("Выбор нового елемента", $"Не указано название", "OK");
                    return;
                }
            }
            else
            {
                names.Add("+ Новый елемент");
                string _nameSelector = await Shell.Current.DisplayActionSheet("Выбор названия елемента", "Cancel", null, names.ToArray());
                if (String.IsNullOrWhiteSpace(_type) | _type == "Cancel")
                {
                    await Shell.Current.DisplayAlert("Выбор нового елемента", $"Не выбрано название", "OK");
                    return;
                }

                if (_nameSelector == "+ Новый елемент")
                {
                    _name = await Shell.Current.DisplayPromptAsync($"Выбор нового елемента", $"Введите название", maxLength: 25);
                    if (String.IsNullOrWhiteSpace(_name))
                    {
                        await Shell.Current.DisplayAlert("Выбор нового елемента", $"Не указано название", "OK");
                        return;
                    }
                }
                else
                {
                    _name = _nameSelector;
                }
            }

            string _description = await Shell.Current.DisplayPromptAsync($"Выбор нового елемента", $"Описание", maxLength: 100);
            int placeholder = await App.DataBase.GetLastPeriodFromObjectsAsync(_type, _name);
            string _periodString = await Shell.Current.DisplayPromptAsync($"Выбор нового елемента", $"Введите период в днях", initialValue: placeholder.ToString(), maxLength: 17);
            int _period;
            if (!int.TryParse(_periodString, out _period))
            {
                _period = 0;
            }

            DateTime _date = DateTime.Now;
            string _dateString = await Shell.Current.DisplayPromptAsync($"Выбор нового елемента", $"Описание", initialValue:_date.ToString(), maxLength: 19);
            if (!DateTime.TryParse(_dateString, out _date))
            {
                await Shell.Current.DisplayAlert("Выбор нового елемента", $"Не верно указано время", "OK");
                return;
            }

            try
            {
                await App.DataBase.SaveObjectAsync(
                new DiaryClass
                {
                    Type = _type,
                    Name = _name,
                    Descripton = _description,
                    Period = _period,
                    SaveDate = Convert.ToDateTime(_date)
                }
                );
                await Shell.Current.DisplayAlert($"Выбор нового елемента", $"Сохранено", "ОК");

            }
            catch
            {
                await Shell.Current.DisplayAlert($"Выбор нового елемента", $"Ошибка", "ОК");
            }
            finally
            {
                await Refresh();
            }
          
        }

        async void Tapped(DiaryClass item)
        {

            if (item == null)
                return;

            if (Switcher == "🗑")
            {
                bool answer = await Shell.Current.DisplayAlert("Удалить", $"{item.Type}\n{item.Name}\n{item.SaveDate}", "Да", "Нет");
                if (answer)
                {
                    try
                    {
                        await App.DataBase.DeleteObjectAsync(item);
                    }
                    catch
                    {
                        await Shell.Current.DisplayAlert($"Удаление елемента", $"Ошибка", "ОК");
                    }
                    finally
                    {
                        await Refresh();
                    }
                }
            }
            else
            {
                bool answer = await Shell.Current.DisplayAlert("Клонировать?", $"{item.Type}\n{item.Name}\nпериод: {item.Period}", "Да", "Нет");
                if (answer)
                {
                    try
                    {
                        var cloneItem = item;
                        cloneItem.SaveDate = DateTime.Now;
                        cloneItem.N = 0;
                        await App.DataBase.SaveObjectAsync(item);
                    }
                    catch
                    {
                        await Shell.Current.DisplayAlert($"Клонирование елемента", $"Ошибка", "ОК");
                    }
                    finally
                    {
                        await Refresh();
                    }
                }
            }   
        }

        async Task Reminder()
        {
            foreach (var type in App.Types)
            {
                List<string> names = new List<string>();
                try
                {
                    foreach (var item in await App.DataBase.GetObjectsFromTypeAsync(type))
                    {
                        if (item.Period != 0)
                        {
                            names.Add(item.Name);
                        }
                    }

                    if (names.Count == 0)
                    {
                        return;
                    }
                    else
                    {
                        names = names.Distinct().ToList();
                        foreach(string name in names)
                        {
                            var subitem = await App.DataBase.GetLastObjectsFromTypeAndNameAsync(type, name);
                            if (subitem != null)
                            {
                                if ((DateTime.Now.DayOfYear - subitem.SaveDate.DayOfYear) >= subitem.Period)
                                {
                                    await Shell.Current.DisplayAlert("Напоминание", $"Срок {subitem.Name} {subitem.SaveDate}", "OK");
                                }
                            }                          
                        }
                    }
                }
                catch
                {

                }
            }             
        }

        private void Sw()
        {
            if (Switcher == "📍")
            {
                Switcher = "🗑";
            }
            else
            {
                Switcher = "📍";
            }
        }
    }
}
