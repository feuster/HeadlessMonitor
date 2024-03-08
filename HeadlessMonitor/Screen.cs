using Windows.Win32;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.Foundation;
using System.Runtime.InteropServices;

//Disable unimportant warnings
#pragma warning disable S125
#pragma warning disable SYSLIB1054

namespace Screen
{
    public class PrimaryScreen
    {
        #region DPI Awareness
        public class DpiAwareness : PrimaryScreen
        {
            /// <summary>
            /// DPI unaware. This app does not scale for DPI changes and is always assumed to have a scale factor of 100% (96 DPI). It will be automatically scaled by the system on any other DPI setting.
            /// </summary>
            /// <returns>A 32-bit value that is used to describe an error or warning.</returns>
            public static bool SetDPIUnaware()
            {
                HRESULT result = PInvoke.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE);
                return result.Succeeded;
            }

            /// <summary>
            /// Per monitor DPI aware. This app checks for the DPI when it is created and adjusts the scale factor whenever the DPI changes. These applications are not automatically scaled by the system.
            /// </summary>
            /// <returns>A 32-bit value that is used to describe an error or warning.</returns>
            public static bool SetDPIPerMonitorAware()
            {
                HRESULT result = PInvoke.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
                return result.Succeeded;
            }
            /// <summary>
            /// System DPI aware. This app does not scale for DPI changes. It will query for the DPI once and use that value for the lifetime of the app. If the DPI changes, the app will not adjust to the new DPI value. It will be automatically scaled up or down by the system when the DPI changes from the system value.
            /// </summary>
            /// <returns>A 32-bit value that is used to describe an error or warning.</returns>
            public static bool SetSystemDPIAware()
            {
                HRESULT result = PInvoke.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE);
                return result.Succeeded;
            }
        }
        #endregion

        #region DPI Conversions
        public class DpiConversions : PrimaryScreen
        {
            /// <summary>
            /// Converts the DPI value into the DPI percentage value
            /// </summary>
            /// <param name="DPI">DPI value</param>
            /// <returns>DPI in percent</returns>
            public static uint DPItoDPIPERCENT(uint DPI)
            {
                return DPI switch
                {
                    96 => 100,
                    120 => 125,
                    144 => 150,
                    168 => 175,
                    192 => 200,
                    _ => Convert.ToUInt16(Math.Abs(100m / 96m * DPI)) //this calculates the actual non-standard factor
                };
            }

            /// <summary>
            /// Converts the DPI value into the internal DPI index
            /// </summary>
            /// <param name="DPI">DPI value</param>
            /// <returns>DPI index</returns>
            public static uint DPItoDPIINDEX(uint DPI)
            {
                return DPI switch
                {
                    100 => 0,//Default 100%
                    120 => 1,//125%
                    144 => 2,//150%
                    168 => 3,//175%
                    192 => 4,//200%
                    _ => Convert.ToUInt16(Math.Abs((DPI - 96) / 24)) //this calculates the actual non-standard index
                };
            }

            /// <summary>
            /// Converts the DPI percentage value into the DPI value
            /// </summary>
            /// <param name="DPIPERCENT">DPI in percent</param>
            /// <returns>DPI value</returns>
            public static uint DPIPERCENTtoDPI(uint DPIPERCENT)
            {
                return DPIPERCENT switch
                {
                    100 => 96,
                    125 => 120,
                    150 => 144,
                    175 => 168,
                    200 => 192,
                    _ => Convert.ToUInt16(Math.Abs(DPIPERCENT / 100m * 96m)) //this calculates the actual non-standard factor
                };
            }

            /// <summary>
            /// Converts the DPI percentage value into the internal DPI index
            /// </summary>
            /// <param name="DPIPERCENT">DPI in percent</param>
            /// <returns>DPI index</returns>
            public static uint DPIPERCENTtoDPIINDEX(uint DPIPERCENT)
            {
                return DPIPERCENT switch
                {
                    100 => 0,//Default 100%
                    125 => 1,//125%
                    150 => 2,//150%
                    175 => 3,//175%
                    200 => 4,//200%
                    _ => Convert.ToUInt16(Math.Abs((DPIPERCENTtoDPI(DPIPERCENT) - 96) / 24)) //this calculates the actual non-standard index
                };
            }
        }
        #endregion

        #region Primary screen stats
        public class PrimaryScreenStats : PrimaryScreen
        {
            /// <summary>
            /// Get the width of the primary screen
            /// </summary>
            /// <returns>Width in pixels</returns>
            public static int GetPrimaryScreenWidth()
            {
                unsafe
                {
                    HMONITOR hmonitor = PInvoke.MonitorFromWindow(PInvoke.GetShellWindow(), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
                    MONITORINFO info = new()
                    {
                        cbSize = (uint)sizeof(MONITORINFO)
                    };
                    PInvoke.GetMonitorInfo(hmonitor, ref info);
                    return info.rcMonitor.Width;
                }
            }

            /// <summary>
            /// Get the height of the primary screen
            /// </summary>
            /// <returns>Height in pixels</returns>
            public static int GetPrimaryScreenHeight()
            {
                unsafe
                {
                    HMONITOR hmonitor = PInvoke.MonitorFromWindow(PInvoke.GetShellWindow(), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
                    MONITORINFO info = new()
                    {
                        cbSize = (uint)sizeof(MONITORINFO)
                    };
                    PInvoke.GetMonitorInfo(hmonitor, ref info);
                    return info.rcMonitor.Height;
                }
            }

            /// <summary>
            /// Get the DPI of the primary screen
            /// </summary>
            /// <param name="DPIinPercent">return value is DPI or DPI in percent</param>
            /// <returns>DPI</returns>
            public static uint GetPrimaryScreenDPI(bool DPIinPercent = true)
            {
                //Alternative API function for getting the DPI: PInvoke.GetDpiForWindow(PInvoke.GetShellWindow());
                HMONITOR hmonitor = PInvoke.MonitorFromWindow(PInvoke.GetShellWindow(), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY);
                PInvoke.GetDpiForMonitor(hmonitor, MONITOR_DPI_TYPE.MDT_DEFAULT, out uint dpiX, out uint dpiY);
                if(DPIinPercent)
                    return DpiConversions.DPItoDPIPERCENT(Convert.ToUInt16(Math.Round((dpiX + dpiY) / 2.0)));
                else
                    return Convert.ToUInt16(Math.Round((dpiX + dpiY) / 2.0));
            }
        }
        #endregion

        #region Primary screen actions
        public class Actions : PrimaryScreen
        {
            //Do not use PInvoke for SystemParametersInfo since we need a int parameter instead an uint for uiParam
            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool SystemParametersInfo(uint uiAction, int uiParam, IntPtr pvParam, uint fWinIni);

            /// <summary>
            /// Set the resolution of the primary screen
            /// </summary>
            /// <param name="Width">new width in pixels</param>
            /// <param name="Height"><new height in pixels/param>
            /// <returns></returns>
            public static bool SetPrimaryResolution(uint Width = 1920, uint Height = 1080)
            {
                //do nothing if resolution is already set
                if (PrimaryScreenStats.GetPrimaryScreenWidth() == Width && PrimaryScreenStats.GetPrimaryScreenHeight() == Height)
                    return true;

                //Get actual display infos and set new resolution
                DISPLAY_DEVICEW d = new();
                d.cb = (uint)Marshal.SizeOf(d);
                DEVMODEW dm = new();
                PInvoke.EnumDisplayDevices(null, 0, ref d, 0);
                PInvoke.EnumDisplaySettings(d.DeviceName.ToString(), 0, ref dm);
                dm.dmPelsWidth = Width;
                dm.dmPelsHeight = Height;
                DISP_CHANGE dp;
                unsafe
                {
                    dp = PInvoke.ChangeDisplaySettingsEx(d.DeviceName.ToString(), dm, CDS_TYPE.CDS_UPDATEREGISTRY, (void*)IntPtr.Zero);
                }
                if (dp == DISP_CHANGE.DISP_CHANGE_SUCCESSFUL)
                    return true;
                else
                    return false;
            }

            /// <summary>
            /// Set DPI for primary screen
            /// </summary>
            /// <param name="ScalingPercent"></param>
            /// <returns>Result state</returns>
            public static bool SetPrimaryDPI(uint ScalingPercent = 125)
            {
                //do nothing if DPI is already set
                if (PrimaryScreenStats.GetPrimaryScreenDPI() == ScalingPercent)
                    return true;

                //loop DPI values until the desired DPI is found and set (should work on all monitor sizes and DPI aware independent)
                bool result = false;
                for (int DPIScalingIndex = -4; DPIScalingIndex<5; DPIScalingIndex++)
                {
                    result = SystemParametersInfo(0x009F, DPIScalingIndex, (IntPtr)0, 0x0001);
                    if (PrimaryScreenStats.GetPrimaryScreenDPI() == ScalingPercent)
                        break;
                }
                return result;
            }
        }
        #endregion
    }
}
