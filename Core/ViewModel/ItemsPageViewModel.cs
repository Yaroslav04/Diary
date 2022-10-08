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
            Title = $"{DateTime.Now.ToShortDateString()}";
            Items = new ObservableCollection<DiaryClass>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
            AddCommand = new Command(Add);
            SearchCommand = new Command(Search);
            TapSwitchCommand = new Command(TappedSwitch);
            ItemsSwitchCommand = new Command(ItemsSwitch);
            ItemTapped = new Command<DiaryClass>(Tapped);
        }

        private async void Search()
        {
            string _type = await Shell.Current.DisplayActionSheet("Выбор типа нового елемента", "Cancel", null, App.Types.ToArray());
            if (String.IsNullOrWhiteSpace(_type) | _type == "Cancel")
            {
                await Shell.Current.DisplayAlert("Поиск", $"Не выбран тип", "OK");
                await Refresh();
                return;
            }

            string _name = "";
            var names = await App.DataBase.GetNamesFromObjectsAsync(_type);
            if (names == null)
            {
                await Shell.Current.DisplayAlert("Поиск", $"Пусто", "OK");
                await Refresh();
                return;
            }
            else
            {
                _name = await Shell.Current.DisplayActionSheet("Выбор названия елемента", "Cancel", null, names.ToArray());
                if (String.IsNullOrWhiteSpace(_type) | _type == "Cancel")
                {
                    await Shell.Current.DisplayAlert("Поиск", $"Не выбрано название", "OK");
                    await Refresh();
                    return;
                }
            }

            var result = await App.DataBase.GetObjectsAsync(_type, _name);
            if (result != null)
            {
                Items.Clear();
                foreach (var item in result)
                {
                    var subitem = item;
                    subitem.Progress = await GetProgress(item);
                    Items.Add(subitem);
                }
            }
            else
            {
                await Refresh();
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        #region Properties

        public ObservableCollection<DiaryClass> Items { get; }

        private string tapSwitcher = "📍";
        public string TapSwitcher
        {
            get => tapSwitcher;
            set
            {
                SetProperty(ref tapSwitcher, value);
            }
        }

        private string itemsSwitcher = "🏳";
        public string ItemsSwitcher
        {
            get => itemsSwitcher;
            set
            {
                SetProperty(ref itemsSwitcher, value);
            }
        }

        #endregion

        #region Commands

        public Command<DiaryClass> ItemTapped { get; }
        public Command LoadItemsCommand { get; }
        public Command AddCommand { get; }
        public Command TapSwitchCommand { get; }
        public Command ItemsSwitchCommand { get; }
        public Command SearchCommand { get; }

        #endregion

        async Task ExecuteLoadItemsCommand()
        {
            if (App.StartSwitch)
            {
                string password;
                do
                {
                    password = await Shell.Current.DisplayPromptAsync($"Вход в систему", $"Введте пароль", maxLength: 5);

                } while (password != "662");

                App.StartSwitch = false;
                await Reminder();
            }

            IsBusy = true;

            try
            {
                await Refresh();
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
            if (ItemsSwitcher == "🏳")
            {
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
                        var subitem = item;
                        subitem.Progress = await GetProgress(item);
                        Items.Add(subitem);
                    }
                }
            }
            else
            {
                foreach (var type in App.Types)
                {
                    List<string> names = new List<string>();

                    foreach (var item in await App.DataBase.GetObjectsAsync(type))
                    {

                        if (item.Period != 0)
                        {
                            names.Add(item.Name);
                        }
                    }

                    if (names.Count == 0)
                    {
                    }
                    else
                    {
                        names = names.Distinct().ToList();
                        foreach (string name in names)
                        {
                            foreach (var dayPath in App.DayPaths)
                            {
                                var subitem = await App.DataBase.GetLastObjectsAsync(type, name, dayPath);
                                if (subitem != null)
                                {
                                    var sub = subitem;
                                    sub.Progress = await GetProgress(subitem);
                                    Items.Add(sub);
                                }
                            }
                        }
                    }
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

            string _dayPath = App.DayPaths[0];
            if (_period != 0)
            {
                _dayPath = await Shell.Current.DisplayActionSheet("Период дня", "Cancel", null, App.DayPaths.ToArray());
                if (String.IsNullOrWhiteSpace(_type) | _type == "Cancel")
                {
                    await Shell.Current.DisplayAlert("Выбор нового елемента", $"Не выбран тип", "OK");
                    return;
                }
            }

            DateTime _date = DateTime.Now;
            string _dateString = await Shell.Current.DisplayPromptAsync($"Выбор нового елемента", $"Дата события", initialValue: _date.ToString(), maxLength: 19);
            if (!DateTime.TryParse(_dateString, out _date))
            {
                await Shell.Current.DisplayAlert("Выбор нового елемента", $"Не верно указано дата-время", "OK");
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
                    DayPath = _dayPath,
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

            if (TapSwitcher == "🗑")
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
                bool answer = await Shell.Current.DisplayAlert("Клонировать?", $"{item.Type}\n{item.Name}\nпериод: {item.Period} {item.DayPath}", "Да", "Нет");
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

        private void TappedSwitch()
        {
            if (TapSwitcher == "📍")
            {
                TapSwitcher = "🗑";
            }
            else
            {
                TapSwitcher = "📍";
            }
        }

        private void ItemsSwitch()
        {
            if (ItemsSwitcher == "🏳")
            {
                ItemsSwitcher = "🏁";
            }
            else
            {
                ItemsSwitcher = "🏳";
            }
            Refresh();
        }

        async Task Reminder()
        {
            foreach (var type in App.Types)
            {
                List<string> names = new List<string>();
                try
                {
                    foreach (var item in await App.DataBase.GetObjectsAsync(type))
                    {
                        if (item.Period != 0)
                        {
                            names.Add(item.Name);
                        }
                    }

                    if (names.Count == 0)
                    {

                    }
                    else
                    {
                        names = names.Distinct().ToList();
                        foreach (string name in names)
                        {
                            foreach (var dayPath in App.DayPaths)
                            {
                                var subitem = await App.DataBase.GetLastObjectsAsync(type, name, dayPath);
                                if (subitem != null)
                                {
                                    if ((DateTime.Now.DayOfYear - subitem.SaveDate.DayOfYear) >= subitem.Period)
                                    {
                                        if (subitem.DayPath == "Весь день")
                                        {
                                            await Shell.Current.DisplayAlert("Напоминание", $"Срок {subitem.Name} {subitem.DayPath} последний: {subitem.SaveDate.ToShortDateString()}", "OK");
                                        }

                                        if (subitem.DayPath == "Утро")
                                        {
                                            if (DateTime.Now.Hour < 10)
                                            {
                                                await Shell.Current.DisplayAlert("Напоминание", $"Срок {subitem.Name} {subitem.DayPath} последний: {subitem.SaveDate.ToShortDateString()}", "OK");
                                            }
                                        }

                                        if (subitem.DayPath == "Обед")
                                        {
                                            if (DateTime.Now.Hour > 10 & DateTime.Now.Hour < 18)
                                            {
                                                await Shell.Current.DisplayAlert("Напоминание", $"Срок {subitem.Name} {subitem.DayPath} последний: {subitem.SaveDate.ToShortDateString()}", "OK");
                                            }
                                        }

                                        if (subitem.DayPath == "Вечер")
                                        {
                                            if (DateTime.Now.Hour > 18)
                                            {
                                                await Shell.Current.DisplayAlert("Напоминание", $"Срок {subitem.Name} {subitem.DayPath} последний: {subitem.SaveDate.ToShortDateString()}", "OK");
                                            }
                                        }

                                        if (subitem.DayPath == "До обеда")
                                        {
                                            if (DateTime.Now.Hour < 14)
                                            {
                                                await Shell.Current.DisplayAlert("Напоминание", $"Срок {subitem.Name} {subitem.DayPath} последний: {subitem.SaveDate.ToShortDateString()}", "OK");
                                            }
                                        }

                                        if (subitem.DayPath == "После обеда")
                                        {
                                            if (DateTime.Now.Hour > 14)
                                            {
                                                await Shell.Current.DisplayAlert("Напоминание", $"Срок {subitem.Name} {subitem.DayPath} последний: {subitem.SaveDate.ToShortDateString()}", "OK");
                                            }
                                        }
                                    }
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

        async Task<double> GetProgress(DiaryClass item)
        {
            if (item.Period == 0)
            {
                return 0;
            }
            else
            {
                try
                {
                    var result = await App.DataBase.GetObjectsAsync();
                    if (result == null)
                    {
                        return 0;
                    }
                    else
                    {
                        result = result.Where(x => x.Type == item.Type & x.Name == item.Name & (DateTime.Now - x.SaveDate).TotalDays < 30).ToList();
                        if (result == null)
                        {
                            return 0;
                        }
                        else
                        {
                            double initDay = Math.Round((DateTime.Now - result.OrderBy(x => x.SaveDate).First().SaveDate).TotalDays);
                            var grouped = result.GroupBy(x => x.DayPath);

                            if (grouped.Count() == 0)
                            {
                                foreach (var group in grouped)
                                {
                                    double res = group.Count() / (initDay / group.First().Period);
                                    return res;
                                }
                            }
                            else
                            {
                                int count = 0;
                                double res = 0;
                                foreach (var group in grouped)
                                {
                                    res = res + group.Count() / (initDay / group.First().Period);
                                    count = count + 1;
                                }
                                return res / count;
                            }
                        }
                    }
                    return 0;

                }
                catch
                {
                    return 0;
                }            
            }
        }
    }
}
