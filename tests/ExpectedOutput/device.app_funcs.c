#include "app_funcs.h"
#include "app_ios_and_regs.h"
#include "hwbp_core.h"


/************************************************************************/
/* Create pointers to functions                                         */
/************************************************************************/
extern AppRegs app_regs;

void (*app_func_rd_pointer[])(void) = {
	&app_read_REG_DIGITAL_INPUTS,
	&app_read_REG_ANALOG_DATA,
	&app_read_REG_COMPLEX_CONFIGURATION,
	&app_read_REG_VERSION,
	&app_read_REG_CUSTOM_PAYLOAD,
	&app_read_REG_CUSTOM_RAW_PAYLOAD,
	&app_read_REG_CUSTOM_MEMBER_CONVERTER,
	&app_read_REG_BITMASK_SPLITTER,
	&app_read_REG_COUNTER0,
};

bool (*app_func_wr_pointer[])(void*) = {
	&app_write_REG_DIGITAL_INPUTS,
	&app_write_REG_ANALOG_DATA,
	&app_write_REG_COMPLEX_CONFIGURATION,
	&app_write_REG_VERSION,
	&app_write_REG_CUSTOM_PAYLOAD,
	&app_write_REG_CUSTOM_RAW_PAYLOAD,
	&app_write_REG_CUSTOM_MEMBER_CONVERTER,
	&app_write_REG_BITMASK_SPLITTER,
	&app_write_REG_COUNTER0,
};

/************************************************************************/
/* REG_DIGITAL_INPUTS                                                   */
/************************************************************************/
void app_read_REG_DIGITAL_INPUTS(void) {}
bool app_write_REG_DIGITAL_INPUTS(void *a) { return false; }

/************************************************************************/
/* REG_ANALOG_DATA                                                      */
/************************************************************************/
void app_read_REG_ANALOG_DATA(void) {}
bool app_write_REG_ANALOG_DATA(void *a) { return false; }

/************************************************************************/
/* REG_COMPLEX_CONFIGURATION                                            */
/************************************************************************/
void app_read_REG_COMPLEX_CONFIGURATION(void) {}
bool app_write_REG_COMPLEX_CONFIGURATION(void *a) { return true; }

/************************************************************************/
/* REG_VERSION                                                          */
/************************************************************************/
void app_read_REG_VERSION(void) {}
bool app_write_REG_VERSION(void *a) { return false; }

/************************************************************************/
/* REG_CUSTOM_PAYLOAD                                                   */
/************************************************************************/
void app_read_REG_CUSTOM_PAYLOAD(void) {}
bool app_write_REG_CUSTOM_PAYLOAD(void *a) { return false; }

/************************************************************************/
/* REG_CUSTOM_RAW_PAYLOAD                                               */
/************************************************************************/
void app_read_REG_CUSTOM_RAW_PAYLOAD(void) {}
bool app_write_REG_CUSTOM_RAW_PAYLOAD(void *a) { return false; }

/************************************************************************/
/* REG_CUSTOM_MEMBER_CONVERTER                                          */
/************************************************************************/
void app_read_REG_CUSTOM_MEMBER_CONVERTER(void) {}
bool app_write_REG_CUSTOM_MEMBER_CONVERTER(void *a) { return false; }

/************************************************************************/
/* REG_BITMASK_SPLITTER                                                 */
/************************************************************************/
void app_read_REG_BITMASK_SPLITTER(void) {}
bool app_write_REG_BITMASK_SPLITTER(void *a) { return true; }

/************************************************************************/
/* REG_COUNTER0                                                         */
/************************************************************************/
void app_read_REG_COUNTER0(void) {}
bool app_write_REG_COUNTER0(void *a) { return false; }
