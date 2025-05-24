using Stego.UI.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Stego.UI.ViewModel
{
    public class DecryptionPageViewModel: INotifyPropertyChanged
    {
        // Input type (string or file)
        private InputDataType _inputType = InputDataType.String;
        public InputDataType InputType
        {
            get => _inputType;
            set
            {
                if (_inputType == value) return;
                _inputType = value;
                OnPropertyChanged(nameof(InputType));
            }
        }

        // Data to be decrypted
        private byte[]? _data;
        public byte[]? Data
        {
            get => _data;
            set
            {
                if (_data == value) return;
                _data = value;
                OnPropertyChanged(nameof(Data));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
