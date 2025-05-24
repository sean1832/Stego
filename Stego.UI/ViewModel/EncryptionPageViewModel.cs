using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Stego.UI.Helpers;

namespace Stego.UI.ViewModel
{
    public class EncryptionPageViewModel: INotifyPropertyChanged
    {
        // Argon2 iterations
        private long _argon2Argon2Cost = 2;
        public long Argon2Cost
        {
            get => _argon2Argon2Cost;
            set
            {
                if (_argon2Argon2Cost == value) return;
                _argon2Argon2Cost = value;
                OnPropertyChanged(nameof(Argon2Cost));
            }
        }

        // Argon2 memory size in KB
        private long _argon2Argon2Memory = 1024;
        public long Argon2Memory
        {
            get => _argon2Argon2Memory;
            set
            {
                if (_argon2Argon2Memory == value) return;
                _argon2Argon2Memory = value;
                OnPropertyChanged(nameof(Argon2Memory));
            }
        }

        // Data to be encrypted
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

        // Status
        private bool _isOutputSuccess;
        public bool IsOutputSuccess
        {
            get => _isOutputSuccess;
            set
            {
                if (_isOutputSuccess == value) return;
                _isOutputSuccess = value;
                OnPropertyChanged();           // notify bindings/listeners
            }
        }

        private string _outputMessage = "";
        public string OutputMessage
        {
            get => _outputMessage;
            set
            {
                if (_outputMessage == value) return;
                _outputMessage = value;
                OnPropertyChanged();
            }
        }

        private string _outputFilePath = "";
        public string OutputFilePath
        {
            get => _outputFilePath;
            set
            {
                if (_outputFilePath == value) return;
                _outputFilePath = value;
                OnPropertyChanged();
            }
        }

        // steganography
        public SteganographyViewModel SteganographyViewModel { get; } = new SteganographyViewModel() {
            CoverImagePath = null,
            Spacing = 10,
            LsbCount = 1
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
