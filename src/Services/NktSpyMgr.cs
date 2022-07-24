using System;

namespace GdiHook.Services
{
    public class NktSpyMgr : Nektra.Deviare2.NktSpyMgrClass
    {
        public NktSpyMgr()
        {
            var initializeResult = Initialize();
            if (initializeResult < 0)
            {
                throw new Exception($"Cannot initialize Deviare2 (error code {initializeResult}).");
            }
        }
    }
}
