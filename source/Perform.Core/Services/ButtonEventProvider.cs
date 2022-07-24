using Perform.Model;
using SharpHook;
using SharpHook.Native;

namespace Perform.Services
{
    public class ButtonEventService : IDisposable
    {
        private TaskPoolGlobalHook? _hook;
        private readonly List<Button> _buttons;
        private Task? _task;

        public event ButtonEventDelegate? ButtonEvent;

        public ButtonEventService(Model.Buttons? buttons)
        {
            _buttons = buttons == null
                ? new List<Button>()
                : buttons.Keys
                    .Select((keyCode, index) => new Button(keyCode, index, buttons.LongPress))
                    .ToList();
        }

        public void Start()
        {
            if (_hook != null || _buttons.Count == 0)
            {
                return;
            }

            _task = Task.Run(async() =>
            {
                _hook = new TaskPoolGlobalHook(new TaskPoolGlobalHookOptions(4));
                
                _hook.KeyPressed += OnKeyPressed;
                _hook.KeyReleased += OnKeyReleased;
                _hook.KeyTyped += OnKeyTyped;

                await _hook.RunAsync();
            });
        }

        private void OnKeyTyped(object? sender, KeyboardHookEventArgs e)
        {
            if (TryGetButton(e.Data.RawCode, out _))
            {
                e.Reserved = EventReservedValueMask.SuppressEvent;
            }
        }

        public void Stop()
        {
            _hook?.Dispose();
            _task?.Dispose();
            _hook = null;
            _task = null;
        }

        private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
        {
            if (!TryGetButton(e.Data.RawCode, out var button))
            {
                return;
            }

            e.Reserved = EventReservedValueMask.SuppressEvent;
                
            if (button.Press())
            {
                ButtonEvent?.Invoke(ButtonPressType.Down, button.Index);
            }
            else
            {
                if (button.LongPress())
                {
                    ButtonEvent?.Invoke(ButtonPressType.Long, button.Index);
                }
            }
        }

        private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
        {
            if (!TryGetButton(e.Data.RawCode, out var button))
            {
                return;
            }

            e.Reserved = EventReservedValueMask.SuppressEvent;

            button.Release();
            ButtonEvent?.Invoke(ButtonPressType.Up, button.Index);
        }

        private bool TryGetButton(ushort keyCode, out Button button)
        {
            var foundButton = _buttons.FirstOrDefault(b => b.KeyCode == keyCode);
            if (foundButton != null)
            {
                button = foundButton;
                return true;
            }

            button = new Button(-1, 0, 0);
            return false;
        }

        public void Dispose()
        {
            _hook?.Dispose();
            _task?.Dispose();
            _hook = null;
            _task = null;
        }
    }
}