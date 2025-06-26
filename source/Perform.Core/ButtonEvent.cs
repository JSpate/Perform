using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Perform
{
    public class ButtonEvent(int buttonId, ButtonState pressType) : INotification
    {
        public int ButtonId { get; } = buttonId;

        public ButtonState PressType { get; } = pressType;
    }
}
