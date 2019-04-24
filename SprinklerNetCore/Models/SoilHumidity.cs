using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Device.Gpio;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace SprinklerNetCore.Models
{
    public class SoilHumidity : IDisposable
    {
        private const int GPIO_PIN = 21;
        private GpioController _controller;

        public SoilHumidity()
        {        
#if DEBUG
            Debug.WriteLine($"Soilhumidity openining pin");
#else
            _controller = new GpioController();
            _controller.OpenPin(GPIO_PIN, PinMode.Input);
#endif
        }

        [Display(Name = nameof(Resources.Text.IsHumid), ResourceType = typeof(Resources.Text))]
        public bool IsHumid
        {
            get
            {
#if DEBUG
                Console.WriteLine("Soil humidity true");
                return true;
#else
                return _controller.Read(GPIO_PIN) == PinValue.High;
#endif
            }
        }

        public void Dispose()
        {
            // close port
#if DEBUG
            Debug.WriteLine($"Cloding Soilhumidity");
#else
            _controller.ClosePin(GPIO_PIN);
#endif
        }
    }
}
