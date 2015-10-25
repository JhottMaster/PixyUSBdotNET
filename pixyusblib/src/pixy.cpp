#include <stdio.h>
#include "pixy.h"
#include "pixyinterpreter.hpp"

PixyInterpreter interpreter;

/** 

  \mainpage libpixyusb-0.4 API Reference

  \section introduction Introduction

  libpixyusb is an open source library that allows you to communicate with
  Pixy over the USB protocol.

  This documentation is aimed at application developers wishing to send
  commands to Pixy or read sensor data from Pixy.

  \section library_features Library features

  - Read blocks with or without color codes
  - RGB LED control (color/intensity)
  - Auto white balance control
  - Auto exposure compensation control
  - Brightness control
  - Servo position control/query
  - Custom commands

  \section dependencies Dependencies

  Required to build:

  - <a href=http://www.cmake.org>cmake</a>
 
  Required for runtime:

  - <a href=http://www.libusb.org>libusb</a>
  - <a href=http://www.boost.org>libboost</a>

  \section getting_started Getting Started
 
  The libpixyusb API reference documentation can be found here:

  libpixyusb API Reference

  Some tutorials that use libpixyusb can be found here:

  <a href=http://cmucam.org/projects/cmucam5/wiki/Hooking_up_Pixy_to_a_Raspberry_Pi>Hooking up Pixy to a Raspberry Pi</a>

  <a href=http://cmucam.org/projects/cmucam5/wiki/Hooking_up_Pixy_to_a_Beaglebone_Black>Hooking up Pixy to a BeagleBone Black</a>

  \section getting_help Getting Help

  Tutorials, walkthroughs, and more are available on the Pixy wiki page:

  <a href=http://www.cmucam.org/projects/cmucam5/wiki>Pixy Developer Wiki Page</a>

  Our friendly developers and users might be able to answer your question on the forums:

  <a href=http://www.cmucam.org/projects/cmucam5/boards/9>Pixy Software Discussion Forum</a>
  
  <a href=http://www.cmucam.org/projects/cmucam5/boards/8>Pixy Hardware Discussion Forum</a>

*/

// Pixy C API //
__declspec(dllexport)
extern "C" 
{
  static struct
  {
    int           error;
    const char *  text;
  } PIXY_ERROR_TABLE[] = {
    { 0,                          "Success" },
    { PIXY_ERROR_USB_IO,          "USB Error: I/O" },
    { PIXY_ERROR_USB_BUSY,        "USB Error: Busy" },
    { PIXY_ERROR_USB_NO_DEVICE,   "USB Error: No device" },
    { PIXY_ERROR_USB_NOT_FOUND,   "USB Error: Target not found" },
    { PIXY_ERROR_CHIRP,           "Chirp Protocol Error" },
    { PIXY_ERROR_INVALID_COMMAND, "Pixy Error: Invalid command" },
    { 0,                          0 }
  };

  static int pixy_initialized = false;

  int __declspec(dllexport) pixy_init()
  {
    int return_value;

    return_value = interpreter.init();

    if(return_value == 0) 
    {
      pixy_initialized = true;
    }

    return return_value;
  }

  int  __declspec(dllexport) pixy_get_blocks(uint16_t max_blocks, struct Block * blocks)
  {
    return interpreter.get_blocks(max_blocks, blocks);
  }

  int  __declspec(dllexport) pixy_blocks_are_new()
  {
    return interpreter.blocks_are_new();
  }

  int  __declspec(dllexport) pixy_command(const char *name, ...)
  {
    va_list arguments;
    int     return_value;

    if(!pixy_initialized) return -1;

    va_start(arguments, name);
    return_value = interpreter.send_command(name, arguments);
    va_end(arguments);

    return return_value;
  }

  int  __declspec(dllexport) pixy_cam_getFrame(int mode, int X, int Y, int Width, int Height, unsigned char* *leFramePointer, uint32_t *numberPixels)
  {
	unsigned char *pixels;  //returned pointer to video frame buffer
	int32_t response, fourcc;
	int8_t renderflags;
	int return_value, res;
	uint16_t width, height;
	uint32_t  numPixels;

	unsigned char current_frame[72000];

	//  stop blob processing    
	return_value = pixy_command("stop", END_OUT_ARGS, &response, END_IN_ARGS);
	printf("STOP returned %d response %d\n", return_value, response);

	response = 0;
	return_value = pixy_command("cam_getFrame",  // String id for remote procedure
		0x01, mode,      // mode 0 = 1280x800 25 fps
		0x02, X,        // xoffset
		0x02, Y,         // yoffset
		0x02, Width,       // width
		0x02, Height,       // height (56 max @ 1280 w)
		0,              // separator
		&response,      // pointer to mem address for return value
		&fourcc,         //contrary to docs, the next 5 args are needed
		&renderflags,
		&width,
		&height,
		&numPixels,
		&pixels,        // pointer to mem address for returned frame
		0);

	//printf("getFrame returned %d response %d\n", return_value, response);
	//printf("returned w %d h %d npix %d\n", width, height, numPixels);

	// quit now if not successful:
	if (return_value != 0) return return_value;

	// save this block
	memcpy(&current_frame, pixels, numPixels);
	*numberPixels = numPixels;
	*leFramePointer = current_frame;
  }

  /*
  int renderBA81(uint8_t renderFlags, uint16_t width, uint16_t height, uint32_t frameLen, uint8_t *frame)
  {
	  uint16_t x, y;
	  uint32_t *line;
	  uint32_t r, g, b;
	  
	  // skip first line
	  frame += width;

	  // don't render top and bottom rows, and left and rightmost columns because of color
	  // interpolation
	  int arrayLen = (width - 2) * (height - 1) * 3;
	  uint32_t** pictureData = new uint32_t*[arrayLen];

	  for (y = 1; y<height - 1; y++)
	  {
		  frame++;
		  for (x = 1; x<width - 1; x++, frame++)
		  {
			  interpolateBayer(width, x, y, frame, r, g, b);
			  if (m_highlightOverexp && (r == 0xff || g == 0xff || b == 0xff))
				  *line++ = (0xff << 24); // | 0xff0000;
			  else
				  *line++ = (0xff << 24) | (r << 16) | (g << 8) | (b << 0);
		  }
		  frame++;
	  }
	  // send image to ourselves across threads
	  // from chirp thread to gui thread
	  emit image(img, renderFlags);

	  m_background = img;

	  return 0;
  }

  inline void interpolateBayer(unsigned int width, unsigned int x, unsigned int y, unsigned char *pixel, unsigned int &r, unsigned int &g, unsigned int &b)
  {
	  if (y & 1)
	  {
		  if (x & 1)
		  {
			  r = *pixel;
			  g = (*(pixel - 1) + *(pixel + 1) + *(pixel + width) + *(pixel - width)) >> 2;
			  b = (*(pixel - width - 1) + *(pixel - width + 1) + *(pixel + width - 1) + *(pixel + width + 1)) >> 2;
		  }
		  else
		  {
			  r = (*(pixel - 1) + *(pixel + 1)) >> 1;
			  g = *pixel;
			  b = (*(pixel - width) + *(pixel + width)) >> 1;
		  }
	  }
	  else
	  {
		  if (x & 1)
		  {
			  r = (*(pixel - width) + *(pixel + width)) >> 1;
			  g = *pixel;
			  b = (*(pixel - 1) + *(pixel + 1)) >> 1;
		  }
		  else
		  {
			  r = (*(pixel - width - 1) + *(pixel - width + 1) + *(pixel + width - 1) + *(pixel + width + 1)) >> 2;
			  g = (*(pixel - 1) + *(pixel + 1) + *(pixel + width) + *(pixel - width)) >> 2;
			  b = *pixel;
		  }
	  }
  }
  */

  void  __declspec(dllexport) pixy_close()
  {
    if(!pixy_initialized) return;

    interpreter.close();
  }

  void  __declspec(dllexport) pixy_error(int error_code)
  {
    int index;

    // Convert pixy error code to string and display to stdout //

    index = 0;

    while(PIXY_ERROR_TABLE[index].text != 0) {

      if(PIXY_ERROR_TABLE[index].error == error_code) {
        printf("%s\n", PIXY_ERROR_TABLE[index].text);
        return;
      }

      index += 1;
    }

    printf("Undefined error: [%d]\n", error_code);
  }

  int  __declspec(dllexport) pixy_led_set_RGB(uint8_t red, uint8_t green, uint8_t blue)
  {
    int      chirp_response;
    int      return_value;
    uint32_t RGB;

    // Pack the RGB value //
    RGB = blue + (green << 8) + (red << 16);

    return_value = pixy_command("led_set", INT32(RGB), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_led_set_max_current(uint32_t current)
  {
    int chirp_response;
    int return_value;

    return_value = pixy_command("led_setMaxCurrent", INT32(current), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_led_get_max_current()
  {
    int      return_value;
    uint32_t chirp_response;

    return_value = pixy_command("led_getMaxCurrent", END_OUT_ARGS, &chirp_response, END_IN_ARGS);

    if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_cam_set_auto_white_balance(uint8_t enable)
  {
    int      return_value;
    uint32_t chirp_response;

    return_value = pixy_command("cam_setAWB", UINT8(enable), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_cam_get_auto_white_balance()
  {
    int      return_value;
    uint32_t chirp_response;

    return_value = pixy_command("cam_getAWB", END_OUT_ARGS, &chirp_response, END_IN_ARGS);

    if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  uint32_t  __declspec(dllexport) pixy_cam_get_white_balance_value()
  {
    int      return_value;
    uint32_t chirp_response;

    return_value = pixy_command("cam_getWBV", END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_cam_set_white_balance_value(uint8_t red, uint8_t green, uint8_t blue)
  {
    int      return_value;
    uint32_t chirp_response;
    uint32_t white_balance;

    white_balance = green + (red << 8) + (blue << 16);

    return_value = pixy_command("cam_setAWB", UINT32(white_balance), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int __declspec(dllexport)  pixy_cam_set_auto_exposure_compensation(uint8_t enable)
  {
    int      return_value;
    uint32_t chirp_response;

    return_value = pixy_command("cam_setAEC", UINT8(enable), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
}
  
  int  __declspec(dllexport) pixy_cam_get_auto_exposure_compensation()
  {
    int      return_value;
    uint32_t chirp_response;

    return_value = pixy_command("cam_getAEC", END_OUT_ARGS, &chirp_response, END_IN_ARGS);

    if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_cam_set_exposure_compensation(uint8_t gain, uint16_t compensation)
  {
    int      return_value;
    uint32_t chirp_response;
    uint32_t exposure;

    exposure = gain + (compensation << 8);

    return_value = pixy_command("cam_setECV", UINT32(exposure), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_cam_get_exposure_compensation(uint8_t * gain, uint16_t * compensation)
  {
    uint32_t exposure;
    int      return_value;

    return_value = pixy_command("cam_getECV", END_OUT_ARGS, &exposure, END_IN_ARGS);

    if (return_value < 0) {
      // Chirp error //
      return return_value;
    }

    if(gain == 0 || compensation == 0) {
      // Error: Null pointer //
      return PIXY_ERROR_INVALID_PARAMETER;
    }

    printf("exp:%08x\n", exposure);

    *gain         = exposure & 0xFF;
    *compensation = 0xFFFF & (exposure >> 8);

    return 0;
  }

  int  __declspec(dllexport) pixy_cam_set_brightness(uint8_t brightness)
  {
    int chirp_response;
    int return_value;

    return_value = pixy_command("cam_setBrightness", UINT8(brightness), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_cam_get_brightness()
  {
    int chirp_response;
    int return_value;

    return_value = pixy_command("cam_getBrightness", END_OUT_ARGS, &chirp_response, END_IN_ARGS);

    if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_rcs_get_position(uint8_t channel)
  {
    int chirp_response;
    int return_value;

    return_value = pixy_command("rcs_getPos", UINT8(channel), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

    if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_rcs_set_position(uint8_t channel, uint16_t position)
  {
    int chirp_response;
    int return_value;

    return_value = pixy_command("rcs_setPos", UINT8(channel), INT16(position), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_rcs_set_frequency(uint16_t frequency)
  {
    int chirp_response;
    int return_value;

    return_value = pixy_command("rcs_setFreq", UINT16(frequency), END_OUT_ARGS, &chirp_response, END_IN_ARGS);

   if (return_value < 0) {
      // Error //
      return return_value;
    } else {
      // Success //
      return chirp_response;
    }
  }

  int  __declspec(dllexport) pixy_get_firmware_version(uint16_t * major, uint16_t * minor, uint16_t * build)
  {
    uint16_t * pixy_version;
    uint32_t   version_length;
    uint32_t   response;
    uint16_t   version[3];
    int        return_value;
    int        chirp_response;

    if(major == 0 || minor == 0 || build == 0) {
      // Error: Null pointer //
      return PIXY_ERROR_INVALID_PARAMETER;
    }

    return_value = pixy_command("version",  END_OUT_ARGS, &response, &version_length, &pixy_version, END_IN_ARGS);

    if (return_value < 0) {
      // Error //
      return return_value;
    }

    memcpy((void *) version, pixy_version, 3 * sizeof(uint16_t));

    *major = version[0];
    *minor = version[1];
    *build = version[2];

    return 0;
  }
}
