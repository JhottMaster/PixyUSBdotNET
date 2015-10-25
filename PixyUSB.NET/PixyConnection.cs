/*
* Created by Pablo D Aizpiri
* 10/24/2015
*
* .NET Library to talk to PixyCam using the PixyCam libraries.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace PixyUSBNet
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Block
    {
        public ushort Type;
        public ushort Signature;
        public ushort X;
        public ushort Y;
        public ushort Width;
        public ushort Height;
        public short Angle;
    }

    public enum FrameMode
    { 
        Size1280x800 = 0x00,
        Size640x400 = 0x11,
        Size320x200 = 0x21
    }

    public class PixyConnection
    {
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        private static extern int LoadLibrary(
            [MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        private static extern IntPtr GetProcAddress(int hModule,
            [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        private static extern bool FreeLibrary(int hModule);

        #region "PixyUSB External Call Registration"
        
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_init();
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_blocks_are_new();
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_get_blocks([In, Out] UInt16 max_blocks, ref Block bl);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_command(byte[] name);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void pixy_close();
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void pixy_error(int error_code);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_led_set_RGB(byte red, byte green, byte blue);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_led_set_max_current(UInt32 cur);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_led_get_max_current();
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_set_auto_white_balance_del(IntPtr v);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_get_auto_white_balance();
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static UInt32 pixy_cam_get_white_balance_value();
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_set_white_balance_value(byte red, byte green, byte blue);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_set_auto_exposure_compensation(IntPtr en);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_get_auto_exposure_compensation();
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_set_exposure_compensation(IntPtr gain, UInt16 compensa);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_get_exposure_compensation(IntPtr gain, IntPtr compensa);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_set_brightness(IntPtr bright);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_get_brightness();
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_rcs_get_position(byte cha);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_rcs_set_position(byte channel, ushort posi);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_rcs_set_frequency(ushort frequ);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_get_firmware_version(out ushort major, out ushort minor, out ushort b);
        [DllImport("pixyusblib.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int pixy_cam_getFrame(int mode, int X, int Y, int Width, int Height, out IntPtr current_frame, out uint size);

        #endregion

        private static int hModule = -1;

        /// <summary>Loads library to talk to pixy.</summary>
        /// <param name="pixyUSBLibDLLPath">Path to pixy DLL library.</param>
        public PixyConnection(string pixyUSBLibDLLPath = "pixyusblib.dll")
        {
            if (LoadPixyUSBLibrary(pixyUSBLibDLLPath) != true)
                throw new Exception("Failed to load pixie USB library!");
        }

        private bool LoadPixyUSBLibrary(string DLLLocation)
        {
            if (hModule > 0) 
                FreeLibrary(hModule);

            hModule = LoadLibrary(DLLLocation);
            return (hModule > 0);
        }
        
        /// <summary>Creates a connection with Pixy and listens for Pixy messages.</summary>
        /// <returns>Result of connection attempt.</returns>
        public PixyResult Initialize()
        {
            return (PixyResult)pixy_init();
        }

        /// <summary>Terminates connection with Pixy.</summary>
        public void Close()
        {
            pixy_close();
        }

        /// <summary>Indicates when new block data from Pixy is received. Returns true if block data has been updated 
        /// and false if block data has not changed since GetBlocks() was last called.</summary>
        /// <returns>Returns true if block data has been updated; false if stale.</returns>
        public bool BlockDataAvailable()
        {
            return (pixy_blocks_are_new() == 1);
        }

        /// <summary>Copies up to 'max_blocks' number of Blocks to the address pointed to by 'blocks'.</summary>
        /// <param name="MaxBlocks">Maximum number of Blocks to copy.</param>
        /// <returns></returns>
        public Block[] GetBlocks(int MaxBlocks)
        {
            Block[] tmpBlk = new Block[MaxBlocks];
            int results = pixy_get_blocks(Convert.ToUInt16(MaxBlocks), ref tmpBlk[0]);
            if (results >= 0)
            {
                Block[] tmpBlkReturn = new Block[results];
                if (results > 0)
                    Array.Copy(tmpBlk, tmpBlkReturn, results);
                return tmpBlkReturn;
            }
            else
            {
                PixyError errResults = (PixyError)results;
                System.Diagnostics.Debug.WriteLine("Error reading blocks: " + errResults.ToString());
            }
            return null;
        }

        /// <summary>Used to get a PixyCam Frame.</summary>
        /// <param name="Mode">Image mode</param>
        /// <param name="X">X offset to use to grab image data</param>
        /// <param name="Y">Y offset to use to grab image data</param>
        /// <param name="Width">Image width</param>
        /// <param name="Height">Image height</param>
        /// <returns></returns>
        public byte[] GetFrame(FrameMode Mode, int X, int Y, int Width, int Height)
        {
            if (Mode == FrameMode.Size1280x800 && (Width > 1280 || Width < 1 || Height > 800 || Height < 1))
                throw new Exception("Illegal frame size for selected frame mode!");
            if (Mode == FrameMode.Size640x400 && (Width > 640 || Width < 1 || Height > 400 || Height < 1))
                throw new Exception("Illegal frame size for selected frame mode!");
            if (Mode == FrameMode.Size320x200 && (Width > 320 || Width < 1 || Height > 200 || Height < 1))
                throw new Exception("Illegal frame size for selected frame mode!");

            IntPtr current_frame;
            uint size = 0;

            pixy_cam_getFrame((int)Mode, X, Y, Width, Height, out current_frame, out size);
            
            byte[] memStorage = new byte[size];
            Marshal.Copy(current_frame, memStorage, 0, Convert.ToInt32(size));
            
            return memStorage;
        }

        /// <summary>Sends a command to Pixy.</summary>
        /// <param name="command">Command to send.</param>
        /// <returns>Returns false if send failed.</returns>
        public bool SendCommand(string command)
        { 
            byte[] commandIn8bitFormat = new byte[command.Length+1];
            for(int x = 0; x < command.Length; x++)
                commandIn8bitFormat[x] = Convert.ToByte(command[x]);
            commandIn8bitFormat[command.Length] = Convert.ToByte('\n');

            return (pixy_command(commandIn8bitFormat) != -1);
        }

        /// <summary>Send description of pixy error to stdout.</summary>
        /// <param name="error">Pixy error code</param>
        public void PrintError(PixyError error)
        {
            pixy_error((int)error);
        }

        /// <summary> Set color of pixy LED. </summary>
        /// <param name="LEDColor">Color to set light</param>
        /// <returns>Returns false if request failed.</returns>
        public bool SetLEDColor(Color LEDColor)
        {
            return (pixy_led_set_RGB(LEDColor.R, LEDColor.G, LEDColor.B) == 0);
        }

        /// <summary> Set pixy LED maximum current (microamps)</summary>
        /// <param name="LEDColor">Maximum current (microamps)</param>
        /// <returns>Returns false if request failed.</returns>
        public bool SetLEDMaxCurrent(int current)
        {
            return (pixy_led_set_max_current(Convert.ToUInt32(current)) == 0);
        }

        /// <summary>Get pixy LED maximum current (microamps). Returns -1 on request failure;</summary>
        /// <returns>Non-negative Maximum LED current value (microamps). (-1 on error)</returns>
        public int GetLEDMaxCurrent()
        {
            return pixy_led_get_max_current();
        }

        /* TODO:
         * 
         * pixy_cam_set_auto_white_balance
         * pixy_cam_get_auto_white_balance
         * pixy_cam_get_white_balance_value
         * pixy_cam_set_white_balance_value
         * pixy_cam_set_auto_exposure_compensation
         * pixy_cam_get_auto_exposure_compensation
         * pixy_cam_set_exposure_compensation
         * pixy_cam_get_exposure_compensation
         * pixy_cam_set_brightness
         * pixy_cam_get_brightness 
         * 
        */

        /// <summary>Get pixy servo axis position. Returns -1 on request failure</summary>
        /// <param name="ServoChannel">Channel value. Range: [0, 1]</param>
        /// <returns>Returns position of channel. Range: [0, 999] (-1 on error)</returns>
        public int ServoGetPosition(int ServoChannel)
        {
            return pixy_rcs_get_position(Convert.ToByte(ServoChannel));
        }

        /// <summary>Set pixy servo axis position. Returns -1 on request failure.</summary>
        /// <param name="ServoChannel">Channel value. Range: [0, 1]</param>
        /// <param name="Position">Position value of the channel. Range: [0, 999]</param>
        /// <returns>Returns false if request failed.</returns>
        public bool ServoSetPosition(int ServoChannel, int Position)
        {
            return (pixy_rcs_set_position(Convert.ToByte(ServoChannel), Convert.ToUInt16(Position)) == 0);
        }

        /// <summary>Set pixy servo pulse width modulation (PWM) frequency.</summary>
        /// <param name="Frequency">Frequency Range: [20, 300] Hz Default: 50 Hz</param>
        /// <returns>Returns false if request failed.</returns>
        public bool ServoSetFrequency(int Frequency)
        {
            return (pixy_rcs_set_frequency(Convert.ToUInt16(Frequency)) == 0);
        }


        /// <summary>Get pixy firmware version. Returns null if request failed.</summary>
        /// <returns>Pixy firmware version; null if request failed.</returns>
        public Version GetFirmwareVersion()
        {
            ushort major = 0;
            ushort minor = 0;
            ushort build = 0;
            
            int result = pixy_get_firmware_version(out major, out minor, out build);
            var pixyVer = new Version(major, minor, build);

            if (result == 0)
                return pixyVer;
            return null;
        }

        ~PixyConnection()
        {
            if (hModule == 0)
                FreeLibrary(hModule);
        }
    }
}
