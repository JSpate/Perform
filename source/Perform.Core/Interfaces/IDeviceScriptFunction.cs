using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perform.Interfaces;

public interface IDeviceScriptFunction
{
    public void Initialize(ShowScript showScript, params string[] devices);

    public void Loop();

    public bool IsFinished { get; }
}