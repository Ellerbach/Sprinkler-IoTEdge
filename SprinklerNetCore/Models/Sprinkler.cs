using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Diagnostics;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SprinklerNetCore.Models
{

    public class Sprinkler : IDisposable
    {
        private const int GPIO_PIN_D0 = 5;
        private const int GPIO_PIN_D1 = 6;
        private const int GPIO_PIN_D2 = 13;
        private const int GPIO_PIN_D3 = 19;
        private const int GPIO_PIN_D4 = 26;

        private bool _isSprinklerOpen = false;
        private GpioController _controller;
        private int _sprinkler = -1;
        private PinValue _pinValue;
        private Timer _timerCallBack;
        private long _ticksToWait;
        private int _number = -1;

        [Display(Name = nameof(Resources.Text.IsInverted), ResourceType = typeof(Resources.Text))]
        public bool IsInverted { get; set; }
        [Display(Name = nameof(Resources.Text.Number), ResourceType = typeof(Resources.Text))]
        public int Number
        {
            get { return _number; }
            set
            {
                if (_number != value)
                {

#if DEBUG
                    Debug.WriteLine($"closing pin {_number}");
#else
                    if(_sprinkler!=-1)
                        _controller.ClosePin(_sprinkler);
#endif
                    _number = value;
                    switch (_number)
                    {
                        case 0:
                            _sprinkler = GPIO_PIN_D0;
                            break;
                        case 1:
                            _sprinkler = GPIO_PIN_D1;
                            break;
                        case 2:
                            _sprinkler = GPIO_PIN_D2;
                            break;
                        case 3:
                            _sprinkler = GPIO_PIN_D3;
                            break;
                        case 4:
                            _sprinkler = GPIO_PIN_D4;
                            break;
                        default:
                            throw new Exception("Sprinkler are from 0 to 4");
                    }
#if DEBUG
                    Debug.WriteLine($"Opening GPIO ${Number}");
#else
                _controller.OpenPin(_sprinkler, PinMode.Output);
                _pinValue = IsInverted ? PinValue.High : PinValue.Low;
                _controller.Write(_sprinkler, _pinValue);
#endif
                }
            }
        }

        [Display(Name = nameof(Resources.Text.Name), ResourceType = typeof(Resources.Text))]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.Text.TypicalProgram), ResourceType = typeof(Resources.Text))]
        public TypicalProgram TypicalProgram { get; set; }

        public Sprinkler()
        {
            _timerCallBack = new Timer(TimerCallBackTick, this, Timeout.Infinite, Timeout.Infinite);
            //MyTimerCallBack.Tick += MyTimerCallBack_Tick;

            _ticksToWait = DateTime.Now.Ticks;
#if DEBUG
            Debug.WriteLine($"Creating GPIO controller");
#else
            _controller = new GpioController();
#endif
        }

        // open or close a sprinkler
        [Display(Name = nameof(Resources.Text.Open), ResourceType = typeof(Resources.Text))]
        [JsonIgnore]
        public bool Open
        {
            get { return _isSprinklerOpen; }
            set
            {
                _isSprinklerOpen = value;

                if (_isSprinklerOpen)
                    _pinValue = IsInverted ? PinValue.Low : PinValue.High;
                else
                    _pinValue = IsInverted ? PinValue.High : PinValue.Low;
#if DEBUG
                Debug.WriteLine($"Open/Close {Number}, status: {_isSprinklerOpen}, pinstatus: {_pinValue}");
#else
                _controller.Write(_sprinkler, _pinValue);
#endif
            }
        }

        [Display(Name = nameof(Resources.Text.CloseAfter), ResourceType = typeof(Resources.Text))]
        [JsonIgnore]
        public TimeSpan CloseAfter
        {
            set { _timerCallBack.Change(value, TimeSpan.Zero); }
            //set { MyTimerCallBack = value; }
        }

        //public HumiditySensor HumiditySensor;

        private void TimerCallBackTick(object sender)
        {
            Sprinkler Sprinklers = (Sprinkler)sender;
            Sprinklers.Open = false;
            //Sprinklers.TimerCallBack.Stop();
            Sprinklers.CloseAfter = new TimeSpan(0, 0, 0, 0, Timeout.Infinite);
        }

        public void Dispose()
        {
            // clean and clode port
#if DEBUG
            Debug.WriteLine($"Closing port {Number}");
#else
            _controller.ClosePin(_sprinkler);
#endif
        }
    }
}
