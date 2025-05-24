using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stego.UI.ViewModel
{
    public class SteganographyViewModel : INotifyPropertyChanged
    {
        private string? _coverImagePath;
        public string? CoverImagePath
        {
            get => _coverImagePath;
            set { if (_coverImagePath == value) return; _coverImagePath = value; OnPropertyChanged(); }
        }
        private short _lsbCount;
        public short LsbCount
        {
            get => _lsbCount;
            set { if (_lsbCount == value) return; _lsbCount = value; OnPropertyChanged(); }
        }
        private int _spacing;
        public int Spacing
        {
            get => _spacing;
            set { if (_spacing == value) return; _spacing = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
