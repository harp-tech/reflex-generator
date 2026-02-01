#ifndef _APP_IOS_AND_REGS_H_
#define _APP_IOS_AND_REGS_H_
#include "cpu.h"

void init_ios(void);
/************************************************************************/
/* Definition of input pins                                             */
/************************************************************************/
// POKE0_IR          Description: Poke 0 infrared

#define read_POKE0_IR read_io(PORTD, 4)    // POKE0_IR

/************************************************************************/
/* Definition of output pins                                            */
/************************************************************************/
// DO3               Description: Output DO0

/* DO3 */
#define set_DO3 set_io(PORTC, 0)
#define clr_DO3 clear_io(PORTC, 0)
#define tgl_DO3 toggle_io(PORTC, 0)
#define read_DO3 read_io(PORTC, 0)

/************************************************************************/
/* Registers' structure                                                 */
/************************************************************************/
typedef struct
{
    uint8_t REG_DIGITAL_INPUTS;
    float_t REG_ANALOG_DATA[6];
    uint8_t REG_COMPLEX_CONFIGURATION[17];
    uint8_t REG_VERSION[32];
    uint32_t REG_CUSTOM_PAYLOAD[3];
    uint32_t REG_CUSTOM_RAW_PAYLOAD[3];
    uint8_t REG_CUSTOM_MEMBER_CONVERTER[3];
    uint8_t REG_BITMASK_SPLITTER;
} AppRegs;

/************************************************************************/
/* Registers' address                                                   */
/************************************************************************/
/* Registers */
#define ADD_REG_DIGITAL_INPUTS             32 // U8     
#define ADD_REG_ANALOG_DATA                33 // Float  
#define ADD_REG_COMPLEX_CONFIGURATION      34 // U8     
#define ADD_REG_VERSION                    35 // U8     
#define ADD_REG_CUSTOM_PAYLOAD             36 // U32    
#define ADD_REG_CUSTOM_RAW_PAYLOAD         37 // U32    
#define ADD_REG_CUSTOM_MEMBER_CONVERTER    38 // U8     
#define ADD_REG_BITMASK_SPLITTER           39 // U8     

/************************************************************************/
/* Tests registers' memory limits                                       */
/*                                                                      */
/* DON'T change the APP_REGS_ADD_MIN value !!!                          */
/* DON'T change these names !!!                                         */
/************************************************************************/
/* Memory limits */
#define APP_REGS_ADD_MIN                    0x20
#define APP_REGS_ADD_MAX                    0x27
#define APP_NBYTES_OF_REG_BANK              102

/************************************************************************/
/* Registers' bits                                                      */
/************************************************************************/
#define MSK_PWM_PORT_SEL    (7<<0)       // 
#define GM_PWM_PORT_PWM0    (1<<0)       // 
#define GM_PWM_PORT_PWM1    (2<<0)       // 
#define GM_PWM_PORT_PWM2    (4<<0)       // 

#endif /* _APP_REGS_H_ */