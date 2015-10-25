namespace PixyUSBNet
{
    public enum LibUSBError
    {
        /** Success (no error) */
        SUCCESS = 0,

        /** Input/output error */
        ERROR_IO = -1,

        /** Invalid parameter */
        ERROR_INVALID_PARAM = -2,

        /** Access denied (insufficient permissions) */
        ERROR_ACCESS = -3,

        /** No such device (it may have been disconnected) */
        ERROR_NO_DEVICE = -4,

        /** Entity not found */
        ERROR_NOT_FOUND = -5,

        /** Resource busy */
        ERROR_BUSY = -6,

        /** Operation timed out */
        ERROR_TIMEOUT = -7,

        /** Overflow */
        ERROR_OVERFLOW = -8,

        /** Pipe error */
        ERROR_PIPE = -9,

        /** System call interrupted (perhaps due to signal) */
        ERROR_INTERRUPTED = -10,

        /** Insufficient memory */
        ERROR_NO_MEM = -11,

        /** Operation not supported or unimplemented on this platform */
        ERROR_NOT_SUPPORTED = -12,

        /* NB! Remember to update error_name()
           when adding new error codes here. */

        /** Other error */
        ERROR_OTHER = -99,
    }

    public enum PixyError
    {
        PIXY_VID       = 0xB1AC,
        PIXY_PID       = 0xF000,
        PIXY_DFU_VID   = 0x1FC9,
        PIXY_DFU_PID   = 0x000C,
    }

   public enum PixyResult
   {
       SUCCESSFUL_CONNECTION = 0,
       USB_NOT_FOUND = LibUSBError.ERROR_NOT_FOUND,
       USB_BUSY = LibUSBError.ERROR_BUSY,
       USB_NO_DEVICE = LibUSBError.ERROR_NO_DEVICE,
       INVALID_PARAMETER = -150,
       USB_IO = LibUSBError.ERROR_IO,
       CHIRP = -151,
       INVALID_COMMAND = -152,
    }

   public enum PixyCRP
   {
       ARRAY             =  0x80, // bit
       FLT               =  0x10, // bit
       NO_COPY           =  (0x10 | 0x20),
       NULLTERM_ARRAY    =  (0x20 | ARRAY) ,// bits
       INT8              =  0x01,
       UINT8             =  0x01,
       INT16             =  0x02,
       UINT16            =  0x02,
       INT32             =  0x04,
       UINT32            =  0x04,
       FLT32             =  (FLT | 0x04),
       FLT64             =  (FLT | 0x08),
       STRING            =  (NULLTERM_ARRAY | INT8),
       TYPE_HINT         =  0x64, // type hint identifier
       INTS8             =  (INT8 | ARRAY),
       INTS16            =  (INT16 | ARRAY),
       INTS32            =  (INT32 | ARRAY),
       UINTS8            =  INTS8,
       UINTS8_NO_COPY    =  (INTS8 | NO_COPY),
       UINTS16_NO_COPY   =  (INTS16 | NO_COPY),
       UINTS32_NO_COPY   =  (INTS32 | NO_COPY),
       UINTS16           =  INTS16,
       UINTS32           =  INTS32,
       FLTS32            =  (FLT32 | ARRAY),
       FLTS64 = (FLT64 | ARRAY)
   }

}
