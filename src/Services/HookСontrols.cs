 using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Nektra.Deviare2;
using Interop.Win32;
using GdiHook.DTO;
using GdiHook.Interop;

namespace GdiHook.Services
{
    public class HookСontrols
    {
        enum GrapStatus { Idle, Work };
        NktSpyMgr SpyMgr { get; }
        Dictionary<IntPtr, ControlElement> GrabControls { get; set; }
        Stack<ControlElement> StackPaintControl { get; set; } = new Stack<ControlElement>();
        private ControlElement StackPeek()
        {
            if (StackPaintControl.Count() > 0)
            {
                return StackPaintControl.Peek();
            }
            else
            {
                return null;
            }
        }
        GrapStatus Status { get; set; } = GrapStatus.Idle;

        public HookСontrols(NktSpyMgr spyMgr)
        {
            SpyMgr = spyMgr;
        }

        public ControlElement Grab(IntPtr hWnd)
        {
            if (Status != GrapStatus.Idle)
            {
                throw new Exception($"You can only start capturing when the previous one is finished. (Status = `{Status}`)");
            }

            StackPaintControl.Clear();

            var result = GetWindowStructure(hWnd);
            GrabControls = result.ToDictionary();
            Status = GrapStatus.Idle;

            // Получение id процесса в который мы будем внедрятся
            NktHook[] hooks = Array.Empty<NktHook>();
            try
            {
                hooks = InitializationHook(result.ProcessId);
                // Запускае процедура захвата, для этого выставляем флаг в нужный статус и отправляем команду WinAPi на перерисовку окна
                Status = GrapStatus.Work;
                User32.RedrawWindow(
                        hWnd,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        RedrawWindowFlags.RDW_UPDATENOW |
                        RedrawWindowFlags.RDW_FRAME |
                        RedrawWindowFlags.RDW_INVALIDATE |
                        RedrawWindowFlags.AllChildren |
                        RedrawWindowFlags.Erase);
            }
            finally
            {
                Status = GrapStatus.Idle;
                foreach (var hook in hooks)
                {
                    hook.Detach(result.ProcessId, true);
                    hook.Unhook(true);
                }
            }

            GrabControls.ToList().AsParallel().ForAll(s =>
            {
                s.Value.Items = s.Value.Items.Distinct().ToList();
            });
            return result;
        }

        private bool IsGrab()
            => Status == GrapStatus.Work;

        private NktHook[] InitializationHook(int processId)
        {
            List<NktHook> result = new List<NktHook>();
            void RegisterHook(string func, DNktHookEvents_OnFunctionCalledEventHandler @event, int hookFlags)
            {
                var hook = SpyMgr.CreateHook(func, hookFlags);
                hook.Attach(processId, true);
                hook.Hook(true);
                hook.OnFunctionCalled += @event;
                result.Add(hook);
            }

            RegisterHook("user32.dll!BeginPaint", BeginPaint, (int)eNktHookFlags.flgOnlyPostCall);
            RegisterHook("user32.dll!EndPaint", EndPaint, (int)eNktHookFlags.flgOnlyPostCall);

            RegisterHook("user32.dll!DrawTextA", DrawText, (int)(eNktHookFlags.flgOnlyPreCall));
            RegisterHook("user32.dll!DrawTextW", DrawText, (int)(eNktHookFlags.flgOnlyPreCall));
            RegisterHook("user32.dll!DrawTextExA", DrawTextEx, (int)(eNktHookFlags.flgOnlyPreCall));
            RegisterHook("user32.dll!DrawTextExW", DrawTextEx, (int)(eNktHookFlags.flgOnlyPreCall));

            RegisterHook("gdi32.dll!BeginPaint", BeginPaint, (int)eNktHookFlags.flgOnlyPostCall);
            RegisterHook("gdi32.dll!EndPaint",   EndPaint, (int)eNktHookFlags.flgOnlyPostCall);

            RegisterHook("gdi32.dll!TextOutA",     TextOut, (int)(eNktHookFlags.flgOnlyPreCall));//??
            RegisterHook("gdi32.dll!TextOutW",     TextOut, (int)(eNktHookFlags.flgOnlyPreCall));//??

            RegisterHook("gdi32.dll!ExtTextOutA",     ExtTextOut, (int)(eNktHookFlags.flgOnlyPostCall));
            RegisterHook("gdi32.dll!ExtTextOutW",     ExtTextOut, (int)(eNktHookFlags.flgOnlyPostCall));
            return result.ToArray();
        }

        #region Start paint
        private void BeginPaint(NktHook hook, NktProcess proc, NktHookCallInfo callInfo)
        {
            var @params = callInfo.Params();

            var hWnd = @params.GetAt(0).PointerVal;

            if (IsGrab())
            {
                if (IsGrab() && GrabControls.TryGetValue(hWnd, out var grabData))
                {
                    StackPaintControl.Push(grabData);
                }
                else
                {
                    StackPaintControl.Push(null);
                }
            }
        }

        private void EndPaint(NktHook hook, NktProcess proc, NktHookCallInfo callInfo)
        {
            if (IsGrab())
            { 
                if (StackPaintControl.Count() != 0)
                {
                    _ = StackPaintControl.Pop();
                }
            }
        }
        #endregion

        #region USER32
        private void DrawText(NktHook hook, NktProcess proc, NktHookCallInfo callInfo)
        {
            var @params = callInfo.Params();
            var hdc = @params.GetAt(0).PointerVal;
            var lpchText = @params.GetAt(1).ReadString();
            var lprc = @params.GetAt(3).PointerVal;

            if (lpchText.Trim() == "")
            {
                return;
            }

            var hWnd = User32.WindowFromDC(hdc);
            var rect = GetStructure<tagRECT>(proc, lprc);

            LogUser32(hook.FunctionName, hWnd, hdc, lpchText, rect);

            if (IsGrab())
            {
                StackPeek()?.Add(rect.Value.Left, rect.Value.Top, lpchText, rect);
            }
        }

        private void DrawTextEx(NktHook hook, NktProcess proc, NktHookCallInfo callInfo)
        {
            var @params = callInfo.Params();
            var hdc = @params.GetAt(0).PointerVal;
            var lpchText = @params.GetAt(1).ReadString();
            var lprc = @params.GetAt(3).PointerVal;

            if (lpchText.Trim() == "")
            {
                return;
            }

            var hWnd = User32.WindowFromDC(hdc);
            var rect = GetStructure<tagRECT>(proc, lprc);
            if (!rect.HasValue || (rect.Value.Height == rect.Value.Width && rect.Value.Width == 0))
            {
                return;
            }
            LogUser32(hook.FunctionName, hWnd, hdc, lpchText, rect);

            if (IsGrab())
            {
                StackPeek()?.Add(rect.Value.Left, rect.Value.Top, lpchText, rect);
            }
        }
        #endregion

        #region GDI32

        private void TextOut(NktHook hook, NktProcess proc, NktHookCallInfo callInfo)
        {
            var @params = callInfo.Params();

            var hdc = @params.GetAt(0).PointerVal;
            var nXStart = @params.GetAt(1).LongVal;
            var nYStart = @params.GetAt(2).LongVal;
            var lpchText = @params.GetAt(3).ReadString();

            if (lpchText.Trim() == "")
            {
                return;
            }

            var hWnd = User32.WindowFromDC(hdc);
            LogGdi32(hook.FunctionName, hWnd, hdc, nXStart, nYStart, lpchText, null, null);
            
            if (IsGrab())
            {
                StackPeek()?.Add(nXStart, nYStart, lpchText, null);
            }
        }

        private void ExtTextOut(NktHook hook, NktProcess proc, NktHookCallInfo callInfo)
        {
            var @params = callInfo.Params();
            
            var hdc = @params.GetAt(0).PointerVal;
            var x = @params.GetAt(1).LongVal;
            var y = @params.GetAt(2).LongVal;
            var options = (ETOOptions)@params.GetAt(3).ULongVal;
            var lprect = @params.GetAt(4).PointerVal;
            var lpString = @params.GetAt(5).ReadString();

            var с = @params.GetAt(6).LongVal;
            var lpDx = @params.GetAt(7);

            if (lpString.Trim() == "")
            {
                return;
            }
            var rect = GetStructure<tagRECT>(proc, lprect);
            if (!rect.HasValue)
            {
                return;
            }


            var hWnd = User32.WindowFromDC(hdc);
            LogGdi32(hook.FunctionName, hWnd, hdc, x, y, lpString, rect, string.Join(',', GetAssignedFlags(options).Select(f => Enum.GetName(f.GetType(), f))));
         
            if (IsGrab())
            {
                StackPeek()?.Add(x, y, lpString, rect);
            }
        }
        #endregion

        #region LogConsole
        private void LogUser32(string functionName, IntPtr hWnd, IntPtr hdc, string lpchText, tagRECT? rect)
        {
            _ = $"{functionName,-25} HWnd=`{hWnd}` {{ hdc={hdc}, lpchText=`{lpchText}`, rect=`{rect}`}} ";
        }
        private void LogGdi32(string functionName, IntPtr hWnd, IntPtr hdc, int x, int y, string lpchText, tagRECT? lprect, string options)
        {
            _ = $"{functionName,-25}  HWnd=`{hWnd}` {{ hdc={hdc}, lpchText=`{lpchText}`, X=`{x}`, Y=`{y}`, lprect={lprect}, options=`{options}`}} ";
        }
        #endregion

        #region Service function
        public IEnumerable<T> GetAssignedFlags<T>(T options) where T: Enum
            => Enum.GetValues(typeof(T)).Cast<T>().Where(value => value.HasFlag(options));

        private T? GetStructure<T>(NktProcess proc, IntPtr lp) where T: struct
        {
            if (lp == IntPtr.Zero)
            {
                return null;
            }

            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];

            GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr pDest = pinnedBuffer.AddrOfPinnedObject();
            _ = proc.Memory().ReadMem(pDest, lp, (IntPtr)size).ToInt64();
            pinnedBuffer.Free();

            return Marshal.PtrToStructure<T>(pDest);
        }
        #endregion

        public static ControlElement GetWindowStructure(IntPtr hWnd)
        {
            var result = new ControlElement();
            result.HWnd = hWnd;
            result.ProcessId = User32.GetProcessId(hWnd);
            if (User32.GetWindowRect(hWnd, out tagRECT windowRect))
            {
                result.WindowRect = windowRect;
            }
            else
            {
                throw new Exception($"I can't get the control window rect from hWnd = {hWnd}");
            }

            if (User32.GetClientRect(hWnd, out tagRECT clientRect))
            {
                result.ClientRect = clientRect;
            }
            else
            {
                throw new Exception($"I can't get the control client rect from hWnd = {hWnd}");
            }
           
            var point = new System.Drawing.Point();
            if (!User32.ClientToScreen(hWnd, ref point))
            {
                throw new Exception($"I can't get client to screen point {result.ClientRect} for hWnd = {hWnd}");
            }
            var screenClientRect = result.ClientRect;
            screenClientRect.Offset(point);
            result.ScreenClientRect = screenClientRect;
            result.Children = WindowHelper.ChildHandles(hWnd).Select(hWnd => GetWindowStructure(hWnd)).ToArray();

            return result;
        }
    }
}
