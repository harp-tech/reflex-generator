#ifndef _APP_H_
#define _APP_H_
#include <avr/io.h>


/************************************************************************/
/* Define versions                                                      */
/************************************************************************/
#define MAJOR_HW_VERSION 1
#define MINOR_HW_VERSION 0
#define MAJOR_FW_VERSION 0
#define MINOR_FW_VERSION 1
#define ASSEMBLY_VERSION 0


/************************************************************************/
/* Enable the interrupts                                                */
/************************************************************************/
#define hwbp_app_enable_interrupts 	PMIC_CTRL = PMIC_CTRL | PMIC_RREN_bm | PMIC_LOLVLEN_bm | PMIC_MEDLVLEN_bm | PMIC_HILVLEN_bm; __asm volatile("sei");


/************************************************************************/
/* Initialize the application                                           */
/************************************************************************/
void hwbp_app_initialize(void);


#endif /* _APP_H_ */