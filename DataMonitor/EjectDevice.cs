using System;
using System.Runtime.InteropServices;

namespace DataMonitor
{
    class EjectDevice
    {
        /* Ejects a drive based on provided drive letter. This is done by creating an
         * handle with the drive letter, passing that through along with the eject code
         * '0x2D4808' to eject the drive. Then the handle is closed. These methods are
         * all from the 'kernel32.dll'.
         * 
         * Param: String - letter of the drive to eject.
         * Return: None.
         */
        public void EjectDrive(String driveLetter)
        {
            String path = @"\\.\" + driveLetter;
            IntPtr handle = CreateFile(path, GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);

            int bytesReturned = 0;
            DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0,
                IntPtr.Zero, 0, ref bytesReturned, IntPtr.Zero);

            CloseHandle(handle);
        }

        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const int FILE_SHARE_READ = 0x1;
        const int FILE_SHARE_WRITE = 0x2;
        const int FSCTL_LOCK_VOLUME = 0x00090018;
        const int FSCTL_DISMOUNT_VOLUME = 0x00090020;
        const int IOCTL_STORAGE_EJECT_MEDIA = 0x2D4808;
        const int IOCTL_STORAGE_MEDIA_REMOVAL = 0x002D4804;

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr CreateFile(string filename,
            uint desiredAccess, uint shareMode, IntPtr securityAttributes,
            int creationDisposition, int flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32")]
        private static extern int DeviceIoControl(IntPtr deviceHandle,
            uint ioControlCode, IntPtr inBuffer, int inBufferSize,
            IntPtr outBuffer, int outBufferSize, ref int bytesReturned, IntPtr overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
