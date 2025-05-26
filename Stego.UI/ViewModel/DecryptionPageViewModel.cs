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

        private string? _inputFilePath;
        public string? InputFilePath
        {
            get => _inputFilePath;
            set
            {
                if (_inputFilePath == value) return;
                _inputFilePath = value;
                OnPropertyChanged(nameof(InputFilePath));
            }
        }

        // TextBoxData to be decrypted
        private byte[]? _textBoxData;
        public byte[]? TextBoxData
        {
            get => _textBoxData;
            set
            {
                if (_textBoxData == value) return;
                _textBoxData = value;
                OnPropertyChanged(nameof(TextBoxData));
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

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
